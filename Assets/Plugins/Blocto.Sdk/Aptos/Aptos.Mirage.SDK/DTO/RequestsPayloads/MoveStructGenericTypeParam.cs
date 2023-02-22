using Newtonsoft.Json;

namespace Mirage.Aptos.SDK.DTO
{
	public class MoveStructGenericTypeParam
	{
		[JsonProperty(PropertyName = "constraints")]
		public string[] Constraints;
	}
}