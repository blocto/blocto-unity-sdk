using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Flow.FCL.Models
{
    public class PayerSignResponse
    {
        [JsonProperty("status")]
        public PollingStatusEnum Status { get; set; }
        
        [JsonProperty("data")]
        public JObject Data { get; set; }
    }
}