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
		public ICommand AddNoteFileCommand { get; private set; }
		public ICommand DeleteNoteFileCommand { get; private set; }

		public event PropertyChangedEventHandler PropertyChanged;
		private Note _note;
		private string _text;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
		    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

	    public Note Note
		{
			get { return _note; }
			internal set { _note = value; OnPropertyChanged(); }
		}

		public BoundObservableCollection<NoteFileViewModel, NoteFile> NoteFiles { get; internal set; }

		public string Text
		{
			get { return _text; }
			private set { _text = value; OnPropertyChanged();
			}
		}

		public NoteEditorViewModel(Note note)
		{
			Note = note;
            
			Text = Note.DecryptedText;

			NoteFiles = new BoundObservableCollection<NoteFileViewModel, NoteFile>(Note.Files, nf => new NoteFileViewModel(nf), (vm, m) => vm.NoteFile == m);

			AddNoteFileCommand = new NoteFileCommand(AddNoteFile);
			DeleteNoteFileCommand = new NoteFileCommand(DeleteNoteFile);
			//WeakEventManager<NoteEditorViewModel, NoteFileEventArgs>.AddHandler(this, "NoteFileDeleted", null);
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
			public NoteFile NoteFile { get; private set; }

			public NoteFileEventArgs(NoteFile noteFile)
			{
				NoteFile = noteFile;
			}
		}
	}
}