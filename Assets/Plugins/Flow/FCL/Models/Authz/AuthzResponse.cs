using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Flow.FCL.Models.Authz
{
    public partial class AuthzResponse : IResponse
    {
        [JsonProperty("status")]
        public PollingStatusEnum Status { get; set; }

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