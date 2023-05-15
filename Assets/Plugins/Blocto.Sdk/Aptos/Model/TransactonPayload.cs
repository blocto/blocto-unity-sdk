using Newtonsoft.Json;

namespace Blocto.Sdk.Aptos.Model
{
    public class TransactonPayload
    {
        [JsonProperty("from")]
        public string Address { get; set; }
        
        [JsonProperty("arguments")]
        public object[] Arguments { get; set; }
        
        [JsonProperty("type_arguments")]
        public string[] TypeArguments { get; set; }
        
        [JsonProperty("max_gas_amount")]
        public string MaxGasAmount { get; set; }
    }
}