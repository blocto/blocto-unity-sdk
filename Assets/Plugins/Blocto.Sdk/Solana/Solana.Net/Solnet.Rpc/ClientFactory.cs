using Microsoft.Extensions.Logging;
using Solnet.Rpc.Utilities;
using System.Net.WebSockets;
using Blocto.Sdk.Core.Extension;
using Blocto.Sdk.Core.Utility;

namespace Solnet.Rpc
{
    /// <summary>
    /// Implements a client factory for Solana RPC and Streaming RPC APIs.
    /// </summary>
    public static class ClientFactory
    {
        /// <summary>
        /// The dev net cluster.
        /// </summary>
        private const string RpcDevNet = "https://api.devnet.solana.com";

        /// <summary>
        /// The test net cluster.
        /// </summary>
        private const string RpcTestNet = "https://api.testnet.solana.com";

        /// <summary>
        /// The main net cluster.
        /// </summary>
        private const string RpcMainNet = "https://api.mainnet-beta.solana.com";


        /// <summary>
        /// The dev net cluster.
        /// </summary>
        private const string StreamingRpcDevNet = "wss://api.devnet.solana.com";

        /// <summary>
        /// The test net cluster.
        /// </summary>
        private const string StreamingRpcTestNet = "wss://api.testnet.solana.com";

        /// <summary>
        /// The main net cluster.
        /// </summary>
        private const string StreamingRpcMainNet = "wss://api.mainnet-beta.solana.com";

        /// <summary>
        /// Instantiate a http client.
        /// </summary>
        /// <param name="cluster">The network cluster.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>The http client.</returns>
        public static IRpcClient GetClient(Cluster cluster, ILogger logger)
        {
            return GetClient(cluster, logger, null);
        }

        /// <summary>
        /// Instantiate a http client.
        /// </summary>
        /// <param name="cluster">The network cluster.</param>
        /// <param name="webRequestUtility">WebRequestUtility instance</param>
        /// <returns>The http client.</returns>
        public static IRpcClient GetClient(Cluster cluster, WebRequestUtility webRequestUtility)
        {
            return GetClient(cluster, webRequestUtility, logger: null, rateLimiter: null);
        }

        /// Instantiate a http client.
        /// </summary>
        /// <param name="cluster">The network cluster.</param>
        /// <param name="webRequestUtility">WebRequestUtility instance</param>
        /// <param name="logger">The logger.</param>
        /// <param name="rateLimiter">An IRateLimiter instance or null.</param>
        /// <returns>The http client.</returns>
        public static IRpcClient GetClient(
            Cluster cluster,
            WebRequestUtility webRequestUtility, 
            ILogger logger = null,
            IRateLimiter rateLimiter = null)
        {
            return GetClient(cluster, logger, webRequestUtility, rateLimiter: rateLimiter);
        }

        /// <summary>
        /// Instantiate a http client.
        /// </summary>
        /// <param name="cluster">The network cluster.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="httpClient">A HttpClient instance. If null, a new instance will be created.</param>
        /// <param name="rateLimiter">An IRateLimiter instance or null.</param>
        /// <returns>The http client.</returns>
        public static IRpcClient GetClient(Cluster cluster, ILogger logger = null,
                WebRequestUtility webRequestUtility = null, IRateLimiter rateLimiter = null)
        {
            var url = cluster switch
            {
                Cluster.DevNet => RpcDevNet,
                Cluster.TestNet => RpcTestNet,
                _ => RpcMainNet,
            };

            $"RpcClient url: {url}".ToLog();
            
            return GetClient(url, logger, webRequestUtility, rateLimiter);
        }

        /// <summary>
        /// Instantiate a http client.
        /// </summary>
        /// <param name="url">The network cluster url.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>The http client.</returns>
        public static IRpcClient GetClient(string url, ILogger logger)
        {
            return GetClient(url, logger, null);
        }
        

        /// <summary>
        /// Instantiate a http client.
        /// </summary>
        /// <param name="url">The network cluster url.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="webRequestUtility">A WebRequestUtility instance. If null, a new instance will be created.</param>
        /// <param name="rateLimiter">An IRateLimiter instance or null.</param>
        /// <returns>The http client.</returns>
        public static IRpcClient GetClient(string url, ILogger logger = null, WebRequestUtility webRequestUtility = null, IRateLimiter rateLimiter = null)
        {
            return new SolanaRpcClient(url, webRequestUtility);
        }

        /// <summary>
        /// Instantiate a streaming client.
        /// </summary>
        /// <param name="cluster">The network cluster.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>The streaming client.</returns>
        public static IStreamingRpcClient GetStreamingClient(
            Cluster cluster,
            ILogger logger = null)
        {
            var url = cluster switch
            {
                Cluster.DevNet => StreamingRpcDevNet,
                Cluster.TestNet => StreamingRpcTestNet,
                _ => StreamingRpcMainNet,
            };

            return GetStreamingClient(url, logger);
        }

        /// <summary>
        /// Instantiate a streaming client.
        /// </summary>
        /// <param name="url">The network cluster url.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="clientWebSocket">A ClientWebSocket instance. If null, a new instance will be created.</param>
        /// <returns>The streaming client.</returns>
        public static IStreamingRpcClient GetStreamingClient(string url, ILogger logger = null, ClientWebSocket clientWebSocket = null)
        {
            return new SolanaStreamingRpcClient(url, logger, null, clientWebSocket);
        }
    }
}