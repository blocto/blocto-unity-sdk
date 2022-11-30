using Newtonsoft.Json;

namespace Blocto.Sdk.Solana.Model
{
    public class CreateRawTxRequest
    {
        [JsonProperty("sol_address")]
        public string Address { get; set; }
        
        [JsonProperty("raw_tx")]
        public string RawTx { get; set; }
    }
}