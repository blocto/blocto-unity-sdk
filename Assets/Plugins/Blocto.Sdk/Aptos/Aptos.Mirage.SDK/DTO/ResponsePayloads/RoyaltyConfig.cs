using Newtonsoft.Json;

namespace Mirage.Aptos.SDK.DTO.ResponsePayloads
{
	public class RoyaltyConfig
	{
		[JsonProperty(PropertyName = "payee_address")]
		public string PayeeAddress;
		[JsonProperty(PropertyName = "royalty_points_denominator")]
		public string RoyaltyPointsDenominator;
		[JsonProperty(PropertyName = "royalty_points_numerator")]
		public string RoyaltyPointsNumerator;
	}
}