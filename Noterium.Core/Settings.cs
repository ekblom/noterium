using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Noterium.Core.Annotations;
using Noterium.Core.DataCarriers;
using Noterium.Core.Helpers;

namespace Noterium.Core
{
    public class Settings : INotifyPropertyChanged
    {
        private readonly DataCarriers.Settings _settings;
        private readonly Storage _storage;
        private readonly object _tagsLockObject = new object();

        public Settings(Storage storage)
        {
            _storage = storage;
            _settings = _storage.GetSettings();
            _settings.PropertyChanged += SettingsPropertyChanged;
            if (_settings == null)
            {
                _settings = new DataCarriers.Settings
                {
                    EnableTextClipper = false
                };
                Save();
            }
        }

        public bool EnableAdvancedControls
        {
            get { return _settings.EnableAdvancedControls; }
            set { _settings.EnableAdvancedControls = value; }
        }

        public bool AutoBackup
        {
            get { return _settings.AutoBackup; }
            set { _settings.AutoBackup = value; }
        }

        public bool EnableTextClipper
        {
            get { return _settings.EnableTextClipper; }
            set { _settings.EnableTextClipper = value; }
        }

        public int NumberOfBackupsToKeep
        {
            get { return _settings.NumberOfBackupsToKeep; }
            set { _settings.NumberOfBackupsToKeep = value; }
        }

        public int AutoLockMainWindowAfter
        {
            get { return _settings.AutoLockMainWindowAfter; }
            set { _settings.AutoLockMainWindowAfter = value; }
        }

        public string PandocParameters
        {
            get { return _settings.PandocParameters; }
            set { _settings.PandocParameters = value; }
        }

        public string PandocPath
        {
            get { return _settings.PandocPath; }
            set { _settings.PandocPath = value; }
        }

        public ObservableCollection<Tag> Tags => _settings.Tags;

        public string Theme
        {
            get { return _settings.Theme; }
            set
            {
                _settings.Theme = value;
                OnPropertyChanged();
            }
        }

        public string Accent
        {
            get { return _settings.Accent; }
            set
            {
                _settings.Accent = value;
                OnPropertyChanged();
            }
        }

        public string DefaultNoteView
        {
            get { return _settings.DefaultNoteView; }
            set
            {
                _settings.DefaultNoteView = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void SettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        public void Save()
        {
            _storage.SaveSettings(_settings);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RefreshTags()
        {
            lock (_tagsLockObject)
            {
                var notes = Hub.Instance.Storage.GetAllNotes().Where(n => !n.InTrashCan);

                var tagCounts = new Dictionary<string, int>();
                foreach (var note in notes)
                {
                    if (note.Tags != null && note.Tags.Any())
                    {
                        foreach (var t in note.Tags)
                        {
                            var tag = t.ToLower().Trim();
                            if (!tagCounts.ContainsKey(tag))
                                tagCounts.Add(tag, 1);
                            else
                                tagCounts[tag] += 1;
                        }
                    }
                }

                foreach (var keyValuePair in tagCounts)
                {
                    var tag = Hub.Instance.Settings.Tags.FirstOrDefault(t => t.Name == keyValuePair.Key);
                    if (tag == null)
                    {
                        tag = new Tag {Name = keyValuePair.Key};
                        
                        Hub.Instance.Settings.Tags.Add(tag);
                    }

                    tag.Instances = keyValuePair.Value;
                }

                var missing = Hub.Instance.Settings.Tags.Where(t => !tagCounts.ContainsKey(t.Name)).ToList();
                missing.ForEach(t => Hub.Instance.Settings.Tags.Remove(t));

                Hub.Instance.Settings.Tags.Sort();
                Save();
            }
        }
    }
}