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
		private Markdown _markdown;
		private TextToFlowDocumentConverter _markdownToFlowDocumentConverter;
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
		}

		private void NoteViewDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (CurrentModel == null)
				return;

			SelectViewMode();

			CurrentModel.EditNoteCommand = new BasicCommand(SwitchToEditPanel);
			CurrentModel.SaveNoteCommand = new BasicCommand(SaveNote);
		}

		void NoteViewPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "SearchText")
			{
				if (_markdown.Highlights.Count > 0)
				{
					_markdown.Highlights[0].BringIntoView();
				}
			}
		}

		private bool SwitchToEditPanel(object arg)
		{
			ViewTabItem.IsSelected = false;
			EditTabItem.IsSelected = true;
			TabControl.SelectedIndex = 0;
			TabControl.SelectedItem = EditTabItem;
			return true;
		}

		private bool SaveNote(object arg)
		{
			CurrentModel?.SaveNote();
			SelectViewMode();
			return true;
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
			if (_markdown == null)
				return;

			FlowDocument doc = _markdown.Transform(ContextNote.DecryptedText);

			Stream s = new MemoryStream();
			XamlWriter.Save(doc, s);

			s.Seek(0, SeekOrigin.Begin);

			FileStream fs = File.Create("C:\\Temp\\doc.xaml");
			s.CopyTo(fs);
			fs.Close();
			s.Close();
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
			_markdown = Resources["Markdown"] as Markdown;
			_markdownToFlowDocumentConverter = Resources["TextToFlowDocumentConverter"] as TextToFlowDocumentConverter;
			if (_markdown != null)
				_markdown.CheckBoxCheckedCommand = new BasicCommand(DocumentCheckBoxChecked);
		}

		private bool DocumentCheckBoxChecked(object arg)
		{

			_markdownToFlowDocumentConverter.Pause = true;
			CurrentModel.DocumentCheckBoxCheckedCommand?.Execute(arg);
			_markdownToFlowDocumentConverter.Pause = false;

			return true;
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
				ViewModelLocator.Instance.Main.ToggleSearchFlyoutCommand.Execute(e);
			}
		}

		private void FlowDocumentPageViewer_OnKeyDown(object sender, KeyEventArgs e)
		{
			if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.SystemKey == Key.LeftCtrl && e.Key == Key.F)
			{
				e.Handled = true;
				ViewModelLocator.Instance.Main.ToggleSearchFlyoutCommand.Execute(e);
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
