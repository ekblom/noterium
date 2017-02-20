using System;
using System.Windows;
using System.Windows.Controls;

namespace Noterium.Controls
{
    public class BaseUserControl : UserControl
    {
        public void InvokeOnCurrentDispatcher(Action a)
        {
            Application.Current.Dispatcher.Invoke(a);
        }
    }
}