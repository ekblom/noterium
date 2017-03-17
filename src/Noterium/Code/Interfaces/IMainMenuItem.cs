using Noterium.Code.Messages;
using Noterium.ViewModels;
using System.Collections.ObjectModel;

namespace Noterium.Code.Interfaces
{
	public interface IMainMenuItem
	{
		string Name { get; }

		MenuItemType MenuItemType { get; }

		ObservableCollection<NoteViewModel> Notes { get; }
	}
}