using Newtonsoft.Json;

namespace Flow.FCL.Models.Authz
{
    public class Account : BaseAccount
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }
        
        [JsonProperty("tempId")]
        public string TempId { get; set; }
        
        [JsonProperty("keyId")]
        public uint KeyId { get; set; }
        
        [JsonProperty("sequenceNum")]
        public ulong SequenceNum { get; set; }
        
        [JsonProperty("signature")]
        public string Signature { get; set; }
        
        [JsonProperty("signingFunction")]
        public object SigningFunction { get; set; }
        
        [JsonProperty("role")]
        public Role Role { get; set; }
    }
}