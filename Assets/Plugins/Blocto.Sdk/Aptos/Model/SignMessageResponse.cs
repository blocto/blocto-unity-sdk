using System.Collections.Generic;
using Newtonsoft.Json;

namespace Blocto.Sdk.Aptos.Model
{
    public class SignMessageResponse
    {
        [JsonProperty("signatureId")]
        public string SignatureId { get; set; }

        [JsonProperty("signature")]
        public List<string> Signatures { get; set; }

        [JsonProperty("bitmap")]
        public int[] Bitmap { get; set; }

        [JsonProperty("fullMessage")]
        public string FullMessage { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("nonce")]
        public string Nonce { get; set; }

        [JsonProperty("prefix")]
        public string Prefix { get; set; }
        
        [JsonProperty("address")]
        public string Address { get; set; }
        
        [JsonProperty("application")]
        public string Application { get; set; }
        
        [JsonProperty("chainId")]
        public int ChainId { get; set; }
    }
}