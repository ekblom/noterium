using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using log4net;
using Noterium.Code.Commands;
using Noterium.Code.Data;
using Noterium.Code.Markdown;
using Noterium.Core;
using Noterium.Core.DataCarriers;
using Noterium.Core.DropBox;
using Noterium.Properties;
using Noterium.ViewModels;
using Noterium.Windows;
using GalaSoft.MvvmLight;

namespace Noterium.Views
{
	/// <summary>
	/// Interaction logic for NoteEditor.xaml
	/// </summary>
	public partial class NoteEditor : INotifyPropertyChanged
	{
		private const string MarkDownImageFormat = "![{1}]({0})";
		private readonly ILog _log = LogManager.GetLogger(typeof(NoteEditor));

		public static readonly DependencyProperty NoteViewModelProperty = DependencyProperty.Register("NoteViewModel", typeof(NoteViewModel), typeof(NoteEditor), new UIPropertyMetadata(null));
		public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(NoteEditor), new UIPropertyMetadata(null));
		private readonly Timer _saveTimer;
		private List<DocumentEntitiy> _documentEntities = new List<DocumentEntitiy>();

		public DocumentEntitiy CurrentEntity { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public NoteViewModel CurrentModel
		{
			get
			{
				if (DataContext is NoteViewModel)
					return ((NoteViewModel)DataContext);
				return null;
			}
		}

		public bool SupressOnTextChanged { get; set; }

		public NoteEditor()
		{
			// https://social.msdn.microsoft.com/Forums/vstudio/en-US/e2bf5cc5-618f-4d46-bf22-e07d0b4bc64a/wpf-45-win-8-problem-with-presentationuiaero2?forum=wpf
			InitializeComponent();

			if (Hub.Instance.Settings != null)
				TagControl.AllTags = Hub.Instance.Settings.Tags.ToList().ConvertAll(t => t.Name);
			DataContextChanged += OnDataContextChanged;

			//MarkdownText.TextArea.MouseWheel += TextAreaMouseWheel;
			//MarkdownText.TextArea.Caret.PositionChanged += CaretOnPositionChanged;

			_saveTimer = new Timer();
			_saveTimer.AutoReset = false;
			_saveTimer.Elapsed += SaveTimerElapsed;
			_saveTimer.Interval = 250;
		}

		private void TextAreaMouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (TableToolbarPopup.IsOpen)
				TableToolbarPopup.IsOpen = false;
		}

		private void CaretOnPositionChanged(object sender, EventArgs eventArgs)
		{
			if (!Hub.Instance.Settings.EnableAdvancedControls)
				return;

			Popup toolbar = null;

			if (_documentEntities.Any())
			{
				//DocumentEntitiy ent = _documentEntities.FirstOrDefault(en => en.InRange(MarkdownText.CaretOffset));
				//if (ent != null)
				//{
				//    var textArea = MarkdownText.TextArea;
				//    var position = ent.Type == EntityType.Table ? new TextViewPosition(ent.StartIndex.Location) : textArea.Caret.Position;

				//    Point p = textArea.TextView.GetVisualPosition(position, VisualYPosition.LineTop) - textArea.TextView.ScrollOffset;

				//    CurrentEntity = ent;
				//    if (ent.Type == EntityType.Anchor || ent.Type == EntityType.SimpleAnchor)
				//    {
				//        toolbar = LinkToolbarPopup;
				//    }
				//    else if (ent.Type == EntityType.Table)
				//    {
				//        toolbar = TableToolbarPopup;
				//    }

				//    if (toolbar != null)
				//    {
				//        toolbar.HorizontalOffset = p.X + 10;
				//        toolbar.VerticalOffset = p.Y - 30;

				//        if (!toolbar.IsOpen)
				//            toolbar.IsOpen = true;
				//    }
				//}
			}

			if (toolbar == null || Equals(toolbar, LinkToolbarPopup))
				TableToolbarPopup.IsOpen = false;

			if (toolbar == null || Equals(toolbar, TableToolbarPopup))
				LinkToolbarPopup.IsOpen = false;
		}

