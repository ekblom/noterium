using System;
using System.Collections.Generic;
using Noterium.Core.Constants;

namespace Noterium.Core.DataCarriers
{
    public class Recurrence
    {
        public Frequency Frequency { get; set; }


        /*
			INTERVAL: Specifies the interval between 2 occurrences of the event. e.g. FREQ=DAILY and INTERVAL=5 means that the event occurs every 5th day.
			A very important point to note is that if user selected an Interval of 1 (for any frequency), 
			this Interval attribute would be absent from the Recurrence string. 
			So, you can safely assume a default of 1 if Interval is absent from the string.
		 */
        public int Interval { get; set; }

        /*
			COUNT: The number of times the event recurs.
			e.g. FREQ=WEEKLY;INTERVAL=2;COUNT=3 means the event recurs every second week for 3 times (i.e. in total 6 weeks but every second week).
		 */
        public int Count { get; set; }


        /*
			UNTIL: The end date for the event. The event recurs only until this date.
			Note that COUNT and UNTIL properties are mutually exclusive. 
			Only one of them can be specified  (which makes sense, COUNT means the event occurs this many times, UNTIL means the event recurs until this date).
		 */
        public DateTime Until { get; set; }

        public string GetRuleString(DateTime date)
        {
            var temp = new List<string>();
            if (Frequency == Frequency.Daily)
            {
                temp.Add("FREQ=DAILY");
            }
            else if (Frequency == Frequency.Monthly)
            {
                temp.Add("FREQ=MONTHLY");
                temp.Add("BYMONTHDAY=" + date.Day);
            }
            else if (Frequency == Frequency.Weekly)
            {
                // "RRULE:FREQ=WEEKLY;UNTIL=20101012T165959Z;INTERVAL=4;BYDAY=TU"
                temp.Add("FREQ=WEEKLY");
                temp.Add("BYDAY=" + GetDayString(date.DayOfWeek));
            }
            else if (Frequency == Frequency.Yearly)
            {
                // "RRULE:FREQ=YEARLY;BYMONTH=11;BYMONTHDAY=14"
                temp.Add("FREQ=YEARLY");
                temp.Add("BYMONTH=" + date.Month);
                temp.Add("BYMONTHDAY=" + date.Day);
            }

            if (Count != 0)
            {
                temp.Add("COUNT=" + Count);
            }
            else if (Until != DateTime.MinValue)
            {
                temp.Add("UNTIL=" + Until.ToUniversalTime().ToString("yyyyMMdd'T'HHmmssK"));
            }


            //FrequencyType ft = GetFrequencyType();
            //RecurrencePattern ptn = new RecurrencePattern(ft);

            //if (ft == FrequencyType.Daily)
            //{
            //	//ptn.ByDay.Add(new WeekDay(date.DayOfWeek));
            //	ptn.ByHour.Add();
            //}

            //ptn.Until = Until;

            return "RRULE:" + string.Join(";", temp.ToArray());
        }

        private string GetDayString(DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    return "MO";
                case DayOfWeek.Tuesday:
                    return "TU";
                case DayOfWeek.Wednesday:
                    return "WE";
                case DayOfWeek.Thursday:
                    return "TH";
                case DayOfWeek.Friday:
                    return "FR";
                case DayOfWeek.Saturday:
                    return "SA";
                case DayOfWeek.Sunday:
                    return "SU";
            }
            return string.Empty;
        }
    }

    /*
	 
		Now we have properties specific for each FREQ type.

		DAILY
		This frequency does not have any specific property and only supports the above properties common for all frequency types.

		WEEKLY
		BYDAY: This property specifies the days of the week the event should recur.
		e.g. FREQ=WEEKLY;INTERVAL=3;BYDAY=MO,WE;COUNT=5 means that the event recurs every 3rd week on Monday and Wednesday 
		and it has 5 recurrences spanning over 15 weeks (notice the interval is 3 meaning every 3rd week, so the total span is 15 weeks for COUNT=5).
		If BYDAY is absent for a WEEKLY frequency, you can interpret that to mean for all days of the week. If specified, 
		the value for this property for WEEKLY frequency would be a comma-separated list of first 2 characters for a week day 
		corresponding to the days chosen on the UI.

		MONTHLY
		For this frequency, you can choose if the event recurs on a fixed day of the month or a specified day of 
		the specified week of the month. Accordingly, you have 2 mutually exclusive properties for this frequency.
		BYMONTHDAY: This property means that the event recurs on a fixed day of the month.
		e.g. FREQ=MONTHLY;INTERVAL=4;BYMONTHDAY=20;COUNT=5 means that the event recurs every 4 months, on 20th of each 
		month and recurring for 5 times (spanning over 20 months).
	
		BYDAY: This property means that he event recurs on a fixed day of the fixed week of the month.
		e.g. FREQ=MONTHLY;INTERVAL=4;BYDAY=3MO;COUNT=5 means that the event recurs on 3rd Monday of every 4th month for 5 times.
		Notice BYDAY here contains 2 parts, the first being the week of the month (between 1 and 5), 
		and the second the day of the week (first 2 characters of the week day).
	
		YEARLY
		This frequency recurrence rule is a bit confusing containing a required and an optional property.
		The required property is BYMONTH (containing the index of the month) and the optional is BYDAY. 
		The interpretation of BYMONTH can vary slightly by the absence or presence of the BYDAY property. 
		Let's take an example first before discussing these 2 properties.

		i) FREQ=YEARLY;BYMONTH=12;COUNT=5
			means that the event occurs every year in the month of December (BYMONTH=12 implies the month of December). 
			The date of the month is decided by the start date of the event. If the event start date is 12th December, 
			then the above rule means that the event occurs on 12th December of each year for 5 years.

		ii) FREQ=YEARLY;BYMONTH=12;BYDAY=3MO;COUNT=5
			means that the event occurs on 3rd Monday in the month of December each year for 5 years.

		Here's a more official description of the above properties:
		BYMONTH: This property specifies the month of the year (between 1 and 12). 
				IF BYDAY is absent, then the day of the month is decided by the start date of the event in the specified month.
		BYDAY: This property means that the event recurs in the fixed day of the fixed week of the month specified by BYMONTH property. 
				It again consists of 2 parts week of the month and day of the week (e.g. 3MO meaning 3rd Monday).
		As another example, BYMONTH=5;BYDAY=2TU means the event recurs on 2nd Tueday of the month of May.
	 
	 */
}