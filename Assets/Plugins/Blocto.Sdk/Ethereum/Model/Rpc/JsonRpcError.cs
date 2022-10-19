using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Blocto.Sdk.Ethereum.Model.Rpc
{
    [JsonObject]
    public class JsonRpcError
    {
        [JsonConstructor]
        private JsonRpcError()
        {
        }
    
        /// <summary>
        /// Rpc error code (Required)
        /// </summary>
        [JsonProperty("code", Required = Required.Always)]
        public int Code { get; private set; }

        /// <summary>
        /// Error message (Required)
        /// </summary>
        [JsonProperty("message", Required = Required.Always)]
        public string Message { get; private set; }

        /// <summary>
        /// Error data (Optional)
        /// </summary>
        [JsonProperty("data")]
        public JToken Data { get; private set; }
    }
}