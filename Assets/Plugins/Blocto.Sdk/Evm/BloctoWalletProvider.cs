using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Blocto.Sdk.Core.Extension;
using Blocto.Sdk.Core.Model;
using Blocto.Sdk.Core.Utility;
using Blocto.Sdk.Evm.Model;
using Nethereum.Web3;
using UnityEngine;
using UnityEngine.Networking;

namespace Blocto.Sdk.Evm
{
    public class BloctoWalletProvider : BaseWalletProvider
    {
        public ChainEnum Chain { get; set; }
        
        public EthereumClient EthereumClient { get; private set; }
        
        public string NodeUrl { get; set;}
        
        private static EnvEnum env;
        
        private WebRequestUtility _webRequestUtility;
        
        private Action<string> _signMessageCallback;

        /// <summary>
        /// Create blocto wallet provider instance
        /// </summary>
        /// <param name="gameObject">Game object</param>
        /// <param name="env">Env</param>
        /// <param name="bloctoAppIdentifier">Blocto sdk appId</param>
        /// <param name="rpcUrl">RPC url</param>
        /// <returns>BloctoWalletProvider</returns>
        public static BloctoWalletProvider CreateBloctoWalletProvider(GameObject gameObject, EnvEnum env, Guid bloctoAppIdentifier, string rpcUrl = default)
        {
            var bloctoWalletProvider = gameObject.AddComponent<BloctoWalletProvider>();
            bloctoWalletProvider._webRequestUtility = gameObject.AddComponent<WebRequestUtility>();
            bloctoWalletProvider._webRequestUtility.BloctoAppId = bloctoAppIdentifier.ToString();
            
            if(rpcUrl != default)
            {
                bloctoWalletProvider.EthereumClient = new EthereumClient(rpcUrl, bloctoWalletProvider._webRequestUtility);
            }
            
            try
            {
                bloctoWalletProvider.gameObject.name = "bloctowalletprovider";
                bloctoWalletProvider.isCancelRequest = false;
                bloctoWalletProvider.bloctoAppIdentifier = bloctoAppIdentifier;
                BloctoWalletProvider.env = env;
            
                if(env == EnvEnum.Devnet)
                {
                    bloctoWalletProvider.backedApiDomain = bloctoWalletProvider.backedApiDomain.Replace("api", "api-dev");
                    bloctoWalletProvider.androidPackageName = $"{bloctoWalletProvider.androidPackageName}.dev";
                    bloctoWalletProvider.appSdkDomain = bloctoWalletProvider.appSdkDomain.Replace("blocto.app", "dev.blocto.app");
                    bloctoWalletProvider.webSdkDomain = bloctoWalletProvider.webSdkDomain.Replace("wallet.blocto.app", "wallet-dev.blocto.app");
                    bloctoWalletProvider.webSdkDomainV2 = bloctoWalletProvider.webSdkDomainV2.Replace("wallet-v2.blocto.app", "wallet-v2-dev.blocto.app");
                } 
            
                if(Application.platform == RuntimePlatform.Android)
                {
                    bloctoWalletProvider.InitializePlugins("com.blocto.unity.UtilityActivity");
                }
            
                bloctoWalletProvider.isInstalledApp = bloctoWalletProvider.IsInstalledApp(BloctoWalletProvider.env);
            }
            catch (Exception e)
            {
                e.Message.ToLog();
                throw;
            }
            
            return bloctoWalletProvider;
        }
        
        /// <summary>
        /// User connect wallet get account
        /// </summary>
        /// <param name="callBack">Get wallet address, then execute instructions</param>
        public new void RequestAccount(Action<string> callBack)
        {
            base.RequestAccount(callBack);
            
            $"Installed App: {isInstalledApp}, ForceUseWebView: {ForceUseWebView}".ToLog();
            if(isInstalledApp && ForceUseWebView == false)
            {
                var parameters = new Dictionary<string, string>
                                 {
                                     {"blockchain", Chain.ToString().ToLower()},
                                     {"method", "request_account"},
                                     {"request_id", requestId.ToString()}, 
                                 };
                
                var appSb = GenerateUrl(appSdkDomain, parameters);
                $"Url: {appSb}".ToLog();
                StartCoroutine(OpenUrl(appSb));
                return;
            }
            
            var webSb = CreateRequestAccountUrlV2(Chain.ToString().ToLower(), bloctoAppIdentifier.ToString());
            $"Url: {webSb}".ToLog();
            StartCoroutine(OpenUrl(webSb));
        }
        
