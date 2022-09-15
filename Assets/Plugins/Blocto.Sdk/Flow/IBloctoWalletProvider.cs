using Flow.FCL.WalletProvider;

namespace Blocto.SDK.Flow
{
    public interface IBloctoWalletProvider : IWalletProvider
    {
        public void CloseWebView();
    }
}