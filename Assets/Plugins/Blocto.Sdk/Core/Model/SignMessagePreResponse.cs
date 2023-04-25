using Newtonsoft.Json;

namespace Blocto.Sdk.Core.Model
{
    public class SignMessagePreResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }
        
        [JsonProperty("signatureId")]
        public string SignatureId { get; set; }
        
        [JsonProperty("reason")]
        public string Reason { get; set; }
    }
}