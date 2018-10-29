using System;
using System.Collections.ObjectModel;
using Noterium.Code.Interfaces;
using Noterium.Code.Messages;
using Noterium.Core.DataCarriers;

namespace Noterium.ViewModels
{
    public class TagViewModel : NoteriumViewModelBase, IMainMenuItem
    {
        private bool _isSelected;

        public TagViewModel(Tag tag)
        {
            Tag = tag;
        }

        public Tag Tag { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                RaisePropertyChanged();
            }
        }

        public string Name => Tag.Name;

        public MenuItemType MenuItemType => MenuItemType.Tag;

        public ObservableCollection<NoteViewModel> Notes => throw new NotImplementedException();
    }
}