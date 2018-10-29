using System.Collections.ObjectModel;
using Noterium.Code.Interfaces;
using Noterium.Code.Messages;
using Noterium.Core;
using Noterium.Core.DataCarriers;

namespace Noterium.ViewModels
{
    public class NotebookViewModel : NoteriumViewModelBase, IMainMenuItem
    {
        private bool _isSelected;
        public Notebook Notebook { get; private set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                RaisePropertyChanged();
            }
        }

        public string Name => Notebook.Name;

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