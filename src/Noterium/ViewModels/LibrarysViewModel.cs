using GalaSoft.MvvmLight.CommandWpf;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.WindowsAPICodePack.Dialogs;
using Noterium.Code.Commands;
using Noterium.Code.Helpers;
using Noterium.Core;
using Noterium.Core.DataCarriers;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System;
using GalaSoft.MvvmLight.Messaging;
using Noterium.Code.Messages;

namespace Noterium.ViewModels
{
	internal class LibrarysViewModel : NoteriumViewModelBase
	{
		private Library _currentLibrary;

		public ICommand AddLibraryCommand { get; set; }

		public Library CurrentLibrary
		{
			get { return _currentLibrary; }
			set { _currentLibrary = value; RaisePropertyChanged(); }
		}

		public ObservableCollection<LibraryViewModel> Librarys { get; }

		public LibrarysViewModel()
		{
			CurrentLibrary = Hub.Instance.CurrentLibrary;

			AddLibraryCommand = new RelayCommand(AddLibrary);
			Hub.Instance.AppSettings.Librarys.CollectionChanged += LibrarysOnCollectionChanged;

			Librarys = new ObservableCollection<LibraryViewModel>();
			ReloadLibrary();
		}

		private void LibrarysOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			ReloadLibrary();
		}

		private void ReloadLibrary()
		{
			Librarys.Clear();
			Hub.Instance.AppSettings.Librarys.ToList().ConvertAll(li => new LibraryViewModel(li)).ForEach(Librarys.Add);
		}

		private void AddLibrary()
		{
			var dialog = new CommonOpenFileDialog
			{
				IsFolderPicker = true,
				AllowNonFileSystemItems = false,
				EnsurePathExists = true,
				Multiselect = false
			};

			CommonFileDialogResult result = dialog.ShowDialog();
			if (result == CommonFileDialogResult.Ok)
			{
				DirectoryInfo di = new DirectoryInfo(dialog.FileName);
				if (di.Exists)
				{
					string path = dialog.FileName;
					string name = Path.GetFileName(path);
					var library = new Library
					{
						Name = name,
						Path = path,
						StorageType = StorageType.Disc
					};
					library.Save();

					Hub.Instance.AppSettings.Librarys.Add(library);
					Hub.Instance.AppSettings.LibraryFiles.Add(library.FilePath);
					Hub.Instance.AppSettings.Save();

					Messenger.Default.Send(new ChangeLibrary(library));
				}
			}
		}
	}
}