using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop;
using MahApps.Metro.Controls.Dialogs;
using MimeTypes;
using Noterium.Code.Helpers;
using Noterium.Code.Messages;
using Noterium.Controls;
using Noterium.Core;
using Noterium.Core.DataCarriers;
using Noterium.Properties;
using DragDrop = GongSolutions.Wpf.DragDrop.DragDrop;

namespace Noterium.ViewModels
{
    [DebuggerDisplay("{Note.Name} - Index: {Note.SortIndex} - ID: {Note.ID}")]
    public class NoteViewModel : NoteriumViewModelBase, IDragSource, IDropTarget
    {
        private bool _isDirty;
        private bool _isSelected;
        private Note _note;
        private Notebook _notebook;
        private NoteFile _selectedNoteFile;

        private bool _visible = true;

        public ICommand EditNoteCommand { get; set; }
        public ICommand SaveNoteCommand { get; set; }
        public ICommand CopyNoteCommand { get; set; }
        public ICommand RenameNoteCommand { get; set; }
        public ICommand DeleteNoteCommand { get; set; }
        public ICommand OpenFileCommand { get; set; }
        public ICommand RenameFileCommand { get; set; }
        public ICommand DeleteFileCommand { get; set; }

        public ObservableCollection<TokenizedTagItem> Tags { get; internal set; }

        public Note Note
        {
            get => _note;
            set
            {
                _note = value;
                RaisePropertyChanged();
            }
        }

        public string CreatedDateText
        {
            get
            {
                if (Note.Created.Year == DateTime.Now.Year)
                {
                    var ts = DateTime.Now - Note.Created;
                    if (ts.TotalMinutes < 1)
                        return Resources.Time_Now;
                    if (ts.TotalHours < 1)
                        return Convert.ToInt32(Math.Floor(ts.TotalMinutes)) + " " + Resources.Time_Min;
                    if (ts.TotalDays > 1 && ts.TotalDays < 31)
                    {
                        var totalDays = Convert.ToInt32(Math.Floor(ts.TotalDays));
                        var suffix = " " + (totalDays == 1 ? Resources.Time_Day : Resources.Time_Days);
                        return totalDays + suffix;
                    }

                    if (Note.Created.Month != DateTime.Now.Month)
                        return Note.Created.ToString(Resources.Time_NoteCreatedDateFormatSameYear);
                }

                return Note.Created.ToString(Resources.Time_NoteCreatedDateFormat);
            }
        }

        public bool Visible
        {
            get => _visible;
            set
            {
                _visible = value;
                RaisePropertyChanged();
            }
        }

        public bool IsDirty
        {
            get => _isDirty;
            set
            {
                _isDirty = value;
                RaisePropertyChanged();
            }
        }

        public NoteFile SelectedNoteFile
        {
            get => _selectedNoteFile;
            set
            {
                _selectedNoteFile = value;
                RaisePropertyChanged();
            }
        }

        public Notebook Notebook
        {
            get => _notebook;
            private set
            {
                _notebook = value;
                RaisePropertyChanged();
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                RaisePropertyChanged();
            }
        }

        public bool IsSaving { get; set; }

        public void StartDrag(IDragInfo dragInfo)
        {
            DragDrop.DefaultDragHandler.StartDrag(dragInfo);
        }

        public bool CanStartDrag(IDragInfo dragInfo)
        {
            return true;
        }

        public void Dropped(IDropInfo dropInfo)
        {
        }

        public void DragCancelled()
        {
        }

        public bool TryCatchOccurredException(Exception exception)
        {
            return true;
        }

