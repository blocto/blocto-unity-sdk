using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Blocto.Sdk.Core.Extension;
using Blocto.Sdk.Core.Model;
using Blocto.Sdk.Core.Utility;
using Blocto.Sdk.Ethereum.Model.Eth;
using Blocto.Sdk.Ethereum.Utility;
using Blocto.Sdk.Flow.Model;
using Flow.Net.Sdk.Core;
using UnityEngine;
using UnityEngine.Networking;

namespace Blocto.Sdk.Ethereum
{
    public class BloctoWalletProvider : BaseWalletProvider
    {
        public EthereumClient EthereumClient { get; private set; }
        
        private static string env;
        
        private Action<string> _connectWalletCallback;
        
        private Action<string> _signMessageCallback;
        
        private Action<string> _sendTransactionCallback;
        
        /// <summary>
        /// Create blocto wallet provider instance
        /// </summary>
        /// <param name="env">Env</param>
        /// <param name="bloctoAppIdentifier">Blocto sdk appId</param>
        /// <returns>BloctoWalletProvider</returns>
        public static BloctoWalletProvider CreateBloctoWalletProvider(GameObject gameObject, string env, Guid bloctoAppIdentifier)
        {
            // var bloctoWalletProvider = initialFun.Invoke((gameObject, flowClient, resolveUtility) => {
            //                                                  var provider = gameObject.AddComponent<BloctoWalletProvider>();
            //                                                  provider.WebRequestUtility = gameObject.AddComponent<WebRequestUtility>();
            //                                                  provider._resolveUtility = resolveUtility;
            //                                                  provider._flowClient = flowClient;
            //                                                  provider._requestIdActionMapper = new Dictionary<string, string>();
            //                                                  provider.ForcedUseWebView = false;
            //                                                  
            //                                                  return provider;
            //                                              });
            var bloctoWalletProvider = gameObject.AddComponent<BloctoWalletProvider>();
            var webRequestUtility = gameObject.AddComponent<WebRequestUtility>();
            webRequestUtility.BloctoAppId = bloctoAppIdentifier.ToString();
            try
            {
                bloctoWalletProvider.EthereumClient = new EthereumClient(webRequestUtility);
                bloctoWalletProvider.gameObject.name = "bloctowalletprovider";
                bloctoWalletProvider.isCancelRequest = false;
                bloctoWalletProvider.bloctoAppIdentifier = bloctoAppIdentifier;
                BloctoWalletProvider.env = env.ToLower();
            
                if(env.ToLower() == "testnet")
                {
                    bloctoWalletProvider.backedApiDomain = bloctoWalletProvider.backedApiDomain.Replace("api", "api-dev");
                    bloctoWalletProvider.androidPackageName = $"{bloctoWalletProvider.androidPackageName}.staging";
                    bloctoWalletProvider.appSdkDomain = bloctoWalletProvider.appSdkDomain.Replace("blocto.app", "dev.blocto.app");
                    bloctoWalletProvider.webSdkDomain = bloctoWalletProvider.webSdkDomain.Replace("wallet.blocto.app", "wallet-dev.blocto.app");
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
            var url = default(string);
            var requestId = Guid.NewGuid();
            requestIdActionMapper.Add(requestId.ToString(), "CONNECTWALLET");
            _connectWalletCallback = callBack;
            
            $"Installed App: {isInstalledApp}, ForceUseWebView: {ForceUseWebView}".ToLog();
            if(isInstalledApp && ForceUseWebView == false)
            {
                var appSb = new StringBuilder(appSdkDomain);
                appSb.Append($"app_id={Uri.EscapeUriString(bloctoAppIdentifier.ToString())}" + "&")
                  .Append($"blockchain=ethereum" + "&")
                  .Append($"method=request_account" + "&")
                  .Append($"request_id={requestId}");
                url = appSb.ToString();
                
                $"Url: {url}".ToLog();
                StartCoroutine(OpenUrl(url));
                return;
            }
            
            $"WebSDK domain: {webSdkDomain}".ToLog();
            var webSb = new StringBuilder(webSdkDomain);
            webSb.Append($"app_id={Uri.EscapeUriString(bloctoAppIdentifier.ToString())}" + "&")
                 .Append($"blockchain=ethereum" + "&")
                 .Append($"method=request_account" + "&")
                 .Append($"request_id={requestId}");
            url = webSb.ToString();
            
            $"Url: {url}".ToLog();
            StartCoroutine(OpenUrl(url));
        }
        
        public void SignMessage(string originMessage, SignTypeEnum signType, string address, Action<string> callback)
        {
            if(signType == SignTypeEnum.Eth_Sign)
            {
                originMessage = originMessage.StringToHex();
            }
            
            var requestId = Guid.NewGuid();
            requestIdActionMapper.Add(requestId.ToString(), "SIGNMESSAGE");
            _signMessageCallback = callback;
            
            if(isInstalledApp && ForceUseWebView == false)
            {
                var appSb = new StringBuilder(appSdkDomain);
                appSb.Append($"app_id={bloctoAppIdentifier}" + "&")
                  .Append("blockchain=ethereum" + "&")
                  .Append("method=sign_message" + "&")
                  .Append($"from={address}" + "&")
                  .Append($"type={signType.GetEnumDescription()}" + "&")
                  .Append($"message={Uri.EscapeUriString(originMessage)}" + "&")
                  .Append($"request_id={requestId}");
                
                $"Url: {appSb}".ToLog();
                StartCoroutine(OpenUrl(appSb.ToString()));
                return;
            }
            
            var webSb = new StringBuilder(webSdkDomain);
            webSb.Append($"app_id={bloctoAppIdentifier}" + "&")
              .Append("blockchain=ethereum" + "&")
              .Append("method=sign_message" + "&")
              .Append($"from={address}" + "&")
              .Append($"type={signType.GetEnumDescription()}" + "&")
              .Append($"message={Uri.EscapeUriString(originMessage)}" + "&")
              .Append($"request_id={requestId}");
            
            $"Url: {webSb}".ToLog();
            StartCoroutine(OpenUrl(webSb.ToString()));
        }
        
        public void SendTransaction(string fromAddress, string toAddress, decimal value, string data, Action<string> callback)
        {
            var transactionValue = EthConvert.ToWei(value);
            transactionValue = 1;
            var valueHex = transactionValue.ToString("X");
            
            
            var requestId = Guid.NewGuid();
            requestIdActionMapper.Add(requestId.ToString(), "SENDTRANSACTION");
            
            var sb = new StringBuilder("https://wallet-testnet.blocto.app/sdk?");
            sb.Append($"app_id={bloctoAppIdentifier}" + "&")
              .Append("blockchain=ethereum" + "&")
              .Append("method=send_transaction" + "&")
              .Append($"from={fromAddress}" + "&")
              .Append($"to={toAddress}" + "&")
              .Append($"value={transactionValue}" + "&")
              // .Append($"value=1" + "&")
              .Append($"data={data.StringToHex()}" + "&")
              .Append($"request_id={requestId}");
            
            $"Url: {sb}".ToLog();
            _sendTransactionCallback = callback;
            StartCoroutine(OpenUrl(sb.ToString()));
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
                        _connectWalletCallback.Invoke(result);
                        break;
                    
                    case "SIGNMESSAGE":
                        var signature = UniversalLinkHandler(item.RemainContent, "signature=");
                        _signMessageCallback.Invoke(signature);
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