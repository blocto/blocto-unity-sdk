using Newtonsoft.Json;

namespace Blocto.Sdk.Flow.Model
{
    public class ConnectWalletConfig
    {
        [JsonProperty("appId")]
        public string AppId { get; set; }
    }
}