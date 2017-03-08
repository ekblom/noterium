using System;
using Noterium.Core.Constants;

namespace Noterium.Core.DataCarriers
{
    public class Reminder : IComparable, IComparable<Reminder>, IEquatable<Reminder>
    {
        public Guid ID { get; set; }
        public DateTime Time { get; set; }
        public DateTime PostponedTo { get; set; }
        public string Description { get; set; }
        public string CalendarId { get; set; }
        public string CalendarEventId { get; set; }
        public string CustomEventName { get; set; }
        public bool AllDayEvent { get; set; }
        public bool Dismissed { get; set; }

        public string DisplayString
        {
            get
            {
                var text = string.Empty;

                if (AllDayEvent)
                {
                    if (Recurrence == null)
                        text = Time.ToString("yyyy-MM-dd");
                    else if (Recurrence.Frequency == Frequency.Weekly)
                        text = $"Every {Time.DayOfWeek}";
                    else if (Recurrence.Frequency == Frequency.Yearly)
                        text = $"Every {Time:dd MMMM}";
                }
                else
                {
                    if (Recurrence == null)
                        text = Time.ToString("yyyy-MM-dd HH:mm");
                    else if (Recurrence.Frequency == Frequency.Weekly)
                        text = $"Every {Time.DayOfWeek} at {Time:HH:mm}";
                    else
                        text = $"{Recurrence.Frequency} at {Time:dd MMMM, HH:mm}";
                }

                if (!string.IsNullOrWhiteSpace(CustomEventName))
                    text += " (" + CustomEventName + ")";
                return text;
            }
        }

        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public DateTime Changed { get; set; }

        public Recurrence Recurrence { get; set; }

        public int CompareTo(object obj)
        {
            var reminder = obj as Reminder;
            if (reminder != null)
                return CompareTo(reminder);
            return -1;
        }

        public int CompareTo(Reminder other)
        {
            return ID.CompareTo(other.ID);
        }

        public bool Equals(Reminder other)
        {
            return ID == other.ID;
        }
    }
}