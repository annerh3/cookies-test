using Newtonsoft.Json;
namespace CookiesProyect.Dtos.Common
{
    public class ResponseDto<T> 
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public T Data { get; set; }

        public string Message { get; set; }

        [JsonIgnore]
        public int StatusCode { get; set; }
        public bool Status { get; set; }

    }
}
