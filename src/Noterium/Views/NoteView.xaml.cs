using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using GalaSoft.MvvmLight.CommandWpf;
using MahApps.Metro;
using Noterium.Code.Commands;
using Noterium.Code.Markdown;
using Noterium.Core;
using Noterium.Core.DataCarriers;
using Noterium.Properties;
using Noterium.ViewModels;

namespace Noterium.Views
{
	/// <summary>
	/// Interaction logic for NoteView.xaml
	/// </summary>
	public partial class NoteView : INotifyPropertyChanged
	{
		public static readonly DependencyProperty EditNoteProperty = DependencyProperty.Register("EditNote", typeof(ICommand), typeof(NoteView), new UIPropertyMetadata(null));

		public ICommand EditNote
		{
			get { return (ICommand)GetValue(EditNoteProperty); }
			set { SetValue(EditNoteProperty, value); }
		}

		private string _searchText;
		private TextToFlowDocumentConverter _markdownToFlowDocumentConverter;
		private XamlFormatter _xamlFormatter;
		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public string SearchText
		{
			get { return _searchText; }
			set { _searchText = value; OnPropertyChanged(); }
		}

		public Note ContextNote => CurrentModel.Note;

		public NoteViewModel CurrentModel => ((NoteViewModel)DataContext);

		public NoteView()
		{
			InitializeComponent();

			PropertyChanged += NoteViewPropertyChanged;

			DataContextChanged += NoteViewDataContextChanged;

			DataObject.AddCopyingHandler(FlowDocumentPageViewer, OnTextCopy);
		}

		private void OnTextCopy(object sender, DataObjectCopyingEventArgs e)
		{
			e.CancelCommand();
			if (FlowDocumentPageViewer.Selection != null)
			{
				using (var ms = new MemoryStream())
				{
					TextPointer potStart = FlowDocumentPageViewer.Selection.Start;
					TextPointer potEnd = FlowDocumentPageViewer.Selection.End;
					TextRange range = new TextRange(potStart, potEnd);
					range.Save(ms, DataFormats.Xaml, true);

					ms.Position = 0;
					var sr = new StreamReader(ms);
					var xamlString = sr.ReadToEnd();

					var dto = new DataObject();
					dto.SetText(ConvertXamlToRtf(xamlString), TextDataFormat.Rtf);
					string unformattedText = range.Text.Replace("\n", Environment.NewLine);
					dto.SetText(unformattedText, TextDataFormat.UnicodeText);

					Clipboard.Clear();
					Clipboard.SetDataObject(dto);
				}
			}
		}

		private static string ConvertXamlToRtf(string xamlText)
		{
			var richTextBox = new RichTextBox();
			if (string.IsNullOrEmpty(xamlText)) return "";
			var textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
			using (var xamlMemoryStream = new MemoryStream())
			{
				using (var xamlStreamWriter = new StreamWriter(xamlMemoryStream))
				{
					xamlStreamWriter.Write(xamlText);
					xamlStreamWriter.Flush();
					xamlMemoryStream.Seek(0, SeekOrigin.Begin);
					textRange.Load(xamlMemoryStream, DataFormats.Xaml);
				}
			}
			using (var rtfMemoryStream = new MemoryStream())
			{
				textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
				textRange.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);

				textRange.Save(rtfMemoryStream, DataFormats.Rtf);
				rtfMemoryStream.Seek(0, SeekOrigin.Begin);
				using (var rtfStreamReader = new StreamReader(rtfMemoryStream))
				{
					return rtfStreamReader.ReadToEnd();
				}
			}
		}

		private void NoteViewDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (CurrentModel == null)
				return;

			SelectViewMode();

