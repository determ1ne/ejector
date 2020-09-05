using System.Text.Json.Serialization;

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
        } 
    }
}