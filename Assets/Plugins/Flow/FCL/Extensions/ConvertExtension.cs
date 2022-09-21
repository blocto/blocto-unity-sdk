using System.Collections.Generic;
using Flow.FCL.Models;
using Flow.FCL.Models.Authz;
using Flow.Net.Sdk.Core.Models;
using Newtonsoft.Json.Linq;

namespace Flow.FCL.Extensions
{
    public static class ConvertExtension
    {
        public static FlowAccount ConvertToFlowAccount(this AuthInformation data)
        {
            var authorize = new FlowAccount
                            {
                                Address = new FlowAddress(data.Identity.Address),
                                Keys = new List<FlowAccountKey>
                                       {
                                           new FlowAccountKey
                                           {
                                               Index = data.Identity.KeyId
                                           }
                                       }
                            };
            return authorize;
        }
        
        public static (JToken Signature, JToken KeyId, JToken Address) SignatureInfo(this AuthzAdapterResponse response)
        {
            var signature =response?.CompositeSignature.GetValue("signature");
            var addr = response?.CompositeSignature.GetValue("addr");
            var keyId = response?.CompositeSignature.GetValue("keyId");
            return (signature, keyId, addr);
        }
        
        public static (JToken Signature, JToken KeyId, JToken Address) SignatureInfo(this SignatureResponse response)
        {
            var signature = response.Data.GetValue("signature");
            var addr = response.Data.GetValue("addr");
            var keyId = response.Data.GetValue("keyId");
            return (signature, keyId, addr);
        }
    }
}