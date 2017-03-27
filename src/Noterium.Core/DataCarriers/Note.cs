using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Noterium.Core.Interfaces;

namespace Noterium.Core.DataCarriers
{
    [DataContract]
	[DebuggerDisplay("{Name} - Index: {SortIndex} - ID: {ID}")]
    public class Note : IComparable, IComparable<Note>, IEquatable<Note>, INotifyPropertyChanged, ISortable
    {
        private readonly object _deleteLockObject = new object();
        private readonly object _saveLockObject = new object();
        private bool _archived;
        private DateTime _archivedDate;
        private DateTime _changed;
        private DateTime _created;
        private DateTime _dueDate;
        private bool _encrypted;
        private bool _favourite;
        private ObservableCollection<NoteFile> _files;
        private Guid _group;
        private bool _important;
        private bool _inTrashCan;
        private string _name;
        private Guid _notebook;
        private bool _protected;
        private string _secureText;
        private int _sortIndex;
        private ObservableCollection<string> _tags;
        private string _text;
		private bool _initialized = false;

        public delegate void NoteRefreshedFromDiskEventHandler();

        public Note()
        {
            ID = Guid.NewGuid();
            Tags = new ObservableCollection<string>();
            Tags.CollectionChanged += Tags_CollectionChanged;
            Files = new ObservableCollection<NoteFile>();
            Files.CollectionChanged += FilesCollectionChanged;
        }

		public void SetIsInitialized()
		{
			_initialized = true;
		}

		public override string ToString()
		{
			return $"{Name} - Index: {SortIndex} - ID: {ID}";
		}

		public bool IsUpdatingFromDisc{ get; internal set; }

		[DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaiseOnPropetyChanged();
            }
        }

