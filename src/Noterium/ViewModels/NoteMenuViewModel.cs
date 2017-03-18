using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;
using Noterium.Code.Commands;
using Noterium.Core.DataCarriers;
using Noterium.ViewModels;
using DragDrop = GongSolutions.Wpf.DragDrop.DragDrop;
using Noterium.Code.Messages;
using Noterium.Core;
using Noterium.Code.Helpers;
using MahApps.Metro.Controls.Dialogs;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.CommandWpf;

namespace Noterium.ViewModels
{
	public class NoteMenuViewModel : NoteriumViewModelBase, IDragSource, IDropTarget
	{
		private NoteViewModel _selectedNote;
		private string _sortMode = "Index";
		private readonly object _currentNotesLockObject = new object();

		public ICommand CreateNewNoteCommand { get; set; }
		public ICommand CreateNewSecureNoteCommand { get; set; }
		public ICommand SelectedItemChangedCommand { get; set; }
		public ICommand FilterNotesCommand { get; set; }
		public ICommand ClearFilterCommand { get; set; }
		public ICommand AddNoteCommand { get; set; }
		public ICommand ShowNoteCommandsCommand { get; set; }

		public NoteMenuViewModel()
		{
			DataSource = new ObservableCollection<NoteViewModel>();
			BindingOperations.EnableCollectionSynchronization(DataSource, _currentNotesLockObject);

			FilterNotesCommand = new SimpleCommand(FilterNotes);
			ClearFilterCommand = new SimpleCommand(ClearFilter);
			ShowNoteCommandsCommand = new SimpleCommand(ShowNoteCommands);
			CreateNewNoteCommand = new RelayCommand(CreateNewNote);
			CreateNewSecureNoteCommand = new RelayCommand(CreateNewSecureNote);

			PropertyChanged += OnPropertyChanged;

			var saveTimer = new Timer(1000)
			{
				AutoReset = true,
				Enabled = true
			};
			saveTimer.Elapsed += SaveNotIfDirty;
			saveTimer.Start();

			MessengerInstance.Register<ReloadNoteMenuList>(this, DoReloadNoteList);
			MessengerInstance.Register<DeleteNote>(this, DeleteNote);
		}

		private void DeleteNote(DeleteNote message)
		{
			// TODO: i18n
			if (SelectedNote == null || SelectedNote != message.Note)
				return;

			Note note = SelectedNote.Note;
			if (note.InTrashCan)
			{
				return;
			}

			if (note.Protected)
			{
				MessengerInstance.Send(new QuickMessage("You cant delete this note, it's protected from that."));

				return;
			}

			var settings = DialogHelpers.GetDefaultDialogSettings();

			MainWindowInstance.ShowMessageAsync("Delete", $"Do you want to delete {note.Name}?", MessageDialogStyle.AffirmativeAndNegative, settings).ContinueWith(delegate (Task<MessageDialogResult> task)
			{
				if (task.Result == MessageDialogResult.Affirmative)
				{
					InvokeOnCurrentDispatcher(() =>
					{
						note.InTrashCan = true;

						DataSource.Remove(SelectedNote);
						SelectedNote = DataSource.FirstOrDefault();
						Hub.Instance.Settings.RefreshTags();
					});
				}
			});
		}

		private void DoReloadNoteList(ReloadNoteMenuList obj)
		{
			List<Note> notes = new List<Note>();

			if (obj.Tag != null)
			{
				var tempNotes = Hub.Instance.Storage.GetAllNotes();
				notes = tempNotes.Where(n => n.Tags.Contains(obj.Tag.Name)).Where(n => !n.InTrashCan).ToList();
			}
			else if (obj.Notebook != null)
			{
				notes = Hub.Instance.Storage.GetNotes(obj.Notebook).Where(n => !n.InTrashCan).ToList();
			}
			else if (obj.LibraryType != MenuItemType.Undefined)
			{
				if (obj.LibraryType == MenuItemType.Trashcan)
				{
					var tempNotes = Hub.Instance.Storage.GetAllNotes();
					notes = tempNotes.Where(n => n.InTrashCan).ToList();
				}
				else if (obj.LibraryType == MenuItemType.Favorites)
				{
					var tempNotes = Hub.Instance.Storage.GetAllNotes();
					notes = tempNotes.Where(n => n.Favourite).Where(n => !n.InTrashCan).ToList();
				}
				else if (obj.LibraryType == MenuItemType.All)
				{
					notes = Hub.Instance.Storage.GetAllNotes().Where(n => !n.InTrashCan).OrderBy(n => n.Name).ToList();
				}
				else if (obj.LibraryType == MenuItemType.Recent)
				{
					notes = Hub.Instance.Storage.GetAllNotes().Where(n => !n.InTrashCan).OrderBy(n => n.Changed).Take(15).ToList();
				}
			}

			var models = ViewModelLocator.Instance.GetNoteViewModels(notes);
			SelectedNote = null;
			UpdateDataSource(models);

			if (obj.SelectedNote != null)
			{
				var selected = ViewModelLocator.Instance.GetNoteViewModel(obj.SelectedNote);
				SelectedNote = selected;
			}
			else
				SelectedNote = models.Any() ? models[0] : null;
		}

