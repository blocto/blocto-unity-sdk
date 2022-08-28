using System;
using System.Collections.Generic;
using System.Text;

namespace Plugins.Blocto.Sdk.Core.Extension
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
    }
}