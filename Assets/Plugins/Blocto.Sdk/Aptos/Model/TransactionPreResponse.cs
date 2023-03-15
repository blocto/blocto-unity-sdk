using Newtonsoft.Json;

namespace Blocto.Sdk.Aptos.Model
{
    public class TransactionPreResponse
    {
        [JsonProperty("authorizationId")]
        public string AuthorizationId { get; set; }
    }
}