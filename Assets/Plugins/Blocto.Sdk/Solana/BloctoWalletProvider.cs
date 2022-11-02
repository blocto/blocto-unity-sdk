using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Blocto.Sdk.Core.Extension;
using Blocto.Sdk.Core.Model;
using Blocto.Sdk.Core.Utility;
using Blocto.Sdk.Flow.Model;
// using Solnet.Rpc;
// using Solnet.Rpc.Builders;
// using Solnet.Rpc.Models;
// using Solnet.Wallet;
using UnityEngine;
using UnityEngine.Networking;

namespace Blocto.Sdk.Solana
{
    public class BloctoWalletProvider : BaseWalletProvider
    {
        // public IRpcClient SolanaClient { get; set; }
        
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
            var bloctoWalletProvider = gameObject.AddComponent<BloctoWalletProvider>();
            var webRequestUtility = gameObject.AddComponent<WebRequestUtility>();
            webRequestUtility.BloctoAppId = bloctoAppIdentifier.ToString();
            try
            {
                // bloctoWalletProvider.SolanaClient = ClientFactory.GetClient(Cluster.DevNet);
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
        
        
        // public void SignAndSendTransaction(string fromAddress, Transaction transaction)
        // {
        //     byte[] tx = new TransactionBuilder()
        //                .SetRecentBlockHash(transaction.RecentBlockHash)
        //                .SetFeePayer(transaction.FeePayer)
        //                .AddInstruction(transaction.Instructions.First())
        //                .BuildExecludeSign();        
        //     var stx = tx.Select(b => (sbyte)b).ToArray();
        //     var message = stx.ToHex();
        // }
        
        
        
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