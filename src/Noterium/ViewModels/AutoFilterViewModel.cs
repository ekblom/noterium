using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Noterium.Code.Interfaces;
using Noterium.Code.Messages;
using Noterium.Core;
using Noterium.Core.DataCarriers;

namespace Noterium.ViewModels
{
    public class AutoFilterViewModel : NoteriumViewModelBase, IMainMenuItem
    {
        private string _name;

        public AutoFilterViewModel(string name, MenuItemType type)
        {
            Name = name;
            MenuItemType = type;

            MessengerInstance.Register<UpdateNoteList>(this, ReloadNoteList);
        }

        public string Name
        {
            get => _name;
            private set
            {
                _name = value;
                RaisePropertyChanged();
            }
        }

        public MenuItemType MenuItemType { get; }

        public ObservableCollection<NoteViewModel> Notes { get; } = new ObservableCollection<NoteViewModel>();

        private void ReloadNoteList(UpdateNoteList obj)
        {
            if (obj.MenuItem.MenuItemType == MenuItemType.Notebook)
                return;

            List<Note> notes;
            if (MenuItemType == MenuItemType.Trashcan)
            {
                var tempNotes = Hub.Instance.Storage.GetAllNotes();
                notes = tempNotes.Where(n => n.InTrashCan).ToList();
            }
            else if (MenuItemType == MenuItemType.Favorites)
            {
                var tempNotes = Hub.Instance.Storage.GetAllNotes();
                notes = tempNotes.Where(n => n.Favourite).Where(n => !n.InTrashCan).ToList();
            }
            else if (MenuItemType == MenuItemType.All)
            {
                notes = Hub.Instance.Storage.GetAllNotes().Where(n => !n.InTrashCan).OrderBy(n => n.Name).ToList();
            }
            else if (MenuItemType == MenuItemType.Recent)
            {
                notes = Hub.Instance.Storage.GetAllNotes().Where(n => !n.InTrashCan).OrderBy(n => n.Changed).Take(15).ToList();
            }
            else
            {
                notes = new List<Note>();
            }

            var noteModels = ViewModelLocator.Instance.GetNoteViewModels(notes);
            Notes.Clear();
            noteModels.ForEach(Notes.Add);
        }
    }
}