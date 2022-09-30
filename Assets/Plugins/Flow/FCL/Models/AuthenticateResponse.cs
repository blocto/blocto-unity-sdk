using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Flow.FCL.Models.Authz;

namespace Flow.FCL.Models
{
    public partial class AuthenticateResponse : IResponse
    {
        [JsonProperty("f_type")]
        public string FType { get; set; }

        [JsonProperty("f_vsn")]
        public string FVsn { get; set; }

        [JsonProperty("status")]
        public ResponseStatusEnum ResponseStatus { get; set; }

        [JsonProperty("data")]
        public AuthenticateData Data { get; set; }
        
        public static AuthenticateResponse FromJson(string json) => JsonConvert.DeserializeObject<AuthenticateResponse>(json, AuthenticateResponse.Settings);

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