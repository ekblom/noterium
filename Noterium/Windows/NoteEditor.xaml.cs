using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using MimeTypes;
using Noteray.Code.Commands;
using Noteray.Code.Data;
using Noteray.Code.Markdown;
using Noteray.Core;
using Noteray.Core.DataCarriers;
using Noteray.Core.Exceptions;
using Noteray.Core.Helpers;
using Noteray.ViewModels;

namespace Noteray.Windows
{
	/// <summary>
	/// Interaction logic for NoteEditor.xaml
	/// </summary>
	public partial class NoteEditor
	{
		private const string MdImageFormat = "![{1}]({0})";
		private const string ImageFormat = "<img src=\"{0}\" alt=\"{1}\" />";

		private Note _editedNote;
		private readonly List<NoteFile> _newFiles = new List<NoteFile>();
		private readonly DispatcherTimer _messageClearTimer;
		private bool _textChanged;

		public NoteFileCommand InsertAsImgCommand { get; set; }
		public NoteFileCommand InsertAsMdCommand { get; set; }
		public DocumentEntitiy CurrentEntity { get; set; }

		private readonly List<DocumentEntitiy> _documentEntitiys;

		private Note EditedNote => _editedNote ?? (_editedNote = ((NoteEditorViewModel)DataContext).Note);

		public NoteEditorViewModel Model => ((NoteEditorViewModel)DataContext);

		public NoteEditor()
		{
			InitializeComponent();

			//CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, SaveNote));
			CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, CloseEditor));

			InsertAsImgCommand = new NoteFileCommand(InsertNoteFileAsImg);
			InsertAsMdCommand = new NoteFileCommand(InsertNoteFileAsMd);

			_messageClearTimer = new DispatcherTimer();
			_messageClearTimer.Tick += ClearMessage;
			_messageClearTimer.Interval = TimeSpan.FromMilliseconds(2500);

			_documentEntitiys = new List<DocumentEntitiy>();
			var documentEntitiesTimer = new DispatcherTimer();
			documentEntitiesTimer.Tick += UpdateDocumentEntities;
			documentEntitiesTimer.Interval = TimeSpan.FromMilliseconds(1000);
			documentEntitiesTimer.Start();

			Closing += NoteEditorClosing;

