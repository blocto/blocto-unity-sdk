using System.Runtime.Serialization;

namespace Blocto.Flow.Client.Http.Models.Enums
{
    public enum SigningAlgorithm
    {
        [EnumMember(Value = @"BLSBLS12381")]
        BLSBLS12381 = 0,

        [EnumMember(Value = @"ECDSA_P256")]
        ECDSA_P256 = 1,

        [EnumMember(Value = @"ECDSA_secp256k1")]
        ECDSA_secp256k1 = 2,
    }
}