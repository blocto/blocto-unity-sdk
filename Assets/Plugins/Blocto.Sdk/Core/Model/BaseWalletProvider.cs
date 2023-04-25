using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Blocto.Sdk.Core.Extension;
using UnityEngine;

namespace Blocto.Sdk.Core.Model
{
public class BaseWalletProvider : MonoBehaviour
    {
        public bool ForceUseWebView { get; set; }
        
        #if UNITY_IOS
        /// <summary>
        /// iOS swift open ASWebAuthenticationSession method
        /// </summary>
        /// <param name="goName">swift complete event then callback class name of unity</param>
        /// <param name="callFnName">swift complete event then callback method name of unity</param>
        /// <param name="webUrl">open page url</param>
        /// <param name="appUrl">open app url</param>
        [DllImport ("__Internal")]
        protected static extern void OpenUrl(string goName, string callFnName, string webUrl, string appUrl);
        
        /// <summary>
        /// Close ASWebAuthenticationSession
        /// </summary>
        [DllImport ("__Internal")]
        protected static extern void CloseWindow();
        
        /// <summary>
        /// Check specify app installed
        /// </summary>
        /// <param name="appUrl"></param>
        /// <returns></returns>
        [DllImport ("__Internal")]
        protected static extern bool IsInstalled(string appUrl);
        
        /// <summary>
        /// Get universal link data
        /// </summary>
        /// <returns></returns>
        [DllImport("__Internal")]
        protected static extern string UniversalLink_GetURL();

        /// <summary>
        /// Reset universal link on iOS code
        /// </summary>
        /// <returns></returns>
        [DllImport("__Internal")]
        protected static extern string UniversalLink_Reset();
        #endif
        
        /// <summary>
        /// Android instance
        /// </summary>
        protected AndroidJavaObject pluginInstance;
        
        protected bool isCancelRequest;
        
        protected Guid bloctoAppIdentifier;
        
        protected string webSdkDomain = "https://wallet.blocto.app/sdk?";
        
        protected string webSdkDomainV2 = "https://wallet-v2.blocto.app";
        
        protected string appSdkDomain = "https://blocto.app/sdk?";
        
        protected string backedApiDomain = "https://api.blocto.app";
        
        protected string androidPackageName = "com.portto.blocto";
        
        protected string sessionId;
        
        protected string signatureId;
 
        protected bool isInstalledApp = false;
        
        protected Guid requestId;
        
        protected Dictionary<string, string> requestIdActionMapper;
        
        protected Action<string> _connectWalletCallback;
        
        protected Action<string> _sendTransactionCallback;

        public void Awake()
        {
            requestIdActionMapper = new Dictionary<string, string>();
            ForceUseWebView = false;
        }
        
        protected virtual void RequestAccount(Action<string> callback)
        {
            requestId = Guid.NewGuid();
            requestIdActionMapper.Add(requestId.ToString(), "CONNECTWALLET");
            _connectWalletCallback = callback;
        }
        
        protected virtual void SignMessage()
        {
            requestId = Guid.NewGuid();
            requestIdActionMapper.Add(requestId.ToString(), "SIGNMESSAGE");
        }
        
        protected virtual void SendTransaction(Action<string> callback, string action = null)
        {
            requestId = Guid.NewGuid();
            requestIdActionMapper.Add(requestId.ToString(), action ?? "SENDTRANSACTION");
            _sendTransactionCallback = callback;
        }
        
        protected virtual string CreateRequestAccountUrl(bool isInstallApp, string chainName)
        {
            var parameters = new Dictionary<string, string>
                             {
                                 {"blockchain", chainName.ToLower() },
                                 {"method", "request_account" },
                                 {"request_id", requestId.ToString() }, 
                             };
            
            if(isInstalledApp && ForceUseWebView == false)
            {
                var appSb = GenerateUrl(appSdkDomain, parameters);
                
                $"Url: {appSb}".ToLog();
                StartCoroutine(OpenUrl(appSb));
                return appSb;
            }
            
            $"WebSDK domain: {webSdkDomain}".ToLog();
            var webSb = GenerateUrl(webSdkDomain, parameters);
            return webSb;
        }
        
        protected virtual string CreateRequestAccountUrlV2(string chainName, string appId)
        {
            var url = $"{webSdkDomainV2}/{appId}/{chainName}/authn/?request_id={requestId}&request_source=sdk_unity";
            return url;
        }

