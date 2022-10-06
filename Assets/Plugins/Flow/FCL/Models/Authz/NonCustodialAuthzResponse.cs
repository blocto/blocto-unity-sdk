using Newtonsoft.Json;

namespace Flow.FCL.Models.Authz
{
    public class NonCustodialAuthzResponse : IResponse
    {
        [JsonProperty("status")]
        public ResponseStatusEnum ResponseStatus { get; set; }

        [JsonProperty("reason")]
        public object Reason { get; set; }
        
        [JsonProperty("updates")]
        public AuthorizationUpdates AuthorizationUpdates { get; set; }

        [JsonProperty("local")]
        public AuthzLocal Local { get; set; }
    }
}