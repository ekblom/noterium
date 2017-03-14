﻿using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;
using Noterium.Core;
using Noterium.Core.DataCarriers;
using Noterium.ViewModels;
using Noterium.Code.Commands;
using Noterium.Code.Messages;
using static Noterium.Code.Messages.ConfigureControlsForParnetType;
using System.Collections.Generic;
using Noterium.Code.Helpers;
using MahApps.Metro.Controls.Dialogs;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Windows.Data;
using GalaSoft.MvvmLight.CommandWpf;

namespace Noterium.Components.NotebookMenu
{
	public class NotebookMenuViewModel : NoteriumViewModelBase, IDropTarget
	{
		private NotebookMenuItem _selectedNotebook;
		private Tag _selectedTag;
		private IMainMenuItem _selectedMenuItem;
		private readonly object _mainMenuListLockObject = new object();
		private readonly object _currentNotesbooksLockObject = new object();

		#region -- Static menu items --
		private readonly LibraryMenuItem _favoritesLibraryMenuItem = new LibraryMenuItem("Favorites", LibraryType.Favorites);
		private readonly LibraryMenuItem _trashLibraryMenuItem = new LibraryMenuItem("Trash", LibraryType.Trashcan);
		private readonly LibraryMenuItem _allLibraryMenuItem = new LibraryMenuItem("All notes", LibraryType.All);
		private readonly LibraryMenuItem _recentLibraryMenuItem = new LibraryMenuItem("Recent", LibraryType.Recent);
		#endregion

		public ICommand SelectedNoteContainerChangedCommand { get; set; }
		public ICommand SelectedItemChangedCommand { get; set; }
		public ICommand DeleteItemCommand { get; set; }
		public ICommand RenameItemCommand { get; set; }
		public ICommand AddNotebookCommand { get; set; }
		public ICommand EmptyTrashCommand { get; set; }
		public ICommand PasteNoteCommand { get; set; }
		public ICommand DeleteNoteBookCommand { get; set; }

		public Settings Settings { get; set; }
		public IMainMenuItem SelectedMenuItem
		{
			get { return _selectedMenuItem; }
			set { _selectedMenuItem = value; RaisePropertyChanged(); }
		}

		public ObservableCollection<NotebookMenuItem> NotebookMenuItems { get; set; }

		public ObservableCollection<NotebookMenuItem> Notebooks { get; set; }

		public NotebookMenuItem SelectedNotebook
		{
			get { return _selectedNotebook; }
			set { _selectedNotebook = value; RaisePropertyChanged(); }
		}

		public Tag SelectedTag
		{
			get { return _selectedTag; }
			set { _selectedTag = value; RaisePropertyChanged(); }
		}

		public NotebookMenuViewModel()
		{

			Settings = Hub.Instance.Settings;

			DeleteNoteBookCommand = new RelayCommand(DeleteCurrentNoteBook);
			SelectedItemChangedCommand = new SimpleCommand(SelectedNoteContainerChanged);
			RenameItemCommand = new SimpleCommand(RenameItem);
			DeleteItemCommand = new SimpleCommand(DeleteItem);
			AddNotebookCommand = new SimpleCommand(CreateNotebook);
			EmptyTrashCommand = new SimpleCommand(EmptyTrash);
			PasteNoteCommand = new SimpleCommand(PasteNote);
			SelectedNoteContainerChangedCommand = new SimpleCommand(SelectedNoteContainerChanged);

			MessengerInstance.Register<ApplicationUnlocked>(this, OnApplicationUnlocked);

			InitMainMenu();
		}

		private void InitMainMenu()
		{
			NotebookMenuItems = new ObservableCollection<NotebookMenuItem>();

			List<NotebookMenuItem> items = new List<NotebookMenuItem>();

			foreach (Notebook notebook in Hub.Instance.Storage.GetNotebooks())
			{
				var item = new NotebookMenuItem(notebook);
				NotebookMenuItems.Add(item);
				items.Add(item);
			}

			items.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));

