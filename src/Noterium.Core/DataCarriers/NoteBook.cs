using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Noterium.Core.Annotations;

namespace Noterium.Core.DataCarriers
{
    [DataContract]
    [DebuggerDisplay("{Name} - {ID} - {BaseHashCode}")]
    public class Notebook : IComparable<Notebook>, IComparable, IEquatable<Notebook>, IEqualityComparer<Notebook>, INotifyPropertyChanged
    {
        private readonly object _deleteLockObject = new object();
        private readonly object _saveLockObject = new object();
        private bool _deleted;
        private string _name;

        public Notebook()
        {
            ID = Guid.NewGuid();
        }

        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public DateTime Created { get; set; }

        public bool Deleted
        {
            get { return _deleted; }
            private set
            {
                _deleted = value;
                OnPropertyChanged();
            }
        }

        public int BaseHashCode => base.GetHashCode();

        public int CompareTo(object obj)
        {
            Notebook other = obj as Notebook;
            if (other != null)
            {
                return CompareTo(other);
            }
            return -1;
        }

        public int CompareTo(Notebook other)
        {
            return ID.CompareTo(other.ID);
        }

        public bool Equals(Notebook x, Notebook y)
        {
            return x.ID == y.ID;
        }

        public int GetHashCode(Notebook obj)
        {
            return obj.ID.GetHashCode();
        }

        public bool Equals(Notebook other)
        {
            return ID == other.ID;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Save()
        {
            lock (_saveLockObject)
            {
                if (Deleted)
                    return;

                Hub.Instance.Storage.SaveNotebook(this);
            }
        }

        public void Delete()
        {
            lock (_deleteLockObject)
            {
                if (!Deleted)
                {
                    Hub.Instance.Storage.DeleteGroup(this);
                    Deleted = true;
                }
            }
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0} - {{{1}}} - {2}", Name, ID, base.GetHashCode());
        }
    }
}