using System.Collections.Generic;
using Solnet.Rpc.Models;
using Solnet.Wallet;

namespace Blocto.Sdk.Solana.Extensions
{
    public static class TransactionExtensions
    {
        public static void PartialSign(this Transaction tx, IList<Account> signers)
        {
            tx.PartialSign(signers);
        }
    }
}