using System;
using System.Collections.Generic;
using System.Text;

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
        
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(@"BEGIN:VCALENDAR
X-WR-CALNAME:Ejector 课程表
X-APPLE-CALENDAR-COLOR:#2BBFF0
PRODID:-//Azuk Workshop//Ejector 0.1//EN
VERSION:2.0
METHOD:PUBLISH
BEGIN:VTIMEZONE
TZID:China Standard Time
BEGIN:STANDARD
DTSTART:16010101T000000
TZOFFSETFROM:+0800
TZOFFSETTO:+0800
END:STANDARD
END:VTIMEZONE
");
            foreach (var vEvent in VEvents)
                sb.Append(vEvent);
            sb.Append("END:VCALENDAR\n");
            return sb.ToString();
        }
    }
}