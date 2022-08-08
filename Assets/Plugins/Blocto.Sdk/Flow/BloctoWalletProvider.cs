using System;
using System.Collections;
using System.Runtime.InteropServices;
using Blocto.Sdk.Core.Utility;
using Flow.FCL.Models;
using Flow.FCL.WalletProvider;
using Flow.Net.SDK.Extensions;
using Newtonsoft.Json;
using Plugins.Flow.FCL.Models;
using UnityEngine;
using UnityEngine.Networking;

namespace Blocto.Flow
{
    public class BloctoWalletProvider : MonoBehaviour, IWalletProvider
    {
        public PollingResponse PollingResponse { get; set; }
        
        [DllImport ("__Internal")]
        private static extern void OpenUrl(string goName, string callFnName, string webUrl, string appUrl);
        
        [DllImport ("__Internal")]
        private static extern void CloseWindow();
        
        private WebRequestHelper _webRequestHelper;
        
        private AuthnResponse _authnResponse;
        
        private GameObject _gameObject;

        public void Init(GameObject gameObject)
        {
            _webRequestHelper = gameObject.AddComponent<WebRequestHelper>();
            PollingResponse = default(PollingResponse);
        }
        
        public void Login(string authnUrl, string pollingUrl, Action internalCallback, Action callback = null)
        {
            StartCoroutine(OpenUrl(authnUrl));
            StartCoroutine(GetService(pollingUrl, internalCallback, callback));
        }
        
        public void SignMessage(string iframeUrl, string pollingUrl, Action internalCallback = null)
        {
            StartCoroutine(OpenUrl(iframeUrl));
        }
        
        
        private IEnumerator GetService(string pollingUrl, Action internalCallback, Action callback = null)
        {
            var isApprove = false;
            while (!isApprove)
            {
                var webRequest = _webRequestHelper.CreateUnityWebRequest(pollingUrl, "GET", "application/json", new DownloadHandlerBuffer());
                PollingResponse = _webRequestHelper.ProcessWebRequest<PollingResponse>(webRequest);
                isApprove = PollingResponse.Status == PollingStatusEnum.APPROVED ? true : false;
                
                Debug.Log($"Polling response status: {PollingResponse.Status}");
                yield return new WaitForSeconds(1f);
            }

            if (PollingResponse.Status == PollingStatusEnum.PENDING)
            {
                yield break;
            }

            var jsonStr = JsonConvert.SerializeObject(PollingResponse);
            jsonStr.ToLog();
            BloctoWalletProvider.CloseWindow();

            internalCallback.Invoke();
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