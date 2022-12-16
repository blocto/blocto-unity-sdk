using Newtonsoft.Json;

namespace Blocto.Sdk.Evm.Model
{
    public class AbiResult
    {
        [JsonProperty("status")]
        public string Status { get; set; }
        
        [JsonProperty("message")]
        public string Message { get; set; }
        
        [JsonProperty("result")]
        public string Result { get; set; }
    }
}