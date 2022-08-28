using Newtonsoft.Json;

namespace Flow.FCL.Models.Authz
{
    public class BaseAccount
    {
        [JsonProperty("addr")]
        public string Addr { get; set; }
    }
}