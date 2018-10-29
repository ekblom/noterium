using System.Windows;
using Noterium.ViewModels;

namespace Noterium.Windows
{
    /// <summary>
    ///     Interaction logic for BackupManager.xaml
    /// </summary>
    public partial class BackupManager
    {
        public BackupManager()
        {
            InitializeComponent();
        }

        public BackupManagerViewModel Model => (BackupManagerViewModel) DataContext;

        private void BackupTree_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Model.SelectedFileNode = e.NewValue as FileTreeNode;
        }
    }
}