using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Flow.FCL.Models
{
    public partial class FclService
    {
        [JsonProperty("f_type")]
        public string FType { get; set; }

        [JsonProperty("f_vsn")]
        public string FVsn { get; set; }

        [JsonProperty("type")]
        public ServiceTypeEnum Type { get; set; }

        [JsonProperty("uid")]
        public string Uid { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("identity", NullValueHandling = NullValueHandling.Ignore)]
        public Identity Identity { get; set; }

        [JsonProperty("scoped", NullValueHandling = NullValueHandling.Ignore)]
        public JObject PollingScoped { get; set; }

        [JsonProperty("provider", NullValueHandling = NullValueHandling.Ignore)]
        public JObject PollingServiceProvider { get; set; }

        [JsonProperty("authn", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Authn { get; set; }

        [JsonProperty("method", NullValueHandling = NullValueHandling.Ignore)]
        public string Method { get; set; }

        [JsonProperty("address", NullValueHandling = NullValueHandling.Ignore)]
        public string Address { get; set; }

        [JsonProperty("addr", NullValueHandling = NullValueHandling.Ignore)]
        public string Addr { get; set; }

        [JsonProperty("keyId", NullValueHandling = NullValueHandling.Ignore)]
        public long? KeyId { get; set; }

        [JsonProperty("endpoint", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Endpoint { get; set; }

        [JsonProperty("params", NullValueHandling = NullValueHandling.Ignore)]
        public JObject PollingParams { get; set; }

        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public FclServiceData Data { get; set; }
    }
}