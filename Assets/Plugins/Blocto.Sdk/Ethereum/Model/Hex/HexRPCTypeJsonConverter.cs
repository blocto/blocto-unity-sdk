using System;
using Blocto.Sdk.Ethereum.Model.Hex.Util;
using Newtonsoft.Json;


namespace Blocto.Sdk.Ethereum.Model.Hex
{
    public class HexRPCTypeJsonConverter<T, TValue> : JsonConverter where T : HexRPCType<TValue>
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var hexRPCType = (T) value;
            writer.WriteValue(hexRPCType.HexValue);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value switch
                   {
                       null => null,
                       string value => HexTypeFactory.CreateFromHex<TValue>(value),
                       _ => HexTypeFactory.CreateFromObject<TValue>(reader.Value)
                   };
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(T);
        }
    }
}