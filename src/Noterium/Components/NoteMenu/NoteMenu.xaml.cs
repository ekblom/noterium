using System.Windows;
using Noterium.ViewModels;

namespace Noterium.Components.NoteMenu
{
    /// <summary>
    ///     Interaction logic for NoteMenu.xaml
    /// </summary>
    public partial class NoteMenu
    {
        public NoteMenu()
        {
            InitializeComponent();
        }

        public NoteMenuViewModel Model => DataContext as NoteMenuViewModel;

        private void NoteMenu_OnLoaded(object sender, RoutedEventArgs e)
        {
        }
    }
}