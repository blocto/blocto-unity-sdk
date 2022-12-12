using Newtonsoft.Json;

namespace Blocto.Sdk.Solana.Model
{
    public class RawTxAppendData
    {
        [JsonProperty("append_tx")]
        public string AppendTx { get; set; }
    }
}