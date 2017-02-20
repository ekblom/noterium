using System.Linq;
using Noterium.Core.License;
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

        public LicenseManager LicenseManager { get; private set; }

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

        public void Init()
        {
            var library = AppSettings.Librarys.First();

            _storage.Init(library);
            LicenseManager = new LicenseManager(_storage);
            SearchManager = new SearchManager(_storage);
            _settings = new Settings(_storage);
            _gc = new GoogleCalendar(ref _settings);
            ReminderManager = new GoogleCalendarReminderManager(ref _gc);
            TextClipper = new TextClipper();
            Reminders = new Reminders(ref _storage);
            EncryptionManager = new EncryptionManager(_storage.DataStore);
            TagManager = new TagManager();
        }
    }
}