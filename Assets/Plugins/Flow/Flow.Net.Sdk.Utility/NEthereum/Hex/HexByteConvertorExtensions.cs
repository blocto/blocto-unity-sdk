using System.Linq;

namespace Flow.Net.Sdk.Utility.NEthereum.Hex
{
    public static class HexByteConvertorExtensions
    {
        public static string ToHex(this byte[] value, bool prefix = false)
        {
            var strPrex = prefix ? "0x" : "";
            return strPrex + string.Concat(value.Select(b => b.ToString("x2")).ToArray());
        }
    }
}