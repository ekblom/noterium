using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Noterium.Core.DataCarriers;
using Noterium.Core.Utilities;

namespace Noterium.Core
{
    public class Reminders
    {
        public delegate void ReminderDue(SimpleReminder reminder);

        private readonly Storage _storage;

        public Reminders(ref Storage storage)
        {
            _storage = storage;

            var timer = new Timer { Interval = 1000 };
            timer.Elapsed += TimerElapsed;
            timer.Enabled = true;
        }

        private List<SimpleReminder> RemindersList => _storage.GetReminders();

        public event ReminderDue OnReminderDue;

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (OnReminderDue == null || RemindersList == null)
                return;

            var reminders = RemindersList.Where(r => r.Date <= DateTime.Now).ToList();
            foreach (var reminder in reminders)
            {
                OnReminderDue(reminder);
            }
        }

        public List<SimpleReminder> GetAllReminders()
        {
            return _storage.GetReminders();
        }

        public void AddReminder(SimpleReminder reminder)
        {
            _storage.SaveReminder(reminder);
        }

        public void RemoveReminder(SimpleReminder reminder)
        {
            _storage.DeleteReminder(reminder);
        }

        public void SaveReminder(SimpleReminder reminder)
        {
            _storage.SaveReminder(reminder);
        }

        public void Clear(bool keepStandaloneReminders)
        {
            var remindersToRemove = new List<SimpleReminder>(RemindersList);
            foreach (var reminder in remindersToRemove)
            {
                if (!keepStandaloneReminders || reminder.NoteID != Guid.Empty)
                {
                    _storage.DeleteReminder(reminder);
                }
            }
        }

        public bool Exists(SimpleReminder sm)
        {
            var existingReminder = RemindersList.FirstOrDefault(rm =>
                rm.NoteID == sm.NoteID &&
                rm.ReminderID == sm.ReminderID &&
                rm.Date.Equals(sm.Date));

            return existingReminder != null;
        }

        public void UpdateReminders(Note n)
        {
            if (n.Archived || n.InTrashCan)
            {
                foreach (var sm in GetAllReminders())
                {
                    if (sm.NoteID == n.ID)
                        RemoveReminder(sm);
                }
                return;
            }

            // NOTE: Kanske är bätte att han ett menyalternativ, typ due today, eller en påminnelse när appen aktiveras och visa alla som är due today.
            //if (n.DueDate > DateTime.Now)
            //{
            //	SimpleReminder sm = new SimpleReminder();
            //	sm.Date = n.DueDate;
            //	sm.Text = n.Name;
            //	sm.NoteID = n.ID;

            //	if (!Hub.Instance.Reminders.Exists(sm))
            //		Hub.Instance.Reminders.AddReminder(sm);
            //}

            if (n.Reminders != null && n.Reminders.Count > 0)
            {
                foreach (var reminder in n.Reminders)
                {
                    UpdateReminders(n, reminder);
                }
            }
        }

        public void UpdateReminders(Note n, Reminder reminder, bool clear = false)
        {
            if (clear)
            {
                RemoveReminders(reminder);
            }

            var reminderDates = DateUtilities.GetReminders(reminder);
            foreach (var date in reminderDates)
            {
                var theDate = date;
                if (theDate < DateTime.Now)
                    continue;

                if ((theDate.Hour == 0 && theDate.Minute == 0) && (reminder.Time.Hour > 0 || reminder.Time.Minute > 0))
                {
                    if (reminder.Time.Hour > 0)
                        theDate = theDate.AddHours(reminder.Time.Hour);

                    if (reminder.Time.Minute > 0)
                        theDate = theDate.AddMinutes(reminder.Time.Minute);
                }

                var sm = new SimpleReminder
                {
                    Text = n.Name,
                    NoteID = n.ID,
                    ReminderID = reminder.ID,
                    Date = theDate
                };

                if (!Hub.Instance.Reminders.Exists(sm))
                    Hub.Instance.Reminders.AddReminder(sm);
            }
        }

        public void RemoveReminders(Reminder reminder)
        {
            var reminders = new List<SimpleReminder>(GetAllReminders());
            foreach (var sm in reminders)
            {
                if (sm.ReminderID == reminder.ID)
                {
                    RemoveReminder(sm);
                }
            }
        }
    }
}