using Newtonsoft.Json;

namespace Flow.FCL.Models
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
    }
}