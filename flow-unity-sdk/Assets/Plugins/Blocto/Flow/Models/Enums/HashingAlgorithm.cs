using System.Runtime.Serialization;

namespace Blocto.Flow.Client.Http.Models.Enums
{
    public enum HashingAlgorithm
    {
        [EnumMember(Value = @"SHA2_256")]
        SHA2_256 = 0,

        [EnumMember(Value = @"SHA2_384")]
        SHA2_384 = 1,

        [EnumMember(Value = @"SHA3_256")]
        SHA3_256 = 2,

        [EnumMember(Value = @"SHA3_384")]
        SHA3_384 = 3,

        [EnumMember(Value = @"KMAC128")]
        KMAC128 = 4,
    }
}