using Newtonsoft.Json;

namespace Blocto.Sdk.Solana.Model
{
    public class SendTransactionPreResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }
        
        [JsonProperty("authorizationId")]
        public string AuthorizationId { get; set; }
        
        [JsonProperty("reason")]
        public string Reason { get; set; }
    }
}