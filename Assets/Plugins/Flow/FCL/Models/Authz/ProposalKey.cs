using Newtonsoft.Json;

namespace Flow.FCL.Models.Authz
{
    public partial class ProposalKey
    {
        [JsonProperty("address")]
        public object Address { get; set; }

        [JsonProperty("keyId")]
        public uint KeyId { get; set; }

        [JsonProperty("sequenceNum")]
        public ulong SequenceNum { get; set; }
    }
}