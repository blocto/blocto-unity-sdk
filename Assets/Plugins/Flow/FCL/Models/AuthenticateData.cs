using System;
using Newtonsoft.Json;

namespace Flow.FCL.Models
{
    public partial class AuthenticateData
    {
        [JsonProperty("addr")]
        public string Addr { get; set; }

        [JsonProperty("paddr")]
        public string Paddr { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("expires")]
        public long Expires { get; set; }

        [JsonProperty("hks")]
        public Uri Hks { get; set; }

        [JsonProperty("services")]
        public FclService[] Services { get; set; }
    }
}