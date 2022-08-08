using Newtonsoft.Json;

namespace Flow.FCL.Models
{
    public partial class PollingIdentity
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("keyId", NullValueHandling = NullValueHandling.Ignore)]
        public long? KeyId { get; set; }

        [JsonProperty("addr", NullValueHandling = NullValueHandling.Ignore)]
        public string Addr { get; set; }
    }
}