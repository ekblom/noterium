using System;
using System.Windows.Input;

namespace Noterium.Code.Commands
{
	public class BasicCommand : ICommand
	{
		private readonly Func<object, bool> _func;

		public BasicCommand(Func<object, bool> func)
		{
			_func = func;
		}

		public bool CanExecute(object parameter)
		{
			return _func != null;
		}

		public void Execute(object parameter)
		{
			_func(parameter);
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}
	}
}