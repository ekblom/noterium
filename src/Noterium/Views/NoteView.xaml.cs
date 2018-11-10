using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Printing;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro;
using Noterium.Code.Commands;
using Noterium.Code.Markdown;
using Noterium.Code.Messages;
using Noterium.Core;
using Noterium.Core.Constants;
using Noterium.Properties;
using Noterium.ViewModels;
using File = System.IO.File;

namespace Noterium.Views
{
    /// <summary>
    ///     Interaction logic for NoteView.xaml
    /// </summary>
    public partial class NoteView : INotifyPropertyChanged
    {
        public static readonly DependencyProperty EditNoteProperty = DependencyProperty.Register("EditNote", typeof(ICommand), typeof(NoteView), new UIPropertyMetadata(null));
        private TextToFlowDocumentConverter _markdownToFlowDocumentConverter;

        private string _searchText;

        public NoteView()
        {
            InitializeComponent();

            PropertyChanged += NoteViewPropertyChanged;

            DataContextChanged += NoteViewDataContextChanged;

            DataObject.AddCopyingHandler(FlowDocumentPageViewer, OnTextCopy);
            Messenger.Default.Register<ChangeViewMode>(this, SelectViewMode);
            Messenger.Default.Register<SelectedNoteChanged>(this, OnSelectedNoteChanged);
        }

        public ICommand EditNote
        {
            get => (ICommand) GetValue(EditNoteProperty);
            set => SetValue(EditNoteProperty, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }

        public NoteViewerViewModel Model => (NoteViewerViewModel) DataContext;
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnSelectedNoteChanged(SelectedNoteChanged obj)
        {
            SelectViewMode(NoteViewModes.Default);
        }

        private void OnTextCopy(object sender, DataObjectCopyingEventArgs e)
        {
            e.CancelCommand();
            if (FlowDocumentPageViewer.Selection != null)
                using (var ms = new MemoryStream())
                {
                    var potStart = FlowDocumentPageViewer.Selection.Start;
                    var potEnd = FlowDocumentPageViewer.Selection.End;
                    var range = new TextRange(potStart, potEnd);
                    range.Save(ms, DataFormats.Xaml, true);

                    ms.Position = 0;
                    var sr = new StreamReader(ms);
                    var xamlString = sr.ReadToEnd();

                    var dto = new DataObject();
                    dto.SetText(ConvertXamlToRtf(xamlString), TextDataFormat.Rtf);
                    var unformattedText = range.Text.Replace("\n", Environment.NewLine);
                    dto.SetText(unformattedText, TextDataFormat.UnicodeText);

                    Clipboard.Clear();
                    Clipboard.SetDataObject(dto);
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
            if (Model == null)
                return;

            SelectViewMode(NoteViewModes.Default);

            Model.SaveNoteCommand = new RelayCommand(SaveNote);
        }

        private void NoteViewPropertyChanged(object sender, PropertyChangedEventArgs e)
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
            Model.CurrentNote?.SaveNote();
            SelectViewMode(NoteViewModes.Default);
        }

        private void SelectViewMode(ChangeViewMode message)
        {
            SelectViewMode(message.Mode);
        }

        private void SelectViewMode(NoteViewModes mode)
        {
            if (mode == NoteViewModes.Default)
            {
                if (Hub.Instance.Settings.DefaultNoteView == "View")
                    mode = NoteViewModes.View;
                else if (Hub.Instance.Settings.DefaultNoteView == "Split")
                    mode = NoteViewModes.Split;
                else if (Hub.Instance.Settings.DefaultNoteView == "Edit")
                    mode = NoteViewModes.Edit;
            }

            Button source = null;
            if (Model.CurrentNote != null && string.IsNullOrEmpty(Model.CurrentNote.Note.DecryptedText))
            {
                if (mode == NoteViewModes.Split)
                    source = SplitModeButton;
                else
                    source = EditModeButton;
            }
            else
            {
                if (mode == NoteViewModes.View)
                    source = ViewModeButton;
                else if (mode == NoteViewModes.Split)
                    source = SplitModeButton;
                else if (mode == NoteViewModes.Edit)
                    source = EditModeButton;
            }

            if (source != null)
                ViewModeButtonClicked(source, null);
        }

        private void GetXamlButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_markdownToFlowDocumentConverter == null)
                return;

