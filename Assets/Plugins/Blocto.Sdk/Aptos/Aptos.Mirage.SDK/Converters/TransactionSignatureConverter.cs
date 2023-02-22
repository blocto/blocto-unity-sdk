using System;
using Mirage.Aptos.SDK.Constants;
using Mirage.Aptos.SDK.DTO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mirage.Aptos.SDK.Converters
{
	public class TransactionSignatureConverter : JsonConverter<TransactionSignature>
	{
		private const string TypeFieldName = "type";

		public override void WriteJson(JsonWriter writer, TransactionSignature value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, value);
		}

		public override TransactionSignature ReadJson(
			JsonReader reader,
			Type objectType,
			TransactionSignature existingValue,
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
				case SignatureTypes.SimpleSignature:
					return jsonObj.ToObject<Ed25519Signature>(serializer);
				case SignatureTypes.MultiSignature:
					return jsonObj.ToObject<MultiEd25519Signature>(serializer);
				case SignatureTypes.MultiAgentSignature:
					return jsonObj.ToObject<MultiAgentSignature>(serializer);
				default:
					throw new JsonException($"Converter fot signature type {type} doesn't exist.");
			}
		}
	}
}