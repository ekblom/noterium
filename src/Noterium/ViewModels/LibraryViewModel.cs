using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls.Dialogs;
using Noterium.Code.Commands;
using Noterium.Code.Helpers;
using Noterium.Code.Messages;
using Noterium.Core;
using Noterium.Core.DataCarriers;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Noterium.ViewModels
{
	public class LibraryViewModel : NoteriumViewModelBase
	{
		public ICommand ChangeLibraryCommand { get; set; }
		public ICommand SetDefaultLibraryCommand { get; set; }
		public ICommand DeleteLibraryCommand { get; set; }

		public Library Library { get; }

		public bool IsCurrent => Hub.Instance.CurrentLibrary.Equals(Library);

		public LibraryViewModel(Library library)
		{
			Library = library;

			ChangeLibraryCommand = new RelayCommand(SendChangeLibraryMessage);
			DeleteLibraryCommand = new RelayCommand(DeleteLibrary);
			SetDefaultLibraryCommand = new RelayCommand(SetDefaultLibrary);
		}

		private void SendChangeLibraryMessage()
		{
			Messenger.Default.Send(new ChangeLibrary(Library));
		}

		private void SetDefaultLibrary()
		{
				Hub.Instance.AppSettings.Librarys.ToList().ForEach(l =>
				{
					l.Default = Library == l;
					l.Save();
				});
				Hub.Instance.AppSettings.DefaultLibrary = Library.Name;
				Hub.Instance.AppSettings.Save();
		}

		private void DeleteLibrary()
		{
			var settings = DialogHelpers.GetDefaultDialogSettings();

			string message = $"Do you want to delete the library '{Library.Name}'?\nAll files will be left untouched, it's just the connection that is removed.";
			MainWindowInstance.ShowMessageAsync("Delete library", message, MessageDialogStyle.AffirmativeAndNegative, settings).
				ContinueWith(delegate (Task<MessageDialogResult> task)
				{
					if (task.Result == MessageDialogResult.Affirmative)
					{
						InvokeOnCurrentDispatcher(() =>
						{
							Hub.Instance.AppSettings.Librarys.Remove(Library);
							Library.Delete();
						});
					}
				});
		}
	}
}