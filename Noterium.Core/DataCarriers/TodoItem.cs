using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Noterium.Core.Annotations;

namespace Noterium.Core.DataCarriers
{
    public class ToDoItem : IComparable, IComparable<ToDoItem>, IEquatable<ToDoItem>, INotifyPropertyChanged
    {
        private bool _done;
        private string _text;

        public ToDoItem()
        {
            ID = Guid.NewGuid();
        }

        public Guid ID { get; set; }

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                OnPropertyChanged();
            }
        }

        public bool Done
        {
            get { return _done; }
            set
            {
                _done = value;
                OnPropertyChanged();
            }
        }

        public int CompareTo(object obj)
        {
            var note = obj as ToDoItem;
            if (note != null)
                return CompareTo(note);
            return -1;
        }

        public int CompareTo(ToDoItem other)
        {
            return ID.CompareTo(other.ID);
        }

        public bool Equals(ToDoItem other)
        {
            return ID == other.ID;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int GetHashCode(ToDoItem obj)
        {
            return obj.ID.GetHashCode();
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}