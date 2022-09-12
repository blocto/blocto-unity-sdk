using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Blocto.Sdk.Core.Extension;
using Flow.Net.Sdk.Core;
using Flow.Net.Sdk.Core.Models;
using Flow.Net.SDK.Extensions;
using Flow.Net.Sdk.Utility.NEthereum.Hex;
using Newtonsoft.Json;

namespace Blocto.Sdk.Core.Utility
{
    public static class RLP
    {
        public static string GetEncodeMessage(FlowTransaction tx)
        {
            var data = RLP.EncodeTransaction(tx);
            var encodeBytes = RlpEncode(data);
            var message = RLP.CreateEncodeMessageWithDomainTag(encodeBytes);

            return message;
        }
        
        public static string GetEncodeMessage(string appIdentifier, string address, string nonce)
        {
            var datas = new List<object>(); 
            var leftPaddedhexAddress = address.PadLeft(16, '0');
            datas.Add(Encoding.UTF8.GetBytes(appIdentifier).ToList());
            datas.Add(leftPaddedhexAddress.HexToBytes().ToList());
            datas.Add(nonce.HexToBytes().ToList());
            var enocdeBytes = RlpEncode(datas);
            var message = enocdeBytes.ToHex();
            return message;
        }

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

        private static List<byte> HexToByte(string hex) => Enumerable.Range(0, hex.Length)
                              .Where(x => x % 2 == 0)
                              .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                              .ToList();

        private static List<object> EncodeTransaction(FlowTransaction tx)
        {
            var datas = new List<object>();
            datas.Add(Encoding.UTF8.GetBytes(tx.Script).ToList());
            
            var tmp = tx.Arguments.Select(item => { 
                                                      $"Arg: {JsonConvert.SerializeObject(item)}".ToLog();
                                                      return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(item)).ToList();
                                                  })
                                               .ToList();
            datas.Add(tmp);
            datas.Add(tx.ReferenceBlockId.HexToBytes().ToList());
            datas.Add(GetBytes(tx.GasLimit));
            datas.Add(tx.ProposalKey.Address.Address.HexToBytes().ToList());
            datas.Add(GetBytes(tx.ProposalKey.KeyId).ToList());
            datas.Add(GetBytes(tx.ProposalKey.SequenceNumber).ToList());
            datas.Add(tx.Payer.Address.ToString().HexToBytes().ToList());
            datas.Add(new List<List<byte>> { tx.Authorizers.First().Address.HexToBytes().ToList() });

            return datas;
        }

        private static string CreateEncodeMessageWithDomainTag(List<byte> encodeBytes)
        {
            var messageBytes = DomainTag.AddTransactionDomainTag(encodeBytes.ToArray());
            var message = messageBytes.ToHex();
            return message;
        }

        public static string EncodedCanonicalAuthorizationEnvelope(FlowTransaction tx)
        {
            var tmp = tx.PayloadSignatures as List<FlowSignature>;
            var authEnvelopeElements = new List<object>
                                       {
                                           EncodeTransaction(tx),
                                           EncodedSignatures(tmp, tx.SignerList)
                                       };
            
            var bytes = RlpEncode(authEnvelopeElements);
            var messageBytes = DomainTag.AddTransactionDomainTag(bytes.ToArray());
            var message = messageBytes.ToHex();
            return message;
        }
        
        private static List<List<List<byte>>> EncodedSignatures(IReadOnlyList<FlowSignature> signatures, Dictionary<string, int> signers)
        {
            var signatureElements = new List<List<List<byte>>>();
            for (var i = 0; i < signatures.Count; i++)
            {
                var index = i;
                if (signers.ContainsKey(signatures[i].Address.Address))
                {
                    index = signers[signatures[i].Address.Address];
                }
                else
                {
                    signers.Add(signatures[i].Address.Address, i);
                }

                var signatureEncoded = EncodedSignature(signatures[i], index);
                signatureElements.Add(signatureEncoded);
            }

            return signatureElements;
        }
        
        private static List<List<byte>> EncodedSignature(FlowSignature signature, int index)
        {
            var signatures = new List<List<byte>>
                             {
                                 GetBytes(index),
                                 GetBytes(signature.KeyId),
                                 signature.Signature.ToList()
                             };
            
            return signatures;
        }
        
        private static bool IsCollection(object input)
        {
            if (input is not IList tmp)
            {
                return false;
            }
            
            if(tmp.Count == 0)
            {
                return false;
            }

            var elementType = tmp[0].GetType();                
            var isList = elementType.GetInterfaces().Any(s => s.Name is "IEnumerable" or "ICollection" or "IList");
            var isArray = elementType.IsArray;
            return isList || isArray;
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
        
        private static List<byte> RlpEncode(IEnumerable? inputs)
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

        private static List<byte> GetBytes(int item)
        {
            if(item == 0)
            {
                return new List<byte>();
            }
            
            var hex = item.IntToHex();
            var result = HexToByte(hex);
            return result.ToList();
        }

        private static List<byte> GetBytes(ulong item)
        {
            if(item == 0)
            {
                return new List<byte>();
            }
            
            var hex = item.UlongToHex();
            var result = HexToByte(hex);
            return result.ToList();
        }

        private static List<byte> GetBytes(uint item)
        {
            if(item == 0)
            {
                return new List<byte>();
            }
            
            var hex = item.IntToHex();
            var result = HexToByte(hex);
            return result.ToList();
        } 
        
        
    }
}