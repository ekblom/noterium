using log4net;

namespace Noterium.Core.Services
{
    public class GoogleCalendar
    {
        private readonly ILog _log = LogManager.GetLogger(typeof (GoogleCalendar));

        //private bool _initialized;
        //static CalendarService _service;
        //static IList<string> _scopes = new List<string>();
        //private IList<CalendarListEntry> _calendarList;
        private Settings _settings;
        //UserCredential _credential = default(UserCredential);

        internal GoogleCalendar(ref Settings settings)
        {
            _settings = settings;
        }

        //	get
        //{

        //public IList<CalendarListEntry> Calendars
        //	{
        //		Authenticate();

        //		if (_calendarList == null)
        //			_calendarList = _service.CalendarList.List().Execute().Items;

        //		return _calendarList;
        //	}
        //}

        //public void Authenticate()
        //{
        //	if (!_initialized)
        //	{
        //		_scopes.Add(CalendarService.Scope.Calendar);

        //		using (Stream stream = new MemoryStream(Properties.Resources.google_client_secrets))
        //		{
        //			GoogleClientSecrets temp = GoogleClientSecrets.Load(stream);
        //			ClientSecrets secrets = temp.Secrets;

        //			FileDataStore fds = new FileDataStore("Noterium.Google.Data");

        //			_credential = GoogleWebAuthorizationBroker.AuthorizeAsync(secrets, _scopes, "user", CancellationToken.None, fds).Result;
        //		}

        //		// Create the calendar service using an initializer instance
        //		BaseClientService.Initializer initializer = new BaseClientService.Initializer();
        //		initializer.HttpClientInitializer = _credential;
        //		initializer.ApplicationName = "Noterium";
        //		_service = new CalendarService(initializer);

        //		_initialized = true;
        //	}
        //}

        //public bool RemoveAuthentication()
        //{
        //	if (_initialized)
        //	{
        //		return _credential.RevokeTokenAsync(CancellationToken.None).Result;
        //	}
        //	return true;
        //}

        //public bool AddReminder(Reminder m)
        //{
        //	try
        //	{
        //		Authenticate();
        //		string calendarId = _settings.GoogleCalendarId;
        //		Event e = GetEventBody(m);

        //		var request = _service.Events.Insert(e, calendarId);
        //		Event createdEvent = request.Execute();
        //		m.CalendarId = calendarId;
        //		m.CalendarEventId = createdEvent.Id;
        //		m.Created = DateTime.Now;
        //	}
        //	catch (Exception e)
        //	{
        //		_log.Error(e.Message, e);
        //		return false;
        //	}

        //	return true;
        //}

        //private Event GetEventBody(Reminder reminder, Event existingEvent = null)
        //{
        //	//     "RRULE:FREQ=WEEKLY;UNTIL=20110701T100000-07:00",
        //	// Important: The start.timeZone and end.timeZone elements are required and must be equal when inserting recurring events.

        //	string rrule = string.Empty;
        //	if (reminder.Recurrence != null)
        //	{
        //		rrule = reminder.Recurrence.GetRuleString(reminder.Time);
        //	}

        //	Event e = existingEvent ?? new Event() { Sequence = 1 };

        //	e.Transparency = "transparent";

        //	if (string.IsNullOrWhiteSpace(reminder.CustomEventName))
        //		e.Summary = reminder.Description;
        //	else
        //		e.Summary = reminder.CustomEventName;

        //	e.Visibility = "private";
        //	e.Status = "confirmed";

        //	DateTime date = reminder.Time;

        //	if (reminder.AllDayEvent)
        //	{
        //		e.Start = new EventDateTime { Date = date.ToString("yyyy-MM-dd") };
        //		e.End = new EventDateTime { Date = date.ToString("yyyy-MM-dd") };
        //	}
        //	else
        //	{
        //		e.Start = new EventDateTime { DateTime = date };
        //		e.End = new EventDateTime { DateTime = date.AddMinutes(_settings.CalendarEventLength) };
        //	}

