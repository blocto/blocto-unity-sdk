using Flow.FCL.Models.Authz;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Flow.FCL.Models
{
    public class SignatureResponse : IResponse
    {
        [JsonProperty("status")]
        public ResponseStatusEnum ResponseStatus { get; set; }
        
        [JsonProperty("data")]
        public JObject Data { get; set; }
    }
}