using Mirage.Aptos.SDK.Converters;
using Newtonsoft.Json;

namespace Mirage.Aptos.SDK.DTO
{
	public class WrappedTransaction
	{
		[JsonProperty(PropertyName = "transaction")]
		[JsonConverter(typeof(TransactionConverter))]
		public TypedTransaction Transaction;
	}
}