namespace Script.Model
{
    public static class EvmChain
    {
        static EvmChain()
        {
            ETHEREUM = new ChainInformation
                       {
                           Title = "Ethereum",
                           Symbol = "ETH",
                           MainnetContractAddress = "your contract address",
                           TestnetContractAddress = "your contract address",
                           MainnetRpcUrl = "your Rpc Url",
                           TestnetRpcUrl = "your Rpc Url",
                           MainnetExplorerDomain = "etherscan.io",
                           TestnetExplorerDomain = "rinkeby.etherscan.io",
                       };    
        }
        
        public static ChainInformation ETHEREUM { get; set; }

        public static ChainInformation BNB_CHAIN { get; set; }
        
        public static ChainInformation POLYGON { get; set; }
        
        public static ChainInformation AVALANCHE { get; set; }
    }
}