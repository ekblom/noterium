using System;
using System.Collections.Generic;
using Noterium.Core.Constants;
using Noterium.Core.DataCarriers;
using ScheduleWidget.Enums;
using ScheduleWidget.ScheduledEvents;

namespace Noterium.Core.Utilities
{
    public static class DateUtilities
    {
        public static List<DateTime> GetReminderDates(Note n)
        {
            var result = new List<DateTime>();
            if (n.Archived)
                return result;

            if (n.Reminders.Count == 0 && n.DueDate == DateTime.MinValue)
                return result;

            if (n.DueDate != DateTime.MinValue)
                result.Add(n.DueDate);

            foreach (var rm in n.Reminders)
            {
                if (rm.Recurrence != null)
                {
                    var ev = new Event();

                    var range = new DateRange
                    {
                        StartDateTime = rm.Time,
                        EndDateTime = DateTime.Now.AddDays(30)
                    };

                    if (rm.Recurrence.Frequency == Frequency.Yearly)
                    {
                        //DateTime date = new DateTime(DateTime.Now.Year, rm.Time.Month, rm.Time.Day, rm.Time.Hour, rm.Time.Minute, rm.Time.Second);
                        //AddCalendarItem(n, date, n.Name);
                        ev.FrequencyTypeOptions = FrequencyTypeEnum.Yearly;
                        ev.Anniversary = new Anniversary {Day = rm.Time.Day, Month = rm.Time.Month};
                    }
                    else if (rm.Recurrence.Frequency == Frequency.Monthly)
                    {
                        //DateTime date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, rm.Time.Day, rm.Time.Hour, rm.Time.Minute, rm.Time.Second);
                        //AddCalendarItem(n, date, n.Name);
                        ev.FrequencyTypeOptions = FrequencyTypeEnum.Monthly;
                    }
                    else if (rm.Recurrence.Frequency == Frequency.Weekly)
                    {
                        //DateTime date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, rm.Time.Day, rm.Time.Hour, rm.Time.Minute, rm.Time.Second);

                        //int dayOfWeekNow = (int)date.DayOfWeek;
                        //int dayOfWeekReminder = (int)rm.Time.DayOfWeek;
                        //DateTime firstDate =date;// date.AddDays(dayOfWeekNow - dayOfWeekReminder);
                        //while(firstDate <= CalendarControl.ViewEnd)
                        //{
                        //	AddCalendarItem(n, firstDate, n.Name);
                        //	firstDate = firstDate.AddDays(7);
                        //}
                        ev.FrequencyTypeOptions = FrequencyTypeEnum.Weekly;
                        switch (rm.Time.DayOfWeek)
                        {
                            case DayOfWeek.Monday:
                                ev.DaysOfWeekOptions = DayOfWeekEnum.Mon;
                                break;
                            case DayOfWeek.Tuesday:
                                ev.DaysOfWeekOptions = DayOfWeekEnum.Tue;
                                break;
                            case DayOfWeek.Wednesday:
                                ev.DaysOfWeekOptions = DayOfWeekEnum.Wed;
                                break;
                            case DayOfWeek.Thursday:
                                ev.DaysOfWeekOptions = DayOfWeekEnum.Thu;
                                break;
                            case DayOfWeek.Friday:
                                ev.DaysOfWeekOptions = DayOfWeekEnum.Fri;
                                break;
                            case DayOfWeek.Saturday:
                                ev.DaysOfWeekOptions = DayOfWeekEnum.Sat;
                                break;
                            case DayOfWeek.Sunday:
                                ev.DaysOfWeekOptions = DayOfWeekEnum.Sun;
                                break;
                        }
                    }
                    else if (rm.Recurrence.Frequency == Frequency.Daily)
                    {
                        ev.FrequencyTypeOptions = FrequencyTypeEnum.Daily;
                    }

                    var schedule = new Schedule(ev);
                    result.AddRange(schedule.Occurrences(range));
                }
                else
                {
                    result.Add(rm.Time);
                }
            }

            return result;
        }

        public static List<DateTime> GetReminders(Reminder rm)
        {
            var result = new List<DateTime>();
            if (rm.Recurrence != null)
            {
                var ev = new Event();

                var range = new DateRange
                {
                    StartDateTime = rm.Time,
                    EndDateTime = DateTime.Now.AddDays(30)
                };

                if (rm.Recurrence.Frequency == Frequency.Yearly)
                {
                    //DateTime date = new DateTime(DateTime.Now.Year, rm.Time.Month, rm.Time.Day, rm.Time.Hour, rm.Time.Minute, rm.Time.Second);
                    //AddCalendarItem(n, date, n.Name);
                    ev.FrequencyTypeOptions = FrequencyTypeEnum.Yearly;
                    ev.Anniversary = new Anniversary {Day = rm.Time.Day, Month = rm.Time.Month};
                    range.EndDateTime = DateTime.Now.AddYears(5);
                }
                else if (rm.Recurrence.Frequency == Frequency.Monthly)
                {
                    //DateTime date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, rm.Time.Day, rm.Time.Hour, rm.Time.Minute, rm.Time.Second);
                    //AddCalendarItem(n, date, n.Name);
                    ev.FrequencyTypeOptions = FrequencyTypeEnum.Monthly;
                    range.EndDateTime = DateTime.Now.AddMonths(6);
                }
                else if (rm.Recurrence.Frequency == Frequency.Weekly)
                {
                    //DateTime date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, rm.Time.Day, rm.Time.Hour, rm.Time.Minute, rm.Time.Second);

                    //int dayOfWeekNow = (int)date.DayOfWeek;
                    //int dayOfWeekReminder = (int)rm.Time.DayOfWeek;
                    //DateTime firstDate =date;// date.AddDays(dayOfWeekNow - dayOfWeekReminder);
                    //while(firstDate <= CalendarControl.ViewEnd)
                    //{
                    //	AddCalendarItem(n, firstDate, n.Name);
                    //	firstDate = firstDate.AddDays(7);
                    //}
                    ev.FrequencyTypeOptions = FrequencyTypeEnum.Weekly;
                    switch (rm.Time.DayOfWeek)
                    {
                        case DayOfWeek.Monday:
                            ev.DaysOfWeekOptions = DayOfWeekEnum.Mon;
                            break;
                        case DayOfWeek.Tuesday:
                            ev.DaysOfWeekOptions = DayOfWeekEnum.Tue;
                            break;
                        case DayOfWeek.Wednesday:
                            ev.DaysOfWeekOptions = DayOfWeekEnum.Wed;
                            break;
                        case DayOfWeek.Thursday:
                            ev.DaysOfWeekOptions = DayOfWeekEnum.Thu;
                            break;
                        case DayOfWeek.Friday:
                            ev.DaysOfWeekOptions = DayOfWeekEnum.Fri;
                            break;
                        case DayOfWeek.Saturday:
                            ev.DaysOfWeekOptions = DayOfWeekEnum.Sat;
                            break;
                        case DayOfWeek.Sunday:
                            ev.DaysOfWeekOptions = DayOfWeekEnum.Sun;
                            break;
                    }
                }
                else if (rm.Recurrence.Frequency == Frequency.Daily)
                {
                    ev.FrequencyTypeOptions = FrequencyTypeEnum.Daily;
                }

                var schedule = new Schedule(ev);
                result.AddRange(schedule.Occurrences(range));
            }
            else
            {
                result.Add(rm.Time);
            }
            return result;
        }
    }
}