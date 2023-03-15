using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Blocto.Sdk.Aptos.Model;
using Blocto.Sdk.Core.Extension;
using Blocto.Sdk.Core.Model;
using Blocto.Sdk.Core.Utility;
using UnityEngine;
using UnityEngine.Networking;

namespace Blocto.Sdk.Aptos
{
    public class BloctoWalletProvider : BaseWalletProvider
    {
        public string NodeUrl { get; set;}
        
        public const string AptosCoinType = "0x1::aptos_coin::AptosCoin";
 
        private static EnvEnum env;
        
        private string _chainName = "aptos";
        
        private string _sessionId = "wIiSdPKvXlk-axLxA7KYh-6MyH7y0-97SHK8GnUCQgE";
        
        private string _signatureId;
        
        private string _authorizationId;
        
        private WebRequestUtility _webRequestUtility;
        
        private Action<string> _connectWalletCallback;
        
        private Action<SignMessageResponse> _signMessageCallback;
        
        private Action<string> _sendTransactionCallback;
        
        /// <summary>
        /// Create blocto wallet provider instance
        /// </summary>
        /// <param name="env">Env</param>
        /// <param name="bloctoAppIdentifier">Blocto sdk appId</param>
        /// <returns>BloctoWalletProvider</returns>
        public static BloctoWalletProvider CreateBloctoWalletProvider(GameObject gameObject, EnvEnum env, Guid bloctoAppIdentifier, string rpcUrl = default)
        {
            var bloctoWalletProvider = gameObject.AddComponent<Aptos.BloctoWalletProvider>();
            bloctoWalletProvider._webRequestUtility = gameObject.AddComponent<WebRequestUtility>();
            bloctoWalletProvider._webRequestUtility.BloctoAppId = bloctoAppIdentifier.ToString();
            
            //// aptos use v2 domain
            bloctoWalletProvider.webSdkDomain = "https://wallet-v2.blocto.app/";
            
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
        
        public void RequestAccount(Action<string> callBack)
        {
            $"Aptos blocto provider".ToLog();
            var url = default(string);
            var requestId = Guid.NewGuid();
            $"Installed App: {isInstalledApp}, ForceUseWebView: {ForceUseWebView}".ToLog();
            
            isInstalledApp = false;
            // url = CreateRequestAccountUrl(isInstalledApp, _chainName, requestId.ToString());
            url = CreateRequestAccountUrlV2(_chainName, requestId.ToString(), bloctoAppIdentifier.ToString());
            $"Url: {url}".ToLog();
            StartCoroutine(OpenUrl(url));
        }
        
        public void SignMessage(string message, string address, Action<SignMessageResponse> callback)
        {
            var requestId = Guid.NewGuid();
            
            var parameters = new Dictionary<string, string>
                             {
                                 {"blockchain", _chainName},
                                 {"method", "sign_message"},
                                 {"from", address},
                                 {"message", message },
                                 {"request_id", requestId.ToString()},
                             };
            
            requestIdActionMapper.Add(requestId.ToString(), "SIGNMESSAGE");
            _signMessageCallback = callback;
            
            if(isInstalledApp && ForceUseWebView == false)
            {
                var appSb = GenerateUrl(appSdkDomain, parameters);
                $"Url: {appSb}".ToLog();
                StartCoroutine(OpenUrl(appSb.ToString()));
                return;
            }
            
            var headers = new Dictionary<string, string>
                          {
                              {"Blocto-Session-Identifier", _sessionId},
                              {"Blocto-Request-Identifier", requestId.ToString()}
                          };
            
            _webRequestUtility.Headers = headers;
            var payload = new SignMessagePreRequest
                          {
                              Address = null,
                              Message = null,
                              Nonce = null,
                              IsIncludeAddress = false,
                              IsIncludeApplication = false,
                              IsIncludeChainId = false
                          };
            
            var preRequestUrl = $"{webSdkDomain}/api/aptos/user-signature";
            var signMessagePreResponse = _webRequestUtility.GetResponse<SignMessagePreResponse>(preRequestUrl, HttpMethod.Post.ToString(), "application/json", payload);
            _signatureId = signMessagePreResponse.SignatureId;
            
            var sb = new StringBuilder(webSdkDomain);
            sb.Append($"/{bloctoAppIdentifier}");
            sb.Append($"/aptos/sdk/user-signature");
            sb.Append($"/{signMessagePreResponse.SignatureId}");
            var webSb = sb.ToString();
            $"Url: {webSb}".ToLog();
            StartCoroutine(OpenUrl(webSb.ToString()));
        }
        
        /// <summary>
        /// Send transaction for apt
        /// </summary>
        /// <param name="from">From address</param>
        /// <param name="to">To address</param>
        /// <param name="amount">Amount</param>
        /// <param name="callback"></param>
        public void SendTransaction(string from, string to, ulong amount, Action<string> callback)
        {
            var payload = new EntryFunctionTransactionPayload
                          {
                              Address = from,
                              Arguments = new string[]
                                          {
                                              to,
                                              amount.ToString()
                                          },
                              TypeArguments = new string[]
                                              {
                                                  BloctoWalletProvider.AptosCoinType
                                              },
                              MaxGasAmount = "9999",
                              Function = "0x1::coin::transfer"
                          };
            SendTransaction(payload, callback);
        }
        
        /// <summary>
        /// Send transaction
        /// </summary>
        /// <param name="from">From address</param>
        /// <param name="to">To address</param>
        /// <param name="amount">Amount</param>
        /// <param name="coinType">Coin type</param>
        /// <param name="function">Entry function name</param>
        /// <param name="callback"></param>
        public void SendTransaction(string from, string to, ulong amount, string coinType, string function, Action<string> callback)
        {
            var payload = new EntryFunctionTransactionPayload
                          {
                              Address = from,
                              Arguments = new string[]
                                          {
                                              to,
                                              amount.ToString()
                                          },
                              TypeArguments = new string[]
                                              {
                                                  coinType
                                              },
                              MaxGasAmount = "9999",
                              Function = function
                          };
            SendTransaction(payload, callback);
        }
        
        /// <summary>
        /// Send transaction
        /// </summary>
        /// <param name="scriptPayload">Script payload body</param>
        /// <param name="callback"></param>
        public void SendTransaction<TTransactionType>(TTransactionType transactionPayload, Action<string> callback)
        {
            var requestId = Guid.NewGuid();
            var parameters = new Dictionary<string, string>
                             {
                                 {"blockchain", _chainName},
                                 {"method", "send_transaction"},
                                 {"request_id", requestId.ToString()},
                             };
            
            requestIdActionMapper.Add(requestId.ToString(), "SENDTRANSACTION");
            _sendTransactionCallback = callback;
            
            var headers = new Dictionary<string, string>
                          {
                              {"Blocto-Session-Identifier", _sessionId},
                              {"Blocto-Request-Identifier", requestId.ToString()}
                          };
            
            _webRequestUtility.Headers = headers;
            var preRequestUrl = $"{webSdkDomainV2}/api/aptos/authz";
            var transactionPreResponse = _webRequestUtility.GetResponse<TransactionPreResponse>(preRequestUrl, HttpMethod.Post.ToString(), "application/json", transactionPayload);
            _authorizationId = transactionPreResponse.AuthorizationId;
            
            var sb = new StringBuilder(webSdkDomainV2);
            sb.Append($"/{bloctoAppIdentifier}");
            sb.Append($"/aptos/sdk/authz");
            sb.Append($"/{transactionPreResponse.AuthorizationId}");
            var webSb = sb.ToString();
            $"Url: {webSb}".ToLog();
            StartCoroutine(OpenUrl(webSb.ToString()));
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
            var result = default(string);
            if(requestIdActionMapper.ContainsKey(item.RequestId))
            {
                var action = requestIdActionMapper[item.RequestId];
                switch (action)
                {
                    case "CONNECTWALLET":
                        result = UniversalLinkHandler(item.RemainContent, "address=");
                        _sessionId = UniversalLinkHandler(item.RemainContent, "session_id=");
                        _connectWalletCallback.Invoke(result);
                        break;
                    
                    case "SIGNMESSAGE":
                        var signatureResult = UniversalLinkHandler(item.RemainContent, "result=");
                        if(signatureResult == "ok")
                        {
                            var headers = new Dictionary<string, string>
                                          {
                                              {"Blocto-Session-Identifier", _sessionId},
                                          };
                            
                             var requestUrl = $"{webSdkDomain}/api/aptos/user-signature/{_signatureId}";
                             var signMessageResponse = _webRequestUtility.GetResponse<SignMessageResponse>(requestUrl, HttpMethod.Get, "application/json");
                             _signMessageCallback.Invoke(signMessageResponse);
                        }
                        
                        break;
                    case "SENDTRANSACTION":
                        var sendTransaction = UniversalLinkHandler(item.RemainContent, "tx_hash");
                        _sendTransactionCallback.Invoke(sendTransaction);
                        break;
                }
            }
        }
        
        private string UniversalLinkHandler(string link, string keyword)
        {
            var result = default(string);
            var data = (MatchContents: new List<string>(), RemainContent: link);
            data = CheckContent(data.RemainContent, keyword);
            result = data.MatchContents.FirstOrDefault().AddressParser().Value;
            return result;
        }
    }
}