			MarkdownText.Focus();
		}

		private void UpdateDocumentEntities(object sender, EventArgs e)
		{
			if (!_textChanged)
				return;

			_textChanged = false;
			_documentEntitiys.Clear();

			MatchEntities(RegularExpressions.ImageAnchorInline, EntityType.Image);
			//MatchEntities(RegularExpressions.ListTopLevel, EntityType.List);
			//MatchEntities(RegularExpressions.ListNested, EntityType.List);
			MatchEntities(RegularExpressions.SimpleAnchorInline, EntityType.SimpleAnchor);
			MatchEntities(RegularExpressions.AnchorInline, EntityType.Anchor);
			MatchEntities(RegularExpressions.Table, EntityType.Table);
			MatchEntities(RegularExpressions.TableNoDelimiter, EntityType.Table);
		}

		private void MatchEntities(Regex reg, EntityType type)
		{
			var matches = reg.Matches(MarkdownText.Text);
			foreach (Match match in matches)
			{
				if (match.Success)
				{
					//DocumentEntitiy ent = new DocumentEntitiy();
					//ent.Match = match;
					//ent.Type = type;
					//_documentEntitiys.Add(ent);
				}
			}
		}

		void NoteEditorClosing(object sender, CancelEventArgs e)
		{

		}

		void ClearMessage(object sender, EventArgs e)
		{
			Dispatcher.Invoke(() =>
			{
				_messageClearTimer.Stop();
				MessageTextBlock.Text = string.Empty;
			});
		}

		private void ShowMessage(string message)
		{
			MessageTextBlock.Text = message;
			_messageClearTimer.Start();
		}

		private void FilesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Add)
			{
				_newFiles.AddRange(e.NewItems.Cast<NoteFile>());
			}
			else if (e.Action == NotifyCollectionChangedAction.Remove)
			{
				foreach (NoteFile noteFile in e.NewItems.Cast<NoteFile>())
				{
					int index = _newFiles.IndexOf(noteFile);
					if (index > -1)
						_newFiles.RemoveAt(index);
				}
			}
		}

		private bool InsertNoteFileAsImg(NoteFile arg)
		{
			InsertImage(arg, ImageFormat);
			return true;
		}

		private bool InsertNoteFileAsMd(NoteFile arg)
		{
			InsertImage(arg, MdImageFormat);
			return true;
		}

		private void DragNoteFile(object sender, MouseButtonEventArgs e)
		{
			Image img = (Image)sender;
			NoteFile nf = img.Tag as NoteFile;
			if (nf == null)
				return;

			DataObject data = new DataObject(typeof(NoteFile), nf);
			DragDrop.DoDragDrop(img, data, DragDropEffects.Copy);
		}

		private void InsertImage(NoteFile nf, string format)
		{
			if (nf != null)
			{
				string content = string.Format(format, nf.FileName, nf.Name);
				MarkdownText.SelectedText = content;
			}
		}

		private void CloseEditor(object sender, ExecutedRoutedEventArgs e)
		{
			if (_newFiles.Any())
			{

			}

			Close();
		}

		private void SaveNote()
		{
			EditedNote.SecureText = MarkdownText.Text;

			//TODO: Implement http://stackoverflow.com/questions/50744/wait-until-file-is-unlocked-in-net, and show progress window explaining why, dropbox has locked the file for sync

			try
			{
				EditedNote.Save();
				Close();
			}
			catch (SaveException saveNoteException)
			{
				Note n = (Note)saveNoteException.UnsavedObject;
				MessageBox.Show("Unable to save " + n.Name + "\n\nCould not get access to file. \nDropBox has probably locked it when syncing, please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Unable to save note.\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void MetroWindowLoaded(object sender, RoutedEventArgs e)
		{
			MarkdownText.Text = Model.Text;
			EditedNote.Files.CollectionChanged += FilesCollectionChanged;
		}

		private void ToolbarButtonClick(object sender, RoutedEventArgs e)
		{
			Button button = (Button)e.Source;
			string command = button.CommandParameter.ToString();

			MarkdownText.Text = ToggleFormatting(MarkdownText.Text, "**", "**", MarkdownText.SelectionStart, MarkdownText.SelectionLength);
			return;

			//string format = string.Empty, defaultText = string.Empty;
			//switch (command)
			//{
			//	case "Bold":
			//		format = "**{0}**";
			//		defaultText = string.Format(format, "Bold");
					
			//		return;
			//		break;
			//	case "Italic":
			//		format = "*{0}*";
			//		defaultText = string.Format(format, "Italic");
			//		break;
			//	case "Strike":
			//		format = "~~{0}~~";
			//		defaultText = string.Format(format, "Strikethrough");
			//		break;
			//	case "Code":
			//		format = "```" + Environment.NewLine + "{0}" + Environment.NewLine + "```";
			//		defaultText = string.Format(format, "function(){ alert('Bananas!'); }");
			//		break;
			//	case "Image":
			//		defaultText = string.Format(MdImageFormat, "Image url", string.Empty);
			//		break;
			//	case "H1":
			//		format = "{0}" + Environment.NewLine + "==";
			//		defaultText = string.Format(format, "Heading");
			//		break;
			//	case "H2":
			//		format = "{0}" + Environment.NewLine + "--";
			//		defaultText = string.Format(format, "Heading");
			//		break;
			//	case "H3":
			//		format = "### {0}";
			//		defaultText = string.Format(format, "Heading");
			//		break;
			//}

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
				$@"{startFormat}
					(?:      # Start of non-capturing group that matches...
					 (? !{startFormat}) # (if startFormat can't be matched here)
					 .       # any character
					) *?      # Repeat any number of times, as few as possible
					{endFormat}";
			Regex reg = new Regex(expression);
			
			MatchCollection matches = reg.Matches(text);
			if (matches.Count > 0)
			{
				foreach (Match m in matches)
				{
					if (m.Index >= selectionStart && (m.Index + m.Length) <= (selectionStart + selectionLength))
					{
						if (m.Value.StartsWith(startFormat) && m.Value.EndsWith(endFormat))
						{
							text = text.Remove(selectionStart, selectionLength);
							text = text.Insert(selectionStart, m.Value.Replace(startFormat, string.Empty).Replace(endFormat, string.Empty));
							return text;
						}
					}
				}
			}
			
			string selection = text.Substring(selectionStart, selectionLength);
			text = text.Remove(selectionStart, selectionLength);
			text = text.Insert(selectionStart, startFormat + selection + endFormat);
			return text;
		}

		private void StripHtmlButtonClick(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(MarkdownText.SelectedText))
				MarkdownText.SelectAll();

			var textToStrip = MarkdownText.SelectedText;

			const string acceptable = "img|a";
			const string stringPattern = @"</?(?(?=" + acceptable + @")notag|[a-zA-Z0-9]+)(?:\s[a-zA-Z0-9\-]+=?(?:(["",']?).*?\1?)?)*\s*/?>";
			string result = Regex.Replace(textToStrip, stringPattern, string.Empty);

			MarkdownText.SelectedText = result;
		}

		private void ConvertToMarkdownButtonClick(object sender, RoutedEventArgs e)
		{
			MarkdownText.Text = ConvertToMd(MarkdownText.Text);
		}

		public string ConvertToMd(string source)
		{
			string processName = Hub.Instance.Settings.PandocPath;
			string args = Hub.Instance.Settings.PandocParameters;

			if (string.IsNullOrEmpty(processName) || string.IsNullOrEmpty(args))
			{
				MessageBox.Show("No settings for pandoc found.");
				return source;
			}

			ProcessStartInfo psi = new ProcessStartInfo(processName, args)
			{
				RedirectStandardOutput = true,
				RedirectStandardInput = true,
				UseShellExecute = false
			};

			Process p = new Process { StartInfo = psi };
			psi.UseShellExecute = false;
			p.Start();

			string outputString;
			byte[] inputBuffer = Encoding.UTF8.GetBytes(source);
			p.StandardInput.BaseStream.Write(inputBuffer, 0, inputBuffer.Length);
			p.StandardInput.Close();

			p.WaitForExit(2000);
			using (StreamReader sr = new StreamReader(p.StandardOutput.BaseStream))
			{
				outputString = sr.ReadToEnd();
			}

			return outputString;
		}

		private void MarkdownText_OnKeyDown(object sender, KeyEventArgs e)
		{
			if (Keyboard.Modifiers == ModifierKeys.Control)
			{
				if (e.Key == Key.B)
					ClickButton(BoldButton);
				else if (e.Key == Key.I)
					ClickButton(ItalicButton);
				else if (e.Key == Key.S)
					SaveNote();
			}

			#region -- wack --

			//switch (e.SystemKey)
			//{
			//	case (Key.Back | Key.LeftCtrl):
			//		#region -- wack --
			//		//e.SuppressKeyPress = true;
			//		//int i;
			//		//if (textbox.SelectionStart.Equals(0))
			//		//{
			//		//	return;
			//		//}
			//		//int space = textbox.Text.LastIndexOf(' ', textbox.SelectionStart - 1);
			//		//int line = textbox.Text.LastIndexOf("\r\n", textbox.SelectionStart - 1);
			//		//if (space > line)
			//		//{
			//		//	i = space;
			//		//}
			//		//else
			//		//{
			//		//	i = line;
			//		//}
			//		//if (i > -1)
			//		//{
			//		//	while (textbox.Text.Substring(i - 1, 1).Equals(' '))
			//		//	{
			//		//		if (i.Equals(0))
			//		//		{
			//		//			break;
			//		//		}
			//		//		i--;
			//		//	}
			//		//	textbox.Text = textbox.Text.Substring(0, i) + textbox.Text.Substring(textbox.SelectionStart);
			//		//	textbox.SelectionStart = i;
			//		//}
			//		//else if (i.Equals(-1))
			//		//{
			//		//	textbox.Text = textbox.Text.Substring(textbox.SelectionStart);
			//		//}
			//		#endregion
			//		SendKeys.SendWait("^+{LEFT}{BACKSPACE}");
			//		e.Handled = true;
			//		e.SuppressKeyPress = true;
			//		break;
			//	case (Keys.Control | Keys.A):
			//		textbox.SelectAll();
			//		e.Handled = true;
			//		e.SuppressKeyPress = true;
			//		break;
			//	case (Keys.Control | Keys.S):
			//		SaveButton_Click(sender, e);
			//		break;
			//	case (Keys.Escape):
			//		if (TableCreatorPanel.Visible)
			//		{
			//			TableCreatorPanel.Visible = false;
			//			e.Handled = true;
			//			e.SuppressKeyPress = true;
			//		}
			//		break;

			//	case (Keys.Control | Keys.B):
			//		Bold_Click(sender, e);
			//		e.Handled = true;
			//		e.SuppressKeyPress = true;
			//		break;
			//	case (Keys.Control | Keys.I):
			//		Italic_Click(sender, e);
			//		e.Handled = true;
			//		e.SuppressKeyPress = true;
			//		break;
			//	case (Keys.Control | Keys.H):
			//		HeadingButton_Click(sender, e);
			//		e.Handled = true;
			//		e.SuppressKeyPress = true;
			//		break;
			//	case (Keys.Control | Keys.T):
			//		ShowTableCreatorPanel(true);
			//		e.Handled = true;
			//		e.SuppressKeyPress = true;
			//		break;
			//}

			//if (PreviewInNoteEditor.Checked)
			//{
			//	if (OnTextUpdated != null)
			//		OnTextUpdated(MarkdownText.Text);
			//}

			#endregion
		}

		private void ClickButton(Button target)
		{
			ButtonAutomationPeer peer = new ButtonAutomationPeer(target);
			IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
			if (invokeProv != null)
				invokeProv.Invoke();
		}

		private void MarkdownText_OnDragOver(object sender, DragEventArgs e)
		{
			//if (!e.Data.GetDataPresent(DataFormats.FileDrop) && !e.Data.GetDataPresent(typeof(NoteFile)))
			//	return;

			//Point p = e.GetPosition(MarkdownText);
			//int charPos = MarkdownText.GetCharacterIndexFromPoint(p, true);

			//int line = MarkdownText.GetLineIndexFromCharacterIndex(charPos);

			//string lineText = MarkdownText.GetLineText(line);

			//bool emptyLine = string.IsNullOrWhiteSpace(lineText.Trim());
			//int lineMiddle = lineText.Length / 2;
			//int col = charPos - MarkdownText.GetCharacterIndexFromLineIndex(line);

			//if (!emptyLine && col > lineMiddle)
			//{
			//	Rect r = MarkdownText.GetRectFromCharacterIndex(charPos, false);
			//	Point winPoint = MarkdownText.PointToScreen(r.TopRight);
			//	if (winPoint.Y > p.Y)
			//		charPos++;
			//}

			//MarkdownText.SelectionStart = charPos;
			//MarkdownText.SelectionLength = 0;
			//MarkdownText.Focus();

			//e.Effects = DragDropEffects.Copy;
			//e.Handled = true;
		}

		private void MarkdownText_OnDragEnter(object sender, DragEventArgs e)
		{
			e.Effects = DragDropEffects.All;
		}

		private void MarkdownText_OnDragLeave(object sender, DragEventArgs e)
		{
			e.Effects = DragDropEffects.All;
		}

		private void MarkdownText_OnPreviewDrop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
				AddFiles(files);
			}
			else if (e.Data.GetDataPresent(typeof(NoteFile)))
			{
				NoteFile nf = e.Data.GetData(typeof(NoteFile)) as NoteFile;
				if (nf != null)
				{
					MarkdownText.SelectedText = string.Format(MdImageFormat, nf.FileName, nf.Name);
				}
			}
		}

		private void AddFiles(string[] files)
		{
			StringBuilder builder = new StringBuilder();
			foreach (string file in files)
			{
				FileInfo fi = new FileInfo(file);
				if (!fi.Exists)
					continue;

				byte[] bytes = File.ReadAllBytes(file);
				string mime = MimeTypeMap.GetMimeType(fi.Extension);
				NoteFile nf = NoteFile.Create(null, mime, bytes, EditedNote);
				builder.AppendFormat(MdImageFormat, nf.FileName, nf.Name);
				_newFiles.Add(nf);
			}

			MarkdownText.SelectedText = builder.ToString();
		}

		private void NoteEditor_OnClosing(object sender, CancelEventArgs e)
		{
			EditedNote.Files.CollectionChanged -= FilesCollectionChanged;
		}

		private void AddImageAsFileToNote_OnClick(object sender, RoutedEventArgs e)
		{
			string mime = null;
			byte[] bytes = null;
			if (MarkdownText.SelectedText.StartsWith("data:image"))
			{
				string[] chunks = MarkdownText.SelectedText.Split(',');
				if (chunks.Length == 2)
				{
					bytes = Convert.FromBase64String(chunks[1]);
					mime = chunks[0].Replace("data:", string.Empty).Replace(";base64", string.Empty);
				}
			}
			else
			{
				Uri uri;
				if (Uri.TryCreate(MarkdownText.SelectedText, UriKind.Absolute, out uri))
				{
					try
					{
						WebClient webClient = new WebClient();
						bytes = webClient.DownloadData(uri);
						mime = MimeType.GetMimeType(bytes);
					}
					catch (Exception)
					{
						// ignored
					}
				}
			}

			if (bytes != null && bytes.Length > 0 && mime != null)
			{
				NoteFile nf = NoteFile.Create(null, mime, bytes, EditedNote);
				MarkdownText.SelectedText = nf.Name;

				_newFiles.Add(nf);
			}
			else
			{
				ShowMessage("Unable to load image.");
			}
		}


		void MarkdownText_OnFilePasted(FileInfo fi)
		{
			byte[] bytes = File.ReadAllBytes(fi.FullName);
			string mime = MimeType.GetMimeType(bytes);
			NoteFile nf = NoteFile.Create(fi.Name, mime, bytes, EditedNote, fi.Extension);

			_newFiles.Add(nf);

			if (MimeType.IsImageExtension(fi.Extension))
			{
				InsertImage(nf, MdImageFormat);
			}
			else
			{
				MarkdownText.SelectedText = string.Format("[{0}](file:///{1})", nf.Name, nf.FullName);
			}
		}

		void MarkdownText_OnHyperLinkPasted(Uri uri)
		{
			MarkdownText.SelectedText = string.Format("<{0}>", uri.OriginalString);
		}

		void MarkdownText_OnImagePasted(System.Drawing.Image image, string fileName)
		{
			if (image == null)
				return;

			using (var ms = new MemoryStream())
			{
				image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
				byte[] bytes = ms.ToArray();
				string mime = MimeType.GetMimeType(bytes);
				if (bytes.Length > 0 && mime != null)
				{
					NoteFile nf = NoteFile.Create(fileName, mime, bytes, EditedNote);
					_newFiles.Add(nf);

					InsertImage(nf, MdImageFormat);
				}
			}
		}

		private void MarkdownText_OnTextChanged(object sender, TextChangedEventArgs e)
		{
			_textChanged = true;
		}

		private void MarkdownText_OnSelectionChanged(object sender, RoutedEventArgs e)
		{
			HideAllPopups();
			EditTableButton.IsEnabled = false;

			if (_documentEntitiys.Any())
			{
				DocumentEntitiy ent = _documentEntitiys.FirstOrDefault(en => en.InRange(MarkdownText.CaretOffset));
				if (ent != null)
				{
					CurrentEntity = ent;
					if (ent.Type == EntityType.Anchor)
					{
						if (LinkToolbarPopup != null)
						{
							PositionPopup(LinkToolbarPopup);
							LinkToolbarPopup.IsOpen = true;
						}
					}
					else if (ent.Type == EntityType.Table)
					{
						EditTableButton.IsEnabled = true;
					}
				}
			}
		}

		private void PositionPopup(Popup pop)
		{
			//Rect r = MarkdownText.GetRectFromCharacterIndex(MarkdownText.CaretIndex);
			//pop.PlacementRectangle = r;
		}

		private void HideAllPopups()
		{
			LinkToolbarPopup.IsOpen = false;
		}

		private void EditTableButton_OnClick(object sender, RoutedEventArgs e)
		{
			if (CurrentEntity != null && CurrentEntity.Type == EntityType.Table)
			{
				//string headString = CurrentEntity.Match.Groups[1].Value;
				//string textAlign = CurrentEntity.Match.Groups[2].Value;
				//string rowsString = CurrentEntity.Match.Groups[3].Value;
			}
		}
	}
}
