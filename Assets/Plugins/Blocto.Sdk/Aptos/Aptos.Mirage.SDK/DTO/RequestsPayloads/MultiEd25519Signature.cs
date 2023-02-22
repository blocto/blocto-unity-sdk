using Newtonsoft.Json;

namespace Mirage.Aptos.SDK.DTO
{
	public class MultiEd25519Signature : TransactionSignature
	{
		[JsonProperty(PropertyName = "public_keys")]
		public string[] PublicKeys;
		[JsonProperty(PropertyName = "signatures")]
		public string[] Signatures;
		[JsonProperty(PropertyName = "threshold")]
		public int Threshold;
		[JsonProperty(PropertyName = "bitmap")]
		public int Bitmap;
	}
}