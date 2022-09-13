using Flow.FCL.WalletProvider;

namespace Blocto.Flow
{
    public interface IBloctoWalletProvider : IWalletProvider
    {
        public void CloseWebView();
    }
}