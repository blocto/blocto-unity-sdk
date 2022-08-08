using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Plugins.Flow.FCL.Models;

namespace Flow.FCL.Models
{
    public partial class PollingResponse
    {
        [JsonProperty("f_type")]
        public string FType { get; set; }

        [JsonProperty("f_vsn")]
        public string FVsn { get; set; }

        [JsonProperty("status")]
        public PollingStatusEnum Status { get; set; }

        [JsonProperty("data")]
        public PollingData Data { get; set; }
        
        public static PollingResponse FromJson(string json) => JsonConvert.DeserializeObject<PollingResponse>(json, PollingResponse.Settings);

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
                                                                  {
                                                                      MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                                                                      DateParseHandling = DateParseHandling.None,
                                                                      Converters =
                                                                      {
                                                                          new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
                                                                      },
                                                                  };
    }
}