            var doc = _markdownToFlowDocumentConverter.GetNewDocument();

            //Stream s = new MemoryStream();
            var xaml = XamlWriter.Save(doc);
            Clipboard.SetText(xaml);
            //s.Seek(0, SeekOrigin.Begin);

            //FileStream fs = File.Create("C:\\Temp\\doc.xaml");
            //s.CopyTo(fs);
            //fs.Close();
            //s.Close();
        }

        private void GoToPage_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var url = e.Parameter as string;

            Uri uri;
            if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri))
            {
                // If the link is not an url, it is probably a path to a note-file, so find it
                var file = Model.CurrentNote.Note.Files.FirstOrDefault(nf => nf.FileName.Equals(url));
                if (file != null)
                    url = file.FullName;
                else
                    return;
            }

            if (!File.Exists(url) && (uri == null || !uri.IsAbsoluteUri))
                return;

            var sInfo = new ProcessStartInfo(url);
            Process.Start(sInfo);
        }

        private void ShowNoteFile_OnClick(object sender, RoutedEventArgs e)
        {
            var path = Hub.Instance.Storage.DataStore.DataFolder;
            var args = path + "\\" + Model.CurrentNote.Notebook + "\\" + Model.CurrentNote.Note.ID + "." + Core.Constants.File.NoteFileExtension;
            Process.Start(args);
        }

        private void ShowNoteFolder_OnClick(object sender, RoutedEventArgs e)
        {
            var path = Hub.Instance.Storage.DataStore.DataFolder;
            var args = "/select,\"" + path + "\\" + Model.CurrentNote.Notebook + "\"";
            Process.Start("explorer.exe", args);
        }

        private void NoteView_OnLoaded(object sender, RoutedEventArgs e)
        {
            _markdownToFlowDocumentConverter = Resources["TextToFlowDocumentConverter"] as TextToFlowDocumentConverter;
            Model.MarkdownConverter = _markdownToFlowDocumentConverter;

            Messenger.Default.Send(new ApplicationPartLoaded(ApplicationPartLoaded.ApplicationParts.NoteView));
        }

        private void DocumentCheckBoxChecked(object arg, ExecutedRoutedEventArgs executedRoutedEventArgs)
        {
            _markdownToFlowDocumentConverter.Pause = true;
            Model.DocumentCheckBoxCheckedCommand?.Execute(executedRoutedEventArgs.Parameter);
            _markdownToFlowDocumentConverter.Pause = false;
        }

        private void ViewModeButtonClicked(object sender, RoutedEventArgs e)
        {
            if (SplitColumn == null)
                return;

            if (e != null)
                Model.CurrentNote?.SaveNote();

            var btn = (Button) sender;
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
                Model.MainWindowInstance.Model.ToggleSearchFlyoutCommand.Execute(e);
            }
        }

        private void FlowDocumentPageViewer_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.SystemKey == Key.LeftCtrl && e.Key == Key.F)
            {
                e.Handled = true;
                Model.MainWindowInstance.Model.ToggleSearchFlyoutCommand.Execute(e);
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
            PrintDocumentImageableArea ia = null;
            var docWriter = PrintQueue.CreateXpsDocumentWriter(ref ia);

            if (docWriter != null && ia != null)
            {
                var paginator = ((IDocumentPaginatorSource) copy).DocumentPaginator;

                // Change the PageSize and PagePadding for the document to match the CanvasSize for the printer device.
                paginator.PageSize = new Size(ia.MediaSizeWidth, ia.MediaSizeHeight);
                var t = new Thickness(72); // copy.PagePadding;
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