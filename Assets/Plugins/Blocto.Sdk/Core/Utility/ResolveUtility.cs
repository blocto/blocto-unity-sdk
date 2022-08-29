using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flow.FCL.Models.Authz;
using Flow.Net.Sdk.Core.Cadence;
using Flow.Net.Sdk.Core.Models;
using Flow.Net.SDK.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugins.Blocto.Sdk.Core.Model;

namespace Blocto.Sdk.Core.Utility
{
    public class ResolveUtility
    {
        public JObject ResolvePreSignable(FlowTransaction tx)
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
                                                                        new JProperty("param", true),
                                                                    })
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

            if(step == "signable")
            {
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
            }
            
            return voucher;
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

        public JObject ResolveSignable(FlowTransaction tx, PreAuthzData preAuthzData, FlowAccount account)
        {
            var proposalKey = GetProposerKey(account, Convert.ToUInt32(preAuthzData.Proposer.Identity.KeyId)).ConfigureAwait(false).GetAwaiter().GetResult();
            var proposer = new Account
                           {
                               Addr = proposalKey.Address.Address,
                               KeyId = Convert.ToUInt32(preAuthzData.Proposer.Identity.KeyId),
                               TempId = $"{preAuthzData.Proposer.Identity.Address}-{preAuthzData.Proposer.Identity.KeyId}",
                               SequenceNum = Convert.ToUInt64(proposalKey.SequenceNumber)
                           };
            
            var payers = preAuthzData.Payer
                                     .Select(payer => new Account
                                                      {
                                                          Addr = payer.Identity.Address,
                                                          KeyId = Convert.ToUInt32(payer.Identity.KeyId),
                                                          TempId = $"{payer.Identity.Address}-{payer.Identity.KeyId}"
                                                      })
                                     .ToList();
            var authorizations = preAuthzData.Authorization
                                             .Select(p => new Account
                                                          {
                                                              Addr = p.Identity.Address,
                                                              KeyId = Convert.ToUInt32(p.Identity.KeyId),
                                                              TempId = $"{p.Identity.Address}-{p.Identity.KeyId}"
                                                          })
                                             .ToList();
            return null;
        }
        
        private static JObject CreageArg(ICadence cadenceArg)
        {
            return JObject.Parse(cadenceArg.Encode());
        }

        private static JObject CreateProposerKey() => CreateProposerKey(null, 0, 0);

        private static JObject CreateProposerKey(string addr , uint keyId, ulong seqNum)
        {
            if(keyId is 0)
            {
                return new JObject
                       {
                           new JProperty("address", null!),
                           new JProperty("keyId", null!),
                           new JProperty("sequenceNum", null!),
                       };
            }

            return new JObject
                   {
                       new JProperty("address", addr),
                       new JProperty("keyId", keyId),
                       new JProperty("sequenceNum", seqNum),
                   };
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
        
        private static JObject CreateInteractionAccounts(Account account)
        {
            return null;
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