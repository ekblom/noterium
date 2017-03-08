using System.Collections.ObjectModel;
using Noterium.Core.DataCarriers;

namespace Noterium.Core
{
    public class TagManager
    {
        public ObservableCollection<Tag> Tags { get; set; }

        public void ReloadTags()
        {
        }
    }
}