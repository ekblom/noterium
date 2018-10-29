using System;
using System.Windows;
using GalaSoft.MvvmLight;

namespace Noterium.ViewModels
{
    public class NoteriumViewModelBase : ViewModelBase
    {
        private MainWindow _mainWindow;

        public MainWindow MainWindowInstance => _mainWindow ?? (_mainWindow = Application.Current.MainWindow as MainWindow);

        public void InvokeOnCurrentDispatcher(Action a)
        {
            Application.Current.Dispatcher.Invoke(a);
        }
    }
}