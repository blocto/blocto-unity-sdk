using System.Collections.Generic;
using Newtonsoft.Json;

namespace Blocto.Sdk.Flow.Model
{
    public class ConnectWalletPayload
    {
        public ConnectWalletPayload(Dictionary<string, object> parameter, string appId)
        {
            if(parameter.TryGetValue("accountProofIdentifier", out var appIdentifier) && 
               parameter.TryGetValue("accountProofNonce", out var nonce))
            {
                Nonce = nonce;
                AppIdentifier = appIdentifier;
            }
            
            Config = new ConnectWalletConfig
            {
                AppId = appId
            };
        }
        
        [JsonProperty("nonce")]
        public object Nonce { get; set; }
        
        [JsonProperty("appIdentifier")]
        public object AppIdentifier { get; set; }
        
        [JsonProperty("config")]
        public ConnectWalletConfig Config { get; set; }
    }
}