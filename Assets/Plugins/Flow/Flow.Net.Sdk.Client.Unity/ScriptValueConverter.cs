using System;
using System.Text;
using Flow.Net.Sdk.Client.Unity.Models.Apis;
using Newtonsoft.Json;

namespace Flow.Net.Sdk.Client.Unity.Unity
{
    public class ScriptValueConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Response);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
            {
                return null;
            }

            var bytes = Convert.FromBase64String(reader.Value.ToString());
            var value = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

            return new Response
                   {
                       Value = value
                   };

        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
    }
}