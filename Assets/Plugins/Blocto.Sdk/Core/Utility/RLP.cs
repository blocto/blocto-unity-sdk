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
        public static string GetEncodeMessage(FlowTransaction tx, string authorizer)
        {
            var data = RLP.EncodeTransaction(tx, authorizer);
            var encodeBytes = RLPUtility.RlpEncode(data);
            var message = RLP.CreateEncodeMessage(encodeBytes);

            return message;
        }

        private static List<object> EncodeTransaction(FlowTransaction tx, string authorizer)
        {
            var datas = new List<object>();
            datas.Add(Encoding.UTF8.GetBytes(tx.Script).ToList());
            
            var tmp = tx.Arguments.Select(item => { 
                                                      $"Arg: {JsonConvert.SerializeObject(item)}".ToLog();
                                                      return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(item)).ToList();
                                                  })
                                               .ToList();
            datas.Add(tmp);
            $"RefBlock: {tx.ReferenceBlockId}".ToLog();
            datas.Add(tx.ReferenceBlockId.HexToBytes().ToList());
            
            $"ComputeLimit: {tx.GasLimit}".ToLog();
            datas.Add(RLPUtility.GetBytes(tx.GasLimit));
            
            $"ProposalKey addr: {tx.ProposalKey.Address.Address}".ToLog();
            datas.Add(tx.ProposalKey.Address.Address.HexToBytes().ToList());
            
            $"ProposalKey keyId: {tx.ProposalKey.KeyId}".ToLog();
            datas.Add(RLPUtility.GetBytes(tx.ProposalKey.KeyId).ToList());
            
            $"ProposalKey seqNum: {tx.ProposalKey.SequenceNumber}".ToLog();
            var seqNumBytes = RLPUtility.GetBytes(tx.ProposalKey.SequenceNumber).ToList();
            
            $"ProposalKey seqNum hex: {seqNumBytes.ToHex()}".ToLog();
            datas.Add(RLPUtility.GetBytes(tx.ProposalKey.SequenceNumber).ToList());
            
            $"Payer addr: {tx.Payer.Address}".ToLog();
            datas.Add(tx.Payer.Address.ToString().HexToBytes().ToList());
            
            $"authorizers: {authorizer}".ToLog();
            datas.Add(new List<List<byte>> { authorizer.HexToBytes().ToList() });

            return datas;
        }

        private static string CreateEncodeMessage(List<byte> encodeBytes)
        {
            var messageBytes = DomainTag.AddTransactionDomainTag(encodeBytes.ToArray());
            var message = messageBytes.ToHex();
            $"Message: {message}".ToLog();
            return message;
        }

        public static string EncodedCanonicalAuthorizationEnvelope(FlowTransaction tx, Signable signable, string authorizer)
        {
            var signatures = signable.Voucher.PayloadSigs.Select(p => {
                                                                     var sign = new FlowSignature {
                                                                                    Address = new FlowAddress(p.GetValue("address")!.ToString()),
                                                                                    KeyId = Convert.ToUInt32(p.GetValue("keyId")),
                                                                                    Signature = p.GetValue("sig")!.ToString().HexToBytes()
                                                                                };
                                                                     return sign;
                                                                 }).ToList();
            $"before collect signers".ToLog();
            var signers = CollectSigners(signable);
            $"after collect signers".ToLog();
            var authEnvelopeElements = new List<object>
                                       {
                                           EncodeTransaction(tx, authorizer),
                                           EncodedSignatures(signatures, signers)
                                       };
            var bytes = RLPUtility.RlpEncode(authEnvelopeElements);
            var messageBytes = DomainTag.AddTransactionDomainTag(bytes.ToArray());
            var message = messageBytes.ToHex();
            message.ToLog();
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