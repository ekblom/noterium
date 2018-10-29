using System.ComponentModel;
using System.Runtime.CompilerServices;
using Noterium.Properties;

namespace Noterium.Views.Dialogs
{
    /// <summary>
    ///     Interaction logic for AuthenticationWindow.xaml
    /// </summary>
    public partial class LoadingWindow : INotifyPropertyChanged
    {
        public LoadingWindow()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void SetMessage(string message)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => SetMessage(message));
                return;
            }

            LoadingText.Text = message;
        }
    }
}