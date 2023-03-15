using Newtonsoft.Json;

namespace Blocto.Sdk.Aptos.Model
{
    public class SignMessagePreRequest
    {
        [JsonProperty("from")]
        public string Address { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("nonce")]
        public string Nonce { get; set; }

        [JsonProperty("address?")]
        public bool IsIncludeAddress { get; set; }

        [JsonProperty("application?")]
        public bool IsIncludeApplication { get; set; }

        [JsonProperty("chainId")]
        public bool IsIncludeChainId { get; set; }
    }
}