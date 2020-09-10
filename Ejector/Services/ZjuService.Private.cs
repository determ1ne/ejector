using System.Linq;
using System.Text.Json.Serialization;

namespace Ejector.Services
{
    public partial class ZjuService
    {
        private class ZjuamPubKey
        {
            [JsonPropertyName("modulus")]
            public string Modulus { get; set; }
            [JsonPropertyName("exponent")]
            public string Exponent { get; set; }
        } 
        
        private class ZjuResWrapperNum<T>
        {
            [JsonPropertyName("data")]
            public T Data { get; set; }
            [JsonPropertyName("error_code")]           
            public int ErrorCode { get; set; }
            [JsonPropertyName("message")]
            public string Message { get; set; }

            public bool IsSuccess => ErrorCode==0 && Message.Contains("请求成功");
        }
        
        private class ZjuResWrapperStr<T>
        {
            [JsonPropertyName("data")]
            public T Data { get; set; }
            [JsonPropertyName("error_code")]           
            public string ErrorCode { get; set; }
            [JsonPropertyName("message")]
            public string Message { get; set; }

            public bool IsSuccess => ErrorCode=="0" && Message.Contains("success");
        }
        
        private class ZjuUserInfoRes
        {
            [JsonPropertyName("yhm")]
            public string Id { get; set; }
            [JsonPropertyName("zt")]
            public string Zt { get; set; }
        }
        
        private class ZjuExamOutlineRes
        {
            [JsonPropertyName("list")]
            public ZjuExamOutline[] ExamOutlineList { get; set; }
            [JsonPropertyName("zt")]
            public string Zt { get; set; }
        }

        private class ZjuWeeklyScheduleRes
        {
            // Not parsing unused parameter here
            [JsonPropertyName("kblist")]
            public ZjuWeeklyScheduleClass[] ClassList { get; set; }
        }

        private class ZjuWeeklyScheduleClass
        {
            [JsonPropertyName("dsz")]
            public string WeekArrangement { get; set; }
            [JsonPropertyName("jc")]
            public string[] Periods { get; set; }
            [JsonPropertyName("jsxm")]
            public string TeacherName { get; set; }
            [JsonPropertyName("kcdm")]
            public string ClassCode { get; set; }
            [JsonPropertyName("kcid")]
            public string ClassId { get; set; }
            [JsonPropertyName("mc")]
            public string ClassName { get; set; }
            [JsonPropertyName("skdd")]
            public string ClassLocation { get; set; }
            [JsonPropertyName("xq")]
            public string TermArrangement { get; set; }
            [JsonPropertyName("xqj")]
            public int DayNumber { get; set; }
            [JsonPropertyName("sfqd")]
            public int IsConfirmed { get; set; }

            public ZjuClass ToZjuClass()
            {
                if (IsConfirmed == 0) return null;
                var zjuClass = new ZjuClass();
               
                if (TermArrangement.Contains("秋"))
                    zjuClass.TermArrangements.Add(ClassTerm.Autumn);
                if (TermArrangement.Contains("冬"))
                    zjuClass.TermArrangements.Add(ClassTerm.Winter);
                if (TermArrangement.Contains("春"))
                    zjuClass.TermArrangements.Add(ClassTerm.Spring);
                if (TermArrangement.Contains("夏"))
                    zjuClass.TermArrangements.Add(ClassTerm.Summer);
                if (TermArrangement.Length == 0)
                    return null;

                if (!int.TryParse(ClassId[1..5], out var year))
                    return null;

                zjuClass.ClassYear = year;
                zjuClass.WeekArrangement = WeekArrangement switch
                {
                    "0" => Services.WeekArrangement.OddOnly,
                    "1" => Services.WeekArrangement.EvenOnly,
                    _ => Services.WeekArrangement.Normal
                };
                var periods = Periods.Select(int.Parse).ToArray();
                zjuClass.StartPeriod = periods.Min();
                zjuClass.EndPeriod = periods.Max();
                zjuClass.TeacherName = TeacherName;
                zjuClass.ClassCode = ClassCode;
                zjuClass.ClassName = ClassName;
                zjuClass.ClassLocation = ClassLocation;
                zjuClass.DayNumber = DayNumber;

                return zjuClass;
            }
        }
    }
}