		private void SaveTimerElapsed(object sender, ElapsedEventArgs e)
		{
			if (!SupressOnTextChanged)
			{
				SaveNoteText();
				UpdateEntities();
			}
		}

		public void SaveNoteText()
		{
			InvokeOnCurrentDispatcher(() =>
			{
				if (CurrentModel != null && CurrentModel.Note != null)
				{
					CurrentModel.Note.DecryptedText = MarkdownText.Text;
					CurrentModel.SaveNote();
				}
			});

		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs i)
		{
			try
			{
				if (i.OldValue != null && i.OldValue is NoteViewModel)
				{
					NoteViewModel oldModel = (NoteViewModel)i.OldValue;
					oldModel.Note.NoteRefreshedFromDisk -= Note_RefreshedFromDisk;
				}
				SupressOnTextChanged = true;

				if (TableToolbarPopup.IsOpen)
					TableToolbarPopup.IsOpen = false;

				if (CurrentModel == null)
					return;

				MarkdownText.Text = CurrentModel.Note.DecryptedText;
				CurrentModel.Note.NoteRefreshedFromDisk += Note_RefreshedFromDisk;

				UpdateEntities();

				ViewModelLocator.Instance.NoteView.CheckBoxCheckUpdatedTextCommand = new SimpleCommand(UpdateTextByCheckBox);
			}
			finally
			{
				SupressOnTextChanged = false;
			}
		}

		private void Note_RefreshedFromDisk()
		{
			InvokeOnCurrentDispatcher(() =>
							{
								MarkdownText.Text = CurrentModel.Note.DecryptedText;
							});

		}

		private void UpdateEntities()
		{
			if (!Hub.Instance.Settings.EnableAdvancedControls)
				return;

			InvokeOnCurrentDispatcher(() =>
			{
				if (_documentEntities == null)
					_documentEntities = new List<DocumentEntitiy>();

				_documentEntities.Clear();

				MatchEntities(RegularExpressions.AnchorInline, EntityType.Anchor);
				MatchEntities(RegularExpressions.SimpleAnchorInline, EntityType.SimpleAnchor);
				MatchEntities(RegularExpressions.Table, EntityType.Table);
				MatchEntities(RegularExpressions.TableNoAlignRow, EntityType.Table);
			});
		}

		private void MatchEntities(Regex reg, EntityType type)
		{
			var matches = reg.Matches(MarkdownText.Text);
			foreach (Match match in matches)
			{
				if (match.Success)
				{
					////TextAnchor anchorStart = MarkdownText.Document.CreateAnchor(match.Index);
					////TextAnchor anchorEnd = MarkdownText.Document.CreateAnchor(match.Index + match.Length);

					//DocumentEntitiy ent = new DocumentEntitiy(anchorStart, anchorEnd) { Type = type };
					//ent.Deleted += DocumentEntityDeleted;

					//_documentEntities.Add(ent);
				}
			}
		}

		private void DocumentEntityDeleted(DocumentEntitiy ent)
		{
			UpdateEntities();
		}

		private void UpdateTextByCheckBox(object obj)
		{
			string newText = (string)obj;
			if (newText != null)
			{
				SupressOnTextChanged = true;
				MarkdownText.Text = newText;
				SupressOnTextChanged = false;
			}
		}

		private void ToolbarTagButtonClick(object sender, RoutedEventArgs e)
		{
			Button button = (Button)e.Source;
			string command = button.CommandParameter.ToString();
			string value = string.Empty;
			switch (command)
			{
				case "Image":
					value = "![Alt text](Image url)";
					break;
				case "InsertDate":
					value = DateTime.Now.ToShortDateString();
					break;
				case "InsertComment":
					value = "/*  COMMENT  */";
					break;
			}

			if (MarkdownText.SelectionStart >= 0 && MarkdownText.SelectionStart <= MarkdownText.Text.Length)
			{
				int selectionStart = MarkdownText.SelectionStart;
				MarkdownText.Text = MarkdownText.Text.Insert(MarkdownText.SelectionStart, value);

				MarkdownText.SelectionStart = selectionStart + value.Length;
			}
		}

