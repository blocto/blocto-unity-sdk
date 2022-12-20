using Newtonsoft.Json;

namespace Blocto.Sdk.Solana.Model
{
    public class CreateRawTxResponse
    {
        [JsonProperty("raw_tx")]
        public string RawTx { get; set; }

        [JsonProperty("extra_data")]
        public RawTxExtendData ExtraData { get; set; }
        
    }
}