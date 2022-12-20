using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Blocto.Sdk.Ethereum.Model.Rpc
{
    [JsonObject]
    public class RpcResponse
    {
        [JsonConstructor]
        protected RpcResponse()
        {
            JsonRpcVersion = "2.0";
        }

        /// <param name="id">Request id</param>
        protected RpcResponse(object id)
        {
            this.Id = id;
            JsonRpcVersion = "2.0";
        }

        /// <param name="id">Request id</param>
        /// <param name="error">Request error</param>
        public RpcResponse(object id, JsonRpcError error) : this(id)
        {
            this.Error = error;
        }

        /// <param name="id">Request id</param>
        /// <param name="result">Response result object</param>
        public RpcResponse(object id, JToken result) : this(id)
        {
            this.Result = result;
        }

        /// <summary>
        /// Request id (Required but nullable)
        /// </summary>
        [JsonProperty("id", Required = Required.AllowNull)]
        public object Id { get; private set; }

        /// <summary>
        /// Rpc request version (Required)
        /// </summary>
        [JsonProperty("jsonrpc", Required = Required.Always)]
        public string JsonRpcVersion { get; private set; } 

        /// <summary>
        /// Reponse result object (Required)
        /// </summary>
        [JsonProperty("result", Required = Required.Default)] //TODO somehow enforce this or an error, not both
        public JToken Result { get; private set; }

        /// <summary>
        /// Error from processing Rpc request (Required)
        /// </summary>
        [JsonProperty("error", Required = Required.Default)]
        public JsonRpcError Error { get; private set; }

        [JsonIgnore]
        public bool HasError { get{ return this.Error != null;}}
    }
}