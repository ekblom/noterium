using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Noterium.Code;
using Noterium.Code.Commands;
using Noterium.Core.DataCarriers;
using Noterium.Properties;

namespace Noterium.ViewModels
{
    public class NoteEditorViewModel : INotifyPropertyChanged
    {
        private Note _note;
        private string _text;

        public NoteEditorViewModel(Note note)
        {
            Note = note;

            Text = Note.DecryptedText;

            NoteFiles = new BoundObservableCollection<NoteFileViewModel, NoteFile>(Note.Files, nf => new NoteFileViewModel(nf), (vm, m) => vm.NoteFile == m);

            AddNoteFileCommand = new NoteFileCommand(AddNoteFile);
            DeleteNoteFileCommand = new NoteFileCommand(DeleteNoteFile);
            //WeakEventManager<NoteEditorViewModel, NoteFileEventArgs>.AddHandler(this, "NoteFileDeleted", null);
        }

        public ICommand AddNoteFileCommand { get; }
        public ICommand DeleteNoteFileCommand { get; }

        public Note Note
        {
            get => _note;
            internal set
            {
                _note = value;
                OnPropertyChanged();
            }
        }

        public BoundObservableCollection<NoteFileViewModel, NoteFile> NoteFiles { get; internal set; }

        public string Text
        {
            get => _text;
            private set
            {
                _text = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool DeleteNoteFile(NoteFile nf)
        {
            var result = MessageBox.Show($"Do you want to delete the file {nf.Name}?", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                nf.Delete();
                Note.Files.Remove(nf);
            }

            return true;
        }

        private bool AddNoteFile(NoteFile nf)
        {
            return true;
        }

        public class NoteFileEventArgs : EventArgs
        {
            public NoteFileEventArgs(NoteFile noteFile)
            {
                NoteFile = noteFile;
            }

            public NoteFile NoteFile { get; }
        }
    }
}