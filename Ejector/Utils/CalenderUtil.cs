using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ejector.Services;

namespace Ejector.Utils.Calender
{
    public class VEvent
    {
        public string Description { get; set; }
        public string Summary { get; set; }
        public string Location { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public override string ToString()
        {
            var nowUtcTimeString = $"{DateTime.UtcNow:yyyyMMdd}T{DateTime.UtcNow:HHmmss}Z";
            var nowLocalTimeString = $"{DateTime.Now:yyyyMMdd}T{DateTime.Now:HHmmss}Z";
            var sb = new StringBuilder();
            sb.Append($"BEGIN:VEVENT\nCLASS:PUBLIC\nCREATED:{nowUtcTimeString}\n");
            if (string.IsNullOrWhiteSpace(Description) == false)
                sb.Append($"DESCRIPTION:{Description}\n");
            sb.Append($"DTEND;TZID=\"China Standard Time\":{EndTime:yyyyMMdd}T{EndTime:HHmmss}\n");
            sb.Append($"DTSTAMP:{nowLocalTimeString}\n");
            sb.Append($"DTSTART;TZID=\"China Standard Time\":{StartTime:yyyyMMdd}T{StartTime:HHmmss}\n");
            sb.Append($"LAST-MODIFIED:{nowLocalTimeString}\n");
            if (string.IsNullOrWhiteSpace(Location) == false)
                sb.Append($"LOCATION:{Location}\n");
            sb.Append($"SEQUENCE:0\nSUMMARY;LANGUAGE=zh-cn:{Summary}\nTRANSP:OPAQUE\nUID:456a6563746f720000{GetHashCode()}\n");
            sb.Append("BEGIN:VALARM\nTRIGGER:-PT15M\nACTION:DISPLAY\nDESCRIPTION:提醒\nEND:VALARM\nEND:VEVENT\n");
            return sb.ToString();
        }

        public override int GetHashCode()
        {
            return Summary.GetHashCode() ^ StartTime.GetHashCode();
        }
    }

    public class VCalendar
    {
        public List<VEvent> VEvents { get; set; }

        public VCalendar()
        {
            VEvents = new List<VEvent>();
        }
        
        public string ToString(string name="Ejector 课程表")
        {
            var sb = new StringBuilder();
            sb.Append($"BEGIN:VCALENDAR\nX-WR-CALNAME:{name}\nX-APPLE-CALENDAR-COLOR:#2BBFF0\nPRODID:-//Azuk Workshop//Ejector 0.1//EN\nVERSION:2.0\nMETHOD:PUBLISH\nBEGIN:VTIMEZONE\nTZID:China Standard Time\nBEGIN:STANDARD\nDTSTART:16010101T000000\nTZOFFSETFROM:+0800\nTZOFFSETTO:+0800\nEND:STANDARD\nEND:VTIMEZONE\n");
            foreach (var vEvent in VEvents)
                sb.Append(vEvent);
            sb.Append("END:VCALENDAR\n");
            return sb.ToString();
        }

        public override string ToString()
        {
            return ToString();
        }
    }

    public static class ZjuClassCalendarParser
    {
        private static bool isEvenWeek(DateTime mondayOfTermBegin, DateTime target)
        {
            return ((target - mondayOfTermBegin).Days / 7) % 2 == 1;
        }
        
