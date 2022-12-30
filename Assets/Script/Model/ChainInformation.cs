namespace Script.Model
{
    public class ChainInformation
    {
        public string Title { get; set; }

        public string Symbol { get; set; }

        public string MainnetContractAddress { get; set; }

        public string TestnetContractAddress { get; set; }

        public string MainnetRpcUrl { get; set; }

        public string TestnetRpcUrl { get; set; }

        public string MainnetExplorerDomain { get; set; }

        public string TestnetExplorerDomain { get; set; }

        public string MainnetExplorerApiUrl { get; set; }
        
        public string TestnetExplorerApiUrl { get; set; }
    }
}