		private void ToolbarButtonClick(object sender, RoutedEventArgs e)
		{
			Button button = (Button)e.Source;
			string command = button.CommandParameter.ToString();

			string startFormat = string.Empty, endFormat = string.Empty;

			switch (command)
			{
				case "Bold":
					startFormat = "**";
					endFormat = "**";
					break;
				case "Italic":
					startFormat = "*";
					endFormat = "*";
					break;
				case "Strike":
					startFormat = "~~";
					endFormat = "~~";
					break;
				case "Code":
					startFormat = "```" + Environment.NewLine;
					endFormat = Environment.NewLine + "```";
					break;
				case "H1":
					startFormat = "#";
					endFormat = Environment.NewLine;
					break;
				case "H2":
					startFormat = "##";
					endFormat = Environment.NewLine;
					break;
				case "H3":
					startFormat = "###";
					endFormat = Environment.NewLine;
					break;
			}

			if (string.IsNullOrWhiteSpace(startFormat))
				return;

			int oldSelectionStart = MarkdownText.SelectionStart;
			MarkdownText.Text = ToggleFormatting(MarkdownText.Text, startFormat, endFormat, MarkdownText.SelectionStart, MarkdownText.SelectionLength);

			if (MarkdownText.Text.Length >= oldSelectionStart)
				MarkdownText.SelectionStart = oldSelectionStart;
			//SurroundSelection(format, defaultText);
		}

		private void SurroundSelection(string format, string defaultValue)
		{
			if (MarkdownText.SelectionStart > -1 && MarkdownText.SelectionLength > 0)
			{
				MarkdownText.SelectedText = string.Format(format, MarkdownText.SelectedText);
			}
			else
			{
				MarkdownText.SelectedText = defaultValue;
				// Todo: change selection
			}
		}

		private string ToggleFormatting(string text, string startFormat, string endFormat, int selectionStart, int selectionLength)
		{
			string expression =
				$@"{Regex.Escape(startFormat)}
					(?:      # Start of non-capturing group that matches...
					 (?!{Regex.Escape(startFormat)}) # (if startFormat can't be matched here)
					 .       # any character
					) *?      # Repeat any number of times, as few as possible
					{Regex.Escape(endFormat)}";
			try
			{
				Regex reg = new Regex(expression, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);

				MatchCollection matches = reg.Matches(text);
				if (matches.Count > 0)
				{
					foreach (Match m in matches)
					{
						if (selectionStart >= m.Index && (selectionStart + selectionLength) <= (m.Index + m.Length))
						{
							if (m.Value.StartsWith(startFormat) && m.Value.EndsWith(endFormat))
							{
								text = text.Remove(m.Index, m.Length);
								text = text.Insert(m.Index, m.Value.Replace(startFormat, string.Empty).Replace(endFormat, string.Empty).TrimStart());
								return text;
							}
						}
					}
				}

				if (selectionLength == 0)
					return text;

				string selection = text.Substring(selectionStart, selectionLength);
				text = text.Remove(selectionStart, selectionLength);
				text = text.Insert(selectionStart, startFormat + selection + endFormat);
				return text;
			}
			catch (Exception e)
			{
				_log.Error("Exception when formatting with " + startFormat, e);
			}

			return text;
		}

		private void ShowHelp()
		{
			Popup.IsOpen = true;
		}

		private void MarkdownText_OnTextChanged(object sender, EventArgs e)
		{
			if (SupressOnTextChanged)
				return;

			_saveTimer.Stop();
			_saveTimer.Start();
		}

		private void ToggleWordWrapButton_OnClick(object sender, RoutedEventArgs e)
		{
			TextWrapping wrap = ToggleWordWrapButton.IsChecked.GetValueOrDefault() ? TextWrapping.Wrap : TextWrapping.NoWrap;
			MarkdownText.TextWrapping = wrap;
		}

