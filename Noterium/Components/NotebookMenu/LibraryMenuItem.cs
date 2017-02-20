using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Noterium.Properties;

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

        public string Type { get; }
        
        public bool IsSelected
        {
            get { return false; }
            set { throw new NotSupportedException(); }
        }
        
        public LibraryMenuItem(string name, string type)
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