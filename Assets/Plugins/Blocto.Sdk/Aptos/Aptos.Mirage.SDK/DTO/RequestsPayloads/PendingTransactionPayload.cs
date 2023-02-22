using Newtonsoft.Json;

namespace Mirage.Aptos.SDK.DTO
{
	public class PendingTransactionPayload : SubmitTransactionRequest
	{
		[JsonProperty(PropertyName = "hash")]
		public string Hash;
	}
}