using System;
using System.Windows.Input;
using Noterium.Core.DataCarriers;

namespace Noterium.Code.Commands
{
	public class ToDoItemCommand : ICommand
	{
		public Func<ToDoItem, string, bool> Function { get; internal set; }

		public ToDoItemCommand(Func<ToDoItem, string, bool> function)
		{
			Function = function;
		}

		public bool CanExecute(object parameter)
		{
			if (parameter is object[])
				return Function != null;
			return false;
		}

		public void Execute(object parameter)
		{
			object[] objects = parameter as object[];
			if(objects != null)
				Function((ToDoItem)objects[0], (string)objects[1]);
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}
	}
}