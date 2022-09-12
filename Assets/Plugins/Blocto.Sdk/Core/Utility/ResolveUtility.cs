using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blocto.Sdk.Core.Flow.Model;
using Flow.FCL.Models.Authz;
using Flow.FCL.Utility;
using Flow.Net.Sdk.Core;
using Flow.Net.Sdk.Core.Cadence;
using Flow.Net.Sdk.Core.Client;
using Flow.Net.Sdk.Core.Models;
using Flow.Net.SDK.Extensions;
using Flow.Net.Sdk.Utility.NEthereum.Hex;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Blocto.Sdk.Core.Utility
{
    public class ResolveUtility : IResolveUtil
    {
        // private IFlowClient _flowClient;
        
        public ResolveUtility(IFlowClient flowClient)
        {
            // _flowClient = flowClient;
        }
        
        public JObject ResolvePreSignable(ref FlowTransaction tx)
        {
            var args = tx.Arguments.Select(cadence => CreageArg(cadence)).ToList();
            var interaction = ResolveUtility.CreateInteraction(tx);
            var voucher = ResolveUtility.CreateVoucher(tx, args);
            var tmp = new JObject
                      {
                          new JProperty(SignablePropertyEnum.args.ToString(), args),
                          new JProperty(SignablePropertyEnum.interaction.ToString(), interaction),
                          new JProperty(SignablePropertyEnum.voucher.ToString(), voucher),
                          new JProperty(SignablePropertyEnum.f_type.ToString(), "PreSignable"),
                          new JProperty(SignablePropertyEnum.f_vsn.ToString(), "1.0.1"),
                          new JProperty(SignablePropertyEnum.addr.ToString(), null!),
                          new JProperty(SignablePropertyEnum.keyId.ToString(), 0),
                          new JProperty(SignablePropertyEnum.cadence.ToString(), tx.Script),
                          new JProperty(SignablePropertyEnum.roles.ToString(), new JObject
                                                                    {
                                                                        new JProperty("proposer", true),
                                                                        new JProperty("authorizer", true),
                                                                        new JProperty("payer", true),
                                                                    })
                      };
            return tmp;
        }
        
        public JObject ResolveSignable(ref FlowTransaction tx, PreAuthzData preAuthzData, FlowAccount authorizer)
        {
            var item = ResolvePreSignable(ref tx);
            item.Remove(SignablePropertyEnum.voucher.ToString());
            item.Remove(SignablePropertyEnum.addr.ToString());
            item.Remove(SignablePropertyEnum.keyId.ToString());
            item.Remove(SignablePropertyEnum.roles.ToString());
            item.Remove(SignablePropertyEnum.f_type.ToString());
            item.Remove(SignablePropertyEnum.interaction.ToString());
            item.Add(new JProperty("f_type", "Signable"));
            
            var participators = ResolveUtility.GetAllAccount(preAuthzData, tx.ProposalKey);
            if(!tx.SignerList.ContainsKey(participators.Proposer.Addr))
            {
                tx.SignerList.Add(participators.Proposer.Addr, 0);
                if(participators.Proposer.Addr != participators.Payer.Addr)
                {
                    var payloadSignature = new FlowSignature
                                    {
                                       Address = new FlowAddress(participators.Proposer.Addr.AddHexPrefix()),
                                       KeyId = participators.Proposer.KeyId,
                                       Signature = participators.Proposer.Signature == null ? 
                                                       Array.Empty<byte>() :
                                                       Encoding.UTF8.GetBytes(participators.Proposer.Signature)
                                    };
                    
                    ResolveUtility.AddPayloadSignature(tx, payloadSignature);
                }
                
                var envelopSignature = new FlowSignature
                                    {
                                        Address = new FlowAddress(participators.Payer.Addr.AddHexPrefix()),
                                        KeyId = participators.Payer.KeyId,
                                        Signature = participators.Payer.Signature == null ?
                                                        Array.Empty<byte>() :
                                                        Encoding.UTF8.GetBytes(participators.Payer.Signature)
                                    };
                
                ResolveUtility.AddEnvelopSignature(tx, envelopSignature);
            }
            
            
            if(!tx.SignerList.ContainsKey(participators.Payer.Addr))
            {
                tx.SignerList.Add(participators.Payer.Addr, tx.SignerList.Count);
            }

            var transaction = tx;
            foreach (var authorization in participators.Authorizations.Where(authorization => !transaction.SignerList.ContainsKey(authorization.Addr)))
            {
                tx.SignerList.Add(authorization.Addr, tx.SignerList.Count);
                var flowSignature = new FlowSignature
                                    {
                                        Address = new FlowAddress(authorization.Addr.AddHexPrefix()),
                                        KeyId = authorization.KeyId,
                                        Signature = authorization.Signature == null ?
                                                        Array.Empty<byte>() :
                                                        Encoding.UTF8.GetBytes(authorization.Signature)
                                    };
                
                ResolveUtility.AddPayloadSignature(tx, flowSignature);
            }
            
            tx.Payer = new FlowAddress(participators.Payer.Addr);
            tx.Authorizers = new List<FlowAddress>
                             {
                                 new FlowAddress(authorizer.Address.Address)
                             };
            
            var args = tx.Arguments.Select(cadence => CreageArg(cadence)).ToList();
            var voucher = CreateVoucher(tx, args, "signable");
            item.Add("voucher", voucher);
            item.Add(SignablePropertyEnum.addr.ToString(), authorizer.Address.Address);
            item.Add(SignablePropertyEnum.keyId.ToString(), authorizer.Keys.First().Index);
            
            var propertys = new List<JProperty>
                            {
                                new JProperty(InteractionPropertyEnum.proposer.ToString(), $"{participators.Proposer.Addr}-{participators.Proposer.KeyId}"),
                                new JProperty(InteractionPropertyEnum.payer.ToString(), $"{participators.Payer.Addr}-{participators.Payer.KeyId}"),
                                new JProperty(InteractionPropertyEnum.authorizations.ToString(), 
                                              participators.Authorizations.Select(p => $"{p.Addr}-{p.KeyId}").ToList())
                            };
            
            var interaction = CreateInteraction(tx, propertys);
            interaction.Add(InteractionPropertyEnum.accounts.ToString(), AddInteractionAccounts(participators));
            item.Add(SignablePropertyEnum.interaction.ToString(), interaction);
            
            if(participators.Proposer.Addr == participators.Payer.Addr)
            {
                item.Add(SignablePropertyEnum.roles.ToString(), new JObject
                                                                        {
                                                                            new JProperty("proposer", false),
                                                                            new JProperty("authorizer", true),
                                                                            new JProperty("payer", false)
                                                                        });
            }
            
            var message = RLP.GetEncodeMessage(tx);
            item.Add("message", message);
            
            $"Tx: {JsonConvert.SerializeObject(tx)}".ToLog();
            return item;
        }

        public JObject ResolvePayerSignable(ref FlowTransaction tx, JObject signable)
        {
            var args = tx.Arguments.Select(cadence => CreageArg(cadence)).ToList();
            var voucher = CreateVoucher(tx, args, "payersignable");
            signable.Remove(SignablePropertyEnum.voucher.ToString());
            signable.Add(SignablePropertyEnum.voucher.ToString(), voucher);
            
            var message = RLP.EncodedCanonicalAuthorizationEnvelope(tx);
            signable.Remove(SignablePropertyEnum.message.ToString());
            signable.Add(SignablePropertyEnum.message.ToString(), message);
            $"Payer signable: {signable}".ToLog();
            return signable;
        }
        
        public JObject ResolveSignMessage(string message, string sessionId)
        {
            var service = new JObject
                          {
                              new JProperty("params", new JObject
                                                      {
                                                          new JProperty("sessionId", sessionId)
                                                      }),
                              new JProperty("type", "user-signature")
                          };
            
            var config = new JObject
                         {
                             new JProperty("services", new JObject
                                                       {
                                                           new JProperty("OpenID.scopes", "email!")
                                                       }),
                             new JProperty("app", new JObject())
                         };
            var result = new JObject
                         {
                             new JProperty(SignablePropertyEnum.service.ToString(), service),
                             new JProperty(SignablePropertyEnum.config.ToString(), config),
                             new JProperty(SignablePropertyEnum.f_vsn.ToString(), "1.0.1"),
                             new JProperty("fclVersion", "1.0.1"),
                             new JProperty("message", message)
                         };
            return result;
        }
        
        private static void AddPayloadSignature(FlowTransaction tx, FlowSignature flowSignature)
        {
            if(tx.PayloadSignatures.All(p => p.Address.Address != flowSignature.Address.Address))
            {
                tx.PayloadSignatures.Add(flowSignature);
            }
        }
        
        private static void AddEnvelopSignature(FlowTransaction tx, FlowSignature flowSignature)
        {
            if(tx.EnvelopeSignatures.All(p => p.Address.Address != flowSignature.Address.Address))
            {
                tx.EnvelopeSignatures.Add(flowSignature);
            }
        }

        private static JObject CreateInteraction(FlowTransaction tx, List<JProperty> jPropertys = null)
        {
            var interactionArg = new JObject();
            var argTempIds = new List<string>();
            foreach (var cadence in tx.Arguments)
            {
                var tmpJObj = CreateInteractionArg(cadence);
                argTempIds.Add(tmpJObj.GetValue("tempId")!.ToString());
                interactionArg.Add(tmpJObj.GetValue("tempId")!.ToString(), tmpJObj);
            }
            var message = ResolveUtility.CreateMessage(tx, argTempIds);
            var interaction = new JObject
                              {
                                  new JProperty(InteractionPropertyEnum.tag.ToString(), "TRANSACTION"),
                                  new JProperty(InteractionPropertyEnum.assigns.ToString(), new JObject()),
                                  new JProperty(InteractionPropertyEnum.reason.ToString(), null),
                                  new JProperty(InteractionPropertyEnum.status.ToString(), "OK"),
                                  new JProperty(InteractionPropertyEnum.arguments.ToString(), interactionArg),
                                  new JProperty(InteractionPropertyEnum.message.ToString(), message),
                                  new JProperty(InteractionPropertyEnum.events.ToString(), null),
                                  new JProperty(InteractionPropertyEnum.account.ToString(), new JObject(new JProperty("addr", null))),
                                  new JProperty(InteractionPropertyEnum.collection.ToString(), null),
                                  new JProperty(InteractionPropertyEnum.transaction.ToString(), null),
                                  new JProperty(InteractionPropertyEnum.block.ToString(), null),
                                  new JProperty("params", new JObject())
                              };
            
            if(jPropertys == null)
            {
                return interaction;
            }

            foreach (var property in jPropertys)
            {
                interaction.Add(property);
            }
            
            return interaction;
        }
        
        private static JObject AddInteractionAccounts((Account Proposer, Account Payer, List<Account> Authoriaztions) participartors)
        {
            var accounts = new Dictionary<string, JObject>();
            var proposerJObject = CreateInteractionAccounts(participartors.Proposer, 
                                                            participartors.Proposer.Role.Proposer,
                                                            participartors.Proposer.Role.Payer,
                                                            participartors.Proposer.Role.Authorizer);
            if(!accounts.ContainsKey(proposerJObject.GetValue("tempId")!.ToString()))
            {
                accounts.Add(proposerJObject.GetValue("tempId")!.ToString(), proposerJObject);
            }
            
            var payerJObject = CreateInteractionAccounts(participartors.Payer,
                                                         participartors.Payer.Role.Proposer,
                                                         participartors.Payer.Role.Payer,
                                                         participartors.Payer.Role.Authorizer);
            if(!accounts.ContainsKey(payerJObject.GetValue("tempId")!.ToString()))
            {
                accounts.Add(payerJObject.GetValue("tempId")!.ToString(), payerJObject);
            }

            foreach (var authorization in participartors.Authoriaztions)
            {
                var authorizerJObject = CreateInteractionAccounts(authorization,
                                                                  authorization.Role.Proposer,
                                                                  authorization.Role.Payer,
                                                                  authorization.Role.Authorizer);

                if (accounts.ContainsKey(authorizerJObject.GetValue("tempId")!.ToString()))
                {
                    continue;
                }

                var key = authorizerJObject.GetValue("tempId")!.ToString();
                accounts.Add(key, authorizerJObject);
            }
            
            var tmp = new JObject();
            foreach (var item in accounts)
            {
                tmp.Add(item.Key, item.Value);
            }
            
            return tmp;
        }
        
        private static JObject CreateMessage(FlowTransaction tx, List<string> argTempIds)
        {
            var message = new JObject
                          {
                              new JProperty(MessagePropertyEnum.cadence.ToString(), tx.Script),
                              new JProperty(MessagePropertyEnum.refBlock.ToString(), tx.ReferenceBlockId),
                              new JProperty(MessagePropertyEnum.computeLimit.ToString(), tx.GasLimit),
                              new JProperty(MessagePropertyEnum.proposer.ToString(), null),
                              new JProperty(MessagePropertyEnum.payer.ToString(), null),
                              new JProperty(MessagePropertyEnum.authorizations.ToString(), new List<JObject>()),
                              new JProperty(MessagePropertyEnum.arguments.ToString(), argTempIds),
                              new JProperty("params", new List<JObject>())
                          };
            
            return message;
        }

        private static JObject CreageArg(ICadence cadenceArg)
        {
            return JObject.Parse(cadenceArg.Encode());
        }

        private static JObject CreateProposerKey() => CreateProposerKey(null, 0, 0);

        private static JObject CreateProposerKey(string addr , uint keyId, ulong seqNum)
        {
            if (keyId != 0)
            {
                return new JObject
                       {
                           new JProperty("address", addr),
                           new JProperty("keyId", keyId),
                           new JProperty("sequenceNum", seqNum),
                       };
            }

            return new JObject
                      {
                          new JProperty("address", null!),
                          new JProperty("keyId", null!),
                          new JProperty("sequenceNum", null!),
                      };;
        }
        
        private static JObject CreateInteractionArg(ICadence arg)
        {
            var tmp = JObject.Parse(arg.Encode());
            var interactionArg = new JObject
                                 {
                                     new JProperty("kind", "ARGUMENT"),
                                     new JProperty("tempId", arg.TempId),
                                     new JProperty("value", tmp.GetValue("value")),
                                     new JProperty("asArgument", tmp),
                                     new JProperty("xform", new JObject
                                                             {
                                                                 new JProperty("label", tmp.GetValue("type"))
                                                             })
                                 };
            return interactionArg;
        }
        
        private static JObject CreateInteractionAccounts(Account account, bool isProposer, bool isPayer, bool isAuthorizer)
        {
            var tmp= new JObject
                     {
                         new JProperty("tempId", $"{account.Addr}-{account.KeyId}"),
                         new JProperty("addr", account.Addr),
                         new JProperty("keyId", account.KeyId),
                         new JProperty("sequenceNum", account.SequenceNum),
                         new JProperty("role", new JObject
                                               {
                                                   new JProperty("proposer", isProposer),
                                                   new JProperty("payer", isPayer),
                                                   new JProperty("authorizer", isAuthorizer),
                                               }),
                     };
            
            $"Account signatore: {account.Signature}".ToLog();
            if(account.Signature != null)
            {
                tmp.Add(new JProperty("signature", account.Signature));
            }
            
            return tmp;
        }
        
        private static JObject CreateVoucher(FlowTransaction tx, List<JObject> args,  string step = "presignable")
        {
            var voucher = new JObject
                          {
                              new JProperty(VoucherPropertyEnum.cadence.ToString(), tx.Script),
                              new JProperty(VoucherPropertyEnum.refBlock.ToString(), tx.ReferenceBlockId),
                              new JProperty(VoucherPropertyEnum.computeLimit.ToString(), tx.GasLimit),
                              new JProperty(VoucherPropertyEnum.arguments.ToString(), args),
                              new JProperty(VoucherPropertyEnum.proposalKey.ToString(), 
                                            ResolveUtility.CreateProposerKey()),
                          };

            if (step == "presignable")
            {
                return voucher;
            }

            var envelopObjs = new List<JObject>();
            var envelopObj = new JObject
                             {
                                 new JProperty("address", tx.EnvelopeSignatures.First().Address.Address),
                                 new JProperty("keyId", tx.EnvelopeSignatures.First().KeyId)
                             };
            
            if(tx.EnvelopeSignatures.First().Signature.Length > 0)
            {
                envelopObj.Add("sig", tx.EnvelopeSignatures.First().Signature.ToHex());
            }
            
            envelopObjs.Add(envelopObj);
            
            var payloadJObj = new List<JObject>();
            foreach (var item in tx.PayloadSignatures)
            {
                var tmpObj = new JObject
                             {
                                 new JProperty("address", item.Address.Address),
                                 new JProperty("keyId", item.KeyId)
                             };
                
                if(item.Signature.Length > 0)
                {
                    tmpObj.Add("sig", item.Signature.ToHex());
                }
                
                payloadJObj.Add(tmpObj);
            }
            
            voucher.Add(new JProperty(VoucherPropertyEnum.payloadSigs.ToString(), payloadJObj));
            voucher.Add(new JProperty(VoucherPropertyEnum.envelopeSigs.ToString(), envelopObjs));
            voucher.Add(VoucherPropertyEnum.payer.ToString(), tx.Payer.Address);
            var newProperty = new JProperty(VoucherPropertyEnum.authorizers.ToString(), new List<string>
                                                                                        {
                                                                                            tx.Authorizers.First().Address
                                                                                        });
            voucher.Add(newProperty);
            
            var token = voucher.SelectToken(VoucherPropertyEnum.proposalKey.ToString());
            var proposerKey = ResolveUtility.CreateProposerKey(tx.ProposalKey.Address.Address,
                                                               tx.ProposalKey.KeyId,
                                                               tx.ProposalKey.SequenceNumber);
            token!["address"] = proposerKey.GetValue("address");
            token["keyId"] = proposerKey.GetValue("keyId");
            token["sequenceNum"] = proposerKey.GetValue("sequenceNum");

            return voucher;
        }
        
        private static (Account Proposer, Account Payer, List<Account> Authorizations) GetAllAccount(PreAuthzData preAuthzData, FlowProposalKey proposalKey)
        {
            var proposer = new Account
                           {
                               Addr = proposalKey.Address.Address,
                               KeyId = Convert.ToUInt32(preAuthzData.Proposer.Identity.KeyId),
                               TempId = $"{preAuthzData.Proposer.Identity.Address}-{preAuthzData.Proposer.Identity.KeyId}",
                               SequenceNum = Convert.ToUInt64(proposalKey.SequenceNumber)
                           };

            var payer = preAuthzData.Payer.Select(payer => new Account
                                                            {
                                                                Addr = payer.Identity.Address,
                                                                KeyId = Convert.ToUInt32(payer.Identity.KeyId),
                                                                TempId = $"{payer.Identity.Address}-{payer.Identity.KeyId}"
                                                            })
                                                 .ToList()
                                                 .First();

            var authorizations = preAuthzData.Authorization.Select(p => new Account
                                                                        {
                                                                            Addr = p.Identity.Address,
                                                                            KeyId = Convert.ToUInt32(p.Identity.KeyId),
                                                                            TempId = $"{p.Identity.Address}-{p.Identity.KeyId}",
                                                                        })
                                                                        .ToList();
            
            var isPayer = proposer.Addr == payer.Addr;
            var isAuthorizer = authorizations.Any(p => p.Addr == proposer.Addr);
            proposer.Role = new Role
                            {
                                Proposer = true,
                                Payer = isPayer,
                                Authorizer = isAuthorizer
                            };
            
            var isProposer = payer.Addr == proposer.Addr;
            isAuthorizer = authorizations.Any(p => p.Addr == payer.Addr);
            payer.Role = new Role
                          {
                              Proposer = isProposer,
                              Payer = true,
                              Authorizer = isAuthorizer
                          };

            foreach (var authorization in authorizations)
            {
                isProposer = authorization.Addr == proposer.Addr;
                isPayer = authorization.Addr == payer.Addr;
                authorization.Role = new Role
                                     {
                                         Proposer = isProposer,
                                         Payer = isPayer,
                                         Authorizer = true
                                     };
            }
            
            return (proposer, payer, authorizations);
        }

        private async Task<FlowProposalKey> GetProposerKey(FlowAccount account, uint keyId)
        {
            var proposalKey = account.Keys.First(p => p.Index == keyId);
            return new FlowProposalKey
                   {
                       Address = account.Address,
                       KeyId = keyId,
                       SequenceNumber = proposalKey.SequenceNumber
                   };
        }
    }
}