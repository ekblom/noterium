using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.WindowsAPICodePack.Dialogs;
using Noterium.Code.Messages;
using Noterium.Core;
using Noterium.Core.DataCarriers;

namespace Noterium.ViewModels
{
    internal class LibrarysViewModel : NoteriumViewModelBase
    {
        private Library _currentLibrary;

        public LibrarysViewModel()
        {
            CurrentLibrary = Hub.Instance.CurrentLibrary;

            AddLibraryCommand = new RelayCommand(AddLibrary);
            Hub.Instance.AppSettings.Librarys.CollectionChanged += LibrarysOnCollectionChanged;

            Librarys = new ObservableCollection<LibraryViewModel>();
            ReloadLibrary();
        }

        public ICommand AddLibraryCommand { get; set; }

        public Library CurrentLibrary
        {
            get => _currentLibrary;
            set
            {
                _currentLibrary = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<LibraryViewModel> Librarys { get; }
        public bool Loaded { get; internal set; }

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

            var result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                var di = new DirectoryInfo(dialog.FileName);
                if (di.Exists)
                {
                    var path = dialog.FileName;
                    var name = Path.GetFileName(path);
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