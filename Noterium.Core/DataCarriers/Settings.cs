using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Noterium.Core.Annotations;

namespace Noterium.Core.DataCarriers
{
    public class Settings : INotifyPropertyChanged
    {
        private string _accent = "Blue";
        private bool _autoBackup = true;
        private int _autoLockMainWindowAfter = 10;
        private string _defaultNoteView = "View";
        private bool _enableAdvancedControls;
        private int _numberOfBackupsToKeep = 10;
        private string _theme = "BaseLight";

        public Settings()
        {
            Tags = new ObservableCollection<Tag>();
        }

        public bool EnableAdvancedControls
        {
            get { return _enableAdvancedControls; }
            set
            {
                _enableAdvancedControls = value;
                OnPropertyChanged();
            }
        }

        [DefaultValue(true)]
        public bool AutoBackup
        {
            get { return _autoBackup; }
            set
            {
                _autoBackup = value;
                OnPropertyChanged();
            }
        }

        [DefaultValue(10)]
        public int NumberOfBackupsToKeep
        {
            get { return _numberOfBackupsToKeep; }
            set
            {
                _numberOfBackupsToKeep = value;
                OnPropertyChanged();
            }
        }

        [DefaultValue(false)]
        public bool EnableTextClipper { get; set; }

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
            get { return _autoLockMainWindowAfter; }
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
            get { return _theme; }
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
            get { return _accent; }
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
            get { return _defaultNoteView; }
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
    }
}