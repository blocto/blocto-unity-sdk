using System;
using System.Collections;
using System.Runtime.InteropServices;
using Blocto.Sdk.Core.Model;
using Blocto.Sdk.Core.Utility;
using Flow.FCL.Models;
using Flow.FCL.Models.Authz;
using Flow.Net.SDK.Extensions;
using UnityEngine;
using UnityEngine.Networking;

namespace Blocto.Flow
{
    public class BloctoWalletProvider : MonoBehaviour, IBloctoWalletProvider
    {
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
        
        private WebRequestUtility _webRequestUtility;
        
        private bool _isCancelRequest;
        
        /// <summary>
        /// Create blocto wallet provider instance
        /// </summary>
        /// <param name="gameObject">Main gameobject</param>
        /// <returns>BloctoWalletProvider</returns>
        public static BloctoWalletProvider CreateBloctoWalletProvider(GameObject gameObject)
        {
            var provider = gameObject.AddComponent<BloctoWalletProvider>();
            provider.gameObject.name = "bloctowalletprovider";
            provider._webRequestUtility = gameObject.AddComponent<WebRequestUtility>();
            provider._isCancelRequest = false;
            
            if(Application.platform == RuntimePlatform.Android)
            {
                provider.InitializePlugins("com.blocto.unity.PluginActivity");
            }
            
            return provider;
        }
        
        /// <summary>
        /// User connect wallet get account
        /// </summary>
        /// <param name="iframeUrl">User approve page</param>
        /// <param name="pollingUri">Service endpoint url</param>
        /// <param name="internalCallback">After, get endpoint response internal callback.</param>
        public void Login(string iframeUrl, Uri pollingUri, Action<object> internalCallback)
        {
            StartCoroutine(OpenUrl(iframeUrl));
            StartCoroutine(GetService<AuthenticateResponse>(pollingUri, internalCallback));
        }
        
        /// <summary>
        /// Get authorizer signature
        /// </summary>
        /// <param name="iframeUrl">User approve page</param>
        /// <param name="pollingUri">Service endpoint url</param>
        /// <param name="internalCallback">After, get endpoint response internal callback.</param>
        public void Authz(string iframeUrl, Uri pollingUri, Action<object> internalCallback)
        {
            StartCoroutine(OpenUrl(iframeUrl));
            StartCoroutine(GetService<AuthzAdapterResponse>(pollingUri, internalCallback));
        }
        
        /// <summary>
        /// SignMessage
        /// </summary>
        /// <param name="iframeUrl">User approve page</param>
        /// <param name="pollingUri">Service endpoint url</param>
        /// <param name="internalCallback">After, get endpoint response internal callback.</param>
        public void SignMessage(string iframeUrl, Uri pollingUri, Action<object> internalCallback)
        {
            StartCoroutine(OpenUrl(iframeUrl));
            StartCoroutine(GetService<SignMessageResponse>(pollingUri, internalCallback));
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
                var webRequest = _webRequestUtility.CreateUnityWebRequest(pollingUri.AbsoluteUri, "GET", "application/json", new DownloadHandlerBuffer());
                response = _webRequestUtility.ProcessWebRequest<TResponse>(webRequest);
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