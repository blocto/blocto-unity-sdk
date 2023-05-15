using Newtonsoft.Json;

namespace Blocto.Sdk.Aptos.Model
{
    public class Code
    {
        [JsonProperty("bytecode")]
        public string ByteCode { get; set; }

        [JsonProperty("abi")]
        public AbiPayload Abi { get; set; }
    }
}