		private void EditTableClick(object sender, RoutedEventArgs e)
		{
			if (CurrentEntity.Type == EntityType.Table)
			{
				TableToolbarPopup.IsOpen = false;

				string text = MarkdownText.Text.Substring(CurrentEntity.StartIndex.Offset, CurrentEntity.EndIndex.Offset - CurrentEntity.StartIndex.Offset);
				text = text.Trim();

				TableEditor editor = new TableEditor(text, CurrentEntity);
				editor.Owner = Application.Current.MainWindow;
				editor.OnTableSave += (table, entitiy) =>
				{
					int index = CurrentEntity.StartIndex.Offset;
					string newText = MarkdownText.Text.Remove(index, CurrentEntity.EndIndex.Offset - CurrentEntity.StartIndex.Offset);
					newText = newText.Insert(index, table);

					MarkdownText.Text = newText;
				};

				editor.OnTableSaveError += (entitiy, exception) =>
				{
					MessageBox.Show(exception.Message, "Error saving table", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				};

				editor.ShowDialog();
			}
		}

		private void EditLinkClick(object sender, RoutedEventArgs e)
		{
			LinkToolbarPopup.IsOpen = false;
			LinkEditor.IsOpen = true;
		}

		private void SaveLink(object sender, RoutedEventArgs e)
		{
			LinkEditor.IsOpen = false;
		}

		private void CloseLinkEditor(object sender, RoutedEventArgs e)
		{
			LinkEditor.IsOpen = false;
		}

		private void MarkdownText_OnLostFocus(object sender, RoutedEventArgs e)
		{
			if (LinkToolbarPopup.IsOpen)
				LinkToolbarPopup.IsOpen = false;
			if (TableToolbarPopup.IsOpen)
				TableToolbarPopup.IsOpen = false;
		}

		private void FileList_Drop(object sender, DragEventArgs e)
		{
			if (e.Data.GetFormats().Length > 0)
			{

			}
		}

		private void FileList_PreviewDrop(object sender, DragEventArgs e)
		{
			if (e.Data.GetFormats().Length > 0)
			{


			}
		}

		private void FileList_PreviewDragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetFormats().Any())
			{

			}
		}

		private void ShowHelpBubble(object sender, RoutedEventArgs e)
		{
			Popup.IsOpen = true;
		}

		private void MarkdownText_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				int lineIndex = MarkdownText.GetLineIndexFromCharacterIndex(MarkdownText.CaretIndex);
				if (lineIndex < 0)
					return;

				string line = MarkdownText.GetLineText(lineIndex);
				string lineTextTrim = line.Trim();

				if (lineTextTrim.Equals("- [ ]") || lineTextTrim.Equals("-"))
				{
					RemoveLine(lineIndex);
					return;
				}

				string lineTextTrimStart = line.TrimStart();
				int leadingWhitespaces = line.Length - lineTextTrimStart.Length;
				string whiteSpaces = string.Empty;
				if (leadingWhitespaces > 0)
					whiteSpaces = new string(' ', leadingWhitespaces);

				string stringToInsert = null;
				if (lineTextTrimStart.StartsWith("- ["))
				{

					stringToInsert = "- [ ] ";
				}
				else if(lineTextTrimStart.StartsWith("- "))
				{
					stringToInsert = "- ";
				}

				if(stringToInsert != null)
				{
					InsertAt("\n" + whiteSpaces + stringToInsert, MarkdownText.CaretIndex);
					e.Handled = true;
				}
			}
		}

		private void RemoveLine(int lineIndex)
		{
			List<string> lines = new List<string>(MarkdownText.Text.Split('\n'));
			lines.RemoveAt(lineIndex);

			int carretPosition = MarkdownText.GetCharacterIndexFromLineIndex(lineIndex);

			MarkdownText.Text = string.Join("\n", lines.ToArray());
			MarkdownText.CaretIndex = carretPosition;
		}

		private void InsertAt(string thisString, int at)
		{
			int newCaretIndex = MarkdownText.CaretIndex + thisString.Length;
			MarkdownText.Text = MarkdownText.Text.Insert(MarkdownText.CaretIndex, thisString);
			if (MarkdownText.Text.Length >= newCaretIndex)
				MarkdownText.CaretIndex = newCaretIndex;
		}
	}
}
