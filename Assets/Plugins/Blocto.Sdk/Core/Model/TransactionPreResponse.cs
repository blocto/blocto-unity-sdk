using Newtonsoft.Json;

namespace Blocto.Sdk.Core.Model
{
    public class TransactionPreResponse
    {
        [JsonProperty("authorizationId")]
        public string AuthorizationId { get; set; }
        
        [JsonProperty("status")]
        public string Status { get; set; }
        
        [JsonProperty("reason")]
        public string Reason { get; set; }
    }
}