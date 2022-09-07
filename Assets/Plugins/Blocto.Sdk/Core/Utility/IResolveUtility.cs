using Flow.FCL.Models.Authz;
using Flow.Net.Sdk.Core.Models;
using Newtonsoft.Json.Linq;

namespace Blocto.Sdk.Core.Utility
{
    public interface IResolveUtility
    {
        JObject ResolvePreSignable(ref FlowTransaction tx);

        JObject ResolveSignable(ref FlowTransaction tx, PreAuthzData preAuthzData, FlowAccount authorizer);

        JObject ResolvePayerSignable(ref FlowTransaction tx, JObject signable);

        JObject ResolveSignMessage(string message, string sessionId);
    }
}