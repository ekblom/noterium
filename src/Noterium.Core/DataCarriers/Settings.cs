using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Noterium.Core.Annotations;
using Noterium.Core.Helpers;

namespace Noterium.Core.DataCarriers
{
    public class Settings : INotifyPropertyChanged
    {
        private readonly object _tagsLockObject = new object();
        private string _accent = "VSDark";
        private bool _autoBackup = true;
        private int _autoLockMainWindowAfter = 10;
        private string _defaultNoteView = "View";
        private bool _enableAdvancedControls;
        private int _numberOfBackupsToKeep = 10;
        private string _theme = "BaseDark";

        public Settings()
        {
            Tags = new ObservableCollection<Tag>();
        }

        public bool EnableAdvancedControls
        {
            get => _enableAdvancedControls;
            set
            {
                _enableAdvancedControls = value;
                OnPropertyChanged();
            }
        }

        [DefaultValue(true)]
        public bool AutoBackup
        {
            get => _autoBackup;
            set
            {
                _autoBackup = value;
                OnPropertyChanged();
            }
        }

        [DefaultValue(10)]
        public int NumberOfBackupsToKeep
        {
            get => _numberOfBackupsToKeep;
            set
            {
                _numberOfBackupsToKeep = value;
                OnPropertyChanged();
            }
        }

        [DefaultValue(false)]
        public bool EnableTextClipper { get; set; } = false;

        [JsonProperty("pandocParameters", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue("-r html -t markdown --preserve-tabs --no-wrap")]
        public string PandocParameters { get; set; } = "-r html -t markdown --preserve-tabs --no-wrap";

        [JsonProperty("pandocPath", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(@"C:\Pandoc\pandoc.exe")]
        public string PandocPath { get; set; } = @"C:\Pandoc\pandoc.exe";

        public ObservableCollection<Tag> Tags { get; set; } = new ObservableCollection<Tag>();

        [JsonProperty("autoLockMainWindowAfter", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(5)]
        public int AutoLockMainWindowAfter
        {
            get => _autoLockMainWindowAfter;
            set
            {
                _autoLockMainWindowAfter = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("theme", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue("BaseDark")]
        public string Theme
        {
            get => _theme;
            set
            {
                _theme = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("accent", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue("VSDark")]
        public string Accent
        {
            get => _accent;
            set
            {
                _accent = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("defaultNoteView", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue("View")]
        public string DefaultNoteView
        {
            get => _defaultNoteView;
            set
            {
                _defaultNoteView = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

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
                    if (note.Tags != null && note.Tags.Any())
                        foreach (var t in note.Tags)
                        {
                            if (t == null)
                                continue;

                            var tag = t.ToLower().Trim();
                            if (!tagCounts.ContainsKey(tag))
                                tagCounts.Add(tag, 1);
                            else
                                tagCounts[tag] += 1;
                        }

                foreach (var keyValuePair in tagCounts)
                {
                    var tag = Tags.FirstOrDefault(t => t.Name == keyValuePair.Key);
                    if (tag == null)
                    {
                        tag = new Tag {Name = keyValuePair.Key};

                        Hub.Instance.Settings.Tags.Add(tag);
                    }

                    tag.Instances = keyValuePair.Value;
                }

                var missing = Tags.Where(t => !tagCounts.ContainsKey(t.Name)).ToList();
                missing.ForEach(t => Tags.Remove(t));

                Tags.Sort();
                Save();
            }
        }

        public void Save()
        {
            Hub.Instance.Storage.SaveSettings(this);
        }
    }
}