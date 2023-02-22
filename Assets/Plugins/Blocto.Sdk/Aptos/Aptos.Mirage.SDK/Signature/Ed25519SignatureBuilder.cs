using System.Linq;
using System.Text;
using Blocto.Sdk.Core.Extension;
using Mirage.Aptos.SDK.DTO;
using Mirage.Aptos.Imlementation.ABI;
using Org.BouncyCastle.Crypto.Digests;

namespace Mirage.Aptos.SDK
{
	public class Ed25519SignatureBuilder
	{
		private const string SignatureSalt = "APTOS::RawTransaction";
		private const string TransactionType = "ed25519_signature";

		public Ed25519Signature GetSignature(Account sender, RawTransaction rawTransaction)
		{
			var signingMessage = DataUtils.Serialize(rawTransaction);

			return new Ed25519Signature
			{
				Type = TransactionType,
				PublicKey = sender.PublicKey,
				Signature = Sign(sender, signingMessage)
			};
		}
		
		public Ed25519Signature GetSimulatedSignature(Account sender, RawTransaction rawTransaction)
		{
			var signingMessage = DataUtils.Serialize(rawTransaction);
			var simulationPrivateKey = new byte[64];
			var simulationAccount = new Account(simulationPrivateKey);
			
			return new Ed25519Signature
			{
				Type = TransactionType,
				PublicKey = sender.PublicKey,
				Signature = Sign(simulationAccount, signingMessage)
			};
		}
		
		private string Sign(Account sender, byte[] transaction)
		{
			var salt = GetSalt();
			var signingMessage = salt.Concat(transaction).ToArray();

			var signature = sender.Sign(signingMessage);
			return signature.Take(64).ToArray().ToHex(true);
		}

		private byte[] GetSalt()
		{
			var digest = new Sha3Digest();
			var salt = Encoding.ASCII.GetBytes(SignatureSalt);
			var result = new byte[digest.GetDigestSize()];
			digest.BlockUpdate(salt, 0, salt.Length);
			digest.DoFinal(result, 0);
			return result;
		}
	}
}