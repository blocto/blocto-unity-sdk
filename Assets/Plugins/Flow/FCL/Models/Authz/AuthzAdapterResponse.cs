using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Flow.FCL.Models.Authz
{
    public partial class AuthzAdapterResponse : IResponse
    {
        [JsonProperty("status")]
        public ResponseStatusEnum ResponseStatus { get; set; }

        [JsonProperty("reason")]
        public object Reason { get; set; }

        [JsonProperty("compositeSignature")]
        public JObject CompositeSignature { get; set; }

        [JsonProperty("authorizationUpdates")]
        public AuthorizationUpdates AuthorizationUpdates { get; set; }

        [JsonProperty("local")]
        public List<AuthzLocal> Local { get; set; }
    }
}