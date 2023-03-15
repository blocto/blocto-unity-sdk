using Newtonsoft.Json;

namespace Blocto.Sdk.Aptos.Model
{
    public class SignMessagePreResponse
    {
        [JsonProperty("signatureId")]
        public string SignatureId { get; set; }
    }
}