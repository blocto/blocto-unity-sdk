using Newtonsoft.Json;

namespace Blocto.Sdk.Aptos.Model
{
    public class AbiPayload
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("visibility")]
        public string Visibility { get; set; }
        
        [JsonProperty("is_entry")]
        public bool IsEntry { get; set; }
        
        [JsonProperty("generic_type_params")]
        public AbiParameterType[] GenericTypeParams { get; set; }
    }
}