			if (!Hub.Instance.EncryptionManager.SecureNotesEnabled && items.Any())
			{
				SelectedNotebook = items[0];
				SelectedNotebook.IsSelected = true;
			}
			Notebooks = new ObservableCollection<NotebookMenuItem>(items);

			BindingOperations.EnableCollectionSynchronization(Notebooks, _mainMenuListLockObject);
			BindingOperations.EnableCollectionSynchronization(NotebookMenuItems, _currentNotesbooksLockObject);
		}

		private void OnApplicationUnlocked(ApplicationUnlocked obj)
		{
			if (SelectedMenuItem == null)
				SelectedItemChangedCommand.Execute(Notebooks.FirstOrDefault());
		}

		public void DragOver(IDropInfo dropInfo)
		{
			NoteViewModel sourceItem = dropInfo.Data as NoteViewModel;
			NotebookMenuItem targetItem = dropInfo.TargetItem as NotebookMenuItem;

			if (sourceItem == null)
				return;

			if (targetItem != null)
			{
				dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
				dropInfo.Effects = DragDropEffects.Copy;
				return;
			}

			ListViewItem targetElement = dropInfo.VisualTarget as ListViewItem;
			if (targetElement != null && (targetElement.Name == "Favorites" || targetElement.Name == "Trash"))
			{
				dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
				dropInfo.Effects = DragDropEffects.Copy;
			}
		}

		public void Drop(IDropInfo dropInfo)
		{
			NoteViewModel sourceItem = dropInfo.Data as NoteViewModel;
			if (sourceItem == null)
				return;

			NotebookMenuItem targetItem = dropInfo.TargetItem as NotebookMenuItem;
			ListViewItem targetElement = dropInfo.VisualTarget as ListViewItem;

			bool removeFromSourceCollection = false;

			if (targetItem != null)
			{
				if (sourceItem.Note.Notebook != targetItem.Notebook.ID)
				{
					if (sourceItem.Note.InTrashCan)
					{
						sourceItem.Note.InTrashCan = false;
						//sourceItem.Note.Save();
					}

					Hub.Instance.Storage.DataStore.MoveNote(sourceItem.Note, targetItem.Notebook);

					if (SelectedNotebook != null)
					{
						removeFromSourceCollection = true;
					}
				}
				else if (sourceItem.Note.InTrashCan)
				{
					sourceItem.Note.InTrashCan = false;
					//sourceItem.Note.Save();

					removeFromSourceCollection = true;
				}
			}
			else if (targetElement != null)
			{
				if (targetElement.Name == "Favorites")
				{
					sourceItem.Note.Favourite = true;
				}
				else if (targetElement.Name == "Trash" && !sourceItem.Note.Protected)
				{
					sourceItem.Note.InTrashCan = true;
					removeFromSourceCollection = true;
				}

				sourceItem.SaveNote();
			}

			if (removeFromSourceCollection)
			{
				ObservableCollection<NoteViewModel> sourceList = dropInfo.DragInfo.SourceCollection as ObservableCollection<NoteViewModel>;
				sourceList?.Remove(sourceItem);
			}
		}

		private void SelectedNoteContainerChanged(object p)
		{
			SelectedNoteContainerChanged(p, null);
		}
		private bool SelectedNoteContainerChanged(object p, NoteViewModel selected)
		{
			if (Hub.Instance.EncryptionManager.SecureNotesEnabled && !Hub.Instance.EncryptionManager.IsAuthenticated)
				return false;

			object addedItem = null;
			SelectionChangedEventArgs selectionChangedEventArgs = p as SelectionChangedEventArgs;
			if (selectionChangedEventArgs != null)
			{
				SelectionChangedEventArgs args = selectionChangedEventArgs;
				if (args.AddedItems.Count > 0)
					addedItem = args.AddedItems[0];
			}
			else if (p is NotebookMenuItem)
				addedItem = p;

			var tag = addedItem as Tag;
			if (tag != null)
			{
				SelectedTag = tag;
				SelectedNotebook = null;
				SelectedMenuItem = null;
				MessengerInstance.Send(new ConfigureControlsForParnetType(ParentType.Tag));
			}
			else if (addedItem is NotebookMenuItem)
			{
				MessengerInstance.Send(new ConfigureControlsForParnetType(ParentType.Notebook));
				SelectedTag = null;
				SelectedNotebook = (NotebookMenuItem)addedItem;
				SelectedMenuItem = SelectedNotebook;
			}
			else if (addedItem is ListViewItem)
			{
				ListViewItem item = (ListViewItem)addedItem;
				switch (item.Name)
				{
					case "Favorites":
						SelectedMenuItem = _favoritesLibraryMenuItem;
						break;
					case "AllNotes":
						SelectedMenuItem = _allLibraryMenuItem;
						break;
					case "Trash":
						SelectedMenuItem = _trashLibraryMenuItem;
						break;
					case "Recent":
						SelectedMenuItem = _recentLibraryMenuItem;
						break;
				}
				SelectedTag = null;
				SelectedNotebook = null;
				MessengerInstance.Send(new ConfigureControlsForParnetType(ParentType.Library));
			}

			if (SelectedNotebook != null || SelectedTag != null || SelectedMenuItem != null)
				ReloadNoteList(selected);

			return true;
		}

		private void ReloadNoteList(NoteViewModel selected = null)
		{
			ReloadNoteList message = new ReloadNoteList(null);

			List<Note> notes = new List<Note>();

			if (SelectedTag != null)
			{
				message = new ReloadNoteList(SelectedTag);
			}
			else if (SelectedNotebook != null)
			{
				message = new ReloadNoteList(SelectedNotebook.Notebook, selected?.Note);
			}
			else if (SelectedMenuItem is LibraryMenuItem)
			{
				var menuItem = SelectedMenuItem as LibraryMenuItem;
				message = new ReloadNoteList(menuItem.Type);
			}

			if (!message.IsEmpty())
			{
				MessengerInstance.Send(message);
			}
		}

		private void RenameItem(object arg)
		{
			if (SelectedMenuItem is TagMenuItem)
				return;

			string name = SelectedMenuItem.Name;
			var settings = DialogHelpers.GetDefaultDialogSettings();
			settings.DefaultText = name;
			MainWindowInstance.ShowInputAsync("Rename", "Enter new name:", settings).ContinueWith(delegate (Task<string> task)
			{
				string newName = task.Result;
				if (!string.IsNullOrWhiteSpace(newName))
				{
					if (SelectedMenuItem is NotebookMenuItem)
					{
						SelectedNotebook.Notebook.Name = newName;
						SelectedNotebook.Notebook.Save();
					}
				}
			});
		}

		private void DeleteItem(object arg)
		{
			if (SelectedMenuItem is TagMenuItem)
				return;

			if (SelectedMenuItem is NotebookMenuItem)
				DeleteCurrentNoteBook();
		}

		private void DeleteCurrentNoteBook()
		{
			// TODO: i18n
			if (SelectedNotebook != null)
			{
				var notes = Hub.Instance.Storage.GetNotes(SelectedNotebook.Notebook);
				if (!notes.Any())
				{
					var settings = DialogHelpers.GetDefaultDialogSettings();

					if (Notebooks.Count == 1)
					{
						MainWindowInstance.ShowMessageAsync("Delete", "You can't delete the last notebook, rename it if the name is wrong.", MessageDialogStyle.Affirmative, settings);
						return;
					}

					MainWindowInstance.ShowMessageAsync("Delete", $"Do you want to delete the notebook {SelectedNotebook.Name}?", MessageDialogStyle.AffirmativeAndNegative, settings).ContinueWith(delegate (Task<MessageDialogResult> task)
					{
						if (task.Result == MessageDialogResult.Affirmative)
						{
							InvokeOnCurrentDispatcher(() =>
							{
								int index = Notebooks.IndexOf(SelectedNotebook);
								if (index >= Notebooks.Count - 1)
									index--;

								SelectedNotebook.Notebook.Delete();

								if (Notebooks.Contains(SelectedNotebook))
									Notebooks.Remove(SelectedNotebook);

								if (NotebookMenuItems.Contains(SelectedNotebook))
									NotebookMenuItems.Remove(SelectedNotebook);

								SelectedNotebook = Notebooks[index];
							});
						}
					});
				}
				else
				{
					MainWindowInstance.ShowMessageAsync("Delete", "You can't delete a notebook that contains notes.");
				}
			}
		}

		private void CreateNotebook(object arg)
		{
			MainWindowInstance.ShowInputAsync("Add notebook", "Name:").ContinueWith(delegate (Task<string> task)
			{
				string newName = task.Result;
				if (string.IsNullOrEmpty(newName))
					return;

				Notebook nb = new Notebook
				{
					Name = newName,
					Created = DateTime.Now,
					SortIndex = 0
				};
				nb.Save();
				var item = new NotebookMenuItem(nb);

				List<NotebookMenuItem> temp = new List<NotebookMenuItem>();
				temp.AddRange(Notebooks.ToArray());
				temp.Add(item);
				temp.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));

				Notebooks.Clear();
				temp.ForEach(Notebooks.Add);

				SelectedNotebook = item;
				SelectedNotebook.IsSelected = true;
			});
		}

		private void EmptyTrash(object arg)
		{
			var settings = DialogHelpers.GetDefaultDialogSettings();

			MainWindowInstance.ShowMessageAsync("Empty trash", "Do you want to delete all notes in the trashcan?\nThis operation can not be undone.", MessageDialogStyle.AffirmativeAndNegative, settings).
				ContinueWith(delegate (Task<MessageDialogResult> task)
				{
					if (task.Result == MessageDialogResult.Affirmative)
					{
						// TODO: Send empty trash message instead
						foreach (NoteViewModel n in ViewModelLocator.Instance.NoteMenu.DataSource)
						{
							Guid noteId = n.Note.ID;
							ViewModelLocator.Instance.Unregister(noteId.ToString());
							n.Note.Delete();
						}

						ViewModelLocator.Instance.NoteMenu.SelectedNote = null;
						ViewModelLocator.Instance.NoteMenu.DataSource.Clear();
					}
				});
		}

		private void PasteNote(object obj)
		{
			Notebook notebook = obj as Notebook;
			if (notebook != null)
			{
				DataFormat format = DataFormats.GetDataFormat(typeof(NoteClipboardData).FullName);
				if (!Clipboard.ContainsData(format.Name))
					return;

				NoteClipboardData data = Clipboard.GetData(format.Name) as NoteClipboardData;
				if (data != null)
				{
					int index = Hub.Instance.Storage.GetNoteCount(notebook);
					Note note = CreateNewNote(data.Name, notebook.ID, false, index, false);
					note.Text = data.Text;
					if (data.Files != null && data.Files.Any())
					{
						foreach (ClipboardFile file in data.Files)
						{
							NoteFile nf = NoteFile.Create(file.Name, file.MimeType, file.Data, note);
							note.Text = note.Text.Replace(file.FileName, nf.FileName);
						}
					}

					note.Save();

					MessengerInstance.Send(new SelectNote(note));
				}
			}

		}

		private void CreateNewNote(string name, bool secure = false)
		{
			CreateNewNote(name, SelectedNotebook.Notebook.ID, secure, ViewModelLocator.Instance.NoteMenu.DataSource.Count);
		}

		private Note CreateNewNote(string name, Guid notebookId, bool secure, int sortIndex, bool focusNote = true)
		{
			Note note = new Note
			{
				ID = Guid.NewGuid(),
				Name = name,
				Notebook = notebookId,
				Created = DateTime.Now,
				Encrypted = secure,
				SortIndex = sortIndex
			};

			note.Save();

			if (focusNote)
				MessengerInstance.Send(new SelectNote(note));

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
			var settings = DialogHelpers.GetDefaultDialogSettings();

			MainWindowInstance.ShowInputAsync("New note", "Name of the new note:", settings).ContinueWith(delegate (Task<string> task)
			{
				string name = task.Result;
				if (!string.IsNullOrWhiteSpace(name))
				{
					CreateNewNote(name, secure);
				}
			});
		}
	}
}