using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Blocto.Sdk.Core.Model;
using Blocto.Sdk.Core.Utility;
using Flow.FCL;
using Flow.FCL.Config;
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
using Newtonsoft.Json.Linq;
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
        
        [DllImport ("__Internal")]
        private static extern bool IsInstalled(string appUrl);
        
        [DllImport("__Internal")]
        private static extern string UniversalLink_GetURL();

        [DllImport("__Internal")]
        private static extern string UniversalLink_Reset();
        
        /// <summary>
        /// Android instance
        /// </summary>
        private AndroidJavaObject _pluginInstance = null;
        
        private IResolveUtility _resolveUtility;
        
        private IFlowClient _flowClient;
        
        private bool _isCancelRequest;
        
        private Guid _bloctoAppIdentifier;
        
        private string _appDomain = "https://staging.blocto.app/sdk?";
        
        private bool _isInstalledApp = false;
        
        private string _universalLink = "default";
        
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
                                                             provider._resolveUtility = resolveUtility;
                                                             provider._flowClient = flowClient;
                                                             return provider;
                                                         });
            
            bloctoWalletProvider.gameObject.name = "bloctowalletprovider";
            bloctoWalletProvider._isCancelRequest = false;
            bloctoWalletProvider._bloctoAppIdentifier = bloctoAppIdentifier;
            bloctoWalletProvider._isInstalledApp = bloctoWalletProvider.IsInstalledApp();
            
            if(Application.platform == RuntimePlatform.Android)
            {
                bloctoWalletProvider.InitializePlugins("com.blocto.unity.PluginActivity");
            }
            
            return bloctoWalletProvider;
        }
        
        public string UniversalLinkHandler()
        {
            $"Universal Link Handler".ToLog();
            if(Application.platform == RuntimePlatform.Android)
            {
                
            }else if(Application.platform == RuntimePlatform.IPhonePlayer)
            {
                _universalLink = BloctoWalletProvider.UniversalLink_GetURL();
                if(_universalLink != "")
                {
                    $"Universal link: {_universalLink}".ToLog();
                    BloctoWalletProvider.UniversalLink_Reset();
                }
            }
            
            return _universalLink;
        }

        public bool IsInstalledApp()
        {
            var isInstallApp = false;
            var testDomain = "blocto://open";
            if(FlowClientLibrary.Config.Get("flow.network", "testnet") == "testnet")
            {
                testDomain = $"blocto-staging://open";
            }
            
            if(Application.platform == RuntimePlatform.Android)
            {
                
            }else if(Application.platform == RuntimePlatform.IPhonePlayer)
            {
                $"App domain: {_appDomain}".ToLog();
                
                isInstallApp = IsInstalled(testDomain);
                $"Is installed App: {isInstallApp}".ToLog();
            }
            
            return isInstallApp;
        }

        /// <summary>
        /// User connect wallet get account
        /// </summary>
        /// <param name="url">fcl authn url</param>
        /// <param name="parameters">parameter of authn</param>
        /// <param name="internalCallback">After, get endpoint response internal callback.</param>
        public void Authenticate(string url, Dictionary<string, object> parameters, Action<object> internalCallback = null)
        {
            $"isInstalledApp: {_isInstalledApp}".ToLog();
            if(_isInstalledApp)
            {
                var sb = new StringBuilder(_appDomain);
                sb.Append($"app_id={_bloctoAppIdentifier}" + "&")
                  .Append($"request_id={Guid.NewGuid()}" + "&")
                  .Append("blockchain=flow" + "&")
                  .Append("method=authn" + "&");
                    
                if(parameters.ContainsKey("accountProofIdentifier") && parameters.ContainsKey("accountProofNonce"))
                {
                    sb.Append($"flow_app_id={parameters["accountProofIdentifier"]}" + "&")
                      .Append($"flow_nonce={parameters["accountProofNonce"]}");
                }
                
                $"Url: {sb.ToString()}".ToLog();
                StartCoroutine(OpenUrl(sb.ToString()));
            }
            else
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
        }
        
        public virtual void SendTransaction(string preAuthzUrl, FlowTransaction tx, Action internalCallback, Action<string> callback = null)
        {
            var preSignableJObj = _resolveUtility.ResolvePreSignable(ref tx);
            var preAuthzResponse = WebRequestUtility.GetResponse<PreAuthzAdapterResponse>(preAuthzUrl, "POST", "application/json", preSignableJObj);
            var isNonCustodial = preAuthzResponse.AuthorizerData.Authorizations.Any(p => p.Endpoint.ToLower().Contains("cosigner") || p.Endpoint.ToLower().Contains("non-custodial"));
            var tmpAccount = GetAccount(preAuthzResponse.AuthorizerData.Proposer.Identity.Address).ConfigureAwait(false).GetAwaiter().GetResult();
            tx.ProposalKey = GetProposerKey(tmpAccount, preAuthzResponse.AuthorizerData.Proposer.Identity.KeyId);
            
            var signableJObj = default(JObject);
            var endpoint = default((string IframeUrl, Uri PollingUrl));
            if(isNonCustodial)
            {
                var authorization = preAuthzResponse.AuthorizerData.Authorizations.First();
                var authorize = authorization.ConvertToFlowAccount();
                var signableJObjs = _resolveUtility.ResolveSignable(ref tx, preAuthzResponse.AuthorizerData, authorize);
                tx.PayloadSignatures.Clear();
            
                for (var index = 0; index < preAuthzResponse.AuthorizerData.Authorizations.Count; index++)
                {
                    signableJObj = signableJObjs[index];
                    var postUrl = preAuthzResponse.AuthorizerData.Authorizations[index].AuthzAdapterEndpoint();
                    var path = postUrl.Split("?").First().Split("/").Last();
                    switch (path)
                    {
                        case "cosigner":
                            var cosigner = WebRequestUtility.GetResponse<SignatureResponse>(postUrl, "POST", "application/json", signableJObj);
                            tx.PayloadSignatures.Add(new FlowSignature
                                                     {
                                                         Address = new FlowAddress(cosigner.SignatureInfo().Address.ToString()),
                                                         Signature = cosigner.SignatureInfo().Signature.ToString().StringToBytes().ToArray(),
                                                         KeyId = Convert.ToUInt32(cosigner.SignatureInfo().KeyId) 
                                                     });
                            break;
                        case "non-custodial":
                            var authzResponse = WebRequestUtility.GetResponse<NonCustodialAuthzResponse>(postUrl, "POST", "application/json", signableJObj);
                            endpoint = authzResponse.AuthzEndpoint();
                            break;
                    }
                }
                
                Authz<SignatureResponse>(endpoint.IframeUrl, endpoint.PollingUrl, response => {
                                                                                      var signInfo = response.SignatureInfo();
                                                                                      if (signInfo.Signature != null)
                                                                                      {
                                                                                          $"Signature info keyId: {signInfo.KeyId}".ToLog();
                                                                                          tx.PayloadSignatures.Add(new FlowSignature
                                                                                                                   {
                                                                                                                       Address = new FlowAddress(signInfo.Address.ToString()),
                                                                                                                       Signature = signInfo.Signature.ToString().StringToBytes().ToArray(),
                                                                                                                       KeyId = Convert.ToUInt32(signInfo.KeyId)
                                                                                                                   });
                                                                                      }
 
                                                                                      var payerEndpoint = preAuthzResponse.PayerEndpoint();
                                                                                      var payerSignable = _resolveUtility.ResolvePayerSignable(ref tx, signableJObj);
                                                                                      var payerSignResponse = WebRequestUtility.GetResponse<SignatureResponse>(payerEndpoint.AbsoluteUri, "POST", "application/json", payerSignable);
                                                                                      signInfo = payerSignResponse.SignatureInfo();
                                                                                      if (signInfo.Signature != null && signInfo.Address != null)
                                                                                      {
                                                                                          var envelopeSignature = tx.EnvelopeSignatures.First(p => p.Address.Address == signInfo.Address.ToString().RemoveHexPrefix());
                                                                                          envelopeSignature.Signature = signInfo.Signature?.ToString().StringToBytes().ToArray();
                                                                                      }
                                                                                      
                                                                                      var txResponse = _flowClient.SendTransactionAsync(tx).ConfigureAwait(false).GetAwaiter().GetResult();
                                                                                      $"TxId: {txResponse.Id}".ToLog();
                                                                                      callback?.Invoke(txResponse.Id);
                                                                                  });  
            }
            else
            {
                var authorization = preAuthzResponse.AuthorizerData.Authorizations.First();
                var postUrl = authorization.AuthzAdapterEndpoint();
                var authorize = authorization.ConvertToFlowAccount();
                signableJObj = _resolveUtility.ResolveSignable(ref tx, preAuthzResponse.AuthorizerData, authorize).First();
                var authzResponse = WebRequestUtility.GetResponse<AuthzAdapterResponse>(postUrl, "POST", "application/json", signableJObj);
                endpoint = authzResponse.AuthzEndpoint();
                
                Authz<AuthzAdapterResponse>(endpoint.IframeUrl, endpoint.PollingUrl, item => {
                                                                                         var response = item as AuthzAdapterResponse;
                                                                                         var signInfo = response.SignatureInfo();
                                                                                         if (signInfo.Signature != null)
                                                                                         {
                                                                                             var payloadSignature = tx.PayloadSignatures.First(p => p.Address.Address == signInfo.Address?.ToString().RemoveHexPrefix());
                                                                                             payloadSignature.Signature = signInfo.Signature?.ToString().StringToBytes().ToArray();
                                                                                         }

                                                                                         var payerEndpoint = preAuthzResponse.PayerEndpoint();
                                                                                         var payerSignable = _resolveUtility.ResolvePayerSignable(ref tx, signableJObj);
                                                                                         var payerSignResponse = WebRequestUtility.GetResponse<SignatureResponse>(payerEndpoint.AbsoluteUri, "POST", "application/json", payerSignable);
                                                                                         signInfo = payerSignResponse.SignatureInfo();
                                                                                         if (signInfo.Signature != null && signInfo.Address != null)
                                                                                         {
                                                                                             var envelopeSignature = tx.EnvelopeSignatures.First(p => p.Address.Address == signInfo.Address.ToString().RemoveHexPrefix());
                                                                                             envelopeSignature.Signature = signInfo.Signature?.ToString().StringToBytes().ToArray();
                                                                                         }

                                                                                         var txResponse = _flowClient.SendTransactionAsync(tx).ConfigureAwait(false).GetAwaiter().GetResult();
                                                                                         $"TxId: {txResponse.Id}".ToLog();
                                                                                         callback?.Invoke(txResponse.Id);
                                                                                     }); 
            }
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
                    $"Open url: {url}".ToLog();
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
        /// Get flow account
        /// </summary>
        /// <param name="address">address of account</param>
        /// <returns></returns>
        private async Task<FlowAccount> GetAccount(string address)
        {
            var account = _flowClient.GetAccountAtLatestBlockAsync(address);
            return await account;
        }
        
        /// <summary>
        /// Get full account key information
        /// </summary>
        /// <param name="account">flow account</param>
        /// <param name="keyId">key id of account</param>
        /// <returns></returns>
        private FlowProposalKey GetProposerKey(FlowAccount account, uint keyId)
        {
            var proposalKey = account.Keys.First(p => p.Index == keyId);
            return new FlowProposalKey
                   {
                       Address = account.Address,
                       KeyId = keyId,
                       SequenceNumber = proposalKey.SequenceNumber
                   };
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