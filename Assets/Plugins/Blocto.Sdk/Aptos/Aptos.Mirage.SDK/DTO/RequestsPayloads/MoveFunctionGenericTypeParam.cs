using Newtonsoft.Json;

namespace Mirage.Aptos.SDK.DTO
{
	public class MoveFunctionGenericTypeParam
	{
		[JsonProperty(PropertyName = "constraints")]
		public string Constraints;
	}
}