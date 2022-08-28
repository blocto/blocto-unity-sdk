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
using UnityEngine;
using UnityEngine.Networking;

namespace Blocto.Flow
{
    public class BloctoWalletProvider : MonoBehaviour, IWalletProvider
    {
        public PollingResponse PollingResponse { get; set; }
        
        public AuthzResponse AuthzResponse { get; set; }
        
        public PreAuthzResponse PreAuthzResponse { get; set; }
        
        [DllImport ("__Internal")]
        private static extern void OpenUrl(string goName, string callFnName, string webUrl, string appUrl);
        
        [DllImport ("__Internal")]
        private static extern void CloseWindow();
        
        private WebRequestUtility _webRequestUtility;
        
        private AuthnResponse _authnResponse;
        
        private GameObject _gameObject;
        
        
        public static BloctoWalletProvider CreateBloctoWalletProvider(GameObject gameObject, string serverUrl)
        {
            var provider = gameObject.AddComponent<BloctoWalletProvider>();
            provider._webRequestUtility = gameObject.AddComponent<WebRequestUtility>();
            provider.PollingResponse = default(PollingResponse);
            
            return provider;
        }
        
        public void Login(string authnUrl, Uri pollingUri, Action internalCallback, Action callback = null)
        {
            Debug.Log($"Authn Url: {authnUrl}, Polling Url:{pollingUri}");
            
            StartCoroutine(OpenUrl(authnUrl));
            StartCoroutine(GetService<PollingResponse>(pollingUri, internalCallback, callback));
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
        
        public void Payer(string url, Action internalCallback)
        {
            
        }
        
        public void SignMessage(string iframeUrl, string pollingUrl, Action internalCallback = null)
        {
            StartCoroutine(OpenUrl(iframeUrl));
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
                        response = _webRequestUtility.ProcessWebRequest<TResponse>(webRequest);
                        PollingResponse = response as PollingResponse;
                        break;
                    case "/api/flow/authz":
                         response = _webRequestUtility.ProcessWebRequest<TResponse>(webRequest);
                         AuthzResponse = response as AuthzResponse;
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
            BloctoWalletProvider.CloseWindow();

            internalCallback.Invoke();
            Debug.Log($"Complete internal callback");
            
            callback?.Invoke();
        }
        
        private IEnumerator OpenUrl(string url)
        {
            Debug.Log($"In open url: {url}");
            try
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    var tmpUrl = url.Replace("https://wallet-testnet.blocto.app/", "https://staging.blocto.app/");
                    // _pluginInstance.Call("openSDK", "com.portto.blocto.staging", url, tmpUrl, new AndroidCallback(), "MainController", "DeeplinkHandler");
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
        
        
        
        private IEnumerator Wait(float time)
        {
            Debug.Log($"In Wait.");
            yield return new WaitForSecondsRealtime(time);
        }
    }
}