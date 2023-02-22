using System;
using Mirage.Aptos.SDK.Constants;
using Mirage.Aptos.SDK.DTO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mirage.Aptos.SDK.Converters
{
	public class TransactionConverter : JsonConverter<TypedTransaction>
	{
		private const string TypeFieldName = "type";

		public override void WriteJson(JsonWriter writer, TypedTransaction value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, value);
		}

		public override TypedTransaction ReadJson(
			JsonReader reader,
			Type objectType,
			TypedTransaction existingValue,
			bool hasExistingValue,
			JsonSerializer serializer
		)
		{
			var jsonObj = JObject.Load(reader);
			var type = jsonObj[TypeFieldName]?.Value<string>();

			if (type == null)
			{
				throw new JsonException($"Field \"{TypeFieldName}\" doesn't exist in json object.");
			}

			switch (type)
			{
				case TransactionTypes.UserTransaction:
					return jsonObj.ToObject<UserTransaction>(serializer);
				case TransactionTypes.GenesisTransaction:
					return jsonObj.ToObject<GenesisTransaction>(serializer);
				case TransactionTypes.PendingTransaction:
					return jsonObj.ToObject<PendingTransaction>(serializer);
				case TransactionTypes.BlockMetadataTransaction:
					return jsonObj.ToObject<BlockMetadataTransaction>(serializer);
				case TransactionTypes.StateCheckpointTransaction:
					return jsonObj.ToObject<StateCheckpointTransaction>(serializer);
				default:
					throw new JsonException($"Converter for transaction type \"{type}\" doesn't exist.");
			}
		}
	}
}