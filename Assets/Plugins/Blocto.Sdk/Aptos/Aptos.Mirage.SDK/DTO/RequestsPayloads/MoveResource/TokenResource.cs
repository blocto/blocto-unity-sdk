using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mirage.Aptos.SDK.DTO
{
	public class TokenResource
	{
		[JsonProperty(PropertyName = "burn_events")]
		public ResourceEvent BurnEvents;
		[JsonProperty(PropertyName = "deposit_events")]
		public ResourceEvent DepositEvents;
		[JsonProperty(PropertyName = "direct_transfer")]
		public bool DirectTransfer;
		[JsonProperty(PropertyName = "mutate_token_property_events")]
		public ResourceEvent MutateTokenPropertyEvents;
		[JsonProperty(PropertyName = "tokens")]
		public DataStorePointer Tokens;
		[JsonProperty(PropertyName = "withdraw_events")]
		public ResourceEvent WithdrawEvents;
	}
}