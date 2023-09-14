using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Blocto.Sdk.Core.Extension;
using Blocto.Sdk.Core.Model;
using Blocto.Sdk.Core.Utility;
using Blocto.Sdk.Evm.Model;
using Nethereum.Web3;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Blocto.Sdk.Evm
{
    public class BloctoWalletProvider : BaseWalletProvider
    {
        public ChainEnum Chain { get; set; }
        
        public EthereumClient EthereumClient { get; private set; }
        
        public string NodeUrl { get; set;}
        
        protected string ConnectedWalletAddress;
        
        protected Action<string> SignMessageCallback;
        
        private static EnvEnum env;
        
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
            bloctoWalletProvider.WebRequestUtility = gameObject.AddComponent<WebRequestUtility>();
            bloctoWalletProvider.WebRequestUtility.BloctoAppId = bloctoAppIdentifier.ToString();
            
            if(rpcUrl != default)
            {
                bloctoWalletProvider.EthereumClient = new EthereumClient(rpcUrl, bloctoWalletProvider.WebRequestUtility);
            }
            
            try
            {
                bloctoWalletProvider.gameObject.name = "bloctowalletprovider";
                bloctoWalletProvider.IsCancelRequest = false;
                bloctoWalletProvider.BloctoAppIdentifier = bloctoAppIdentifier;
                BloctoWalletProvider.env = env;
            
                if(env == EnvEnum.Devnet)
                {
                    bloctoWalletProvider.BackedApiDomain = bloctoWalletProvider.BackedApiDomain.Replace("api", "api-dev");
                    bloctoWalletProvider.AndroidPackageName = $"{bloctoWalletProvider.AndroidPackageName}.dev";
                    bloctoWalletProvider.AppSdkDomain = bloctoWalletProvider.AppSdkDomain.Replace("blocto.app", "dev.blocto.app");
                    bloctoWalletProvider.WebSdkDomain = bloctoWalletProvider.WebSdkDomain.Replace("wallet.blocto.app", "wallet-dev.blocto.app");
                    bloctoWalletProvider.WebSdkDomainV2 = bloctoWalletProvider.WebSdkDomainV2.Replace("wallet-v2.blocto.app", "wallet-v2-dev.blocto.app");
                } 
            
                if(Application.platform == RuntimePlatform.Android)
                {
                    bloctoWalletProvider.InitializePlugins("com.blocto.unity.UtilityActivity");
                }
            
                bloctoWalletProvider.IsInstalledApp = bloctoWalletProvider.CheckInstalledApp(BloctoWalletProvider.env);
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
            var url = CreateConnectWalletUrl();
            StartCoroutine(OpenUrl(url));
        }

        /// <summary>
        /// Create url for AppSDK or WebSDK
        /// </summary>
        /// <returns></returns>
        protected virtual string CreateConnectWalletUrl()
        {
            $"Installed App: {IsInstalledApp}, ForceUseWebView: {ForceUseWebView}".ToLog();
            if(IsInstalledApp && ForceUseWebView == false)
            {
                var parameters = new Dictionary<string, string>
                {
                    {"blockchain", Chain.ToString().ToLower()},
                    {"method", "request_account"},
                    {"request_id", RequestId.ToString()}, 
                };
                
                var appSb = GenerateUrl(AppSdkDomain, parameters);
                $"Url: {appSb}".ToLog();
                return appSb;
            }
            
            var webSb = CreateRequestAccountUrlV2(Chain.ToString().ToLower(), BloctoAppIdentifier.ToString());
            $"Url: {webSb}".ToLog();
            return webSb;
        }
        
        /// <summary>
        /// Sign message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="signType">Sing type</param>
        /// <param name="callback"></param>
        public void SignMessage(string message, SignTypeEnum signType, Action<string> callback)
        {
            base.SignMessage();
            SignMessageCallback = callback; 
            
            var url = CreateSignMessageUrl(message, signType);
            $"Url: {url}".ToLog();
            StartCoroutine(OpenUrl(url));
        }

        protected virtual string CreateSignMessageUrl(string message, SignTypeEnum signType)
        {
            if (IsInstalledApp && ForceUseWebView == false)
            {
                var parameters = new Dictionary<string, string>
                {
                    { "blockchain", Chain.ToString().ToLower() },
                    { "method", "sign_message" },
                    { "from", ConnectedWalletAddress },
                    { "type", signType.GetEnumDescription() },
                    { "message", message },
                    { "request_id", RequestId.ToString() },
                };

                var appSb = GenerateUrl(AppSdkDomain, parameters);
                $"Url: {appSb}".ToLog();
                return appSb;
            }

            var preRequest = new SignMessagePreRequest
            {
                From = ConnectedWalletAddress,
                Message = message,
                Method = signType.GetEnumDescription()
            };

            var signMessagePreResponse =
                SendData<SignMessagePreRequest, SignMessagePreResponse>(Chain.ToString(), UserSignatureApiUrl, preRequest);
            SignatureId = signMessagePreResponse.SignatureId;
            $"SignatureId: {SignatureId}, Response: {JsonConvert.SerializeObject(signMessagePreResponse)}".ToLog();

            var webSb = UserSignatureWebUrl(Chain.ToString());
            return webSb;
        }

        /// <summary>
        /// Send transaction
        /// </summary>
        /// <param name="fromAddress">wallet address</param>
        /// <param name="toAddress">recipient address or contract address</param>
        /// <param name="value">token amount</param>
        /// <param name="data">data</param>
        /// <param name="callback"></param>
        public void SendTransaction(string fromAddress, string toAddress, decimal value, string data, Action<string> callback)
        {
            var transaction = new EvmTransaction
                              {
                                  From = fromAddress,
                                  To = toAddress,
                                  Value = value,
                                  Data = data
                              };
            SendTransaction(transaction, callback);
        }
        
        /// <summary>
        /// Send Transaction
        /// </summary>
        /// <param name="evmTransaction">Transaction data</param>
        /// <param name="callback"></param>
        public void SendTransaction(EvmTransaction evmTransaction, Action<string> callback)
        {
            base.SendTransaction(callback);
            
            if(IsInstalledApp && ForceUseWebView == false)
            {
                var parameters = new Dictionary<string, string>
                                 {
                                     {"blockchain", Chain.ToString().ToLower()},
                                     {"method", "send_transaction"},
                                     {"from", evmTransaction.From},
                                     {"to", evmTransaction.To},
                                     {"value", $"0x{evmTransaction.HexValue}"},
                                     {"data", evmTransaction.Data},
                                     {"request_id", RequestId.ToString()},
                                 };
                var appSb = GenerateUrl(AppSdkDomain, parameters);
                $"Url: {appSb}".ToLog();
                StartCoroutine(OpenUrl(appSb));
                return;
            }
            
            var transactionPreResponse = SendData<List<EvmTransaction>, TransactionPreResponse>(Chain.ToString(),AuthzApiUrl, new List<EvmTransaction> {evmTransaction});
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
            var api = WebRequestUtility.GetResponse<AbiResult>(abiUrl.ToString(), HttpMethod.Get, "application/json");
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
            if (!RequestIdActionMapper.TryGetValue(item.RequestId, out var action))
            {
                return;
            }

            switch (action)
            {
                case "CONNECTWALLET":
                    var result = UniversalLinkHandler(item.RemainContent, "address=");
                    ConnectedWalletAddress = result;

                    if (item.RemainContent.Contains("session_id"))
                    {
                        SessionId = UniversalLinkHandler(item.RemainContent, "session_id=");
                        $"SessionId: {SessionId}, ConnectedWalletAddress: {ConnectedWalletAddress}".ToLog();
                    }
                    
                    ConnectWalletCallback.Invoke(result);
                    break;
                    
                case "SIGNMESSAGE":
                    var signature = UniversalLinkHandler(item.RemainContent, "signature=");
                    SignMessageCallback.Invoke(signature);
                    break;
                case "SENDTRANSACTION":
                    var sendTransaction = UniversalLinkHandler(item.RemainContent, "tx_hash");
                    SendTransactionCallback.Invoke(sendTransaction);
                    break;
            }
        }
        

        private new string UniversalLinkHandler(string link, string keyword)
        {
            $"Link: {link}, Keyword: {keyword}".ToLog();
            var data = (MatchContents: new List<string>(), RemainContent: link);
            data = CheckContent(data.RemainContent, keyword);
            var result = data.MatchContents.FirstOrDefault().AddressParser().Value;
            return result;
        }
    }
}