using System;
using Mirage.Aptos.SDK.Converters;
using Newtonsoft.Json;

namespace Mirage.Aptos.SDK.DTO
{
	[Serializable]
	public class TypedTransaction
	{
		[JsonProperty(PropertyName = "type")]
		public string Type;
	}
}