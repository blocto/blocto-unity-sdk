using System.Numerics;
using Blocto.Sdk.Evm.Model.Hex.Util;
using Newtonsoft.Json;

namespace Blocto.Sdk.Evm.Model.Hex
{
    [JsonConverter(typeof(HexRPCTypeJsonConverter<HexBigInteger, BigInteger>))]
    public class HexBigInteger : HexRPCType<BigInteger>
    {
        public HexBigInteger(string hex) : base(new HexBigIntegerBigEndianConvertor(), hex)
        {
        }

        public HexBigInteger(BigInteger value) : base(value, new HexBigIntegerBigEndianConvertor())
        {
        }

        
        public override string ToString()
        {
            return Value.ToString();
        }
    }
}