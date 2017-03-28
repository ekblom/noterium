using System.Linq;
using Noterium.Core.DataCarriers;
using Noterium.Core.Search;
using Noterium.Core.Security;
using Noterium.Core.Services;
using Noterium.Core.Localization;

namespace Noterium.Core
{
    public class Hub
    {
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

        public Settings Settings => _settings;

        public TextClipper TextClipper { get; private set; }

        public EncryptionManager EncryptionManager { get; private set; }

        public TagManager TagManager { get; private set; }

        public AppSettings AppSettings { get; }

        public void Init(Library l)
        {
	        CurrentLibrary = l;
            _storage.Init(l);
            SearchManager = new SearchManager(_storage);
            _settings = _storage.GetSettings();
            TextClipper = new TextClipper();
            EncryptionManager = new EncryptionManager(_storage.DataStore);
            TagManager = new TagManager();
		}

	    public Library CurrentLibrary { get; set; }
    }
}