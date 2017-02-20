using Noterium.Core.Services;

namespace Noterium.Core
{
    public class GoogleCalendarReminderManager
    {
        private readonly GoogleCalendar _gc;
        //private readonly Queue<QueueItem> _queue = new Queue<QueueItem>();
        //private readonly object _queueLock = new object();
        //private readonly CancellationTokenSource _token;

        //public delegate void QueueItemEvent(QueueItem item);
        //public event QueueItemEvent OnQueueItemHandle;

        //public event QueueItemEvent OnQueueItemHandled;

        //public QueueItem CurrentItem { get; private set; }

        //public Task Task { get; private set; }

        public GoogleCalendarReminderManager(ref GoogleCalendar gc)
        {
            _gc = gc;

            //_token = new CancellationTokenSource();
            //Task = Task.Factory.StartNew(async () =>
            //	{
            //		while (true)
            //		{
            //			QueueItem item = null;
            //			lock (_queueLock)
            //			{
            //				if (_queue.Count > 0)
            //					item = _queue.Dequeue();
            //			}

            //			if (item != null)
            //				HandleQueueItem(item);

            //			if (_token.IsCancellationRequested)
            //			{
            //				// TODO: Save queue to disk.
            //			}

            //			await Task.Delay(1000, _token.Token);
            //		}
            //	}, _token.Token);
        }

        //{

        //private void HandleQueueItem(QueueItem item)
        //	CurrentItem = item;

        //	if (OnQueueItemHandle != null)
        //		OnQueueItemHandle(item);

        //	if (!item.RemoveAll)
        //	{
        //		foreach (Reminder reminder in item.Note.Reminders)
        //		{
        //			if (string.IsNullOrWhiteSpace(reminder.CalendarEventId))
        //			{
        //				_gc.AddReminder(reminder);
        //			}
        //			else
        //			{
        //				if (reminder.Changed > reminder.Updated)
        //					_gc.UpdateReminder(reminder);
        //			}
        //		}

        //		foreach (Reminder removedReminder in item.RemovedReminders)
        //		{
        //			_gc.RemoveReminder(removedReminder);
        //		}

        //		item.Note.Save();
        //	}
        //	else
        //	{
        //		foreach (Reminder removedReminder in item.Note.Reminders)
        //		{
        //			_gc.RemoveReminder(removedReminder);
        //		}
        //	}

        //	if (OnQueueItemHandled != null)
        //		OnQueueItemHandled(item);

        //	CurrentItem = null;
        //}

        //public void Enqueue(Note note, List<Reminder> removedReminders)
        //{
        //	_queue.Enqueue(new QueueItem(note, removedReminders));
        //}

        //public void Enqueue(Note note)
        //{
        //	_queue.Enqueue(new QueueItem(note, null) { RemoveAll = true });
        //}

        //public class QueueItem
        //{
        //	public Note Note { get; set; }
        //	public List<Reminder> RemovedReminders { get; set; }

        //	public bool RemoveAll { get; set; }

        //	internal QueueItem(Note note, List<Reminder> removedReminders)
        //	{
        //		Note = note;
        //		RemovedReminders = removedReminders;
        //	}
        //}

        //public void ShutDown()
        //{
        //	_token.Cancel();
        //}
    }
}