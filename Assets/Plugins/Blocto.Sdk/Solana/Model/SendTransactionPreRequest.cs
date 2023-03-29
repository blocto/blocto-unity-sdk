using System.Collections.Generic;
using Newtonsoft.Json;

namespace Blocto.Sdk.Solana.Model
{
    public class SendTransactionPreRequest
    {
        public SendTransactionPreRequest()
        {
            PublicKeySignaturePairs = new Dictionary<string, string>();
            AppenTxDict = new Dictionary<string, string>();    
        }
        
        [JsonProperty("from")]
        public string From { get; set; }
        
        [JsonProperty("message")]
        public string Message { get; set; }
        
        [JsonProperty("isInvokeWrapped")]
        public bool IsInvokeWrapped { get; set; }
        
        [JsonProperty("publicKeySignaturePairs")]
        public Dictionary<string, string> PublicKeySignaturePairs { get; set; }
        
        [JsonProperty("appenTx")]
        public Dictionary<string, string> AppenTxDict { get; set; }
    }
}