        /// <summary>
        /// Check specify app installed
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsInstalledApp(EnvEnum env)
        {
            var isInstallApp = false;
            var testDomain = "blocto://open";
            if(env == EnvEnum.Devnet)
            {
                testDomain = $"blocto-dev://open";
            }
            
            $"Test apoDomain: {testDomain}".ToLog();
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    isInstallApp = pluginInstance.Call<bool>("isInstalledApp", androidPackageName); 
                    break;
                case RuntimePlatform.IPhonePlayer:
                    #if UNITY_IOS
                    isInstallApp = BaseWalletProvider.IsInstalled(testDomain);
                    #endif
                    break;
            }
            
            $"Is installed app: {isInstallApp}".ToLog();
            return isInstallApp;
        }

        /// <summary>
        /// Open Web or App SDK
        /// </summary>
        /// <param name="url">Url</param>
        /// <returns></returns>
        protected virtual IEnumerator OpenUrl(string url)
        {
            $"ForcedUseWebView: {ForceUseWebView}, Url: {url}".ToLog();
            try
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                        if(isInstalledApp && ForceUseWebView == false)
                        {
                            $"Call android app, url: {url}".ToLog();
                            pluginInstance.Call("openSDK", androidPackageName, url, url, new AndroidCallback(), "bloctowalletprovider", "UniversalLinkCallbackHandler");
                        }
                        else
                        {
                            $"Call android webview, url: {url}".ToLog();
                            pluginInstance.Call("webview", url, new AndroidCallback(), "bloctowalletprovider", "UniversalLinkCallbackHandler");
                        }
                        
                        break;
                    case RuntimePlatform.IPhonePlayer:
                        #if UNITY_IOS
                        BaseWalletProvider.OpenUrl("bloctowalletprovider", "UniversalLinkCallbackHandler", url, url);
                        #endif
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Ex: {e.Message}");
            } 
            
            yield return new WaitForSeconds(0.5f);
        }
        
        protected virtual (List<string> MatchContent, string RemainContent) CheckContent(string text, string keyword)
        {
            if (!text.ToLower().Contains(keyword))
            {
                return (new List<string>(), text);
            }

            var elements = text.Split("&").ToList();
            var matchElements = elements.Where(p => p.ToLower().Contains(keyword)).ToList();
            foreach (var element in matchElements)
            {
                elements.Remove(element);
            }
                
            return (matchElements, elements.Count > 0 ? string.Join("&", elements) : string.Empty);
        }
        
        protected virtual string UniversalLinkHandler(string link, string keyword)
        {
            var data = (MatchContents: new List<string>(), RemainContent: link);
            data = CheckContent(data.RemainContent, keyword);
            return data.MatchContents.FirstOrDefault().AddressParser().Value;
        }
        
        protected virtual string GenerateUrl(string domain, Dictionary<string, string> parameters)
        {
            var url = new StringBuilder(domain);
            url.Append($"app_id={bloctoAppIdentifier.ToString()}" + "&");
            foreach (var parameter in parameters)
            {
                url.Append($"{parameter.Key}={parameter.Value}" + "&");
            }
            
            url.Append($"platform=sdk_unity");
            return url.ToString();
        }

        protected virtual string UserSignatureApiUrl(string chain)
        {
            return $"{webSdkDomainV2}/api/{chain.ToLower()}/dapp/user-signature";
        }
        
        protected virtual string UserSignatureWebUrl(string chain)
        {
            var sb = new StringBuilder(webSdkDomainV2);
            sb.Append($"/{bloctoAppIdentifier}");
            sb.Append($"/{chain.ToLower()}/user-signature");
            sb.Append($"/{signatureId}");
            return sb.ToString();
        }
        
        protected virtual string AuthzApiUrl(string chain)
        {
            return $"{webSdkDomainV2}/api/{chain.ToLower()}/dapp/authz";
        }
        
        protected virtual StringBuilder AuthzWebUrl(string authorizationId, string chain)
        {
            var sb = new StringBuilder(webSdkDomainV2);
            sb.Append($"/{bloctoAppIdentifier}");
            sb.Append($"/{chain.ToLower()}/authz");
            sb.Append($"/{authorizationId}");
            return sb;
        }
        
        /// <summary>
        /// Initial android instance
        /// </summary>
        /// <param name="pluginName">Android open add full name</param>
        protected void InitializePlugins(string pluginName)
        {
            try
            {
                $"Init android object, plugin name: {pluginName}".ToLog();
                if (pluginInstance == null )
                {
                    pluginInstance = new AndroidJavaObject(pluginName);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                throw;
            }
        }
    }
}