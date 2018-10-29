using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Noterium.Core.Helpers
{
    public static class CollectionHelpers
    {
        public static void Sort<T>(this ObservableCollection<T> observable) where T : IComparable<T>, IEquatable<T>
        {
            var sorted = observable.OrderBy(x => x).ToList();

            var ptr = 0;
            while (ptr < sorted.Count)
                if (!observable[ptr].Equals(sorted[ptr]))
                {
                    var t = observable[ptr];
                    observable.RemoveAt(ptr);
                    observable.Insert(sorted.IndexOf(t), t);
                }
                else
                {
                    ptr++;
                }
        }
    }
}