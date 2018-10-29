using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Noterium.Components;
using Noterium.Properties;

namespace Noterium.Views.Dialogs
{
    /// <summary>
    ///     Interaction logic for AuthenticationWindow.xaml
    /// </summary>
    public partial class AuthenticationWindow : INotifyPropertyChanged
    {
        public AuthenticationWindow()
        {
            InitializeComponent();
        }

        public AuthenticationForm AuthForm => AuthenticationForm1;

        public event PropertyChangedEventHandler PropertyChanged;

        private void AuthenticationWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            AuthenticationForm1.Password.Focus();
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}