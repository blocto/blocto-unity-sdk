using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flow.FCL.Models.Authz;
using Flow.Net.Sdk.Core.Cadence;
using Flow.Net.Sdk.Core.Client;
using Flow.Net.Sdk.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugins.Blocto.Sdk.Core.Model;

namespace Blocto.Sdk.Core.Utility
{
    public class ResolveUtility
    {
        private IFlowClient _flowClient;
        public ResolveUtility(IFlowClient flowClient)
        {
            _flowClient = flowClient;
        }
        
        public (FlowTransaction Tx, JObject SignabelTemplate) ResolvePreSignable(FlowTransaction tx)
        {
            var args = tx.Arguments.Select(cadence => CreageArg(cadence)).ToList();
            var interactionArg = new JObject();
            var argTempIds = new List<string>();
            foreach (var cadence in tx.Arguments)
            {
                var tmpJObj = CreateInteractionArg(cadence);
                argTempIds.Add(tmpJObj.GetValue("tempId")!.ToString());
                interactionArg.Add(tmpJObj.GetValue("tempId")!.ToString(), tmpJObj);
            }
            
            var message = ResolveUtility.CreateMessage(tx, argTempIds);
            var interaction = ResolveUtility.CreateInteraction(interactionArg, message);
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
            return (tx, tmp);
        }
        
        public (FlowTransaction Tx, JObject SignabelTemplate) ResolveSignable(FlowTransaction tx, PreAuthzData preAuthzData, FlowAccount account)
        {
            var item = ResolvePreSignable(tx);
            item.SignabelTemplate.Remove("voucher");
            item.SignabelTemplate.Remove("addr");
            item.SignabelTemplate.Remove("keyId");
            item.SignabelTemplate.Remove("roles");
            item.SignabelTemplate.Remove("f_type");
            item.SignabelTemplate.Add(new JProperty("f_type", "Signable"));
            
            tx.ProposalKey = GetProposerKey(account, Convert.ToUInt32(preAuthzData.Proposer.Identity.KeyId)).ConfigureAwait(false).GetAwaiter().GetResult();
            var participators = ResolveUtility.GetAllAccount(preAuthzData, tx.ProposalKey);
            if(!tx.SignerList.ContainsKey(participators.Proposer.Addr))
            {
                tx.SignerList.Add(participators.Proposer.Addr, 0);
            }
            
            if(!tx.SignerList.ContainsKey(participators.Payer.Addr))
            {
                tx.SignerList.Add(participators.Payer.Addr, 1);
            }
            
            AddInteractionAccounts(item.SignabelTemplate, participators);
            tx.Payer = new FlowAddress(participators.Payer.Addr);
            foreach (var authorization in participators.Authorizations)
            {
                tx.Authorizers.Add(new FlowAddress(authorization.Addr));
            }
            
            var args = tx.Arguments.Select(cadence => CreageArg(cadence)).ToList();
            var voucher = CreateVoucher(tx, args, "signable");
            item.SignabelTemplate.Add("voucher", voucher);
            item.SignabelTemplate.Add(SignablePropertyEnum.addr.ToString(), participators.Proposer.Addr);
            item.SignabelTemplate.Add(SignablePropertyEnum.keyId.ToString(), participators.Proposer.KeyId);
            
            if(participators.Proposer.Addr == participators.Payer.Addr)
            {
                item.SignabelTemplate.Add(SignablePropertyEnum.roles.ToString(), new JObject
                                                                        {
                                                                            new JProperty("proposer", false),
                                                                            new JProperty("authorizer", true),
                                                                            new JProperty("payer", false)
                                                                        });
            }
            
            
            return (tx, item.SignabelTemplate);
        }

        private static JObject CreateInteraction(JObject interactionArg, JObject message)
        {
            var interaction = new JObject
                              {
                                  new JProperty(InteractionPropertyEnum.tag.ToString(), "TRANSACTION"),
                                  new JProperty(InteractionPropertyEnum.status.ToString(), "OK"),
                                  new JProperty(InteractionPropertyEnum.arguments.ToString(), interactionArg),
                                  new JProperty(InteractionPropertyEnum.message.ToString(), message),
                              };

            return interaction;
        }

        private static JObject AddInteractionAccounts(JObject jTx, (Account Proposer, Account Payer, List<Account> Authoriaztions) participartors)
        {
            var interaction = jTx.SelectToken("interaction");
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
            
            var account = new JProperty("accounts", accounts.Values);
            interaction!.Last!.AddAfterSelf(account);
            return jTx;
        }
        
        private static JObject CreateMessage(FlowTransaction tx, List<string> argTempIds)
        {
            var message = new JObject
                          {
                              new JProperty(MessagePropertyEnum.cadence.ToString(), tx.Script),
                              new JProperty(MessagePropertyEnum.refBlock.ToString(), tx.ReferenceBlockId),
                              new JProperty(MessagePropertyEnum.computeLimit.ToString(), tx.GasLimit),
                              new JProperty(MessagePropertyEnum.arguments.ToString(), argTempIds),
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
                                     new JProperty("xfoorm", new JObject
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
                         new JProperty("signature", account.Signature)
                     };
            
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

            if (step != "signable")
            {
                return voucher;
            }

            voucher.Add(VoucherPropertyEnum.payer.ToString(), tx.Payer.Address);
            voucher.Add(VoucherPropertyEnum.authorizers.ToString(),
                        JsonConvert.SerializeObject(new List<string>
                                                    {
                                                        string.Join(",", tx.Authorizers.Select(p => p.Address))
                                                    }));
            
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