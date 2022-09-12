using System;
using System.Collections.Generic;
using System.Linq;

namespace Blocto.Sdk.Core.Extension
{
    public static class NumberExtension
    {
        public static string IntToHex(this int number)
        {
            var hex = number.ToString("x");
            return hex.Length % 2 == 0 ? hex : hex.PadLeft(hex.Length + 1, '0');
        } 
        
        public static string IntToHex(this uint number)
        {
            var hex = number.ToString("x");
            return hex.Length % 2 == 0 ? hex : hex.PadLeft(hex.Length + 1, '0');
        }
        
        public static string UlongToHex(this ulong number)
        {
            var hex = number.ToString("x");
            return hex.Length % 2 == 0 ? hex : hex.PadLeft(hex.Length + 1, '0');
        }
        
        public static List<byte> GetBytes(this int item)
        {
            var hex = item.IntToHex();
            var result = HexToByte(hex);
            return result.ToList();
        }

        public static List<byte> GetBytes(this ulong item)
        {
            var hex = item.UlongToHex();
            var result = HexToByte(hex);
            return result.ToList();
        }

        public static List<byte> GetBytes(this uint item)
        {
            var hex = item.IntToHex();
            var result = HexToByte(hex);
            return result.ToList();
        } 
        
        private static List<byte> HexToByte(string hex) => Enumerable.Range(0, hex.Length)
                                                                     .Where(x => x % 2 == 0)
                                                                     .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                                                                     .ToList();
    }
}