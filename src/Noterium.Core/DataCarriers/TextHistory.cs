using System;

namespace Noterium.Core.DataCarriers
{
    public class TextHistory : IComparable, IComparable<TextHistory>, IEquatable<TextHistory>
    {
        public Guid ID { get; set; }
        public DateTime Date { get; set; }
        public string Text { get; set; }

        public int CompareTo(object obj)
        {
            var th = obj as TextHistory;
            if (th != null)
                return CompareTo(th);
            return -1;
        }

        public int CompareTo(TextHistory other)
        {
            return ID.CompareTo(other.ID);
        }

        public bool Equals(TextHistory other)
        {
            return ID == other.ID;
        }
    }
}