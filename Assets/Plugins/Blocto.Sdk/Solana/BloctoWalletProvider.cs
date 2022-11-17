using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Blocto.Sdk.Core.Extension;
using Blocto.Sdk.Core.Model;
using Blocto.Sdk.Core.Utility;
using Blocto.Sdk.Flow.Model;
using Blocto.Sdk.Solana.Model;
using Solnet.Rpc;
using Solnet.Rpc.Builders;
using Solnet.Rpc.Models;
using UnityEngine;
using UnityEngine.Networking;

namespace Blocto.Sdk.Solana
{
    public class BloctoWalletProvider : BaseWalletProvider
    {
        public IRpcClient SolanaClient { get; set; }
        
        private static string env;
        
        private static readonly string chainName = "solana";
        
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
            var bloctoWalletProvider = gameObject.AddComponent<BloctoWalletProvider>();
            var webRequestUtility = gameObject.AddComponent<WebRequestUtility>();
            webRequestUtility.BloctoAppId = bloctoAppIdentifier.ToString();
            try
            {
                bloctoWalletProvider.SolanaClient = ClientFactory.GetClient(Cluster.DevNet, webRequestUtility);
                bloctoWalletProvider.gameObject.name = "bloctowalletprovider";
                bloctoWalletProvider.isCancelRequest = false;
                bloctoWalletProvider.bloctoAppIdentifier = bloctoAppIdentifier;
                BloctoWalletProvider.env = env.ToLower();
            
                if(env.ToLower() == "testnet")
                {
                    bloctoWalletProvider.backedApiDomain = bloctoWalletProvider.backedApiDomain.Replace("api", "api-dev");
                    bloctoWalletProvider.androidPackageName = $"{bloctoWalletProvider.androidPackageName}.staging";
                    bloctoWalletProvider.appSdkDomain = bloctoWalletProvider.appSdkDomain.Replace("blocto.app", "dev.blocto.app");
                    bloctoWalletProvider.webSdkDomain = bloctoWalletProvider.webSdkDomain.Replace("wallet.blocto.app", "wallet-testnet.blocto.app");
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
                  .Append($"blockchain={BloctoWalletProvider.chainName}" + "&")
                  .Append($"method={ActionNameEnum.Request_Account.ToString().ToLower()}" + "&")
                  .Append($"request_id={requestId}");
                url = appSb.ToString();
                
                $"Url: {url}".ToLog();
                StartCoroutine(OpenUrl(url));
                return;
            }
            
            $"WebSDK domain: {webSdkDomain}".ToLog();
            var webSb = new StringBuilder(webSdkDomain);
            webSb.Append($"app_id={Uri.EscapeUriString(bloctoAppIdentifier.ToString())}" + "&")
                 .Append($"blockchain={BloctoWalletProvider.chainName}" + "&")
                 .Append($"method={ActionNameEnum.Request_Account.ToString().ToLower()}" + "&")
                 .Append($"request_id={requestId}");
            url = webSb.ToString();
            
            $"Url: {url}".ToLog();
            StartCoroutine(OpenUrl(url));
        }
        
        
        public void SignAndSendTransaction(string fromAddress, Transaction transaction, Action<string> callBack)
        {
            var url = default(string);
            var requestId = Guid.NewGuid();
            requestIdActionMapper.Add(requestId.ToString(), "SIGNANDSENETRANSACTION");
            _connectWalletCallback = callBack;
            
            var tx = new TransactionBuilder()
                       .SetRecentBlockHash(transaction.RecentBlockHash)
                       .SetFeePayer(transaction.FeePayer)
                       .AddInstruction(transaction.Instructions.First())
                       .BuildExecludeSign();        
            
            var stx = tx.Select(b => (sbyte)b).ToArray();
            var message = tx.ToHex();
            $"Message: {message}".ToLog();
            
            $"Installed App: {isInstalledApp}, ForceUseWebView: {ForceUseWebView}".ToLog();
            if(isInstalledApp && ForceUseWebView == false)
            {
                var appSb = new StringBuilder(appSdkDomain);
                appSb.Append($"app_id={Uri.EscapeUriString(bloctoAppIdentifier.ToString())}" + "&")
                     .Append($"method={ActionNameEnum.Sign_And_Send_Transaction.ToString().ToLower()}" + "&")
                     .Append($"blockchain={chainName}" + "&")
                     .Append($"from={fromAddress}" + "&")
                     .Append($"message={message}" + "&")
                     .Append($"is_invoke_wrapped=false" + "&")
                     .Append($"request_id={requestId}");
                url = appSb.ToString();
                
                $"Url: {url}".ToLog();
                StartCoroutine(OpenUrl(url));
                return;
            }
            
            // https: //dev.blocto.app/sdk?
            // app_id=57f397df-263c-4e97-b61f-15b67b9ce285
            //     request_id=09a4f0be-f510-4bc8-b5f2-1ad3a7e625de
            //     method=sign_and_send_transaction
            // blockchain=solana
            // from=CXxPxb5GAkqjVKxb3PkxFZmUus9YrTVuUoLWee4gm8ZR
            // message=01000103ab5e9de22540782ca12919306e8e1811d9e8f2bd1f54886b21837ec5c01a47442f044d6abceb87a88416562a21f1bb49e216f5f7a829bc88763a2b0664680fa3dfc7f2af100827ea33addbb9d430e457f5311bb905cb3a86a721bc58d72b270161f49fde4c3574bdf140d520f2b70dc1a0ffe160c9712cd135bf89c0d61465d701020201000500c78aa900
            //     is_invoke_wrapped=false
                    
            $"WebSDK domain: {webSdkDomain}".ToLog();
            var webSb = new StringBuilder(webSdkDomain);
            webSb.Append($"app_id={Uri.EscapeUriString(bloctoAppIdentifier.ToString())}" + "&")
                 .Append($"method={ActionNameEnum.Sign_And_Send_Transaction.ToString().ToLower()}" + "&")
                 .Append($"blockchain={chainName}" + "&")
                 .Append($"from={fromAddress}" + "&")
                 .Append($"message={message}" + "&")
                 .Append($"is_invoke_wrapped=false" + "&")
                 .Append($"request_id={requestId}");
            url = webSb.ToString();
            
            $"Url: {url}".ToLog();
            StartCoroutine(OpenUrl(url));
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