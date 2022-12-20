using System.Collections.Generic;
using Newtonsoft.Json;

namespace Blocto.Sdk.Solana.Model
{
    public class RawTxExtendData
    {
        [JsonProperty("append_tx")]
        public Dictionary<string, string> AppendData { get; set; }
    }
}