        public void DragOver(IDropInfo dropInfo)
        {
            dropInfo.Effects = DragDropEffects.Copy;

            var box = dropInfo.VisualTarget as TextBox;
            if (box != null)
            {
                var index = box.GetCharacterIndexFromPoint(dropInfo.DropPosition, true);
                if (index > -1)
                {
                    box.Focus();
                    box.CaretIndex = index;
                }
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is NoteFile)
            {
                var box = dropInfo.VisualTarget as TextBox;
                if (box != null) AddFilesToTextBox(box, dropInfo, new List<NoteFile> {(NoteFile) dropInfo.Data});
            }
            else if (dropInfo.Data is DataObject)
            {
                var dataObject = (DataObject) dropInfo.Data;
                if (dataObject.ContainsFileDropList())
                {
                    var fileList = dataObject.GetFileDropList();
                    var createdFiles = new List<NoteFile>();
                    foreach (var filePath in fileList)
                        if (File.Exists(filePath))
                        {
                            var fi = new FileInfo(filePath);
                            var file = File.ReadAllBytes(filePath);

                            var ext = Path.GetExtension(filePath);
                            if (!string.IsNullOrWhiteSpace(ext))
                                ext = ext.ToLower();

                            var noteFileName = NoteFile.ProposeNoteFileName(fi.Name, Note);

                            var mime = MimeTypeMap.GetMimeType(ext);
                            var nf = NoteFile.Create(noteFileName, mime, file, Note, ext);
                            createdFiles.Add(nf);
                        }

                    if (dropInfo.VisualTarget is TextBox) AddFilesToTextBox((TextBox) dropInfo.VisualTarget, dropInfo, createdFiles);
                }
            }
        }

        public void Init(Note note)
        {
            Note = note;
            Notebook = Hub.Instance.Storage.GetNotebook(note.Notebook);

            Tags = new ObservableCollection<TokenizedTagItem>(Note.Tags.ToList().ConvertAll(t => new TokenizedTagItem(t)));
            Tags.CollectionChanged += TagsCollectionChanged;

            Note.PropertyChanged += NotePropertyChanged;

            RenameNoteCommand = new RelayCommand(RenameNote);
            CopyNoteCommand = new RelayCommand(CopyNote);
            OpenFileCommand = new RelayCommand(OpenFile);
            RenameFileCommand = new RelayCommand(RenameFile);
            DeleteFileCommand = new RelayCommand(DeleteFile);
            DeleteNoteCommand = new RelayCommand(SendDeleteNoteMessage);
            SaveNoteCommand = new RelayCommand(StopEditing);
        }

        private void StopEditing()
        {
            SaveNote();
            MessengerInstance.Send(new ChangeViewMode());
        }

        private void SendDeleteNoteMessage()
        {
            MessengerInstance.Send(new DeleteNote(this));
        }

        private void DeleteFile()
        {
            if (SelectedNoteFile != null) MessageBox.Show("Delete " + SelectedNoteFile.Name);
        }

        private void RenameFile()
        {
            // TODO: i18n
            if (SelectedNoteFile != null)
            {
                var name = SelectedNoteFile.Name;
                var settings = DialogHelpers.GetDefaultDialogSettings();
                settings.DefaultText = name;
                MainWindowInstance.ShowInputAsync("Rename", "Enter new name:", settings).ContinueWith(delegate(Task<string> task)
                {
                    var newName = task.Result;
                    if (task.IsCanceled)
                        return;

                    if (!string.IsNullOrWhiteSpace(newName))
                    {
                        var existingNoteFile = Note.Files.FirstOrDefault(enf => enf.Name.Equals(newName));
                        if (existingNoteFile == null)
                        {
                            SelectedNoteFile.Name = newName;
                            Note.DecryptedText = Note.DecryptedText.Replace($"[!{name}]", $"[!{newName}]");
                            IsDirty = true;
                        }
                        else
                        {
                            InvokeOnCurrentDispatcher(() =>
                            {
                                MainWindowInstance.ShowMessageAsync("Error", "You already have a file in this note called " + newName);
                                RenameFile();
                            });
                        }
                    }
                });
            }
        }

        private void OpenFile()
        {
            if (SelectedNoteFile != null)
                if (File.Exists(SelectedNoteFile.FullName))
                    Process.Start(SelectedNoteFile.FullName);
        }

