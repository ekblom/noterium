using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Noterium.Controls
{
	[ValueConversion(typeof (bool), typeof (Visibility))]
	public class InvertedBoolToVisibility : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var enabled = (bool) value;
			if (enabled)
			{
				return Visibility.Collapsed;
			}
			return Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}

	[TemplatePart(Name = "PART_CreateTagButton", Type = typeof (Button))]
	public class TokenizedTagControl : ListBox //, INotifyPropertyChanged
	{
		public static readonly DependencyProperty AllTagsProperty = DependencyProperty.Register("AllTags", typeof (List<string>), typeof (TokenizedTagControl), new PropertyMetadata(new List<string>()));
		public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register("Placeholder", typeof (string), typeof (TokenizedTagControl), new PropertyMetadata("Click here to enter tags..."));
		public static readonly DependencyProperty IsSelectableProperty = DependencyProperty.Register("IsSelectable", typeof (bool), typeof (TokenizedTagControl), new PropertyMetadata(false));
	
		private List<string> _allTags = new List<string>();

		public TokenizedTagControl()
		{
			if (ItemsSource == null)
				ItemsSource = new List<TokenizedTagItem>();

			if (AllTags == null)
				AllTags = new List<string>();

			LostKeyboardFocus += TokenizedTagControlLostKeyboardFocus;
		}

		static TokenizedTagControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof (TokenizedTagControl), new FrameworkPropertyMetadata(typeof (TokenizedTagControl)));
		}

		public List<string> EnteredTags
		{
			get
			{
				if (!ReferenceEquals(ItemsSource, null) && ((IList<TokenizedTagItem>) ItemsSource).Any())
				{
					var tokenizedTagItems = (IList<TokenizedTagItem>) ItemsSource;
					var typedTags = (from TokenizedTagItem item in tokenizedTagItems
						select item.Text);
					return typedTags.ToList();
				}
				return new List<string>(0);
			}
		}

		public List<string> AllTags
		{
			get
			{
				if (!ReferenceEquals(ItemsSource, null) && ((IList<TokenizedTagItem>) ItemsSource).Any())
				{
					var tokenizedTagItems = (IList<TokenizedTagItem>) ItemsSource;
					var typedTags = (from TokenizedTagItem item in tokenizedTagItems select item.Text);

					return (_allTags).Except(typedTags).ToList();
				}
				return _allTags;
			}
			set
			{
				SetValue(AllTagsProperty, value);
				_allTags = value;
			}
		}

		public string Placeholder
		{
			get { return (string) GetValue(PlaceholderProperty); }
			set { SetValue(PlaceholderProperty, value); }
		}

		public bool IsSelectable
		{
			get { return (bool) GetValue(IsSelectableProperty); }
			set { SetValue(IsSelectableProperty, value); }
		}

		public bool IsEditing
		{
			get { return (bool) GetValue(IsEditingProperty); }
			internal set { SetValue(IsEditingPropertyKey, value); }
		}

		public event EventHandler<TokenizedTagEventArgs> TagClick;
		public event EventHandler<TokenizedTagEventArgs> TagAdded;
		public event EventHandler<TokenizedTagEventArgs> TagApplied;
		public event EventHandler<TokenizedTagEventArgs> TagRemoved;

		private void TokenizedTagControlLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			if (!IsSelectable)
			{
				SelectedItem = null;
				return;
			}

			TokenizedTagItem itemToSelect = null;
			if (Items.Count > 0 && !ReferenceEquals((TokenizedTagItem) Items.CurrentItem, null))
			{
				if (SelectedItem != null && ((TokenizedTagItem) SelectedItem).Text != null &&
				    !((TokenizedTagItem) SelectedItem).IsEditing)
				{
					itemToSelect = (TokenizedTagItem) SelectedItem;
				}
				else if (!String.IsNullOrWhiteSpace(((TokenizedTagItem) Items.CurrentItem).Text))
				{
					itemToSelect = (TokenizedTagItem) Items.CurrentItem;
				}
			}

			// select the previous item
			if (!ReferenceEquals(itemToSelect, null))
			{
				e.Handled = true;
				RaiseTagApplied(itemToSelect);
				if (IsSelectable)
				{
					SelectedItem = itemToSelect;
				}
			}
		}

		private void UpdateAllTagsProperty()
		{
			SetValue(AllTagsProperty, AllTags);
		}

		public override void OnApplyTemplate()
		{
			OnApplyTemplate();
		}

	    // ReSharper disable once MethodOverloadWithOptionalParameter
		public void OnApplyTemplate(TokenizedTagItem appliedTag = null)
		{
			var createBtn = GetTemplateChild("PART_CreateTagButton") as Button;
			if (createBtn != null)
			{
				createBtn.Click -= CreateBtnClick;
				createBtn.Click += CreateBtnClick;
			}

			base.OnApplyTemplate();

			if (appliedTag != null && !ReferenceEquals(TagApplied, null))
			{
				TagApplied.Invoke(this, new TokenizedTagEventArgs(appliedTag));
			}
		}

		/// <summary>
		///     Executed when create new tag button is clicked.
		///     Adds an TokenizedTagItem to the collection and puts it in edit mode.
		/// </summary>
		private void CreateBtnClick(object sender, RoutedEventArgs e)
		{
			SelectedItem = InitializeNewTag();
		}

		internal TokenizedTagItem InitializeNewTag(bool suppressEditing = false)
		{
			var newItem = new TokenizedTagItem {IsEditing = !suppressEditing};
			AddTag(newItem);
			UpdateAllTagsProperty();
			IsEditing = !suppressEditing;

			return newItem;
		}

		/// <summary>
		///     Adds a tag to the collection
		/// </summary>
		internal void AddTag(TokenizedTagItem tag)
		{
			TokenizedTagItem itemToSelect = null;
			if (SelectedItem == null && Items.Count > 0)
			{
				itemToSelect = (TokenizedTagItem) SelectedItem;
			}
			((IList) ItemsSource).Add(tag); // assume IList for convenience
			Items.Refresh();

			// select the previous item
			if (!ReferenceEquals(itemToSelect, null))
			{
				RaiseTagClick(itemToSelect);
				if (IsSelectable)
					SelectedItem = itemToSelect;
			}

			if (TagAdded != null)
				TagAdded(this, new TokenizedTagEventArgs(tag));
		}

		/// <summary>
		///     Removes a tag from the collection
		/// </summary>
		internal void RemoveTag(TokenizedTagItem tag, bool cancelEvent = false)
		{
			if (ItemsSource != null)
			{
				((IList) ItemsSource).Remove(tag); // assume IList for convenience
				Items.Refresh();

				if (TagRemoved != null && !cancelEvent)
				{
					TagRemoved(this, new TokenizedTagEventArgs(tag));
				}

				// select the last item
				if (SelectedItem == null && Items.Count > 0)
				{
					//TokenizedTagItem itemToSelect = this.Items.GetItemAt(0) as TokenizedTagItem;
					var itemToSelect = Items.GetItemAt(Items.Count - 1) as TokenizedTagItem;
					if (!ReferenceEquals(itemToSelect, null))
					{
						RaiseTagClick(itemToSelect);
						if (IsSelectable)
							SelectedItem = itemToSelect;
					}
				}
			}
		}

		/// <summary>
		///     Raises the TagClick event
		/// </summary>
		internal void RaiseTagClick(TokenizedTagItem tag)
		{
			if (TagClick != null)
			{
				TagClick(this, new TokenizedTagEventArgs(tag));
			}
		}

		/// <summary>
		///     Raises the TagClick event
		/// </summary>
		internal void RaiseTagApplied(TokenizedTagItem tag)
		{
			if (TagApplied != null)
			{
				TagApplied(this, new TokenizedTagEventArgs(tag));
			}
		}

		/// <summary>
		///     Raises the TagDoubleClick event
		/// </summary>
		internal void RaiseTagDoubleClick(TokenizedTagItem tag)
		{
			UpdateAllTagsProperty();
			tag.IsEditing = true;
		}

		private static readonly DependencyPropertyKey IsEditingPropertyKey = DependencyProperty.RegisterReadOnly("IsEditing", typeof (bool), typeof (TokenizedTagControl), new FrameworkPropertyMetadata(false));
		public static readonly DependencyProperty IsEditingProperty = IsEditingPropertyKey.DependencyProperty;
	}

	public class TokenizedTagEventArgs : EventArgs
	{
		public TokenizedTagEventArgs(TokenizedTagItem item)
		{
			Item = item;
		}

		public TokenizedTagItem Item { get; set; }
	}
}