        //	if (!string.IsNullOrWhiteSpace(rrule))
        //	{
        //		e.Recurrence = new List<string>();
        //		e.Recurrence.Add(rrule);
        //		e.Start.TimeZone = "Europe/Zurich";
        //		e.End.TimeZone = "Europe/Zurich";
        //	}

        //	e.Reminders = new Event.RemindersData();

        //	if (!_settings.UseGoogleDefaultReminder)
        //	{
        //		if (!reminder.AllDayEvent)
        //		{
        //			e.Reminders.Overrides = new List<EventReminder>();
        //			e.Reminders.Overrides.Add(new EventReminder { Method = "popup", Minutes = 0 });
        //			e.Reminders.UseDefault = false;
        //		}
        //	}
        //	else
        //	{
        //		e.Reminders.UseDefault = true;
        //	}
        //	return e;
        //}

        //public bool UpdateReminder(Reminder m)
        //{
        //	try
        //	{
        //		Authenticate();

        //		var eventt = _service.Events.Get(m.CalendarId, m.CalendarEventId);
        //		var existingEvent = eventt.Execute();
        //		if (existingEvent.Sequence != null)
        //			existingEvent.Sequence++;

        //		Event e = GetEventBody(m, existingEvent);
        //		var request = _service.Events.Update(e, m.CalendarId, m.CalendarEventId);
        //		Event evt = request.Execute();
        //		m.Updated = DateTime.Now;
        //		return true;
        //	}
        //	catch (Exception e)
        //	{
        //		_log.Error(e.Message, e);
        //		return false;
        //	}
        //}

        //public bool RemoveReminder(Reminder m)
        //{
        //	try
        //	{
        //		Authenticate();
        //		var request = _service.Events.Delete(m.CalendarId, m.CalendarEventId);
        //		string result = request.Execute();
        //		return string.IsNullOrEmpty(result);
        //	}
        //	catch (Exception e)
        //	{
        //		_log.Error(e.Message, e);
        //		return false;
        //	}
        //}

        //public void Test()
        //{
        //	Authenticate();


        //	// Display all calendars
        //	//DisplayList(list);
        //	foreach (CalendarListEntry calendar in _calendarList)
        //	{
        //		if (calendar.AccessRole == "owner")
        //		{
        //			Debug.WriteLine(calendar.Summary + ": " + calendar.Id);
        //			DisplayFirstCalendarEvents(calendar);
        //		}
        //	}
        //}

        ///// <summary>Displays all calendars.</summary>
        //private static void DisplayList(IList<CalendarListEntry> list)
        //{
        //	Console.WriteLine("Lists of calendars:");
        //	foreach (CalendarListEntry item in list)
        //	{
        //		Debug.WriteLine(item.Summary + ". Location: " + item.Location + ", TimeZone: " + item.TimeZone);
        //	}
        //}

        ///// <summary>Displays the calendar's events.</summary>
        //private static void DisplayFirstCalendarEvents(CalendarListEntry list)
        //{
        //	Console.WriteLine(Environment.NewLine + "Maximum 5 first events from {0}:", list.Summary);
        //	Google.Apis.Calendar.v3.EventsResource.ListRequest requeust = _service.Events.List(list.Id);
        //	// Set MaxResults and TimeMin with sample values
        //	requeust.MaxResults = 50;
        //	requeust.TimeMin = new DateTime(2014, 10, 1, 20, 0, 0);
        //	// Fetch the list of events
        //	foreach (Event calendarEvent in requeust.Execute().Items)
        //	{
        //		string startDate = "Unspecified";
        //		if (((calendarEvent.Start != null)))
        //		{
        //			if (((calendarEvent.Start.Date != null)))
        //			{
        //				startDate = calendarEvent.Start.Date.ToString();
        //			}
        //		}

        //		Debug.WriteLine(calendarEvent.Summary + ". Id: " + calendarEvent.Id);
        //	}
        //}
    }
}