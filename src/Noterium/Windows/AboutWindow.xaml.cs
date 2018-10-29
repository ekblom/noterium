using System.Diagnostics;
using System.Windows.Navigation;
using Noterium.ViewModels;

namespace Noterium.Windows
{
    /// <summary>
    ///     Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        public AboutWindowViewModel CurrentModel => (AboutWindowViewModel) DataContext;

        private void LinkRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}