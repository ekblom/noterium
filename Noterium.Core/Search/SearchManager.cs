using System;
using System.Collections.Generic;
using System.Linq;
using Noterium.Core.DataCarriers;

namespace Noterium.Core.Search
{
    public class SearchManager
    {
        private readonly Storage _storage;

        public SearchManager(Storage storage)
        {
            _storage = storage;
        }

        public List<Note> Search(string searchTerm)
        {
            var notes = _storage.GetAllNotes();
            return notes.Where(n =>
            {
                if (n.InTrashCan)
                    return false;

                var text = n.Name.IndexOf(searchTerm, StringComparison.InvariantCultureIgnoreCase) > -1
                           || n.DecryptedText.IndexOf(searchTerm, StringComparison.InvariantCultureIgnoreCase) > -1;

                var tag = n.Tags.Any(t => t.IndexOf(searchTerm, StringComparison.InvariantCultureIgnoreCase) > -1);

                return (text || tag);
            }).ToList();
        }
    }
}