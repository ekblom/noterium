using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Noterium.Properties;
using Noterium.Components;

namespace Noterium.Views.Dialogs
{
	/// <summary>
	/// Interaction logic for AuthenticationWindow.xaml
	/// </summary>
	public partial class AuthenticationWindow : INotifyPropertyChanged
	{
        public AuthenticationForm AuthForm => AuthenticationForm1;

	    public AuthenticationWindow()
		{
			InitializeComponent();
		}

        private void AuthenticationWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
            AuthenticationForm1.Password.Focus();
		}

        public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
		    handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
