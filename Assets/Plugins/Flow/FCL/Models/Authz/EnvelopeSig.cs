using Newtonsoft.Json;

namespace Flow.FCL.Models.Authz
{
    public partial class EnvelopeSig
    {
        [JsonProperty("address")]
        public object Address { get; set; }

        [JsonProperty("keyId")]
        public object KeyId { get; set; }

        [JsonProperty("sig")]
        public object Sig { get; set; }
    }
}