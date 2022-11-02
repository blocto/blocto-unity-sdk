using System.Collections.Generic;
using Newtonsoft.Json;

namespace Solnet.Rpc.Messages
{
    /// <summary>
    /// Rpc request message.
    /// </summary>
    public class JsonRpcRequest : JsonRpcBase
    {
        /// <summary>
        /// The request method.
        /// </summary>
        [JsonProperty("method")]
        public string Method { get; }

        /// <summary>
        /// The method parameters list.
        /// </summary>
        [JsonProperty("params")]
        public IList<object> Params { get; }

        public JsonRpcRequest(int id, string method, IList<object> parameters)
        {
            if(parameters != null)
            {
                Params = parameters;
            }
            
            Method = method;
            Id = id;
            Jsonrpc = "2.0";
        }
    }
}