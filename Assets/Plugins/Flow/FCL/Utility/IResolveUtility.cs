using System.Collections.Generic;
using Flow.FCL.Models.Authz;
using Flow.Net.Sdk.Core.Models;
using Newtonsoft.Json.Linq;

namespace Flow.FCL.Utility
{
    public interface IResolveUtility
    {
        JObject ResolvePreSignable(ref FlowTransaction tx);

        List<JObject> ResolveSignable(ref FlowTransaction tx, AuthorizerData authorizerData, FlowAccount authorizer);

        JObject ResolvePayerSignable(ref FlowTransaction tx, JObject signable);

        JObject ResolveSignMessage(string message, string sessionId);
    }
}