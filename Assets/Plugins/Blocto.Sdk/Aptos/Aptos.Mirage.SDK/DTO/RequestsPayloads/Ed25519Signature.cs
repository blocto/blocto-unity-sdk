using Newtonsoft.Json;

namespace Mirage.Aptos.SDK.DTO
{
	public class Ed25519Signature : TransactionSignature
	{
		[JsonProperty(PropertyName = "public_key")]
		public string PublicKey;
		[JsonProperty(PropertyName = "signature")]
		public string Signature;
	}
}