using System.Collections.ObjectModel;
using Noterium.Code.Messages;
using Noterium.ViewModels;

namespace Noterium.Code.Interfaces
{
    public interface IMainMenuItem
    {
        string Name { get; }

        MenuItemType MenuItemType { get; }

        ObservableCollection<NoteViewModel> Notes { get; }
    }
}