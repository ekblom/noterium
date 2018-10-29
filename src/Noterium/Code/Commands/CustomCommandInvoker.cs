using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Noterium.Code.Commands
{
    public sealed class CustomCommandInvoker : TriggerAction<DependencyObject>
    {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(CustomCommandInvoker), null);

        public ICommand Command
        {
            get => (ICommand) GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        protected override void Invoke(object parameter)
        {
            if (AssociatedObject != null)
            {
                var command = Command;
                if (command != null && command.CanExecute(parameter)) command.Execute(parameter);
            }
        }
    }
}