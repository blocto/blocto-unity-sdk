using System.Collections.Generic;
using System.Linq;
using System.Text;
using Blocto.Sdk.Core.Extension;
using Blocto.Sdk.Core.Utility;
using Flow.Net.Sdk.Core;
using Flow.Net.Sdk.Core.Models;
using Flow.Net.Sdk.Utility.NEthereum.Hex;
using Newtonsoft.Json;

namespace Blocto.Sdk.Flow.Utility
{
    public static class EncodeUtility
    {
        public static string GetEncodeMessage(FlowTransaction tx)
        {
            var data = EncodeTransaction(tx);
            var encodeBytes = RLP.RlpEncode(data);
            var message = CreateEncodeMessageWithDomainTag(encodeBytes);

            return message;
        }
        
        public static string GetEncodeMessage(string appIdentifier, string address, string nonce)
        {
            var datas = new List<object>(); 
            var leftPaddedhexAddress = address.PadLeft(16, '0');
            datas.Add(Encoding.UTF8.GetBytes(appIdentifier).ToList());
            datas.Add(leftPaddedhexAddress.HexToBytes().ToList());
            datas.Add(nonce.HexToBytes().ToList());
            var enocdeBytes = RLP.RlpEncode(datas);
            var message = enocdeBytes.ToHex();
            return message;
        } 
        
        private static List<object> EncodeTransaction(FlowTransaction tx)
        {
            var datas = new List<object>();
            datas.Add(Encoding.UTF8.GetBytes(tx.Script).ToList());
            
            var tmp = tx.Arguments.Select(item => { 
                                                      return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(item)).ToList();
                                                  })
                                               .ToList();
            datas.Add(tmp);
            datas.Add(tx.ReferenceBlockId.HexToBytes().ToList());
            datas.Add(RLP.GetBytes(tx.GasLimit));
            datas.Add(tx.ProposalKey.Address.Address.HexToBytes().ToList());
            datas.Add(RLP.GetBytes(tx.ProposalKey.KeyId).ToList());
            datas.Add(RLP.GetBytes(tx.ProposalKey.SequenceNumber).ToList());
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
            
            var bytes = RLP.RlpEncode(authEnvelopeElements);
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
                                 RLP.GetBytes(index),
                                 RLP.GetBytes(signature.KeyId),
                                 signature.Signature.ToList()
                             };
            
            return signatures;
        }
    }
}