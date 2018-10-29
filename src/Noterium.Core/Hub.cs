using Noterium.Core.DataCarriers;
using Noterium.Core.Search;
using Noterium.Core.Security;
using Noterium.Core.Services;

namespace Noterium.Core
{
    public class Hub
    {
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static Hub()
        {
        }

        private Hub()
        {
            Storage = new Storage();
            AppSettings = new AppSettings();
        }

        public static Hub Instance { get; } = new Hub();

        public SearchManager SearchManager { get; private set; }

        public Storage Storage { get; }

        public Settings Settings { get; private set; }

        public TextClipper TextClipper { get; private set; }

        public EncryptionManager EncryptionManager { get; private set; }

        public TagManager TagManager { get; private set; }

        public AppSettings AppSettings { get; }

        public Library CurrentLibrary { get; set; }

        public void Init(Library l)
        {
            CurrentLibrary = l;
            Storage.Init(l);
            SearchManager = new SearchManager(Storage);
            Settings = Storage.GetSettings();
            TextClipper = new TextClipper();
            EncryptionManager = new EncryptionManager(Storage.DataStore);
            TagManager = new TagManager();
        }
    }
}