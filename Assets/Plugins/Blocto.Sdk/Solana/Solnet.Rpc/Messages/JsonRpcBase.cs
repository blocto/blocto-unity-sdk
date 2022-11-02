using Newtonsoft.Json;

namespace Solnet.Rpc.Messages
{
    /// <summary>
    /// Base JpnRpc message.
    /// </summary>
    public abstract class JsonRpcBase
    {
        /// <summary>
        /// The rpc version.
        /// </summary>
        [JsonProperty("jsonrpc")]
        public string Jsonrpc { get; protected set; }

        /// <summary>
        /// The id of the message.
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }
    }
}