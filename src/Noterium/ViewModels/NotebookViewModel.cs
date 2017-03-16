using System;
using System.Collections.Generic;
using Noterium.Core.DataCarriers;
using Noterium.Code.Interfaces;
using Noterium.Code.Messages;
using Noterium.Core;
using System.Collections.ObjectModel;

namespace Noterium.ViewModels
{
	public class NotebookViewModel : NoteriumViewModelBase, IMainMenuItem
	{
		private bool _isSelected;
	    public Notebook Notebook { get; private set; }

		public bool IsSelected
		{
			get { return _isSelected; }
			set { _isSelected = value; RaisePropertyChanged(); }
		}

		public string Name { get => Notebook.Name; }

		public ObservableCollection<NoteViewModel> Notes { get; } = new ObservableCollection<NoteViewModel>();

		public MenuItemType MenuItemType => MenuItemType.Notebook;

		internal void Init(Notebook notebook)
		{
			Notebook = notebook;

			MessengerInstance.Register<UpdateNoteList>(this, ReloadNoteList);
		}

		private void ReloadNoteList(UpdateNoteList obj)
		{
			var model = obj.MenuItem as NotebookViewModel;
			if (model == null)
				return;

			if (model.Notebook != Notebook)
				return;

			var notes = Hub.Instance.Storage.GetNotes(Notebook);
			var noteModels = ViewModelLocator.Instance.GetNoteViewModels(notes);
			Notes.Clear();
			noteModels.ForEach(Notes.Add);
		}
	}
}