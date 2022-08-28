using System.Collections.Generic;
using Flow.FCL.Models.Authz;

namespace Flow.FCL.Utility
{
    public interface IResolveUtils
    {
        PreSignable  ResolvePreSignable(List<BaseArgument> args, string script, int computeLimit);
        PreSignable ResolvePreSignable(string addr, string script, string lastBlockId);
        Signable ResolveAuthorizerSignable(Account proposer, Account payer, List<Account> authorizations);
        Signable ResolvePayerSignable(Account payer, Account authorizer, Signable signable, string signature);
    }
}