        [DataMember]
        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                _secureText = null;
                RaiseOnPropetyChanged();
            }
        }

        public string DecryptedText
        {
            get
            {
                if (_secureText == null)
                {
                    var text = string.Empty;
                    if (!string.IsNullOrEmpty(Text))
                        text = Encrypted ? Hub.Instance.EncryptionManager.Decrypt(Text) : Text;
                    _secureText = text;
                }
                return _secureText;
            }
            set
            {
                if(!string.Equals(_secureText, value, StringComparison.Ordinal))
                {
                    _secureText = value;
                    RaiseOnPropetyChanged();
                }
            }
        }

        [DataMember]
        public Guid Group
        {
            get { return _group; }
            set
            {
                _group = value;
                RaiseOnPropetyChanged();
            }
        }

        [DataMember]
        public Guid Notebook
        {
            get
            {
                if (_notebook == Guid.Empty && _group != Guid.Empty)
                    _notebook = _group;
                return _notebook;
            }
            set
            {
                _notebook = value;
                RaiseOnPropetyChanged();
            }
        }

        [DataMember]
        public DateTime DueDate
        {
            get { return _dueDate; }
            set
            {
                _dueDate = value;
                RaiseOnPropetyChanged();
            }
        }

        [DataMember]
        public DateTime Changed
        {
            get { return _changed; }
            set
            {
                _changed = value;
                RaiseOnPropetyChanged();
            }
        }

        [DataMember]
        public bool Protected
        {
            get { return _protected; }
            set
            {
                _protected = value;
                RaiseOnPropetyChanged();
            }
        }

        [DataMember]
        public bool Important
        {
            get { return _important; }
            set
            {
                _important = value;
                RaiseOnPropetyChanged();
            }
        }

        [DataMember]
        public DateTime Created
        {
            get { return _created; }
            set
            {
                _created = value;
                RaiseOnPropetyChanged();
            }
        }

        [DataMember]
        public DateTime ArchivedDate
        {
            get { return _archivedDate; }
            set
            {
                _archivedDate = value;
                RaiseOnPropetyChanged();
            }
        }

        [DataMember]
        public ObservableCollection<string> Tags
        {
            get { return _tags; }
            set
            {
                _tags = value;
                RaiseOnPropetyChanged();
            }
        }

        [DataMember]
        public ObservableCollection<NoteFile> Files
        {
            get { return _files; }
            set
            {
                _files = value;
                RaiseOnPropetyChanged();
            }
        }

        [DataMember]
        [DefaultValue(false)]
        public bool Favourite
        {
            get { return _favourite; }
            set
            {
                _favourite = value;
                RaiseOnPropetyChanged();
            }
        }

        [DataMember]
        [DefaultValue(false)]
        public bool Archived
        {
            get { return _archived; }
            set
            {
                _archived = value;
                RaiseOnPropetyChanged();
            }
        }

        [DataMember]
        [DefaultValue(false)]
        public bool Encrypted
        {
            get { return _encrypted; }
            set
            {
	            bool raiseChanged = _encrypted != value;
                _encrypted = value;

				if (raiseChanged)
				{
					if(_initialized)
						Hub.Instance.EncryptionManager.SwitchTextEncryptionMode(this);
					RaiseOnPropetyChanged();
				}
            }
        }

        [DataMember]
        [DefaultValue(false)]
        public bool InTrashCan
        {
            get { return _inTrashCan; }
            set
            {
                _inTrashCan = value;
                RaiseOnPropetyChanged();
            }
        }

        public bool HasDuedate => DueDate != DateTime.MinValue;

        public int CompareTo(object obj)
        {
            var note = obj as Note;
            if (note != null)
                return CompareTo(note);
            return -1;
        }

        public int CompareTo(Note other)
        {
            return ID.CompareTo(other.ID);
        }

        public bool Equals(Note other)
        {
            return ID == other?.ID;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event NoteRefreshedFromDiskEventHandler NoteRefreshedFromDisk;

        [DataMember]
        [DefaultValue(0)]
        public int SortIndex
        {
            get { return _sortIndex; }
            set
            {
                _sortIndex = value;
                RaiseOnPropetyChanged();
            }
        }

        public bool IsEventHandlerRegistered(Delegate prospectiveHandler)
        {
            if (PropertyChanged != null)
            {
                foreach (var existingHandler in PropertyChanged.GetInvocationList())
                {
                    if (existingHandler == prospectiveHandler)
                        return true;
                }
            }
            return false;
        }

        private void RaiseOnPropetyChanged([CallerMemberName] string propertyName = null)
        {
			if(_initialized)
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal void RaiseRefreshedFromDisk()
        {
            NoteRefreshedFromDisk?.Invoke();
        }

        private void Tags_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaiseOnPropetyChanged("Tags");
        }

        private void FilesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaiseOnPropetyChanged("Files");
        }

        public void Save()
        {
			if (IsUpdatingFromDisc)
				return;

            lock (_saveLockObject)
            {
                if (DecryptedText.Length > 0)
                    Text = Encrypted ? Hub.Instance.EncryptionManager.Encrypt(DecryptedText) : DecryptedText;
                else
                    Text = string.Empty;

                Changed = DateTime.UtcNow;

                Hub.Instance.Storage.SaveNote(this);
            }
        }

        public void Delete()
        {
            lock (_deleteLockObject)
            {
                Hub.Instance.Storage.DeleteNote(this);
            }
        }

        //public void HandleReminders(List<Reminder> removedReminders)
        //{
        //	lock (_remindersLockObject)
        //	{
        //		HandlingReminders = true;

        //		foreach (Reminder reminder in Reminders)
        //		{
        //			if (string.IsNullOrWhiteSpace(reminder.CalendarEventId))
        //				Hub.Instance.GoogleCalendar.AddReminder(reminder);
        //			else
        //				Hub.Instance.GoogleCalendar.UpdateReminder(reminder);
        //		}

        //		HandlingReminders = false;
        //	}
        //}

        public Tag[] GetTags()
        {
            return Hub.Instance.Settings.Tags.Where(t => Tags.Contains(t.Name)).ToArray();
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
    }
}