        public static List<VEvent> ClassToVEvents(ZjuClass[] classes, TermConfig termConfig, Tweak[] tweaks)
        {
            // Tweak the schedule
            
            var dateLength = (termConfig.End.ToDateTime() - termConfig.Begin.ToDateTime()).Days + 2;
            var shadowDates = new Dictionary<Date, Date>(dateLength);
            var modDescriptions = new Dictionary<Date, string>();
            for (var currentDate = termConfig.Begin; currentDate <= termConfig.End; currentDate++)
                shadowDates.Add(currentDate, currentDate);
            foreach (var tweak in tweaks)
            {
                switch (tweak.TweakType)
                {
                    case TweakType.Clear:
                        for (var d = tweak.From; d <= tweak.To; d++)
                            shadowDates.Remove(d);
                        break;
                    case TweakType.Copy:
                        shadowDates[tweak.To] = tweak.From;
                        modDescriptions[tweak.To] = tweak.Description;
                        break;
                    case TweakType.Exchange:
                        shadowDates[tweak.To] = tweak.From;
                        shadowDates[tweak.From] = tweak.To;
                        modDescriptions[tweak.To] = tweak.Description;
                        modDescriptions[tweak.From] = tweak.Description;
                        break;
                }
            }
            
            // Make cache
            
            var classOfDay = new Dictionary<DayOfWeek, List<ZjuClass>>(new []
            {
                new KeyValuePair<DayOfWeek, List<ZjuClass>>(DayOfWeek.Monday, classes.Where(x => x.DayNumber==1).ToList()), 
                new KeyValuePair<DayOfWeek, List<ZjuClass>>(DayOfWeek.Tuesday, classes.Where(x => x.DayNumber==2).ToList()), 
                new KeyValuePair<DayOfWeek, List<ZjuClass>>(DayOfWeek.Wednesday, classes.Where(x => x.DayNumber==3).ToList()), 
                new KeyValuePair<DayOfWeek, List<ZjuClass>>(DayOfWeek.Thursday, classes.Where(x => x.DayNumber==4).ToList()), 
                new KeyValuePair<DayOfWeek, List<ZjuClass>>(DayOfWeek.Friday, classes.Where(x => x.DayNumber==5).ToList()), 
                new KeyValuePair<DayOfWeek, List<ZjuClass>>(DayOfWeek.Saturday, classes.Where(x => x.DayNumber==6).ToList()), 
                new KeyValuePair<DayOfWeek, List<ZjuClass>>(DayOfWeek.Sunday, classes.Where(x => x.DayNumber==7).ToList()), 
            });

            // Make VEvents
            
            var termBeginDayOfWeek = (int)termConfig.Begin.ToDateTime().DayOfWeek;
            // Fix Sunday
            if (termBeginDayOfWeek == 0) termBeginDayOfWeek = 7;
            var mondayOfFirstWeek = termConfig.Begin.ToDateTime()
                .Subtract(TimeSpan.FromDays(termBeginDayOfWeek - 1))
                .Subtract(TimeSpan.FromDays(7*(termConfig.FirstWeekNo - 1)));
            
            var events = new List<VEvent>(3*dateLength);
            foreach (var shadowDate in shadowDates)
            {
                var actualDate = shadowDate.Key.ToDateTime();
                var dateOfClass = shadowDate.Value.ToDateTime();
                var classesOfCurrentDate = classOfDay[dateOfClass.DayOfWeek];
                var isCurrentDateEvenWeek = isEvenWeek(mondayOfFirstWeek, dateOfClass); 
                foreach (var zjuClass in classesOfCurrentDate)
                {
                    if ((isCurrentDateEvenWeek && zjuClass.WeekArrangement==WeekArrangement.OddOnly) ||
                        (!isCurrentDateEvenWeek && zjuClass.WeekArrangement==WeekArrangement.EvenOnly))
                        continue;
                    
                    events.Add(new VEvent
                    {
                        Summary = zjuClass.ClassName,
                        StartTime = zjuClass.GetStartDateTime(actualDate),
                        EndTime = zjuClass.GetEndDateTime(actualDate),
                        Location = zjuClass.ClassLocation,
                        Description = (modDescriptions.TryGetValue(shadowDate.Key, out var desc)? $"{desc}\\n" : string.Empty) +
                                      $"教师: {zjuClass.TeacherName}\\n课程代码: {zjuClass.ClassCode}\\n教学时间安排: {zjuClass.ArrangementDescription()}",
                    });
                }
            }

            return events;
        }
    }
}