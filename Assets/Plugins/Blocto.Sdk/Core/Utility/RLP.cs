using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blocto.Sdk.Core.Extension;

namespace Blocto.Sdk.Core.Utility
{
    public static class RLP
    {
        private static IEnumerable ElementsEncode(IEnumerable inputs)
        {
            var outputs = new List<object>();
            foreach (var input in inputs)
            {
                var isCollection = IsCollection(input);
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
            if(item == 0)
            {
                return new List<byte>();
            }
            
            var hex = item.IntToHex();
            var result = HexToByte(hex);
            return result.ToList();
        }

        public static List<byte> GetBytes(ulong item)
        {
            if(item == 0)
            {
                return new List<byte>();
            }
            
            var hex = item.UlongToHex();
            var result = HexToByte(hex);
            return result.ToList();
        }

        public static List<byte> GetBytes(uint item)
        {
            if(item == 0)
            {
                return new List<byte>();
            }
            
            var hex = item.IntToHex();
            var result = HexToByte(hex);
            return result.ToList();
        } 

        private static List<byte> HexToByte(string hex) => Enumerable.Range(0, hex.Length)
                              .Where(x => x % 2 == 0)
                              .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                              .ToList();
        
        private static bool IsCollection(object input)
        {
            if (input is not IList tmp)
            {
                return false;
            }
            
            var type = input.GetType();
            var isList = default(bool);
            if(type.GenericTypeArguments.Length > 0)
            {
                isList = type.GenericTypeArguments.Any(p => p.GetInterfaces().Any(s => s.Name is "IEnumerable" or "ICollection" or "IList"));
            }
            
            switch (tmp.Count)
            {
                case 0 when isList == false:
                    return false;
                case > 0: {
                    var elementType = tmp[0].GetType();                
                    isList = elementType.GetInterfaces().Any(s => s.Name is "IEnumerable" or "ICollection" or "IList");
                    var isArray = elementType.IsArray;
                    return isList || isArray;
                }
                default:
                    return isList;
            }
        }
        
        private static List<byte> CalculatorLength(List<byte> item, int offset)
        {
            if(item.Count < 56)
            {
                item.Insert(0, Convert.ToByte(item.Count + offset));
            }
            else
            {
                var hexLength = item.Count.IntToHex();
                var lLength = hexLength.Length / 2;
                var firstBtye = (offset + 55 + lLength).IntToHex();
                var result = HexToByte($"{firstBtye}{hexLength}");
                item.InsertRange(0, result);
            }
            
            return item;
        } 
    }
}