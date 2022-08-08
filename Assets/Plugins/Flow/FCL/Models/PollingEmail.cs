using Newtonsoft.Json;

namespace Flow.FCL.Models
{
    public partial class PollingEmail
    {
        [JsonProperty("email")]
        public string EmailEmail { get; set; }

        [JsonProperty("email_verified")]
        public bool EmailVerified { get; set; }
    }
}