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
    }
}