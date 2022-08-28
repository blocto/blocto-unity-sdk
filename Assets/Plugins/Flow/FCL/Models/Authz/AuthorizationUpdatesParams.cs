using Newtonsoft.Json;

namespace Flow.FCL.Models.Authz
{
    public partial class AuthorizationUpdatesParams
    {
        [JsonProperty("authorizationId")]
        public string AuthorizationId { get; set; }

        [JsonProperty("sessionId")]
        public string SessionId { get; set; }
    }
}