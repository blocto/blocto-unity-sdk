using Newtonsoft.Json;

namespace Flow.FCL.Models
{
    public partial class PollingParams
    {
        [JsonProperty("sessionId")]
        public string SessionId { get; set; }
    }
}