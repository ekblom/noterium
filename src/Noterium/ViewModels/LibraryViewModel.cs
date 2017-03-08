using Noterium.Core.DataCarriers;

namespace Noterium.ViewModels
{
	public class LibraryViewModel : NoteriumViewModelBase
	{
		public Library Library { get; }

		public bool IsCurrent => MainWindowInstance.Model.CurrentLibrary.Equals(Library);

		public LibraryViewModel(Library library)
		{
			Library = library;
		}
	}
}