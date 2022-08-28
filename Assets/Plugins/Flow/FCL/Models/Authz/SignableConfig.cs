using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Flow.FCL.Models.Authz
{
    public class SignableConfig
    {
        [JsonProperty("services")]
        public SignableConfig.Service Services { get; set; }
        
        [JsonProperty("app")]
        public JObject App { get; set; }

        public class Service
        {
            [JsonProperty("OpenID.scopes")]
            public string OpenIdScope { get; set; }
        }
    }
}