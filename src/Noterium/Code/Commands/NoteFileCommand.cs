using System;
using System.Windows.Input;
using Noterium.Core.DataCarriers;

namespace Noterium.Code.Commands
{
    public class NoteFileCommand : ICommand
    {
        public NoteFileCommand(Func<NoteFile, bool> function)
        {
            Function = function;
        }

        public Func<NoteFile, bool> Function { get; internal set; }

        public bool CanExecute(object parameter)
        {
            return Function != null;
        }

        public void Execute(object parameter)
        {
            Function((NoteFile) parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}