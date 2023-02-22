using Blocto.Sdk.Core.Extension;
using Chaos.NaCl;
using Org.BouncyCastle.Crypto.Digests;

namespace Mirage.Aptos.SDK
{
	/// <summary>
	/// Class for creating and managing Aptos account.
	/// </summary>
	public class Account
	{
		private const byte AddressEnding = 0x00;

		private byte[] _publicKey;
		private byte[] _privateKey;

		public readonly string Address;
		public readonly string PublicKey;

		/// <summary>
		/// Creates new account instance.
		/// </summary>
		public Account()
		{
			var seed = GenerateRandomSeed();
			CreateKeyPairFromSeed(seed);
			Address = GetAddress();
			PublicKey = _publicKey.ToHex(true);
		}

		/// <summary>
		/// Creates new account instance.
		/// </summary>
		/// <param name="privateKey">Private key from which account key pair will be generated.</param>
		public Account(byte[] privateKey)
		{
			var seed = privateKey.Slice(0, 32);
			CreateKeyPairFromSeed(seed);
			Address = GetAddress();
			PublicKey = _publicKey.ToHex(true);
		}

		/// <summary>
		/// This is the key by which Aptos account is referenced.
		/// It is the 32-byte of the SHA-3 256 cryptographic hash
		///of the public key(s) concatenated with a signature scheme identifier byte
		/// </summary>
		/// <returns>Address associated with the given account.</returns>
		public string GetAddress()
		{
			var digest = new Sha3Digest();
			var result = new byte[digest.GetDigestSize()];
			digest.BlockUpdate(_publicKey, 0, _publicKey.Length);
			digest.Update(AddressEnding);
			digest.DoFinal(result, 0);
			return result.ToHex(true);
		}

		/// <summary>
		/// Signs specified `message` with account's private key
		/// </summary>
		/// <param name="message">A message to sign.</param>
		/// <returns>A signature byte array.</returns>
		public byte[] Sign(byte[] message)
		{
			return Ed25519.Sign(message, _privateKey);
		}

		private static byte[] GenerateRandomSeed()
		{
			byte[] bytes = new byte[Ed25519.PrivateKeySeedSizeInBytes];
			RandomUtils.GetBytes(bytes);
			return bytes;
		}

		private void CreateKeyPairFromSeed(byte[] seed)
		{
			_privateKey = Ed25519.ExpandedPrivateKeyFromSeed(seed);
			_publicKey = Ed25519.PublicKeyFromSeed(seed);
		}
	}
}