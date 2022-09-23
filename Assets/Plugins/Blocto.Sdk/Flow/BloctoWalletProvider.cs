using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Blocto.Sdk.Core.Model;
using Blocto.Sdk.Core.Utility;
using Flow.FCL.Extensions;
using Flow.FCL.Models;
using Flow.FCL.Models.Authn;
using Flow.FCL.Models.Authz;
using Flow.FCL.Utility;
using Flow.FCL.WalletProvider;
using Flow.Net.Sdk.Core;
using Flow.Net.Sdk.Core.Client;
using Flow.Net.Sdk.Core.Models;
using Flow.Net.SDK.Extensions;
using UnityEngine;
using UnityEngine.Networking;

namespace Blocto.SDK.Flow
{
    public class BloctoWalletProvider : MonoBehaviour, IBloctoWalletProvider, IWalletProvider
    {
        /// <summary>
        /// System Thumbnail
        /// </summary>
        public static string Thumbnail;
        
        /// <summary>
        /// System Title
        /// </summary>
        public static string Title;

        /// <summary>
        /// HTTP utility
        /// </summary>
        public WebRequestUtility WebRequestUtility { get; set; }
        
        /// <summary>
        /// iOS swift open ASWebAuthenticationSession method
        /// </summary>
        /// <param name="goName">swift complete event then callback class name of unity</param>
        /// <param name="callFnName">swift complete event then callback method name of unity</param>
        /// <param name="webUrl">open page url</param>
        /// <param name="appUrl">open app url</param>
        [DllImport ("__Internal")]
        private static extern void OpenUrl(string goName, string callFnName, string webUrl, string appUrl);
        
        /// <summary>
        /// Close ASWebAuthenticationSession
        /// </summary>
        [DllImport ("__Internal")]
        private static extern void CloseWindow();
        
        /// <summary>
        /// Android instance
        /// </summary>
        private AndroidJavaObject _pluginInstance = null;
        
        private IResolveUtility _resolveUtility;
        
        private IFlowClient _flowClient;
        
        private bool _isCancelRequest;
        
        private Guid _bloctoAppIdentifier;
        
        /// <summary>
        /// Create blocto wallet provider instance
        /// </summary>
        /// <param name="gameObject">Main gameobject</param>
        /// <param name="initialFun">Blocto initial func</param>
        /// <param name="bloctoAppIdentifier">Blocto sdk appId</param>
        /// <returns>BloctoWalletProvider</returns>
        public static BloctoWalletProvider CreateBloctoWalletProvider(Func<Func<GameObject, IFlowClient, IResolveUtility, BloctoWalletProvider>, BloctoWalletProvider> initialFun, Guid bloctoAppIdentifier)
        {
            var bloctoWalletProvider = initialFun.Invoke((gameObject, flowClient, resolveUtility) => {
                                                             var provider = gameObject.AddComponent<BloctoWalletProvider>();
                                                             provider.WebRequestUtility = gameObject.AddComponent<WebRequestUtility>();
                                                             provider._bloctoAppIdentifier = bloctoAppIdentifier;
                                                             provider._resolveUtility = resolveUtility;
                                                             provider._flowClient = flowClient;
                                                             return provider;
                                                         });
            
            bloctoWalletProvider.gameObject.name = "bloctowalletprovider";
            bloctoWalletProvider._isCancelRequest = false;
            bloctoWalletProvider._bloctoAppIdentifier = bloctoAppIdentifier;
            
            if(Application.platform == RuntimePlatform.Android)
            {
                bloctoWalletProvider.InitializePlugins("com.blocto.unity.PluginActivity");
            }
            
            return bloctoWalletProvider;
        }

        /// <summary>
        /// User connect wallet get account
        /// </summary>
        /// <param name="url">fcl authn url</param>
        /// <param name="parameters">parameter of authn</param>
        /// <param name="internalCallback">After, get endpoint response internal callback.</param>
        public void Authenticate(string url, Dictionary<string, object> parameters, Action<object> internalCallback = null)
        {
            var authnResponse = WebRequestUtility.GetResponse<AuthnAdapterResponse>(url, "POST", "application/json", parameters);
            var endpoint = authnResponse.AuthnEndpoint();
            var element = endpoint.IframeUrl.Split("?")[1].Split("&").ToList();
            var thumbnailElement = element.FirstOrDefault(p => p.ToLower().Contains("thumbnail"));
            var titleElement = element.FirstOrDefault(p => p.ToLower().Contains("title"));
            if(thumbnailElement != null)
            {
                BloctoWalletProvider.Thumbnail = thumbnailElement.Split("=")[1];
            }
            
            if(titleElement != null)
            {
                BloctoWalletProvider.Title = titleElement.Split("=")[1];
            }

            StartCoroutine(OpenUrl(endpoint.IframeUrl));
            StartCoroutine(GetService<AuthenticateResponse>(endpoint.PollingUrl, internalCallback));
        }
        
        /// <summary>
        /// Get authorizer signature
        /// </summary>
        /// <param name="iframeUrl">User approve page</param>
        /// <param name="pollingUri">Service endpoint url</param>
        /// <param name="internalCallback">After, get endpoint response internal callback.</param>
        public void Authz<TResponse>(string iframeUrl, Uri pollingUri, Action<TResponse> internalCallback) where TResponse : IResponse
        {
            StartCoroutine(OpenUrl(iframeUrl));
            StartCoroutine(GetService<TResponse>(pollingUri, internalCallback));
        }
        
