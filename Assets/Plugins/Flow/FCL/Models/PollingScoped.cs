using Newtonsoft.Json;

namespace Flow.FCL.Models
{
    public partial class PollingScoped
    {
        [JsonProperty("email")]
        public string Email { get; set; }
    }
}