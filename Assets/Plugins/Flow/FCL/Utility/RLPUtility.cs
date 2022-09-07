using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Flow.Net.SDK.Extensions;
using Plugins.Blocto.Sdk.Core.Extension;

namespace Flow.FCL.Utility
{
    public static class RLPUtility
    {
        public static List<byte> RlpEncode(IEnumerable? inputs)
        {
            var elements = ElementsEncode(inputs);
            var result = new List<byte>();
            foreach (var element in elements)
            {
                result.AddRange(element as IList<byte> ?? throw new InvalidOperationException());
            }
            
            var finalBytes = CalculatorLength(result, 192);
            return finalBytes;
        } 
        
        public static List<byte> GetBytes(int item)
        {
            var hex = IntToHex(item);
            var result = HexToByte(hex);
            return result.ToList();
        } 
        
        public static List<byte> GetBytes(ulong item)
        {
            var hex = RLPUtility.UlongToHex(item);
            var result = HexToByte(hex);
            return result.ToList();
        } 
        
        public static List<byte> GetBytes(uint item)
        {
            var hex = IntToHex(item);
            var result = HexToByte(hex);
            return result.ToList();
        } 

        
        private static IEnumerable ElementsEncode(IEnumerable? inputs)
        {
            var outputs = new List<object?>();
            foreach (var input in inputs)
            {
                var isCollection = RLPUtility.IsCollection(input);
                if(isCollection)
                {
                    var subCollection = RlpEncode(input as IList);
                    outputs.Add(subCollection);
                }
                else
                {
                    if(input is not List<byte> tmpInput)
                    {
                        throw new Exception("Type is invalided.");
                    }
                    
                    if(tmpInput.Count == 1 && tmpInput[0] < 128)
                    {
                        outputs.Add(tmpInput);
                    }
                    else
                    {
                        var tmp = CalculatorLength(tmpInput, 128);
                        outputs.Add(tmp);
                    }
                }
            }

            return outputs;
        }

        private static bool IsCollection(object input)
        {
            if (input is not IList tmp)
            {
                return false;
            }
            
            // var elementType = tmp.GetType();
            // var isList = elementType.GetInterfaces().Any(s => s.Name is "IEnumerable" or "ICollection" or "IList");
            // var isArray = elementType.IsArray;
            // if(isList || isArray)
            //     
            // if(tmp.Count <= 0 )
            // {
            //     return false;
            // }

            var elementType = tmp[0].GetType();                
            var isList = elementType.GetInterfaces().Any(s => s.Name is "IEnumerable" or "ICollection" or "IList");
            var isArray = elementType.IsArray;
            return isList || isArray;
        }

        private static IList CalculatorLength(IList item, int offset)
        {
            if(item.Count < 56)
            {
                item.Insert(0, Convert.ToByte(item.Count + offset));
            }
            else
            {
                var hexLength = IntToHex(item.Count);
                var lLength = hexLength.Length / 2;
                var firstBtye = IntToHex(offset + 55 + lLength);
                var result = HexToByte($"{firstBtye}{hexLength}");
                for (var i = 0; i < result.Count; i++)
                {
                    item.Insert(i, result[i]);
                }
            }
            
            return item;
        }
        
        private static List<byte> CalculatorLength(List<byte> item, int offset)
        {
            if(item.Count < 56)
            {
                item.Insert(0, Convert.ToByte(item.Count + offset));
            }
            else
            {
                var hexLength = IntToHex(item.Count);
                var lLength = hexLength.Length / 2;
                var firstBtye = IntToHex(offset + 55 + lLength);
                var result = HexToByte($"{firstBtye}{hexLength}");
                item.InsertRange(0, result);
            }
            
            return item;
        }

        private static string IntToHex(int number)
        {
            var hex = number.ToString("x");
            return hex.Length % 2 == 0 ? hex : hex.PadLeft(hex.Length + 1, '0');
        }
        
        private static string UlongToHex(ulong number)
        {
            var hex = number.ToString("x");
            return hex.Length % 2 == 0 ? hex : hex.PadLeft(hex.Length + 1, '0');
        }
        
        private static string IntToHex(uint number)
        {
            var hex = number.ToString("x");
            return hex.Length % 2 == 0 ? hex : hex.PadLeft(hex.Length + 1, '0');
        }
        
        private static List<byte> HexToByte(string hex) => Enumerable.Range(0, hex.Length)
                              .Where(x => x % 2 == 0)
                              .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                              .ToList();
    }
}