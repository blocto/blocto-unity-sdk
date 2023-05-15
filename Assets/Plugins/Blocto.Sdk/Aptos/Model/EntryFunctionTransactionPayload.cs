using Newtonsoft.Json;

namespace Blocto.Sdk.Aptos.Model
{
    public class EntryFunctionTransactionPayload : TransactonPayload
    {
        public EntryFunctionTransactionPayload()
        {
            Type = "entry_function_payload";
            Function = "0x1::coin::transfer";
            MaxGasAmount = "50000";
        }
        
        [JsonProperty("type")]
        public string Type { get; private set; }
        
        [JsonProperty("function")]
        public string Function { get; set; }
    }
}