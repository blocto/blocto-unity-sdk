using System;
using System.Collections.Generic;
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
        
        private readonly string _chainName = "aptos";
        
        private string _keyAddress;
        
        private List<string> _publicKeys;
        
        private WebRequestUtility _webRequestUtility;
        
        private Action<SignMessageResponse> _signMessageCallback;

        /// <summary>
        /// Create blocto wallet provider instance
        /// </summary>
        /// <param name="gameObject">Parent GameObject</param>
        /// <param name="env">Env</param>
        /// <param name="bloctoAppIdentifier">Blocto sdk appId</param>
        /// <param name="nodeUrl"></param>
        /// <returns>BloctoWalletProvider</returns>
        public static BloctoWalletProvider CreateBloctoWalletProvider(GameObject gameObject, EnvEnum env, Guid bloctoAppIdentifier, string nodeUrl = default)
        {
            var bloctoWalletProvider = gameObject.AddComponent<BloctoWalletProvider>();
            bloctoWalletProvider._webRequestUtility = gameObject.AddComponent<WebRequestUtility>();
            bloctoWalletProvider._webRequestUtility.BloctoAppId = bloctoAppIdentifier.ToString();
            
            //// aptos use v2 domain
            bloctoWalletProvider.WebSdkDomain = "https://wallet-v2.blocto.app/";
            
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
            
            if(nodeUrl != default)
            {
                bloctoWalletProvider.NodeUrl = nodeUrl;
            }
            
            return bloctoWalletProvider;
        }
        
        /// <summary>
        /// Get the wallet address
        /// </summary>
        /// <param name="callBack">Get wallet address, then execute instructions</param>
        public new void RequestAccount(Action<string> callBack)
        {
            base.RequestAccount(callBack);
            
            //// just support webSDK now - 20230323
            IsInstalledApp = false;
            $"Installed App: {IsInstalledApp}, ForceUseWebView: {ForceUseWebView}".ToLog();
            
            var url = CreateRequestAccountUrlV2(_chainName, BloctoAppIdentifier.ToString());
            $"Url: {url}".ToLog();
            
            StartCoroutine(OpenUrl(url));
        }
        
        /// <summary>
        /// Get public key of address
        /// </summary>
        /// <param name="address">address of publ</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public List<string> PublicKeys(string address)
        {
            if( _keyAddress != address || _publicKeys.Count == 0)
            {
                var url = $"{BackedApiDomain}/aptos/accounts/{address}";
                var response = _webRequestUtility.GetResponse<PublicKey>(url, HttpMethod.Get, "");
                
                $"Public keys: {string.Join(',', response.Keys)}".ToLog();
                _keyAddress = address;
                _publicKeys = response.Keys;
                return response.Keys;
            }
            
            if( _publicKeys.Count > 0)
            {
                return _publicKeys;
            }
            
            throw new Exception("");
        }
        
        /// <summary>
        /// Sign message
        /// </summary>
        /// <param name="preRequest">sign message body</param>
        /// <param name="callback">Complete sign message call back function</param>
        public void SignMessage(SignMessagePreRequest preRequest, Action<SignMessageResponse> callback)
        {
            base.SignMessage();
            _signMessageCallback = callback; 
            
            if(IsInstalledApp && ForceUseWebView == false)
            {
                var parameters = new Dictionary<string, string>
                                 {
                                     {"blockchain", _chainName},
                                     {"method", "sign_message"},
                                     {"from", preRequest.Address},
                                     {"message", preRequest.Message },
                                     {"request_id", RequestId.ToString()},
                                 };
                
                var appSb = GenerateUrl(AppSdkDomain, parameters);
                $"Url: {appSb}".ToLog();
                StartCoroutine(OpenUrl(appSb));
                return;
            }
            
            _webRequestUtility.Headers = new Dictionary<string, string>
                                         {
                                             {"Blocto-Session-Identifier", SessionId},
                                             {"Blocto-Request-Identifier", RequestId.ToString()}
                                         };
            
            var preRequestUrl = $"{WebSdkDomainV2}/api/aptos/user-signature";
            var signMessagePreResponse = _webRequestUtility.GetResponse<SignMessagePreResponse>(preRequestUrl, HttpMethod.Post.ToString(), "application/json", preRequest);
            SignatureId = signMessagePreResponse.SignatureId;
            $"SignatureId: {SignatureId}".ToLog();
            
            var sb = new StringBuilder(WebSdkDomainV2);
            sb.Append($"/{BloctoAppIdentifier}");
            sb.Append($"/aptos/user-signature");
            sb.Append($"/{signMessagePreResponse.SignatureId}");
            
            var webSb = sb.ToString();
            $"Url: {webSb}".ToLog();
            StartCoroutine(OpenUrl(webSb));
        }

        /// <summary>
        /// Send transaction
        /// </summary>
        /// <param name="transactionPayload">Script payload body</param>
        /// <param name="callback"></param>
        public void SendTransaction<TTransactionType>(TTransactionType transactionPayload, Action<string> callback)
        {
            base.SendTransaction(callback);
            _webRequestUtility.Headers = new Dictionary<string, string>
                                         {
                                             {"Blocto-Session-Identifier", SessionId},
                                             {"Blocto-Request-Identifier", RequestId.ToString()}
                                         };
            
            var preRequestUrl = $"{WebSdkDomainV2}/api/aptos/authz";
            var transactionPreResponse = _webRequestUtility.GetResponse<TransactionPreResponse>(preRequestUrl, HttpMethod.Post.ToString(), "application/json", transactionPayload);
            
            var sb = new StringBuilder(WebSdkDomainV2);
            sb.Append($"/{BloctoAppIdentifier}");
            sb.Append($"/aptos/authz");
            sb.Append($"/{transactionPreResponse.AuthorizationId}");
            
            var webSb = sb.ToString();
            $"Url: {webSb}".ToLog();
            StartCoroutine(OpenUrl(webSb));
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
            if (!RequestIdActionMapper.ContainsKey(item.RequestId))
            {
                return;
            }

            var action = RequestIdActionMapper[item.RequestId];
            switch (action)
            {
                case "CONNECTWALLET":
                    var result = UniversalLinkHandler(item.RemainContent, "address=")  ;
                    SessionId = UniversalLinkHandler(item.RemainContent, "session_id=");
                        
                    $"Address: {result}, SessionId: {SessionId}".ToLog();
                    ConnectWalletCallback.Invoke(result);
                    break;
                    
                case "SIGNMESSAGE":
                    var signatureResult = UniversalLinkHandler(item.RemainContent, "result=");
                    if(signatureResult == "ok")
                    {
                        _webRequestUtility.Headers = new Dictionary<string, string>
                                                     {
                                                         {"Blocto-Session-Identifier", SessionId},
                                                     };
                            
                        var requestUrl = $"{WebSdkDomainV2}/api/aptos/user-signature/{SignatureId}";
                        var signMessageResponse = _webRequestUtility.GetResponse<SignMessageResponse>(requestUrl, HttpMethod.Get, "application/json");
                        _signMessageCallback.Invoke(signMessageResponse);
                    }
                        
                    break;
                case "SENDTRANSACTION":
                    var sendTransaction = UniversalLinkHandler(item.RemainContent, "tx_hash");
                    SendTransactionCallback.Invoke(sendTransaction);
                    break;
            }
        }
    }
}