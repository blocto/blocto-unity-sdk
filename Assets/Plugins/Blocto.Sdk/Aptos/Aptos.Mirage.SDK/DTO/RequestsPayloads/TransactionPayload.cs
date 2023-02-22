using System;
using Newtonsoft.Json;

namespace Mirage.Aptos.SDK.DTO
{
	[Serializable]
	public class TransactionPayload
	{
		[JsonProperty(PropertyName = "type")]
		public string Type;
	}
}