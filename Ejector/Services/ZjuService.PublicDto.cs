using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Ejector.Utils.Calender;

namespace Ejector.Services
{
    public partial class ZjuService
    {
        public class ZjuExamOutline
        {
            [JsonPropertyName("kcid")]
            public string ClassIdWithStuId { get; set; }
            [JsonPropertyName("kcmc")]
            public string ClassName { get; set; }
            [JsonPropertyName("qmkssj")]
            // 2020年01月01日(08:00-18:00)
            public string FinalExamAgenda { get; set; }
            [JsonPropertyName("qmksdd")]
            public string FinalExamPos { get; set; }
            [JsonPropertyName("zwxh")]
            public string FinalExamSeatNum { get; set; }
            [JsonPropertyName("qzkssj")]
            public string MidtermExamAgenda { get; set; }
            [JsonPropertyName("qzksdd")]
            public string MidtermExamPos { get; set; }
            [JsonPropertyName("qzzwxh")]
            public string MidtermExamSeatNum { get; set; }
            [JsonPropertyName("xkkh")]
            public string ClassId { get; set; }
            [JsonPropertyName("xkxf")]
            public string ClassCredit { get; set; }
            [JsonPropertyName("xq")]
            public string ClassTerm { get; set; }

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
                        Location = string.IsNullOrWhiteSpace(FinalExamPos) ? string.Empty : FinalExamPos,
                        Description = string.IsNullOrWhiteSpace(FinalExamSeatNum) ? string.Empty : $"座位号：{FinalExamSeatNum}",
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
                        Location = string.IsNullOrWhiteSpace(FinalExamPos) ? string.Empty : FinalExamPos,
                        Description = string.IsNullOrWhiteSpace(FinalExamSeatNum) ? string.Empty : $"座位号：{FinalExamSeatNum}",
                        StartTime = midTermEventStart,
                        EndTime = midTermEventEnd
                    }; 
                    vEvents.Add(midTermEvent);
                }

                return vEvents;
            }
        } 
    }
}