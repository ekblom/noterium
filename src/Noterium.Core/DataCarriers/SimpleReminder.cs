using System;

namespace Noterium.Core.DataCarriers
{
    public class SimpleReminder : IComparable, IComparable<SimpleReminder>, IEquatable<SimpleReminder>
    {
        public SimpleReminder()
        {
            ID = Guid.NewGuid();
        }

        public Guid ID { get; set; }
        public Guid NoteID { get; set; }
        public Guid ReminderID { get; set; }
        public DateTime Date { get; set; }
        public DateTime PostponedTo { get; set; }
        public string Text { get; set; }

        public int CompareTo(object obj)
        {
            SimpleReminder reminder = obj as SimpleReminder;
            if (reminder != null)
                return ID.CompareTo(reminder.ID);
            return -1;
        }

        public int CompareTo(SimpleReminder other)
        {
            return ID.CompareTo(other.ID);
        }

        public bool Equals(SimpleReminder other)
        {
            return ID.Equals(other.ID);
        }
    }
}