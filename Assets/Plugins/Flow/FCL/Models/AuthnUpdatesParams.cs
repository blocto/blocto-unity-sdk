using System;
using Newtonsoft.Json;

namespace Flow.FCL.Models
{
    public partial class AuthnUpdatesParams
    {
        [JsonProperty("l6n")]
        public Uri L6N { get; set; }

        [JsonProperty("authenticationId")]
        public string AuthenticationId { get; set; }
    }
}