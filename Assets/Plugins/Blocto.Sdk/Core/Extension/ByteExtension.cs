using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blocto.Sdk.Core.Extension
{
    public static class ByteExtension
    {
        public static string ToHex(this List<byte> inputs)
        {
            var sb = new StringBuilder(inputs.Count * 3);
            foreach (var input in inputs)
            {
                sb.Append(Convert.ToString(input, 16).PadLeft(2, '0').PadRight(3, ' '));
            }
            
            return sb.ToString().Replace(" ", "");
        }
        
        public static string ToHex(this byte[] value, bool isPrefix = false)
        {
            var prefix = isPrefix ? "0x" : "";
            return $"{prefix}{string.Concat(value.Select(b => b.ToString("x2")).ToArray())}";
        }
    }
}