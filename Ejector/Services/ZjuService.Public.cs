using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Ejector.Utils.Calender;

namespace Ejector.Services
{
    public enum ClassTerm
    {
        Autumn,
        Winter,
        ShortA,
        SummerVacation,
        Spring,
        Summer,
        ShortB,
    }

    public enum ExamTerm
    {
        AutumnWinter,
        SpringSummer
    }

    public enum WeekArrangement
    {
        Normal,
        OddOnly,
        EvenOnly
    }
    
    public static class TermExtension
    {
        public static string ToQueryString(this ClassTerm t)
        {
            return t switch
            {
                ClassTerm.Autumn => "1|秋",
                ClassTerm.Winter => "1|冬",
                ClassTerm.ShortA => "1|短",
                ClassTerm.SummerVacation => "1|暑",
                ClassTerm.Spring => "2|春",
                ClassTerm.Summer => "2|夏",
                ClassTerm.ShortB => "2|短",
                _ => string.Empty
            };
        }

        public static string ToQueryString(this ExamTerm t)
        {
            return t switch
            {
                ExamTerm.AutumnWinter => "1",
                ExamTerm.SpringSummer => "0",
                _ => string.Empty
            };
        }
    }

    public struct ClassPeriod
    {
        private readonly short Hour;
        private readonly short Minute;

        public ClassPeriod(short PeriodNumber)
        {
            switch (PeriodNumber)
            {
                case  1: Hour =  8; Minute =  0; break;
                case  2: Hour =  8; Minute = 50; break;
                case  3: Hour =  9; Minute = 50; break;
                case  4: Hour = 10; Minute = 40; break;
                case  5: Hour = 11; Minute = 30; break;
                case  6: Hour = 13; Minute = 15; break;
                case  7: Hour = 14; Minute =  5; break;
                case  8: Hour = 14; Minute = 55; break;
                case  9: Hour = 15; Minute = 55; break;
                case 10: Hour = 16; Minute = 45; break;
                case 11: Hour = 18; Minute = 30; break;
                case 12: Hour = 19; Minute = 20; break;
                case 13: Hour = 20; Minute = 10; break;
                default: Hour = 21; Minute =  0; break;
            }
        }

        public DateTime ToStartDateTime(DateTime day)
        {
            return new DateTime(day.Year, day.Month, day.Day, Hour, Minute, 0);
        }

        public DateTime ToEndDateTime(DateTime day)
        {
            return ToStartDateTime(day).AddMinutes(45);
        }
    }

    public class ZjuExamOutline
    {
        [JsonPropertyName("kcid")] public string ClassIdWithStuId { get; set; }
        [JsonPropertyName("kcmc")] public string ClassName { get; set; }
        // 2020年01月01日(08:00-18:00)
        [JsonPropertyName("qmkssj")] public string FinalExamAgenda { get; set; }
        [JsonPropertyName("qmksdd")] public string FinalExamLocation { get; set; }
        [JsonPropertyName("zwxh")] public string FinalExamSeatNum { get; set; }
        [JsonPropertyName("qzkssj")] public string MidtermExamAgenda { get; set; }
        [JsonPropertyName("qzksdd")] public string MidtermExamLocation { get; set; }
        [JsonPropertyName("qzzwxh")] public string MidtermExamSeatNum { get; set; }
        [JsonPropertyName("xkkh")] public string ClassId { get; set; }
        [JsonPropertyName("xkxf")] public string ClassCredit { get; set; }
        [JsonPropertyName("xq")] public string ClassTerm { get; set; }

        public List<VEvent> ToVEventList()
        {
            var vEvents = new List<VEvent>();

            if (!string.IsNullOrWhiteSpace(FinalExamAgenda))
            {
                var finalEventStart = new DateTime(
                    int.Parse(FinalExamAgenda[..4]),
                    int.Parse(FinalExamAgenda[5..7]),
                    int.Parse(FinalExamAgenda[8..10]),
                    int.Parse(FinalExamAgenda[12..14]),
                    int.Parse(FinalExamAgenda[15..17]), 0);
                var finalEventEnd = new DateTime(
                    finalEventStart.Year, finalEventStart.Month, finalEventStart.Day,
                    int.Parse(FinalExamAgenda[18..20]), int.Parse(FinalExamAgenda[21..23]), 0);
                var finalEvent = new VEvent
                {
                    Summary = $"{ClassName} 期末考试",
                    Location = string.IsNullOrWhiteSpace(FinalExamLocation) ? string.Empty : FinalExamLocation,
                    Description =
                        string.IsNullOrWhiteSpace(FinalExamSeatNum) ? string.Empty : $"座位号：{FinalExamSeatNum}",
                    StartTime = finalEventStart,
                    EndTime = finalEventEnd
                };
                vEvents.Add(finalEvent);
            }

            if (!string.IsNullOrWhiteSpace(MidtermExamAgenda))
            {
                var midTermEventStart = new DateTime(
                    int.Parse(MidtermExamAgenda[..4]),
                    int.Parse(MidtermExamAgenda[5..7]),
                    int.Parse(MidtermExamAgenda[8..10]),
                    int.Parse(MidtermExamAgenda[12..14]),
                    int.Parse(MidtermExamAgenda[15..17]), 0);
                var midTermEventEnd = new DateTime(
                    midTermEventStart.Year, midTermEventStart.Month, midTermEventStart.Day,
                    int.Parse(MidtermExamAgenda[18..20]), int.Parse(MidtermExamAgenda[21..23]), 0);
                var midTermEvent = new VEvent
                {
                    Summary = $"{ClassName} 期末考试",
                    Location = string.IsNullOrWhiteSpace(MidtermExamLocation) ? string.Empty : MidtermExamLocation,
                    Description =
                        string.IsNullOrWhiteSpace(MidtermExamSeatNum) ? string.Empty : $"座位号：{MidtermExamSeatNum}",
                    StartTime = midTermEventStart,
                    EndTime = midTermEventEnd
                };
                vEvents.Add(midTermEvent);
            }

            return vEvents;
        }
    }

    public class ZjuClass
    {
        public WeekArrangement WeekArrangement { get; set; }
        public int StartPeriod { get; set; }
        public int EndPeriod { get; set; }
        public string TeacherName { get; set; }
        public string ClassCode { get; set; }
        public string ClassName { get; set; }
        public string ClassLocation { get; set; }
        public List<ClassTerm> TermArrangements { get; set; }
        public int DayNumber { get; set; }
        public int ClassYear { get; set; }

        public ZjuClass()
        {
            TermArrangements = new List<ClassTerm>();
        }
    }

    public struct Date
    {
        public readonly int Year;
        public readonly int Month;
        public readonly int Day;

        public Date(int year, int month, int day)
        {
            Year = year;
            Month = month;
            Day = day;
        }

        public Date(DateTime d)
        {
            Year = d.Year;
            Month = d.Month;
            Day = d.Day;
        }

        public DateTime ToDateTime()
        {
            return new DateTime(Year, Month, Day);
        }

        public Date NextDayDate()
        {
            return new Date(ToDateTime().AddDays(1));
        }
        
        public override bool Equals(object? obj)
        {
            if (obj is Date d)
                return d.Year == Year && d.Month == Month && d.Day == Day;
            return false;
        }

        public override int GetHashCode()
        {
            return Year * 10000 + Month * 100 + Day;
        }
    }
    
    public class TermInfo
    {
        public ClassTerm term;
        public Date StartDate;
        public Date EndDate;
    }
}