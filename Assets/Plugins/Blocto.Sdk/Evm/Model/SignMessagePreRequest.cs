using Blocto.Sdk.Evm.Model.Eth;
using Newtonsoft.Json;

namespace Blocto.Sdk.Evm.Model
{
    public class SignMessagePreRequest
    {
        [JsonProperty("from")]
        public string From { get; set; }
        
        [JsonProperty("message")]
        public string Message { get; set; }
        
        [JsonProperty("method")]
        public string Method { get; set; }
    }
}