using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Flow.FCL.Models.Authn
{
    public partial class AuthnResponse
    {
        [JsonProperty("f_type")]
        public string FType { get; set; }

        [JsonProperty("f_vsn")]
        public string FVsn { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("updates")]
        public AuthnUpdates AuthnUpdates { get; set; }

        [JsonProperty("local")]
        public AuthnLocal AuthnLocal { get; set; }
    }
}