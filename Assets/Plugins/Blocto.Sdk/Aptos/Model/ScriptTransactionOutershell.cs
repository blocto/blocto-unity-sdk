using Newtonsoft.Json;

namespace Blocto.Sdk.Aptos.Model
{
    public class ScriptTransactionOutershell
    {
        [JsonProperty("transaction")]
        public ScriptTransactionPayload Payload { get; set; }
    }
}