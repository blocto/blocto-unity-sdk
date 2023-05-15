using System.Collections.Generic;
using Newtonsoft.Json;

namespace Blocto.Sdk.Aptos.Model
{
    public class PublicKey
    {
        [JsonProperty("public_keys")]
        public List<string> Keys { get; set; }

        [JsonProperty("sequence_number")]
        public long SequenceNumber { get; set; }
    }
}