using System;
using System.Text;
using Flow.Net.Sdk.Core.Cadence;
using Flow.Net.Sdk.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Plugins.Blocto.Sdk.Core.Converts
{
    public class FlowEventConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var array = new JArray(JToken.ReadFrom(reader));
            var result = new FlowEvent
                         {
                             Type = array[0]["type"]?.ToString(),
                             TransactionId = array[0]["transaction_id"]?.ToString(),
                             TransactionIndex = Convert.ToUInt32(array[0]["transaction_index"]),
                             EventIndex = Convert.ToUInt32(array[0]["event_index"]),
                         };
            var payload = Encoding.UTF8.GetString(Convert.FromBase64String(array[0]["payload"]?.ToString() ?? string.Empty));
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