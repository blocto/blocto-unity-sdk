using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Flow.FCL.Models.Authz;
using Flow.FCL.Utility;
using Flow.Net.Sdk.Core;
using Flow.Net.Sdk.Core.Models;
using Flow.Net.SDK.Extensions;
using Flow.Net.Sdk.Utility.NEthereum.Hex;
using Newtonsoft.Json;
using Plugins.Blocto.Sdk.Core.Extension;

namespace Blocto.Sdk.Core.Utility
{
    public static class RLP
    {
        public static string GetEncodeMessage(FlowTransaction tx)
        {
            var data = RLP.EncodeTransaction(tx);
            var encodeBytes = RLPUtility.RlpEncode(data);
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
            var enocdeBytes = RLPUtility.RlpEncode(datas);
            var message = enocdeBytes.ToHex();
            return message;
        }

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
            datas.Add(RLPUtility.GetBytes(tx.GasLimit));
            datas.Add(tx.ProposalKey.Address.Address.HexToBytes().ToList());
            datas.Add(RLPUtility.GetBytes(tx.ProposalKey.KeyId).ToList());
            datas.Add(RLPUtility.GetBytes(tx.ProposalKey.SequenceNumber).ToList());
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
            
            var bytes = RLPUtility.RlpEncode(authEnvelopeElements);
            var messageBytes = DomainTag.AddTransactionDomainTag(bytes.ToArray());
            var message = messageBytes.ToHex();
            return message;
        }
        
        private static Dictionary<string, int> CollectSigners(Signable signable)
        {
            var signers = new Dictionary<string, int>
                          {
                              {signable.Interaction.Proposer.Split("-")[0],  0},
                          };
            
            var payerAddr = signable.Interaction.Payer.Split("-")[0];
            if(!signers.ContainsKey(payerAddr))
            {
                signers.Add(payerAddr, 1);
            }

            foreach (var authorization in signable.Interaction.Authorizations.Where(authorization => !signers.ContainsKey(authorization)))
            {
                var addr = authorization.Split("-")[0];
                if(!signers.ContainsKey(addr))
                {
                    signers.Add(addr, signers.Count);
                }
            }
            
            return signers;
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
                                 RLPUtility.GetBytes(index).ToList(),
                                 RLPUtility.GetBytes(signature.KeyId).ToList(),
                                 signature.Signature.ToList()
                             };
            
            return signatures;
        }
    }
}