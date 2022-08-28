using System;
using Newtonsoft.Json;

namespace Flow.FCL.Models.Authn
{
    public partial class AuthnParams
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("authenticationId")]
        public string AuthenticationId { get; set; }

        [JsonProperty("fclVersion")]
        public string FclVersion { get; set; }
        
        [JsonProperty("signatureId")]
        public string SignatureId { get; set; }
        
        [JsonProperty("sessionId")]
        public string SessionId { get; set; }
        
        [JsonProperty("appId")]
        public string AppId { get; set; }
        
        [JsonProperty("accountProofIdentifier")]
        public string  AccountProofIdentifier { get; set; }
        
        [JsonProperty("accountProofNonce")]
        public string  AccountProofNonce { get; set; }
        
        [JsonProperty("thumbnail")]
        public string Thumbnail { get; set; }
        
        [JsonProperty("title")]
        public string Title { get; set; }
    }
}