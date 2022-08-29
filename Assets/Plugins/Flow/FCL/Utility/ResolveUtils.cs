using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Flow.FCL.Models.Authz;
using Flow.Net.Sdk.Core;
using Flow.Net.Sdk.Core.Cadence;
using Flow.Net.Sdk.Core.Models;
using Flow.Net.SDK.Extensions;
using Flow.Net.Sdk.Utility.NEthereum.Hex;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Flow.FCL.Utility
{
    public class ResolveUtils : IResolveUtils
    {
        private Signable _preSignable;
        
        private FlowTransaction _tx;
        
        public PreSignable ResolvePreSignable(List<BaseArgument> args, string script, int computeLimit)
        {
            var argumentDict = new Dictionary<string, Argument>();
            var messageArgs = new List<string>();
            foreach (var arg in args)
            {
                var tmpId = KeyGenerator.GetUniqueKey(10).ToLower();
                var tmpArgument = CreateArgument(tmpId, arg.Type);
                tmpArgument.Value = arg.Value.ToString();
                tmpArgument.AsBaseArgument.Value = arg.Value;
                argumentDict.Add(tmpId, tmpArgument);
                
                messageArgs.Add(tmpId);
            }    
            
            _tx = new FlowTransaction
                     {
                         Script = script
                     };
            
            var message = new Message
                          {
                              Cadence = _tx.Script,
                              ComputeLimit = computeLimit,
                              Arguments = messageArgs,
                              // RefBlock = "747039f6e88261b8fc87139b5feb0e7f36465134f93db2b0281c2ca5eaed3175"
                          };
            
            var voucher = new Voucher
                          {
                              Cadence = _tx.Script,
                              ComputeLimit = computeLimit,
                              Arguments = args,
                          }; 
            
            var preSignable = new Signable()
                              {
                                  F_Type = "PreSignable",
                                  Roles = new Role
                                          {
                                              Proposer = true,
                                              Authorizer = true,
                                              Payer = true,
                                              Param = false
                                          },
                                  Cadence = _tx.Script,
                                  Args = args,
                                  Interaction = new Interaction
                                                {
                                                    Tag = "TRANSACTION",
                                                    Status = "OK",
                                                    Accounts = new Dictionary<string, JObject>
                                                               {
                                                                   // {
                                                                   //     "CURRENT_USER", new JObject
                                                                   //                     {
                                                                   //                         new JProperty("tempId", "CURRENT_USER"),
                                                                   //                         new JProperty("kind", "ACCOUNT"),
                                                                   //                         new JProperty("role", new JObject
                                                                   //                                               {
                                                                   //                                                   new JProperty("payer", true),
                                                                   //                                                   new JProperty("proposer", true),
                                                                   //                                                   new JProperty("authorizer", true),
                                                                   //                                                   new JProperty("param", false)
                                                                   //                                               })
                                                                   //                     }
                                                                   // }
                                                               },
                                                    Arguments = argumentDict,
                                                    Message = message,
                                                    // Proposer = "CURRENT_USER",
                                                    // Authorizations = new List<string> { "CURRENT_USER" },
                                                    // Payer = "CURRENT_USER",
                                                },
                                  Voucher = voucher
                              };
            
            _preSignable = preSignable;
            return preSignable;
        }
        
        public PreSignable ResolvePreSignable(string addr, string script, string lastBlockId)
        {
            _tx = new FlowTransaction
                  {
                      Script = script
                  };
            
            var tempId = KeyGenerator.GetUniqueKey(10).ToLower();
            $"TempId: {tempId}".ToLog();

            var argument1 = CreateArgument(tempId, "UFix64");
            argument1.Value = 10;
            argument1.AsBaseArgument.Value = 10;

            tempId = KeyGenerator.GetUniqueKey(10);
            $"TempId: {tempId}".ToLog();

            var argument2 = CreateArgument(tempId, "Address");
            argument2.Value = addr;
            argument2.AsBaseArgument.Value = addr;

            var message = new Message
                          {
                              Cadence = _tx.Script,
                              ComputeLimit = 9999,
                              Arguments = new List<string>
                                          {
                                              argument1.TempId,
                                              argument2.TempId
                                          },
                              RefBlock = lastBlockId,
                          };

            var args = new List<BaseArgument>
                       {
                           new BaseArgument
                           {
                               Type = "UFix64",
                               Value = 10,
                           },
                           new BaseArgument
                           {
                               Type = "Address",
                               Value = addr
                           }
                       };

            var arguments = new Dictionary<string, Argument>
                            {
                                { argument1.TempId, argument1 },
                                { argument2.TempId, argument2 }
                            };

            var voucher = new Voucher
                          {
                              Cadence = _tx.Script,
                              RefBlock = lastBlockId,
                              ComputeLimit = 9999,
                              Arguments = args,
                          };

            var preSignable = new Signable()
                              {
                                  F_Type = "PreSignable",
                                  Roles = new Role
                                          {
                                              Proposer = true,
                                              Authorizer = true,
                                              Payer = true,
                                              Param = false
                                          },
                                  Cadence = _tx.Script,
                                  Args = args,
                                  Interaction = new Interaction
                                                {
                                                    Tag = "TRANSACTION",
                                                    Status = "OK",
                                                    Accounts = new Dictionary<string, JObject>
                                                               {
                                                                   {
                                                                       "CURRENT_USER", new JObject
                                                                                       {
                                                                                           new JProperty("tempId", "CURRENT_USER"),
                                                                                           new JProperty("kind", "ACCOUNT"),
                                                                                           new JProperty("role", new JObject
                                                                                                                 {
                                                                                                                     new JProperty("payer", true),
                                                                                                                     new JProperty("proposer", true),
                                                                                                                     new JProperty("authorizer", true),
                                                                                                                     new JProperty("param", false)
                                                                                                                 })
                                                                                       }
                                                                   }
                                                               },
                                                    Arguments = arguments,
                                                    Message = message,
                                                    Proposer = "CURRENT_USER",
                                                    Authorizations = new List<string> { "CURRENT_USER" },
                                                    Payer = "CURRENT_USER",
                                                },
                                  Voucher = voucher
                              };
            
            _preSignable = preSignable;
            return preSignable;
        }
        
        public Signable ResolveAuthorizerSignable(Account proposer, Account payer, List<Account> authorizations)
        {
            foreach (var account in authorizations)
            {
                $"Account: {account.TempId}, KeyId: {account.KeyId}, TempId: {account.TempId}".ToLog();
            }
            
            var payloadSigs = new List<JObject>
                              {
                                  new JObject
                                  {
                                      new JProperty("address", authorizations.First().Addr),
                                      new JProperty("keyId", authorizations.First().KeyId)
                                  } 
                              };
            
            var accountDict = new Dictionary<string, JObject>
                              {
                                  {$"{payer.Addr}-{payer.KeyId}", new JObject
                                                                  {
                                                                      new JProperty("tempId", $"{payer.Addr}-{payer.KeyId}"),
                                                                      new JProperty("addr", payer.Addr),
                                                                      new JProperty("keyId", payer.KeyId),
                                                                      new JProperty("sequenceNum", payer.SequenceNum),
                                                                      new JProperty("role", new JObject
                                                                                            {
                                                                                                new JProperty("proposer", false),
                                                                                                new JProperty("payer", true),
                                                                                                new JProperty("authorizer", false),
                                                                                            }) 
                                                                  }},
                                  {$"{authorizations.First().Addr}-{authorizations.First().KeyId}", new JObject
                                                                                                    {
                                                                                                        new JProperty("tempId", $"{authorizations.First().Addr}-{authorizations.First().KeyId}"),
                                                                                                        new JProperty("addr", authorizations.First().Addr),
                                                                                                        new JProperty("keyId", authorizations.First().KeyId),
                                                                                                        new JProperty("role", new JObject
                                                                                                                              {
                                                                                                                                  new JProperty("payer", false),
                                                                                                                                  new JProperty("proposer", false),
                                                                                                                                  new JProperty("authorizer", true),
                                                                                                                              })
                                                                                                    }}
                              };
            
            if(_preSignable == null)
            {
                throw new Exception("PreAuth not exist");
            }
            
            var signable = _preSignable.DeepCopy();
            $"Create Signable".ToLog();
            CreateSignable(signable, proposer, payer, authorizations, accountDict, payloadSigs);
            signable!.Message = GetEncodeMessage(signable, authorizations.First().Addr, signable.Interaction.Message.RefBlock);
            
            // var tx = new FlowTransaction
            //          {
            //              Script = signable.Cadence,
            //              GasLimit = Convert.ToUInt64(signable.Interaction.Message.ComputeLimit),
            //              Payer = new FlowAddress(payer.Addr),
            //              ProposalKey = new FlowProposalKey
            //                            {
            //                                Address = new FlowAddress(signable.Voucher.ProposalKey.Address.ToString()),
            //                                KeyId = Convert.ToUInt32(signable.Voucher.ProposalKey.KeyId),
            //                                SequenceNumber = Convert.ToUInt64(signable.Voucher.ProposalKey.SequenceNum)
            //                            },
            //              ReferenceBlockId = signable.Interaction.Message.RefBlock,
            //              Arguments = new List<ICadence>
            //                          {
            //                              new CadenceNumber(CadenceNumberType.UFix64, "7.50000000"),
            //                              new CadenceAddress("068606b2acddc1ca")
            //                          }
            //          };
            //
            // tx.Authorizers.Add(new FlowAddress(authorizations.First().Addr));
            // tx.SignerList.Add(authorizations.First().Addr, 1);
            // var canonicalPayload = Rlp.EncodedCanonicalPayload(tx);
            // var message = DomainTag.AddTransactionDomainTag(canonicalPayload);
            // signable.Message = message.BytesToHex();
            
            return signable;
        }
        
        public Signable ResolvePayerSignable(Account payer, Account authorizer, Signable signable, string signature)
        {
           var payloadSigs = new List<JObject>
                              {
                                  new JObject
                                  {
                                      new JProperty("address", authorizer.Addr),
                                      new JProperty("keyId", authorizer.KeyId),
                                      new JProperty("sig", signature)
                                  } 
                              };
            
            var accountDict = new Dictionary<string, JObject>
                              {
                                  {$"{payer.Addr}-{payer.KeyId}", new JObject
                                                                  {
                                                                      new JProperty("tempId", $"{payer.Addr}-{payer.KeyId}"),
                                                                      new JProperty("addr", payer.Addr),
                                                                      new JProperty("keyId", payer.KeyId),
                                                                      new JProperty("sequenceNum", signable.Voucher.ProposalKey.SequenceNum),
                                                                      new JProperty("role", new JObject
                                                                                            {
                                                                                                new JProperty("proposer", false),
                                                                                                new JProperty("payer", true),
                                                                                                new JProperty("authorizer", false),
                                                                                            }) 
                                                                  }},
                                  {$"{authorizer.Addr}-{authorizer.KeyId}", new JObject
                                                                                  {
                                                                                      new JProperty("tempId", $"{authorizer.Addr}-{authorizer.KeyId}"),
                                                                                      new JProperty("addr", authorizer.Addr),
                                                                                      new JProperty("keyId", authorizer.KeyId),
                                                                                      new JProperty("role", new JObject
                                                                                                            {
                                                                                                                new JProperty("payer", false),
                                                                                                                new JProperty("proposer", false),
                                                                                                                new JProperty("authorizer", true),
                                                                                                            }),
                                                                                      new JProperty("signature", signature)
                                                                                  }}
                              };
            
            if(_preSignable == null)
            {
                throw new Exception("PreAuth not exist");
            }
            
            $"Create PayerSignable".ToLog();
            var payerSignable = signable.DeepCopy();
            payerSignable.Interaction.Accounts = accountDict;
            payerSignable.Voucher.PayloadSigs = payloadSigs;
            payerSignable.Addr = payer.Addr;
            payerSignable.KeyId = payer.KeyId;
            payerSignable.Roles = new JObject
                                  {
                                      new JProperty("proposer", false),
                                      new JProperty("payer", true),
                                      new JProperty("authorizer", false),
                                  };
            var message = EncodedCanonicalAuthorizationEnvelope(payerSignable, authorizer.Addr);
            payerSignable.Message = message;
            return payerSignable; 
        }
        
        private string GetEncodeMessage(Signable signable, string authorizer, string lastBlockId)
        {
            var data = EncodeTransaction(signable, authorizer);
            var encodeBytes = RLPUtility.RlpEncode(data);
            var message = ResolveUtils.CreateEncodeMessage(encodeBytes);

            return message;
        }

        private List<object> EncodeTransaction(Signable signable, string authorizer)
        {
            var arguments = signable.Args
                                                    .Select(item => new BaseArgument
                                                                     {
                                                                         Type = item.Type,
                                                                         Value = item.Value.ToString()
                                                                     })
                                                    .ToList();

            $"RLP encode cadence: {signable.Cadence}".ToLog();
            var datas = new List<object>();
            datas.Add(Encoding.UTF8.GetBytes(signable.Cadence).ToList());
            
            var tmp = arguments.Select(item => {
                                           $"Arg: {JsonConvert.SerializeObject(item)}".ToLog();
                                           return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(item)).ToList();
                                       })
                                            .ToList();
            datas.Add(tmp);
            $"RefBlock: {signable.Interaction.Message.RefBlock}".ToLog();
            datas.Add(signable.Interaction.Message.RefBlock.HexToBytes().ToList());
            
            $"ComputeLimit: {signable.Interaction.Message.ComputeLimit}".ToLog();
            datas.Add(RLPUtility.GetBytes(signable.Interaction.Message.ComputeLimit));
            
            $"ProposalKey addr: {signable.Voucher.ProposalKey.Address}".ToLog();
            datas.Add(signable.Voucher.ProposalKey.Address.ToString().HexToBytes().ToList());
            
            $"ProposalKey keyId: {signable.Voucher.ProposalKey.KeyId}".ToLog();
            datas.Add(RLPUtility.GetBytes(signable.Voucher.ProposalKey.KeyId).ToList());
            
            $"ProposalKey seqNum: {signable.Voucher.ProposalKey.SequenceNum}".ToLog();
            datas.Add(RLPUtility.GetBytes(signable.Voucher.ProposalKey.SequenceNum).ToList());
            
            $"Payer addr: {signable.Voucher.Payer}".ToLog();
            datas.Add(signable.Voucher.Payer.ToString().HexToBytes().ToList());
            
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

        private string EncodedCanonicalAuthorizationEnvelope(Signable signable, string authorizer)
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
                                           EncodeTransaction(signable, authorizer),
                                           EncodedSignatures(signatures, signers)
                                       };
            var bytes = RLPUtility.RlpEncode(authEnvelopeElements);
            var messageBytes = DomainTag.AddTransactionDomainTag(bytes.ToArray());
            var message = messageBytes.ToHex();
            message.ToLog();
            return message;
        }
        
        private Dictionary<string, int> CollectSigners(Signable signable)
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

        protected virtual void CreateSignable(Signable signable, Account proposer, Account payer, List<Account> authorizations, Dictionary<string, JObject> accountDict, List<JObject> payloadSigs)
        {
            signable.F_Type = "Signable";
            signable.Addr = authorizations.First().Addr;
            signable.KeyId = Convert.ToUInt32(authorizations.First().KeyId);
            signable.Roles = new JObject
                             {
                                 new JProperty("proposer", false),
                                 new JProperty("payer", false),
                                 new JProperty("authorizer", true),
                             };
            signable.Interaction.Accounts = accountDict;
            signable.Interaction.Proposer = proposer.TempId;
            signable.Interaction.Payer = payer.TempId;
            signable.Interaction.Authorizations = authorizations.Select(p => p.TempId).ToList();
            signable.Voucher.ProposalKey = new ProposalKey
                                           {
                                               Address = proposer.Addr,
                                               KeyId = proposer.KeyId,
                                               SequenceNum = proposer.SequenceNum
                                           };

            signable.Voucher.Payer = payer.Addr;
            signable.Voucher.Authorizers = authorizations.Select(p => p.Addr).ToList();
            signable.Voucher.PayloadSigs = payloadSigs;
            signable.Voucher.EnvelopeSigs = new List<JObject>
                                            {
                                                new JObject
                                                {
                                                    new JProperty("address", payer.Addr),
                                                    new JProperty("keyId", payer.KeyId), 
                                                }
                                            };
            signable.Voucher.RefBlock = signable.Interaction.Message.RefBlock;
        }

        private Argument CreateArgument(string tempId, string type)
        {
            var argument = new Argument
                           {
                               Kind = "ARGUMENT",
                               TempId = tempId,
                               AsBaseArgument = new BaseArgument
                                            {
                                                Type = type,
                                            },
                               XForm = new XForm
                                       {
                                           Label = type
                                       }
                           };    
            return argument;
        }

        protected virtual string ConvertValueForEncode(string type, object value) =>
            type switch
            {
                "UInt" => value.ToString().PadRight(8, '0'),
                "UInt8" => value.ToString().PadRight(8, '0'),
                "UInt16" => value.ToString().PadRight(8, '0'),
                "UInt32" => value.ToString().PadRight(8, '0'),
                "UInt64" => value.ToString().PadRight(8, '0'),
                "UInt128" => value.ToString().PadRight(8, '0'),
                "UInt256" => value.ToString().PadRight(8, '0'),
                "Int" => value.ToString().PadRight(8, '0'),
                "Int8" => value.ToString().PadRight(8, '0'),
                "Int16" => value.ToString().PadRight(8, '0'),
                "Int32" => value.ToString().PadRight(8, '0'),
                "Int64" => value.ToString().PadRight(8, '0'),
                "Int128" => value.ToString().PadRight(8, '0'),
                "Int256" => value.ToString().PadRight(8, '0'),
                "Bool" => value.ToString(),
                "Word8" => value.ToString(),
                "Word16" => value.ToString(),
                "Word32" => value.ToString(),
                "Word64" => value.ToString(),
                "UFix64" => Convert.ToDecimal(value).ToString("0.00000000"),
                "Fix64" => value.ToString(),
                "String" => value.ToString(),
                "Character" => value.ToString(),
                "Address" => value.ToString(),
                _ => throw new Exception("Type not support")
            };
    }
}