using Newtonsoft.Json;

namespace Blocto.Sdk.Aptos.Model
{
    public class ScriptTransactionPayload : TransactonPayload
    {
        public ScriptTransactionPayload()
        {
            MaxGasAmount = "50000";
        }
        
        [JsonProperty("type")]
        public string Type { get; set; }
        
        [JsonProperty("code")]
        public string Code { get; set; }
    }
}