using Chaos.NaCl;
using Solnet.Wallet.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Solnet.Wallet
{
    /// <summary>
    /// Implements the public key functionality.
    /// </summary>
    [DebuggerDisplay("Key = {ToString()}")]
    public class PublicKey
    {
        /// <summary>
        /// Public key length.
        /// </summary>
        public const int PublicKeyLength = 32;

        private string _key;

        /// <summary>
        /// The key as base-58 encoded string.
        /// </summary>
        public string Key
        {
            get
            {
                if (_key == null)
                {
                    Key = Encoders.Base58.EncodeData(KeyBytes);
                }
                return _key;
            }
            set => _key = value;
        }


        private byte[] _keyBytes;

        /// <summary>
        /// The bytes of the key.
        /// </summary>
        public byte[] KeyBytes
        {
            get
            {
                if (_keyBytes == null)
                {
                    KeyBytes = Encoders.Base58.DecodeData(Key);
                }
                return _keyBytes;
            }
            set => _keyBytes = value;
        }
        
        private sbyte[] _keySBytes;

        /// <summary>
        /// The bytes of the key.
        /// </summary>
        public sbyte[] KeySBytes
        {
            get
            {
                if (_keySBytes == null)
                {
                    KeySBytes = Encoders.Base58.DecodeDataWithSByte(Key);
                }
                return _keySBytes;
            }
            set => _keySBytes = value;
        }

        /// <summary>
        /// Initialize the public key from the given byte array.
        /// </summary>
        /// <param name="key">The public key as byte array.</param>
        public PublicKey(byte[] key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (key.Length != PublicKeyLength)
                throw new ArgumentException("invalid key length", nameof(key));
            KeyBytes = new byte[PublicKeyLength];
            Array.Copy(key, KeyBytes, PublicKeyLength);
        }

        /// <summary>
        /// Initialize the public key from the given string.
        /// </summary>
        /// <param name="key">The public key as base58 encoded string.</param>
        public PublicKey(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (!FastCheck(key)) throw new ArgumentException("publickey contains a non-base58 character");
            Key = key;
        }

        /// <summary>
        /// Initialize the public key from the given string.
        /// </summary>
        /// <param name="key">The public key as base58 encoded string.</param>
        public PublicKey(ReadOnlySpan<byte> key)
        {
            if (key.Length != PublicKeyLength)
                throw new ArgumentException("invalid key length", nameof(key));
            KeyBytes = new byte[PublicKeyLength];
            key.CopyTo(KeyBytes.AsSpan());
        }

        /// <summary>
        /// Verify the signed message.
        /// </summary>
        /// <param name="message">The signed message.</param>
        /// <param name="signature">The signature of the message.</param>
        /// <returns></returns>
        public bool Verify(byte[] message, byte[] signature)
        {
            return Ed25519.Verify(signature, message, KeyBytes);
        }

        /// <inheritdoc cref="Equals(object)"/>
        public override bool Equals(object obj)
        {
            if (obj is PublicKey pk) return pk.Key == this.Key;

            return false;
        }

        /// <inheritdoc />
        public static bool operator ==(PublicKey lhs, PublicKey rhs)
        {

            if (lhs is null)
            {
                if (rhs is null)
                {
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }

        /// <inheritdoc />
        public static bool operator !=(PublicKey lhs, PublicKey rhs) => !(lhs == rhs);

        /// <summary>
        /// Conversion between a <see cref="PublicKey"/> object and the corresponding base-58 encoded public key.
        /// </summary>
        /// <param name="publicKey">The PublicKey object.</param>
        /// <returns>The base-58 encoded public key.</returns>
        public static implicit operator string(PublicKey publicKey) => publicKey.Key;

        /// <summary>
        /// Conversion between a base-58 encoded public key and the <see cref="PublicKey"/> object.
        /// </summary>
        /// <param name="address">The base-58 encoded public key.</param>
        /// <returns>The PublicKey object.</returns>
        public static explicit operator PublicKey(string address) => new(address);

        /// <summary>
        /// Conversion between a <see cref="PublicKey"/> object and the public key as a byte array.
        /// </summary>
        /// <param name="publicKey">The PublicKey object.</param>
        /// <returns>The public key as a byte array.</returns>
        public static implicit operator byte[](PublicKey publicKey) => publicKey.KeyBytes;

        /// <summary>
        /// Conversion between a public key as a byte array and the corresponding <see cref="PublicKey"/> object.
        /// </summary>
        /// <param name="keyBytes">The public key as a byte array.</param>
        /// <returns>The PublicKey object.</returns>
        public static explicit operator PublicKey(byte[] keyBytes) => new(keyBytes);
        

        /// <inheritdoc cref="ToString"/>
        public override string ToString() => Key;

        /// <inheritdoc cref="GetHashCode"/>
        public override int GetHashCode() => Key.GetHashCode();


        /// <summary>
        /// Checks if this object is a valid Ed25519 PublicKey.
        /// </summary>
        /// <returns>Returns true if it is a valid key, false otherwise.</returns>
        public bool IsOnCurve()
        {
            return Ed25519Extensions.IsOnCurve(KeyBytes);
        }

        /// <summary>
        /// Checks if this object is a valid Solana PublicKey.
        /// </summary>
        /// <returns>Returns true if it is a valid key, false otherwise.</returns>
        public bool IsValid()
        {
            return KeyBytes != null && KeyBytes.Length == PublicKeyLength;
        }

        /// <summary>
        /// Checks if a given string forms a valid PublicKey in base58.
        /// </summary>
        /// <remarks>
        /// Any set of 32 bytes can constitute a valid solana public key. However, not all 32-byte public keys are valid Ed25519 public keys. <br/>
        /// Two concrete examples: <br/>
        /// - A user wallet key must be on the curve (otherwise a user wouldn't be able to sign transactions).  <br/>
        /// - A program derived address must NOT be on the curve.
        /// </remarks>
        /// <param name="key">The base58 encoded public key.</param>
        /// <param name="validateCurve">Whether or not to validate if the public key belongs to the Ed25519 curve.</param>
        /// <returns>Returns true if the input is a valid key, false otherwise.</returns>
        public static bool IsValid(string key, bool validateCurve = false)
        {
            if (!string.IsNullOrEmpty(key))
            {
                try
                {
                    if (!FastCheck(key)) return false;
                    return IsValid(Encoders.Base58.DecodeData(key), validateCurve);
                }
                catch(Exception)
                {
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if a given set of bytes forms a valid PublicKey.
        /// </summary>
        /// <remarks>
        /// Any set of 32 bytes can constitute a valid solana public key. However, not all 32-byte public keys are valid Ed25519 public keys. <br/>
        /// Two concrete examples: <br/>
        /// - A user wallet key must be on the curve (otherwise a user wouldn't be able to sign transactions).  <br/>
        /// - A program derived address must NOT be on the curve.
        /// </remarks>
        /// <param name="key">The key bytes.</param>
        /// <param name="validateCurve">Whether or not to validate if the public key belongs to the Ed25519 curve.</param>
        /// <returns>Returns true if the input is a valid key, false otherwise.</returns>
        public static bool IsValid(byte[] key, bool validateCurve = false)
        {
            return key != null && key.Length == PublicKeyLength && (!validateCurve || key.IsOnCurve());
        }
        
        /// <summary>
        /// Checks if a given set of bytes forms a valid PublicKey.
        /// </summary>
        /// <remarks>
        /// Any set of 32 bytes can constitute a valid solana public key. However, not all 32-byte public keys are valid Ed25519 public keys. <br/>
        /// Two concrete examples: <br/>
        /// - A user wallet key must be on the curve (otherwise a user wouldn't be able to sign transactions).  <br/>
        /// - A program derived address must NOT be on the curve.
        /// </remarks>
        /// <param name="key">The key bytes.</param>
        /// <param name="validateCurve">Whether or not to validate if the public key belongs to the Ed25519 curve.</param>
        /// <returns>Returns true if the input is a valid key, false otherwise.</returns>
        public static bool IsValid(ReadOnlySpan<byte> key, bool validateCurve = false)
        {
            return key != null && key.Length == PublicKeyLength && (!validateCurve || key.IsOnCurve());
        }

        /// <summary>
        /// Fast validation to determine whether this is a valid public key input pattern. 
        /// Checks are valid characters for base58 and no whitespace.
        /// Avoids performing the conversion to a buffer and checking it is actually 32 bytes as a permformance trade-off.
        /// </summary>
        /// <param name="value">public key value to check</param>
        /// <returns>true means good, false means bad</returns>
        private static bool FastCheck(string value)
        {
            return Base58Encoder.IsValidWithoutWhitespace(value);
        }

        #region KeyDerivation

        /// <summary>
        /// The bytes of the `ProgramDerivedAddress` string.
        /// </summary>
        private static readonly byte[] ProgramDerivedAddressBytes = Encoding.UTF8.GetBytes("ProgramDerivedAddress");

        #endregion
    }
}