using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;
using Noterium.Core;
using Noterium.Core.DataCarriers;
using Noterium.ViewModels;
using Settings = Noterium.Core.Settings;

namespace Noterium.Components.NotebookMenu
{
	public class NotebookMenuViewModel : NoteriumViewModelBase, IDropTarget
	{
		private NotebookMenuItem _selectedNotebook;
		private Tag _selectedTag;

		public Settings Settings { get; set; }

		public NotebookMenuViewModel()
		{
			Settings = Hub.Instance.Settings;
		}

		public ICommand SelectedItemChangedCommand { get; set; }
		public ICommand DeleteItemCommand { get; set; }
		public ICommand RenameItemCommand { get; set; }
		public ICommand AddNotebookCommand { get; set; }
		public ICommand EmptyTrashCommand { get; set; }

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
	}
}