        private void CopyNote()
        {
            if (Note.Encrypted)
            {
                var settings = DialogHelpers.GetDefaultDialogSettings();
                MainWindowInstance.ShowMessageAsync(Resources.Note_Copy_SecureTitle, Resources.Note_Copy_SecureText, MessageDialogStyle.AffirmativeAndNegative, settings).ContinueWith(delegate(Task<MessageDialogResult> task)
                {
                    if (task.Result == MessageDialogResult.Affirmative)
                        DoCopyNote(Note);
                });
            }
            else
            {
                DoCopyNote(Note);
            }
        }

        private static void DoCopyNote(Note note)
        {
            var data = new NoteClipboardData
            {
                Name = note.Name,
                Text = note.DecryptedText,
                Tags = new List<string>(note.Tags)
            };

            if (note.Files.Any())
            {
                data.Files = new List<ClipboardFile>();
                foreach (var noteFile in note.Files)
                {
                    var file = new ClipboardFile
                    {
                        Name = noteFile.Name,
                        Data = File.ReadAllBytes(noteFile.FullName),
                        FileName = noteFile.FileName,
                        MimeType = MimeTypeMap.GetMimeType(Path.GetExtension(noteFile.FileName))
                    };
                    data.Files.Add(file);
                }
            }

            var format = DataFormats.GetDataFormat(typeof(NoteClipboardData).FullName);

            IDataObject dataObj = new DataObject();
            dataObj.SetData(format.Name, data, false);
            Clipboard.SetDataObject(dataObj, false);
        }

        private void RenameNote()
        {
            // TODO: i18n
            var name = Note.Name;
            var settings = DialogHelpers.GetDefaultDialogSettings();
            settings.DefaultText = name;
            MainWindowInstance.ShowInputAsync("Rename", "Enter new name:", settings).ContinueWith(delegate(Task<string> task)
            {
                var newName = task.Result;
                if (!string.IsNullOrWhiteSpace(newName)) Note.Name = newName;
            });
        }

        private void TagsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //if (e.Action == NotifyCollectionChangedAction.Remove)
            //{
            //	foreach (object o in e.OldItems)
            //	{
            //		TokenizedTagItem tag = (TokenizedTagItem)o;
            //		if (tag != null)
            //		{
            //			Note.Tags.Remove(tag.Text);
            //		}
            //	}
            //}
            if (e.Action == NotifyCollectionChangedAction.Add)
                foreach (var o in e.NewItems)
                {
                    var tag = (TokenizedTagItem) o;
                    if (tag != null) tag.PropertyChanged += Tag_PropertyChanged;
                }
        }

        private void Tag_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            StoreTags();
            IsDirty = true;
        }

        private void NotePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is Note && ((Note) sender).IsUpdatingFromDisc)
                return;

            if (e.PropertyName == "Text")
                return;

            if (e.PropertyName != "Changed")
                IsDirty = true;

            if (e.PropertyName == "Notebook")
                Notebook = Hub.Instance.Storage.GetNotebook(Note.Notebook);
        }

        public void SaveNote(bool force = false)
        {
            if (!force)
                if (!IsDirty || IsSaving)
                    return;

            // NOTE: Possibly a bit unsafe when editing a note, might miss chars, but what the heck.
            IsDirty = false;
            IsSaving = true;
            Note.Save();
            IsSaving = false;

            InvokeOnCurrentDispatcher(() => { Hub.Instance.Settings.RefreshTags(); });
        }

        private void StoreTags()
        {
            Note.Tags.Clear();
            foreach (var t in Tags)
                if (!string.IsNullOrWhiteSpace(t.Name))
                    Note.Tags.Add(t.Text);
        }

        private void AddFilesToTextBox(TextBox box, IDropInfo dropInfo, List<NoteFile> files)
        {
            var index = box.GetCharacterIndexFromPoint(dropInfo.DropPosition, true);
            if (index > -1)
            {
                box.Focus();
                box.CaretIndex = index;
            }

            var builder = new StringBuilder();
            foreach (var nf in files)
                if (nf.IsImage)
                    builder.Append(nf.GetAsImageMarkdown());
                else
                    builder.AppendLine(nf.GetAsFileLink());

            box.SelectedText = builder.ToString();
        }
    }
}