        /// <summary>
        /// Sign message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="signType">Sing type</param>
        /// <param name="address">Address</param>
        /// <param name="callback"></param>
        public void SignMessage(string message, SignTypeEnum signType, string address, Action<string> callback)
        {
            base.SignMessage();
            _signMessageCallback = callback; 
            
            if(isInstalledApp && ForceUseWebView == false)
            {
                var parameters = new Dictionary<string, string>
                                 {
                                     {"blockchain", Chain.ToString().ToLower()},
                                     {"method", "sign_message"},
                                     {"from", address},
                                     {"type", signType.GetEnumDescription()},
                                     {"message", message },
                                     {"request_id", requestId.ToString()},
                                 };
                
                var appSb = GenerateUrl(appSdkDomain, parameters);
                $"Url: {appSb}".ToLog();
                StartCoroutine(OpenUrl(appSb));
                return;
            }
            
            var preRequest = new SignMessagePreRequest
                             {
                                 From = address,
                                 Message = message,
                                 Method = signType.GetEnumDescription()
                             };
            
            _webRequestUtility.SetHeader(new []
                                         {
                                             new KeyValuePair<string, string>("Blocto-Session-Identifier", sessionId),
                                             new KeyValuePair<string, string>("Blocto-Request-Identifier", requestId.ToString())
                                         });
            
            var preRequestUrl = UserSignatureApiUrl(Chain.ToString());
            var signMessagePreResponse = _webRequestUtility.GetResponse<SignMessagePreResponse>(preRequestUrl, HttpMethod.Post.ToString(), "application/json", preRequest);
            signatureId = signMessagePreResponse.SignatureId;
            $"SignatureId: {signatureId}".ToLog();
            
            var webSb = UserSignatureWebUrl(Chain.ToString());
            $"Url: {webSb}".ToLog();
            StartCoroutine(OpenUrl(webSb));
        }
        
        /// <summary>
        /// Send Transaction
        /// </summary>
        /// <param name="evmTransaction">Transaction data</param>
        /// <param name="callback"></param>
        public void SendTransaction(EvmTransaction evmTransaction, Action<string> callback)
        {
            base.SendTransaction(callback);
            
            if(isInstalledApp && ForceUseWebView == false)
            {
                var parameters = new Dictionary<string, string>
                                 {
                                     {"blockchain", Chain.ToString().ToLower()},
                                     {"method", "send_transaction"},
                                     {"from", evmTransaction.From},
                                     {"to", evmTransaction.To},
                                     {"value", $"0x{evmTransaction.HexValue}"},
                                     {"data", evmTransaction.Data},
                                     {"request_id", requestId.ToString()},
                                 };
                var appSb = GenerateUrl(appSdkDomain, parameters);
                $"Url: {appSb}".ToLog();
                StartCoroutine(OpenUrl(appSb));
                return;
            }
            
            _webRequestUtility.SetHeader(new []
                                         {
                                             new KeyValuePair<string, string>("Blocto-Session-Identifier", sessionId),
                                             new KeyValuePair<string, string>("Blocto-Request-Identifier", requestId.ToString())
                                         });
            
            var preRequestUrl = AuthzApiUrl(Chain.ToString());
            var transactionPreResponse = _webRequestUtility.GetResponse<TransactionPreResponse>(preRequestUrl, HttpMethod.Post.ToString(), "application/json", new List<EvmTransaction> {evmTransaction});
            
            var webSb = AuthzWebUrl(transactionPreResponse.AuthorizationId, Chain.ToString());
            $"Url: {webSb}".ToLog();
            StartCoroutine(OpenUrl(webSb.ToString()));
        }

        /// <summary>
        /// Get query result from smart contract
        /// </summary>
        /// <param name="abiUrl">Smart contract abi</param>
        /// <param name="contractAddress">Smart contract address</param>
        /// <param name="queryMethod">Query method</param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public TResult QueryForSmartContract<TResult>(Uri abiUrl, string contractAddress, string queryMethod)
        {
            var api = _webRequestUtility.GetResponse<AbiResult>(abiUrl.ToString(), HttpMethod.Get, "application/json");
            var web3 = new Web3(NodeUrl);
            var contract = web3.Eth.GetContract(api.Result, contractAddress);
            
            var funData = contract.GetFunction(queryMethod).CallAsync<TResult>().ConfigureAwait(false).GetAwaiter().GetResult();
            return funData;
        }
        
        /// <summary>
        /// Universal link handler on receive universal link
        /// </summary>
        /// <param name="link">universal link data from iOS</param>
        public void UniversalLinkCallbackHandler(string link)
        {
            $"Universal Link: {link}, in Handler".ToLog();
            var decodeLink = UnityWebRequest.UnEscapeURL(link);
            var item = decodeLink.RequestId();
            if (!requestIdActionMapper.TryGetValue(item.RequestId, out var action))
            {
                return;
            }

            switch (action)
            {
                case "CONNECTWALLET":
                    var result = UniversalLinkHandler(item.RemainContent, "address=");
                    sessionId = UniversalLinkHandler(item.RemainContent, "session_id=");

                    _connectWalletCallback.Invoke(result);
                    break;
                    
                case "SIGNMESSAGE":
                    var signature = UniversalLinkHandler(item.RemainContent, "signature=");
                    _signMessageCallback.Invoke(signature);
                    break;
                case "SENDTRANSACTION":
                    var sendTransaction = UniversalLinkHandler(item.RemainContent, "tx_hash");
                    _sendTransactionCallback.Invoke(sendTransaction);
                    break;
            }
        }
        
        private new string UniversalLinkHandler(string link, string keyword)
        {
            var data = (MatchContents: new List<string>(), RemainContent: link);
            data = CheckContent(data.RemainContent, keyword);
            var result = data.MatchContents.FirstOrDefault().AddressParser().Value;
            return result;
        }
    }
}