		private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "SortMode")
			{
				var selected = SelectedNote;
				UpdateDataSource(new List<NoteViewModel>(DataSource));
				SelectedNote = selected;
			}
		}

		private void ShowNoteCommands(object o)
		{
			if (o is ListViewItem)
			{

			}
		}

		private void SaveNotIfDirty(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			if (SelectedNote != null && SelectedNote.IsDirty)
				SelectedNote.SaveNote();
		}

		private void ClearFilter(object arg)
		{
			foreach (NoteViewModel model in DataSource)
				model.Visible = true;
		}

		private void FilterNotes(object o)
		{
			var arg = o as TextChangedEventArgs;
			if (arg != null)
			{
				TextBox tb = (TextBox)arg.OriginalSource;

				foreach (NoteViewModel model in DataSource)
				{
					if (model.Note.Name.Contains(tb.Text) || ContainsTag(model, tb.Text) || model.Note.DecryptedText.Contains(tb.Text))
					{
						model.Visible = true;
					}
					else
					{
						model.Visible = false;
					}
				}
			}
		}

		private bool ContainsTag(NoteViewModel model, string text)
		{
			return model.Tags.Any(t => t.Text.Contains(text));
		}

		public ObservableCollection<NoteViewModel> DataSource { get; }

		public string SortMode
		{
			get { return _sortMode; }
			set { _sortMode = value; RaisePropertyChanged(); }
		}

		public NoteViewModel SelectedNote
		{
			get { return _selectedNote; }
			set
			{
				if (_selectedNote != null)
				{
					_selectedNote.SaveNote();
					_selectedNote.IsSelected = false;
				}
				_selectedNote = value;
				if (_selectedNote != null)
					_selectedNote.IsSelected = true;

				MessengerInstance.Send(new SelectedNoteChanged(_selectedNote));

				RaisePropertyChanged();
			}
		}


		#region dragdrop


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
			ObservableCollection<NoteViewModel> targetList = dropInfo.TargetCollection as ObservableCollection<NoteViewModel>;
			if (targetList != null)
			{
				for (int i = 0; i < targetList.Count; i++)
				{
					NoteViewModel nvm = targetList[i];
					nvm.Note.SortIndex = i;
					nvm.SaveNote();
				}
			}
		}

		public void DragCancelled()
		{

		}

		public bool TryCatchOccurredException(Exception exception)
		{
			return false;
		}

		#endregion

		public void DragOver(IDropInfo dropInfo)
		{
			if (SortMode != "Index")
				dropInfo.Effects = DragDropEffects.None;
			else
				DragDrop.DefaultDropHandler.DragOver(dropInfo);
		}

		public void Drop(IDropInfo dropInfo)
		{
			if (SortMode != "Index")
				return;

			DragDrop.DefaultDropHandler.Drop(dropInfo);
		}

		public void UpdateDataSource(List<NoteViewModel> models)
		{
			if (SortMode == "Index")
				models.Sort((x, y) => x.Note.SortIndex.CompareTo(y.Note.SortIndex));
			else
			{
				models.Sort((x, y) => String.Compare(x.Note.Name, y.Note.Name, StringComparison.Ordinal));
				if (SortMode == "ZA")
					models.Reverse();
			}

			DataSource.Clear();
			models.ForEach(DataSource.Add);
		}

		internal void CreateNewNote(string name, bool secure = false)
		{
			CreateNewNote(name, ViewModelLocator.Instance.NotebookMenu.SelectedNotebook.Notebook, secure, ViewModelLocator.Instance.NoteMenu.DataSource.Count);
		}

		internal Note CreateNewNote(string name, Notebook notebook, bool secure, int sortIndex, string text = null)
		{
			Note note = new Note
			{
				ID = Guid.NewGuid(),
				Name = name,
				Notebook = notebook.ID,
				Created = DateTime.Now,
				Encrypted = secure,
				SortIndex = sortIndex,
				Text = text ?? string.Empty
			};
			note.SetIsInitialized();
			note.Save();

			//var notebookViewModel = ViewModelLocator.Instance.GetNotebookViewModel(notebook);
			DoReloadNoteList(new ReloadNoteMenuList(notebook, note));
			//MessengerInstance.Send(new UpdateNoteList(notebookViewModel));

			//if (focusNote)
			//	MessengerInstance.Send(new SelectNote(note));

			return note;
		}


		private void CreateNewSecureNote()
		{
			if (!Hub.Instance.EncryptionManager.SecureNotesEnabled)
			{
				MainWindowInstance.ShowMessageAsync("Secure notes", "To create secure notes you need to activate this function in settings.");
				return;
			}

			CreateNewNote(true);
		}

		private void CreateNewNote()
		{
			CreateNewNote(false);
		}

		private void CreateNewNote(bool secure)
		{
			// TODO: i18n
			var settings = DialogHelpers.GetDefaultDialogSettings();

			MainWindowInstance.ShowInputAsync("New note", "Name of the new note:", settings).ContinueWith(delegate (Task<string> task)
			{
				string name = task.Result;
				if (!string.IsNullOrWhiteSpace(name))
				{
					InvokeOnCurrentDispatcher(() => { CreateNewNote(name, secure); });
				}
			});
		}
	}
}