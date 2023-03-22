using Newtonsoft.Json;

namespace Blocto.Sdk.Aptos.Model
{
    public class ScriptTransactionPayload : TransactonPayload
    {
        public ScriptTransactionPayload()
        {
            Type = "script_payload";
            MaxGasAmount = "50000";
        }
        
        [JsonProperty("type")]
        public string Type { get; set; }
        
        [JsonProperty("code")]
        public Code Code { get; set; }
    }
}