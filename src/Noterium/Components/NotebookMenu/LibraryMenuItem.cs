using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Noterium.Properties;
using Noterium.Code.Messages;

namespace Noterium.Components.NotebookMenu
{
    public class LibraryMenuItem : INotifyPropertyChanged, IMainMenuItem
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged(); }
        }

        public LibraryType Type { get; }

        public LibraryMenuItem(string name, LibraryType type)
        {
            _name = name;
            Type = type;
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