        /// <summary>
        /// SignMessage
        /// </summary>
        /// <param name="message">Original message </param>
        /// <param name="signService">FCL signature service</param>
        /// <param name="callback">After, get endpoint response callback.</param>
        public void SignMessage(string message, FclService signService, Action<ExecuteResult<FlowSignature>> callback = null)
        {
            var signUrl = signService.SignMessageAdapterEndpoint();
            
            var hexMessage = message.StringToHex();
            var payload = _resolveUtility.ResolveSignMessage(hexMessage, signService.PollingParams.SessionId());
            var response = WebRequestUtility.GetResponse<AuthnAdapterResponse>(signUrl, "POST", "application/json", payload);
            var endpoint = response.SignMessageEndpoint();
            
            var sb = new StringBuilder(endpoint.IframeUrl);
            sb.Append("&")
              .Append(Uri.EscapeDataString("thumbnail") + "=")
              .Append(BloctoWalletProvider.Thumbnail + "&")
              .Append(Uri.EscapeDataString("title") + "=")
              .Append(BloctoWalletProvider.Title);
            var iframeUrl = sb.ToString();

            StartCoroutine(OpenUrl(iframeUrl));
            StartCoroutine(GetService<SignMessageResponse>(endpoint.PollingUrl, response => {
                                                                                     var signature = response?.Data.First().SignatureStr();
                                                                                     var keyId = Convert.ToUInt32(response?.Data.First().KeyId());
                                                                                     var addr = response?.Data.First().Address();
                                                                                     var result = new ExecuteResult<FlowSignature>
                                                                                                  {
                                                                                                      Data = new FlowSignature
                                                                                                             {
                                                                                                                 Address = new FlowAddress(addr),
                                                                                                                 KeyId = keyId,
                                                                                                                 Signature = Encoding.UTF8.GetBytes(signature!)
                                                                                                             },
                                                                                                      IsSuccessed = true,
                                                                                                      Message = string.Empty
                                                                                                  };
                                                                                     
                                                                                     callback?.Invoke(result);
                                                                                 }));
        }
        
        /// <summary>
        /// Get or post https endpoint
        /// </summary>
        /// <param name="pollingUri">Endpoint url</param>
        /// <param name="internalCallback">After, get endpoint response internal callback.</param>
        /// <typeparam name="TResponse">Return type</typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        protected virtual IEnumerator GetService<TResponse>(Uri pollingUri, Action<TResponse> internalCallback) where TResponse : IResponse
        {
            var response = default(TResponse);
            var isApprove = false;
            _isCancelRequest = false;
            while (isApprove == false && _isCancelRequest == false)
            {
                var webRequest = WebRequestUtility.CreateUnityWebRequest(pollingUri.AbsoluteUri, "GET", "application/json", new DownloadHandlerBuffer());
                response = WebRequestUtility.ProcessWebRequest<TResponse>(webRequest);
                isApprove = response!.ResponseStatus is ResponseStatusEnum.APPROVED or ResponseStatusEnum.DECLINED ? true : false;
                yield return new WaitForSeconds(0.2f);
            }

            if (response!.ResponseStatus == ResponseStatusEnum.PENDING || _isCancelRequest)
            {
                yield break;
            }

            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    CloseWebView();
                    break;
                case RuntimePlatform.IPhonePlayer:
                    #if UNITY_IOS && !UNITY_EDITOR
                    BloctoWalletProvider.CloseWindow();
                    #endif
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Platform", "Platform not support");
                    
            }
            
            internalCallback.Invoke(response);
        }
        
        /// <summary>
        /// Open webview
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private IEnumerator OpenUrl(string url)
        {
            try
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    Debug.Log($"Url: {url}, AppSDK url: {url}");
                    _pluginInstance.Call("openSDK", "com.portto.blocto.staging", url, url, new AndroidCallback(), "bloctowalletprovider", "DeeplinkHandler");
                }
                else if(Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    // var appSdkUrl = url.Replace("https://wallet-testnet.blocto.app/", "https://staging.blocto.app/");
                    OpenUrl("bloctowalletprovider", "DeeplinkHandler", url, url);
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Ex: {e.Message}");
            } 
            
            yield return new WaitForSeconds(0.01f);
        }
        
        /// <summary>
        /// For iOS, ASWebAuthenticationSession closed callback.
        /// </summary>
        /// <param name="message">Message from iOS</param>
        private void FailedHandler(string message)
        {
            _isCancelRequest = true;
        }
        
        /// <summary>
        /// For android, close webview
        /// </summary>
        public void CloseWebView()
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                return;
            }

            _isCancelRequest = true;
            _pluginInstance.Call("onBackPressed");
        }
        
        /// <summary>
        /// Initial android instance
        /// </summary>
        /// <param name="pluginName"></param>
        private void InitializePlugins(string pluginName)
        {
            try
            {
                $"Init android plugin, plugin name: {pluginName}".ToLog();
                _pluginInstance = new AndroidJavaObject(pluginName);
                if (_pluginInstance != null)
                {
                    return;
                }

                return;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                throw;
            }
        }
    }
}