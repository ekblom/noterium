using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using Noterium.Core;
using Noterium.Core.DataCarriers;
using Noterium.Core.DropBox;
using Noterium.Properties;

namespace Noterium.Views.Dialogs
{
    /// <summary>
    ///     Interaction logic for AuthenticationWindow.xaml
    /// </summary>
    public partial class StorageSelector : INotifyPropertyChanged
    {
        private bool _onlyVerifyPassword;

        public StorageSelector()
        {
            InitializeComponent();
        }

        public bool OnlyVerifyPassword
        {
            get => _onlyVerifyPassword;
            set
            {
                _onlyVerifyPassword = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void AuthenticationWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void NextButtonClick(object sender, RoutedEventArgs e)
        {
            if (StorageTypeListBox.SelectedItem != null)
            {
                var item = (ListBoxItem) StorageTypeListBox.SelectedItem;
                if ((string) item.Tag == "DropBox")
                {
                    var dbPath = DataStore.GetDropBoxPath();
                    if (!string.IsNullOrWhiteSpace(dbPath))
                    {
                        var lib = new Library
                        {
                            Name = "DropBox",
                            StorageType = StorageType.DropBox
                        };
                        lib.Save();

                        Hub.Instance.AppSettings.Librarys.Add(lib);
                        Hub.Instance.AppSettings.LibraryFiles.Add(lib.FilePath);
                        Hub.Instance.AppSettings.Save();

                        DialogResult = true;
                    }
                    else
                    {
                        MessageBox.Show(this, "It seems that you dont have Dropbox installed.\n\nIf you have, please contact me via the support form on http://www.homepage.se.\n\nIf you don't, and don't want to install it, please select \"Filesystem\" as storage type where you can select a folder to store the notes.", "Cant find Dropbox...", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
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
                            var lib = new Library
                            {
                                Name = name,
                                Path = path,
                                StorageType = StorageType.Disc
                            };
                            lib.Save();
                            Hub.Instance.AppSettings.Librarys.Add(lib);
                            Hub.Instance.AppSettings.LibraryFiles.Add(lib.FilePath);
                            Hub.Instance.AppSettings.Save();
                            DialogResult = true;
                        }
                    }
                }

                if (DialogResult == true) Close();
            }
        }

        private void StorageTypeListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NextButton.IsEnabled = true;
        }
    }
}