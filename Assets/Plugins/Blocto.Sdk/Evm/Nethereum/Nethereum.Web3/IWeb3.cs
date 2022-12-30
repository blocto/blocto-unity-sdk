using Nethereum.Contracts.Services;
using Nethereum.JsonRpc.Client;
using Nethereum.RPC;
using Nethereum.RPC.DebugNode;
using Nethereum.RPC.TransactionManagers;
using Nethereum.RPC.TransactionReceipts;

namespace Nethereum.Web3
{
    public interface IWeb3
    {
        IClient Client { get; }
        IEthApiContractService Eth { get; }
        INetApiService Net { get; }
        IPersonalApiService Personal { get; }
        IShhApiService Shh { get; }
        ITransactionManager TransactionManager { get; set; }
        ITransactionReceiptService TransactionReceiptPolling { get; set; }
        IDebugApiService Debug { get; }
        FeeSuggestionService FeeSuggestion { get; }
    }
}