using System;
using System.Collections;
using System.Runtime.InteropServices;
using Blocto.Sdk.Core.Utility;
using Flow.FCL.Models;
using Flow.FCL.Models.Authn;
using Flow.FCL.Models.Authz;
using Flow.FCL.WalletProvider;
using Flow.Net.Sdk.Core.Models;
using Flow.Net.SDK.Extensions;
using Newtonsoft.Json;
using Plugins.Blocto.Sdk.Core.Model;
using UnityEngine;
using UnityEngine.Networking;

namespace Blocto.Flow
{
    public class BloctoWalletProvider : MonoBehaviour, IWalletProvider
    {
        public PollingResponse PollingResponse { get; set; }
        
        public AuthzResponse AuthzResponse { get; set; }
        
        public PreAuthzResponse PreAuthzResponse { get; set; }

        public SignMessageResponse SignMessageResponse { get; set; }
        
        [DllImport ("__Internal")]
        private static extern void OpenUrl(string goName, string callFnName, string webUrl, string appUrl);
        
        [DllImport ("__Internal")]
        private static extern void CloseWindow();
        
        private AndroidJavaObject _unityActivity = null;
    
        private AndroidJavaObject _pluginInstance = null;
        
        private WebRequestUtility _webRequestUtility;
        
        private InitResponse _initResponse;
        
        private GameObject _gameObject;
        
        
        public static BloctoWalletProvider CreateBloctoWalletProvider(GameObject gameObject, string serverUrl)
        {
            var provider = gameObject.AddComponent<BloctoWalletProvider>();
            provider._webRequestUtility = gameObject.AddComponent<WebRequestUtility>();
            provider.PollingResponse = default(PollingResponse);
            
            return provider;
        }
        
        public void Login(string authnUrl, Uri pollingUri, Action internalCallback)
        {
            StartCoroutine(OpenUrl(authnUrl));
            StartCoroutine(GetService<PollingResponse>(pollingUri, internalCallback));
        }
        
        public void Authz(string iframeUrl, Uri updateUri, Action internalCallback, Action callback = null)
        {
            StartCoroutine(OpenUrl(iframeUrl));
            StartCoroutine(GetService<AuthzResponse>(updateUri, internalCallback, callback));
        }
        
        public string SendTransaction(FlowTransaction transaction, Action internalCallback)
        {
            return "";
        }
        
        public void SignMessage(string iframeUrl, Uri pollingUrl, Action internalCallback, Action callback = null)
        {
            StartCoroutine(OpenUrl(iframeUrl));
            StartCoroutine(GetService<SignMessageResponse>(pollingUrl, internalCallback, callback));
        }
        
        private IEnumerator GetService<TResponse>(Uri pollingUri, Action internalCallback, Action callback = null) where TResponse : IResponse
        {
            var response = default(TResponse);
            var isApprove = false;
            var pollingUrl = pollingUri.AbsolutePath.Split("%3F")[0];
            Debug.Log($"PollingUri url: {pollingUri.AbsoluteUri}, pollingUrl: {pollingUrl}");
            while (!isApprove)
            {
                var webRequest = _webRequestUtility.CreateUnityWebRequest(pollingUri.AbsoluteUri, "GET", "application/json", new DownloadHandlerBuffer());
                switch (pollingUrl)
                {
                    case "/api/flow/authn":
                        // var mockUrl = "https://run.mocky.io/v3/43a9707f-d396-41cb-9372-2eed1a23bf63";
                        // webRequest = _webRequestUtility.CreateUnityWebRequest(mockUrl, "GET", "application/json", new DownloadHandlerBuffer());
                        response = _webRequestUtility.ProcessWebRequest<TResponse>(webRequest);
                        PollingResponse = response as PollingResponse;
                        break;
                    case "/api/flow/authz":
                         response = _webRequestUtility.ProcessWebRequest<TResponse>(webRequest);
                         AuthzResponse = response as AuthzResponse;
                        break;
                    case "/api/flow/user-signature":
                        response = _webRequestUtility.ProcessWebRequest<TResponse>(webRequest);
                        SignMessageResponse = response as SignMessageResponse;
                        $"Response: {JsonConvert.SerializeObject(response)}".ToLog();
                        break;
                    default:
                        Debug.Log("Get service url path not match.");
                        break;
                }
                
                isApprove = response!.Status == PollingStatusEnum.APPROVED ? true : false;
                Debug.Log($"Response status: {response.Status}");
                yield return new WaitForSeconds(1f);
            }

            if (response.Status == PollingStatusEnum.PENDING)
            {
                yield break;
            }

            var jsonStr = JsonConvert.SerializeObject(response);
            jsonStr.ToLog();
            
            #if UNITY_IOS && !UNITY_EDITOR
            BloctoWalletProvider.CloseWindow();
            #endif

            internalCallback.Invoke();
        }
        
        private IEnumerator OpenUrl(string url)
        {
            Debug.Log($"In open url: {url}");
            try
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    Debug.Log($"Url: {url}, AppSDK url: {url}");
                    _pluginInstance.Call("openSDK", "com.portto.blocto.staging", url, url, new AndroidCallback(), "MainController", "DeeplinkHandler");
                }
                
                
                #if UNITY_IOS && !UNITY_EDITOR
                // var appSdkUrl = url.Replace("https://wallet-testnet.blocto.app/", "https://staging.blocto.app/");
                var appSdkUrl = "";
                Debug.Log($"Url: {url}, AppSDK url: {url}");
                OpenUrl("MainController", "DeeplinkHandler", url, url);
                #endif
            }
            catch (Exception e)
            {
                Debug.Log($"Ex: {e.Message}");
            } 
            
            yield return new WaitForSeconds(0.2f);
        }
        
        private void InitializePlugins(string pluginName)
        {
            try
            {
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