using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Noterium.Core;

namespace Noterium.Controls
{
	[TemplatePart(Name = "PART_InputBox", Type = typeof(AutoCompleteBox))]
	[TemplatePart(Name = "PART_DeleteTagButton", Type = typeof(Button))]
	[TemplatePart(Name = "PART_TagButton", Type = typeof(Button))]
	public class TokenizedTagItem : Control, INotifyPropertyChanged
	{
		public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(TokenizedTagItem), new PropertyMetadata(null));
		public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("ColorProperty", typeof(Color), typeof(TokenizedTagItem), new PropertyMetadata(null));
		private static readonly DependencyPropertyKey IsEditingPropertyKey = DependencyProperty.RegisterReadOnly("IsEditing", typeof(bool), typeof(TokenizedTagItem), new FrameworkPropertyMetadata(false));
		public static readonly DependencyProperty IsEditingProperty = IsEditingPropertyKey.DependencyProperty;

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
		    handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}


		public TokenizedTagItem()
		{
            SetValue(ColorProperty, Color.FromArgb(204, 17, 158, 218));
		}

		public TokenizedTagItem(string text)
			: this()
		{
			Text = text;
		}

		static TokenizedTagItem()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(TokenizedTagItem), new FrameworkPropertyMetadata(typeof(TokenizedTagItem)));
		}

		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set
			{
				SetValue(TextProperty, value);
				var tag = Hub.Instance.Settings.Tags.FirstOrDefault(t => t.Name.Equals(value));
				if (tag != null)
					Color = tag.Color;
				OnPropertyChanged();
			}
		}

		public bool IsEditing
		{
			get { return (bool)GetValue(IsEditingProperty); }
			internal set { SetValue(IsEditingPropertyKey, value); }
		}

		public Color Color
		{
			get { return (Color)GetValue(ColorProperty); }
			internal set
			{
				SetValue(ColorProperty, value);
				OnPropertyChanged();
			}
		}

		/// <summary>
		///     Wires up delete button click and focus lost
		/// </summary>
		public override void OnApplyTemplate()
		{
			var inputBox = GetTemplateChild("PART_InputBox") as AutoCompleteBox;
			if (inputBox != null)
			{
				inputBox.LostKeyboardFocus += InputBoxLostFocus;
				inputBox.Loaded += InputBoxLoaded;
			}

			var btn = GetTemplateChild("PART_TagButton") as Button;
			if (btn != null)
			{
				btn.Loaded += (s, e) =>
				{
					var b = s as Button;
				    var btnDelete = b?.Template.FindName("PART_DeleteTagButton", b) as Button; // will only be found once button is loaded
				    if (btnDelete != null)
				    {
				        btnDelete.Click -= BtnDeleteClick; // make sure the handler is applied just once
				        btnDelete.Click += BtnDeleteClick;
				    }
				};

				btn.Click += (s, e) => //btn.Click 
				{
					var parent = GetParent();
					if (parent != null)
					{
						parent.RaiseTagClick(this); // raise the TagClick event of the TokenizedTagControl

						if (parent.IsSelectable)
						{
							//e.Handled = true;
							parent.SelectedItem = this;
						}
						//parent.SelectedItem = this;
					}
				};

				btn.MouseDoubleClick += (s, e) =>
				{
					var parent = GetParent();
					if (parent != null)
					{
						parent.RaiseTagDoubleClick(this); // raise the TagClick event of the TokenizedTagControl

						if (parent.IsSelectable)
						{
							//e.Handled = true;
							parent.SelectedItem = this;
						}
					}
				};
			}

			base.OnApplyTemplate();
		}

		/// <summary>
		///     Handles the click on the delete glyph of the tag button.
		///     Removes the tag from the collection.
		/// </summary>
		private void BtnDeleteClick(object sender, RoutedEventArgs e)
		{
			var item = FindUpVisualTree<TokenizedTagItem>(sender as FrameworkElement);
			var parent = GetParent();
			if (item != null && parent != null)
				parent.RemoveTag(item);

			e.Handled = true; // bubbling would raise the tag click event
		}

		private static bool IsDuplicate(TokenizedTagControl tagControl, string compareTo)
		{
			var duplicateCount = (from TokenizedTagItem item in (IList)tagControl.ItemsSource
								  where item.Text.ToLower() == compareTo.ToLower()
								  select item).Count();
			if (duplicateCount > 1)
				return true;

			return false;
		}

		/// <summary>
		///     When an AutoCompleteBox is created, set the focus to the textbox.
		///     Wire PreviewKeyDown event to handle Escape/Enter keys
		/// </summary>
		/// <remarks>AutoCompleteBox.Focus() is broken: http://stackoverflow.com/questions/3572299/autocompletebox-focus-in-wpf</remarks>
		private void InputBoxLoaded(object sender, RoutedEventArgs e)
		{
			var acb = sender as AutoCompleteBox;

			if (acb != null)
			{
				var tb = acb.Template.FindName("Text", acb) as TextBox;

				if (tb != null)
					tb.Focus();
				// PreviewKeyDown, because KeyDown does not bubble up for Enter
				acb.PreviewKeyDown += (s, e1) =>
				{
					var parent = GetParent();
					if (parent != null)
					{
						switch (e1.Key)
						{
							case (Key.Enter): // accept tag
								if (!string.IsNullOrWhiteSpace(Text))
								{
									if (IsDuplicate(parent, Text))
										break;
									parent.OnApplyTemplate(this);
									parent.SelectedItem = parent.InitializeNewTag(); //creates another tag
								}
								else
									parent.Focus();
								break;
							case (Key.Escape): // reject tag
								parent.Focus();
								break;
							case (Key.Back):
								if (string.IsNullOrWhiteSpace(Text))
								{
									InputBoxLostFocus(this, new RoutedEventArgs());
									var previousTagIndex = ((IList)parent.ItemsSource).Count - 1;
									if (previousTagIndex < 0) break;

									var previousTag = (((IList)parent.ItemsSource)[previousTagIndex] as TokenizedTagItem);
								    if (previousTag != null)
								    {
								        previousTag.Focus();
								        previousTag.IsEditing = true;
								    }
								}
								break;
						}
					}
				};
			}
		}

		/// <summary>
		///     Set IsEditing to false when the AutoCompleteBox loses keyboard focus.
		///     This will change the template, displaying the tag as a button.
		/// </summary>
		private void InputBoxLostFocus(object sender, RoutedEventArgs e)
		{
			var parent = GetParent();
			if (!string.IsNullOrWhiteSpace(Text))
			{
				if (parent != null)
				{
					if (IsDuplicate(parent, Text))
						parent.RemoveTag(this, true); // do not raise RemoveTag event
				}
			    AutoCompleteBox autoCompleteBox = sender as AutoCompleteBox;
			    if (autoCompleteBox != null && !autoCompleteBox.IsDropDownOpen)
				{
					IsEditing = false;
				}
			}
			else if (parent != null)
				parent.RemoveTag(this, true); // do not raise RemoveTag event

			if (parent != null)
			{
				parent.IsEditing = false;
			}
		}

		private TokenizedTagControl GetParent()
		{
			return FindUpVisualTree<TokenizedTagControl>(this);
		}

		/// <summary>
		///     Walks up the visual tree to find object of type T, starting from initial object
		///     http://www.codeproject.com/Tips/75816/Walk-up-the-Visual-Tree
		/// </summary>
		private static T FindUpVisualTree<T>(DependencyObject initial) where T : DependencyObject
		{
			var current = initial;
			while (current != null && current.GetType() != typeof(T))
			{
				current = VisualTreeHelper.GetParent(current);
			}
			return current as T;
		}

	}
}