using System.Linq;
using Noterium.Core.DataCarriers;
using Noterium.Core.Search;
using Noterium.Core.Security;
using Noterium.Core.Services;

namespace Noterium.Core
{
    public class Hub
    {
        private GoogleCalendar _gc;

        private Settings _settings;
        private Storage _storage;

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static Hub()
        {
        }

        private Hub()
        {
            _storage = new Storage();
            AppSettings = new AppSettings();
        }

        public static Hub Instance { get; } = new Hub();

        public SearchManager SearchManager { get; private set; }

        public Storage Storage => _storage;

        public GoogleCalendarReminderManager ReminderManager { get; private set; }

        public Settings Settings => _settings;

        public GoogleCalendar GoogleCalendar => _gc;

        public TextClipper TextClipper { get; private set; }

        public Reminders Reminders { get; private set; }

        public EncryptionManager EncryptionManager { get; private set; }

        public TagManager TagManager { get; private set; }

        public AppSettings AppSettings { get; }

        public void Init(Library l)
        {
	        CurrentLibrary = l;
            _storage.Init(l);
            SearchManager = new SearchManager(_storage);
            _settings = new Settings(_storage);
            _gc = new GoogleCalendar(ref _settings);
            ReminderManager = new GoogleCalendarReminderManager(ref _gc);
            TextClipper = new TextClipper();
            Reminders = new Reminders(ref _storage);
            EncryptionManager = new EncryptionManager(_storage.DataStore);
            TagManager = new TagManager();
        }

	    public Library CurrentLibrary { get; set; }
    }
}