using System;
using System.Collections.Generic;
using System.Linq;
using Flow.FCL.Models.Authz;
using Flow.Net.Sdk.Core.Cadence;
using Flow.Net.Sdk.Core.Models;
using Newtonsoft.Json.Linq;
using Plugins.Blocto.Sdk.Core.Model;

namespace Blocto.Sdk.Core.Utility
{
    public class ResolveUtility
    {
        public JObject ResolvePreSignable(FlowTransaction tx, string refBlockId)
        {
            var args = tx.Arguments.Select(cadence => CreageArg(cadence)).ToList();
            var interactionArg = new JObject();
            var argTempIds = new List<string>();
            foreach (var cadence in tx.Arguments)
            {
                var tmpJObj = CreateInteractionArg(cadence);
                argTempIds.Add(tmpJObj.GetValue("tempId").ToString());
                interactionArg.Add(tmpJObj.GetValue("tempId").ToString(), tmpJObj);
            }
            
            var message = new JObject
                          {
                              new JProperty(MessagePropertyEnum.cadence.ToString(), tx.Script),
                              new JProperty(MessagePropertyEnum.refBlock.ToString(), refBlockId),
                              new JProperty(MessagePropertyEnum.computeLimit.ToString(), tx.GasLimit),
                              new JProperty(MessagePropertyEnum.arguments.ToString(), argTempIds),
                          };
            
            var interaction = new JObject
                              {
                                  new JProperty(InteractionPropertyEnum.tag.ToString(), "TRANSACTION"),
                                  new JProperty(InteractionPropertyEnum.status.ToString(), "OK"),
                                  new JProperty(InteractionPropertyEnum.arguments.ToString(), interactionArg),
                                  new JProperty(InteractionPropertyEnum.message.ToString(), message),
                              };
            
            var voucher = new JObject
                          {
                              new JProperty(VoucherPropertyEnum.cadence.ToString(), tx.Script),
                              new JProperty(VoucherPropertyEnum.refBlock.ToString(), refBlockId),
                              new JProperty(VoucherPropertyEnum.computeLimit.ToString(), tx.GasLimit),
                              new JProperty(VoucherPropertyEnum.arguments.ToString(), args),
                              new JProperty(VoucherPropertyEnum.proposalKey.ToString(), CreateProposerKey()),
                          };
            
            var tmp = new JObject
                      {
                          new JProperty(SignablePropertyEnum.args.ToString(), args),
                          new JProperty(SignablePropertyEnum.interaction.ToString(), interaction),
                          new JProperty(SignablePropertyEnum.voucher.ToString(), voucher),
                          new JProperty(SignablePropertyEnum.f_type.ToString(), "PreSignable"),
                          new JProperty(SignablePropertyEnum.f_vsn.ToString(), "1.0.1"),
                      };
            return null;
        }
        
        private static JObject CreageArg(ICadence cadenceArg)
        {
            var jobject = JObject.Parse(cadenceArg.Encode());
            return jobject;
        }

        private static JObject CreateProposerKey() => CreateProposerKey(null, null, null);

        private static JObject CreateProposerKey(string? addr , string? keyId, string? seqNum)
        {
            if(keyId is null)
            {
                return new JObject
                       {
                           new JProperty("address", null),
                           new JProperty("keyId", null),
                           new JProperty("sequenceNum", null),
                       };
            }

            return new JObject
                   {
                       new JProperty("address", addr),
                       new JProperty("keyId", Convert.ToUInt32(keyId)),
                       new JProperty("sequenceNum", Convert.ToInt64(seqNum)),
                   };
        }
        
        private static JObject CreateInteractionArg(ICadence arg)
        {
            var tmp = JObject.Parse(arg.Encode());
            var interactionArg = new JObject
                                 {
                                     new JProperty("kind", "ARGUMENT"),
                                     new JProperty("tempId", KeyGenerator.GetUniqueKey(10)),
                                     new JProperty("value", tmp.GetValue("value")),
                                     new JProperty("asArgument", tmp),
                                     new JProperty("xfoorm", new JObject
                                                             {
                                                                 new JProperty("label", tmp.GetValue("type"))
                                                             })
                                 };
            return interactionArg;
        }

    }
}