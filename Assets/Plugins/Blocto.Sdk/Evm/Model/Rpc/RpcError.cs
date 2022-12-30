using Newtonsoft.Json.Linq;

namespace Blocto.Sdk.Evm.Model.Rpc
{
    public class RpcError
    {
        public RpcError(int code, string message, JToken data = null)
        {
            Code = code;
            Message = message;
            Data = data;
        }

        public int Code { get; private set; }
        public string Message { get; private set; }
        public JToken Data { get; private set; }
    }
    
    // [JsonObject]
    // internal class RpcError
    // {
    //     [JsonConstructor]
    //     private RpcError()
    //     {
    //     }
    //     
    //     /// <summary>
    //     /// Rpc error code (Required)
    //     /// </summary>
    //     [JsonProperty("code", Required = Required.Always)]
    //     public int Code { get; private set; }
    //
    //     /// <summary>
    //     /// Error message (Required)
    //     /// </summary>
    //     [JsonProperty("message", Required = Required.Always)]
    //     public string Message { get; private set; }
    //
    //     /// <summary>
    //     /// Error data (Optional)
    //     /// </summary>
    //     [JsonProperty("data")]
    //     public JToken Data { get; private set; }
    // }
}