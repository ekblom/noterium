using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Noterium.Core;
using Noterium.Properties;

namespace Noterium.Components
{
    /// <summary>
    ///     Interaction logic for AuthenticationForm.xaml
    /// </summary>
    public partial class AuthenticationForm : INotifyPropertyChanged
    {
        public delegate void AuthenticatedEventHandler();

        public delegate void CancelAuthenticationEventHandler();

        private bool _onlyVerifyPassword;
        private int _passwordTries;

        public AuthenticationForm()
        {
            InitializeComponent();
            IsVisibleChanged += AuthenticationFormControlIsVisibleChanged;
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
        public event AuthenticatedEventHandler OnAuthenticated;
        public event CancelAuthenticationEventHandler OnAuthentionCanceled;

        private void AuthenticationFormControlIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool) e.NewValue)
                Dispatcher.BeginInvoke(
                    DispatcherPriority.ContextIdle,
                    new Action(delegate { Password.Focus(); })
                );
        }

        private void PasswordBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Password.SecurePassword.Length > 0)
            {
                bool sucess;

                try
                {
                    if (_passwordTries >= 3)
                        Thread.Sleep(3000);

                    if (OnlyVerifyPassword)
                        sucess = Hub.Instance.EncryptionManager.ValidatePassword(Password.SecurePassword);
                    else
                        sucess = Hub.Instance.EncryptionManager.Authenticate(true, Password.SecurePassword);
                }
                catch (Exception)
                {
                    sucess = false;
                }

                if (sucess)
                {
                    //GlowBrush = (SolidColorBrush)FindResource("AccentColorBrush");
                    _passwordTries = 0;
                    e.Handled = true;
                    OnAuthenticated?.Invoke();
                    //DialogResult = true;
                    //Close();
                }
                else
                {
                    //GlowBrush = (SolidColorBrush)FindResource("ValidationBrush5");
                    _passwordTries++;
                    Password.SelectAll();
                }
            }
            else if (e.Key == Key.Escape)
            {
                e.Handled = true;
                OnAuthentionCanceled?.Invoke();
                //DialogResult = null;
                //Close();
            }
        }

        public void Reset()
        {
            Password.Clear();
            Password.Focus();
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}