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
    /// Interaction logic for AuthenticationWindow.xaml
    /// </summary>
    public partial class StorageSelector : INotifyPropertyChanged
    {
        private bool _onlyVerifyPassword;

        public bool OnlyVerifyPassword
        {
            get { return _onlyVerifyPassword; }
            set { _onlyVerifyPassword = value; OnPropertyChanged(); }
        }

        public StorageSelector()
        {
            InitializeComponent();
        }

        private void AuthenticationWindow_OnLoaded(object sender, RoutedEventArgs e)
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;

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
                ListBoxItem item = (ListBoxItem)StorageTypeListBox.SelectedItem;
                if (((string)item.Tag) == "DropBox")
                {
                    string dbPath = DropBoxDataStore.GetDropBoxPath();
                    if (!string.IsNullOrWhiteSpace(dbPath))
                    {
                        Hub.Instance.AppSettings.Librarys.Add(new Library
                        {
                            Name = "DropBox",
                            StorageType = StorageType.DropBox
                        });
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

                    CommonFileDialogResult result = dialog.ShowDialog();
                    if (result == CommonFileDialogResult.Ok)
                    {
                        DirectoryInfo di = new DirectoryInfo(dialog.FileName);
                        if (di.Exists)
                        {
                            string path = dialog.FileName;
                            string name = Path.GetFileName(path);
                            Hub.Instance.AppSettings.Librarys.Add(new Library
                            {
                                Name = name,
                                Path = path,
                                StorageType = StorageType.Disc
                            });
                            Hub.Instance.AppSettings.Save();

                            DialogResult = true;
                        }
                    }
                }

                if (DialogResult == true)
                {
                    Close();
                }
            }
        }

        private void StorageTypeListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NextButton.IsEnabled = true;
        }
    }
}
