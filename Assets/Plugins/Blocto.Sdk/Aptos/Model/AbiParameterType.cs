using Newtonsoft.Json;

namespace Blocto.Sdk.Aptos.Model
{
    public class AbiParameterType
    {
        [JsonProperty("params")]
        public string[] Params { get; set; }
        
        [JsonProperty("return")]
        public string[] Return { get; set; }
    }
}