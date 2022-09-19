using System;
using System.Text;
using Flow.Net.Sdk.Core.Cadence;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Flow.Net.Sdk.Core.Models
{
    public class FlowEventConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var properties = new JArray(JToken.ReadFrom(reader));
            var result = new FlowEvent
                         {
                             Type = properties[0]["type"]?.ToString(),
                             TransactionId = properties[0]["transaction_id"]?.ToString(),
                             TransactionIndex = Convert.ToUInt32(properties[0]["transaction_index"]),
                             EventIndex = Convert.ToUInt32(properties[0]["event_index"]),
                         };
            
            var payload = Encoding.UTF8.GetString(Convert.FromBase64String(properties[0]["payload"]?.ToString() ?? string.Empty));
            if(payload != string.Empty)
            {
                var cadence = payload.Decode();
                result.Payload = cadence;
            }
            else
            {
                result.Payload = null;
            }
            
            return result;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(FlowEvent);
        }
    }
}