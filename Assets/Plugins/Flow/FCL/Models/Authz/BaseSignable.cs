using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Flow.FCL.Models.Authz
{
    public class BaseSignable
    {
        public BaseSignable()
        {
            FclVersion = "";    
            Config = new SignableConfig
                     {
                         Services = new SignableConfig.Service
                                    {
                                        OpenIdScope = "email!"
                                    },
                         App = new JObject()
                     };
        }
        
        [JsonProperty("fclVersion")]
        public string FclVersion { get; set; }
        
        [JsonProperty("service")]
        public JObject Service { get; set; }
        
        [JsonProperty("config")]
        public SignableConfig Config { get; set; }
        
    }
}