			CurrentModel.EditNoteCommand = new RelayCommand(SwitchToEditPanel);
			CurrentModel.SaveNoteCommand = new RelayCommand(SaveNote);
		}

		void NoteViewPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "SearchText")
			{
				//if (_markdown.Highlights.Count > 0)
				//{
				//	_markdown.Highlights[0].BringIntoView();
				//}
			}
		}

		private void SwitchToEditPanel()
		{
			ViewModeButtonClicked(EditModeButton, null);
		}

		private void SaveNote()
		{
			CurrentModel?.SaveNote();
			SelectViewMode();
		}

		private void SelectViewMode()
		{
			if (string.IsNullOrEmpty(CurrentModel.Note.DecryptedText))
			{
				if (Hub.Instance.Settings.DefaultNoteView == "Split")
					ViewModeButtonClicked(SplitModeButton, null);
				else
					ViewModeButtonClicked(EditModeButton, null);
				return;
			}

			if (Hub.Instance.Settings.DefaultNoteView == "View")
				ViewModeButtonClicked(ViewModeButton, null);
			else if (Hub.Instance.Settings.DefaultNoteView == "Split")
				ViewModeButtonClicked(SplitModeButton, null);
			else if (Hub.Instance.Settings.DefaultNoteView == "Edit")
				ViewModeButtonClicked(EditModeButton, null);
		}

		private void GetXamlButton_OnClick(object sender, RoutedEventArgs e)
		{
			if (_markdownToFlowDocumentConverter == null)
				return;

			FlowDocument doc = _markdownToFlowDocumentConverter.GetNewDocument();

			//Stream s = new MemoryStream();
			string xaml = XamlWriter.Save(doc);
			Clipboard.SetText(xaml);
			//s.Seek(0, SeekOrigin.Begin);

			//FileStream fs = File.Create("C:\\Temp\\doc.xaml");
			//s.CopyTo(fs);
			//fs.Close();
			//s.Close();
		}

		private void GoToPage_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			string url = e.Parameter as string;

			Uri uri;
			if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri))
			{
                // If the link is not an url, it is probably a path to a note-file, so find it
				var file = ContextNote.Files.FirstOrDefault(nf => nf.FileName.Equals(url));
				if (file != null)
					url = file.FullName;
				else
					return;
			}

			ProcessStartInfo sInfo = new ProcessStartInfo(url);
			Process.Start(sInfo);
		}

		private void ShowNoteFile_OnClick(object sender, RoutedEventArgs e)
		{
			string path = Hub.Instance.Storage.DataStore.DataFolder;
			string args = path + "\\" + ContextNote.Notebook + "\\" + ContextNote.ID + "." + Core.Constants.File.NoteFileExtension;
			Process.Start(args);
		}

		private void ShowNoteFolder_OnClick(object sender, RoutedEventArgs e)
		{
			string path = Hub.Instance.Storage.DataStore.DataFolder;
			string args = "/select,\"" + path + "\\" + ContextNote.Notebook + "\"";
			Process.Start("explorer.exe", args);
		}

		private void NoteView_OnLoaded(object sender, RoutedEventArgs e)
		{
			_xamlFormatter = Resources["XamlFormatter"] as XamlFormatter;
			_markdownToFlowDocumentConverter = Resources["TextToFlowDocumentConverter"] as TextToFlowDocumentConverter;
			if (_xamlFormatter != null)
				_xamlFormatter.CheckBoxCheckedCommand = new SimpleCommand(DocumentCheckBoxChecked);
		}

		private void DocumentCheckBoxChecked(object arg)
		{
			_markdownToFlowDocumentConverter.Pause = true;
			CurrentModel.DocumentCheckBoxCheckedCommand?.Execute(arg);
			_markdownToFlowDocumentConverter.Pause = false;
		}

		private void ViewModeButtonClicked(object sender, RoutedEventArgs e)
		{
			if (e != null)
				CurrentModel?.SaveNote();

			Button btn = (Button)sender;
			SplitColumn.Visible = false;

			EditModeButton.BorderThickness = new Thickness(0);
			SplitModeButton.BorderThickness = new Thickness(0);
			ViewModeButton.BorderThickness = new Thickness(0);

			EditModeButton.Tag = "";
			SplitModeButton.Tag = "";
			ViewModeButton.Tag = "";

			btn.Tag = "Selected";
			btn.BorderThickness = new Thickness(0, 0, 0, 2);

			if (Equals(btn, EditModeButton))
			{
				EditColumn.Visible = true;
				ViewColumn.Visible = false;

			}
			else if (Equals(btn, SplitModeButton))
			{
				EditColumn.Visible = true;
				SplitColumn.Visible = true;
				ViewColumn.Visible = true;
			}
			else if (Equals(btn, ViewModeButton))
			{
				EditColumn.Visible = false;
				ViewColumn.Visible = true;
			}
		}

		private void FlowDocumentPageViewer_OnPreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.F)
			{
				e.Handled = true;
				CurrentModel.MainWindowInstance.Model.ToggleSearchFlyoutCommand.Execute(e);
			}
		}

		private void FlowDocumentPageViewer_OnKeyDown(object sender, KeyEventArgs e)
		{
			if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.SystemKey == Key.LeftCtrl && e.Key == Key.F)
			{
				e.Handled = true;
				CurrentModel.MainWindowInstance.Model.ToggleSearchFlyoutCommand.Execute(e);
			}
		}

		private void PrintButton_OnClick(object sender, RoutedEventArgs e)
		{
			DoThePrint(_markdownToFlowDocumentConverter.GetNewDocument());
		}

		private void DoThePrint(FlowDocument copy)
		{
			// Clone the source document's content into a new FlowDocument.
			// This is because the pagination for the printer needs to be
			// done differently than the pagination for the displayed page.
			// We print the copy, rather that the original FlowDocument.
			//System.IO.MemoryStream s = new System.IO.MemoryStream();
			//TextRange source = new TextRange(document.ContentStart, document.ContentEnd);
			//source.Save(s, DataFormats.Xaml);
			//FlowDocument copy = new FlowDocument();
			//TextRange dest = new TextRange(copy.ContentStart, copy.ContentEnd);
			//dest.Load(s, DataFormats.Xaml);

			// Create a XpsDocumentWriter object, implicitly opening a Windows common print dialog,
			// and allowing the user to select a printer.

			var theme = ThemeManager.GetAppTheme(Hub.Instance.Settings.Theme);
			var accent = ThemeManager.GetAccent("VSLight");

			ThemeManager.ChangeAppStyle(copy.Resources, accent, theme);

			// get information about the dimensions of the seleted printer+media.
			System.Printing.PrintDocumentImageableArea ia = null;
			System.Windows.Xps.XpsDocumentWriter docWriter = System.Printing.PrintQueue.CreateXpsDocumentWriter(ref ia);

			if (docWriter != null && ia != null)
			{
				DocumentPaginator paginator = ((IDocumentPaginatorSource)copy).DocumentPaginator;

				// Change the PageSize and PagePadding for the document to match the CanvasSize for the printer device.
				paginator.PageSize = new Size(ia.MediaSizeWidth, ia.MediaSizeHeight);
				Thickness t = new Thickness(72);  // copy.PagePadding;
				copy.PagePadding = new Thickness(
								 Math.Max(ia.OriginWidth, t.Left),
								   Math.Max(ia.OriginHeight, t.Top),
								   Math.Max(ia.MediaSizeWidth - (ia.OriginWidth + ia.ExtentWidth), t.Right),
								   Math.Max(ia.MediaSizeHeight - (ia.OriginHeight + ia.ExtentHeight), t.Bottom));

				copy.ColumnWidth = double.PositiveInfinity;
				//copy.PageWidth = 528; // allow the page to be the natural with of the output device

				// Send content to the printer.
				docWriter.Write(paginator);
			}

		}
	}
}
