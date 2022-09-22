using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Blocto.Sdk.Core.Model;
using Blocto.Sdk.Core.Utility;
using Blocto.Sdk.Flow.Utility;
using Flow.FCL.Extensions;
using Flow.FCL.Models;
using Flow.FCL.Models.Authz;
using Flow.FCL.WalletProvider;
using Flow.Net.Sdk.Core;
using Flow.Net.Sdk.Core.Client;
using Flow.Net.Sdk.Core.Models;
using Flow.Net.SDK.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Blocto.SDK.Flow
{
    public class BloctoWalletProvider : MonoBehaviour, IBloctoWalletProvider, IWalletProvider
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
        
        private ResolveUtility _resolveUtility;
        
        private IFlowClient _flowClient;
        
        private bool _isCancelRequest;
        
        private Guid _bloctoAppIdentifier;
        
        private Dictionary<bool, Func<FlowTransaction, PreAuthzAdapterResponse, Action<string>, FlowTransaction>> _keyModeProcessMapper;
        
        private Dictionary<string, Action<string, JObject, FlowTransaction, (string IframeUrl, Uri PollingUrl)>> _noncustodialProcessMapper;

        /// <summary>
        /// Create blocto wallet provider instance
        /// </summary>
        /// <param name="gameObject">Main gameobject</param>
        /// <param name="initialFun">Blocto initial func</param>
        /// <param name="bloctoAppIdentifier">Blocto sdk appId</param>
        /// <returns>BloctoWalletProvider</returns>
        public static BloctoWalletProvider CreateBloctoWalletProvider(Func<Func<GameObject, IFlowClient, ResolveUtility, BloctoWalletProvider>, BloctoWalletProvider> initialFun, Guid bloctoAppIdentifier)
        {
            var bloctoWalletProvider = initialFun.Invoke((gameObject, flowClient, resolveUtility) => {
                                                             var provider = gameObject.AddComponent<BloctoWalletProvider>();
                                                             provider._webRequestUtility = gameObject.AddComponent<WebRequestUtility>();
                                                             provider._bloctoAppIdentifier = bloctoAppIdentifier;
                                                             provider._resolveUtility = resolveUtility;
                                                             provider._flowClient = flowClient;
                                                             return provider;
                                                         });
            
            bloctoWalletProvider.gameObject.name = "bloctowalletprovider";
            bloctoWalletProvider._isCancelRequest = false;
            bloctoWalletProvider._webRequestUtility.BloctoAppId = bloctoAppIdentifier.ToString();
            
            if(Application.platform == RuntimePlatform.Android)
            {
                bloctoWalletProvider.InitializePlugins("com.blocto.unity.PluginActivity");
            }
            
            return bloctoWalletProvider;
        }

        public void Start()
        {
            _keyModeProcessMapper = new Dictionary<bool, Func<FlowTransaction, PreAuthzAdapterResponse, Action<string>, FlowTransaction>>
                                  {
                                      {true, NonCustodialProcess},
                                      {false, CustodialProcess}
                                  };
            
            _noncustodialProcessMapper = new Dictionary<string, Action<string, JObject, FlowTransaction, (string IframeUrl, Uri PollingUrl)>>
                                         {
                                             {"cosigner", Cosigner},
                                             {"non-custodial", Noncustodual}
                                         };
            
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
        public void Authz<TResponse>(string iframeUrl, Uri pollingUri, Action<TResponse> internalCallback) where TResponse : IResponse
        {
            StartCoroutine(OpenUrl(iframeUrl));
            StartCoroutine(GetService<TResponse>(pollingUri, internalCallback));
        }
        
        public virtual FlowTransaction SendTransaction(string preAuthzUrl, FlowTransaction tx, Action internalCallback, Action<string> callback = null)
        {
            $"PreAuth url: {preAuthzUrl}".ToLog();
            var preSignableJObj = _resolveUtility.ResolvePreSignable(ref tx);
            
            $"PreSignable: {JsonConvert.SerializeObject(preSignableJObj)}".ToLog();
            var preAuthzResponse = _webRequestUtility.GetResponse<PreAuthzAdapterResponse>(preAuthzUrl, "POST", "application/json", preSignableJObj);
            
            $"PreAuhtzResponse: {JsonConvert.SerializeObject(preAuthzResponse)}".ToLog();
            var isNonCustodial = preAuthzResponse.AuthorizerData.Authorizations.Any(p => p.Endpoint.ToLower().Contains("cosigner") || p.Endpoint.ToLower().Contains("non-custodial"));
            var tmpAccount = GetAccount(preAuthzResponse.AuthorizerData.Proposer.Identity.Address).ConfigureAwait(false).GetAwaiter().GetResult();
            tx.ProposalKey = GetProposerKey(tmpAccount, preAuthzResponse.AuthorizerData.Proposer.Identity.KeyId);
            
            var signableJObj = default(JObject);
            var endpoint = default((string IframeUrl, Uri PollingUrl));
            var process = _keyModeProcessMapper[isNonCustodial];
            tx = process.Invoke(tx, preAuthzResponse, callback);

            return tx;
        }

        private FlowTransaction NonCustodialProcess(FlowTransaction tx, PreAuthzAdapterResponse preAuthzResponse, Action<string> callback)
        {
            var signableJObj = default(JObject);
            var endpoint = default((string IframeUrl, Uri PollingUrl));
            var authorization = preAuthzResponse.AuthorizerData.Authorizations.First();
            var authorize = authorization.ConvertToFlowAccount();
            var signableJObjs = _resolveUtility.ResolveSignable(ref tx, preAuthzResponse.AuthorizerData, authorize);
            tx.PayloadSignatures.Clear();
            
            for (var index = 0; index < preAuthzResponse.AuthorizerData.Authorizations.Count; index++)
            {
                signableJObj = signableJObjs[index];
                var postUrl = preAuthzResponse.AuthorizerData.Authorizations[index].AuthzAdapterEndpoint();
                var path = postUrl.Split("?").First().Split("/").Last();
                _noncustodialProcessMapper[path].Invoke(postUrl, signableJObj, tx, endpoint);
                $"Tx payload signatures: {JsonConvert.SerializeObject(tx.PayloadSignatures)}".ToLog();
                
                // switch (path)
                // {
                //     case "cosigner":
                //         var cosigner = _webRequestUtility.GetResponse<SignatureResponse>(postUrl, "POST", "application/json", signableJObj);
                //         tx.PayloadSignatures.Add(new FlowSignature
                //                                  {
                //                                      Address = new FlowAddress(cosigner.SignatureInfo().Address.ToString()),
                //                                      Signature = cosigner.SignatureInfo().Signature.ToString().StringToBytes().ToArray(),
                //                                      KeyId = Convert.ToUInt32(cosigner.SignatureInfo().KeyId) 
                //                                  });
                //         break;
                //     case "non-custodial":
                //         var authzResponse = _webRequestUtility.GetResponse<NonCustodialAuthzResponse>(postUrl, "POST", "application/json", signableJObj);
                //         endpoint = authzResponse.AuthzEndpoint();
                //         break;
                // }
            }
            
            Authz<SignatureResponse>(endpoint.IframeUrl, endpoint.PollingUrl, response => {
                                                                                  var signInfo = response.SignatureInfo();
                                                                                  if (signInfo.Signature != null)
                                                                                  {
                                                                                      $"Signature info keyId: {signInfo.KeyId}".ToLog();
                                                                                      tx.PayloadSignatures.Add(new FlowSignature
                                                                                                               {
                                                                                                                   //// wait frontend fix bug
                                                                                                                   Address = new FlowAddress(authorization.Identity.Address),
                                                                                                                   Signature = signInfo.Signature.ToString().StringToBytes().ToArray(),
                                                                                                                   KeyId = Convert.ToUInt32(signInfo.KeyId)
                                                                                                               });
                                                                                  }
 
                                                                                  var payerEndpoint = preAuthzResponse.PayerEndpoint();
                                                                                  var payerSignable = _resolveUtility.ResolvePayerSignable(ref tx, signableJObj);
                                                                                  var payerSignResponse = _webRequestUtility.GetResponse<SignatureResponse>(payerEndpoint.AbsoluteUri, "POST", "application/json", payerSignable);
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

            return tx;
        }
        
        private FlowTransaction CustodialProcess(FlowTransaction tx, PreAuthzAdapterResponse preAuthzResponse, Action<string> callback)
        {
            var authorization = preAuthzResponse.AuthorizerData.Authorizations.First();
            var postUrl = authorization.AuthzAdapterEndpoint();
            var authorize = authorization.ConvertToFlowAccount();
            var signableJObj = _resolveUtility.ResolveSignable(ref tx, preAuthzResponse.AuthorizerData, authorize).First();
            var authzResponse = _webRequestUtility.GetResponse<AuthzAdapterResponse>(postUrl, "POST", "application/json", signableJObj);
            var endpoint = authzResponse.AuthzEndpoint();
            
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
                                                                                     var payerSignResponse = _webRequestUtility.GetResponse<SignatureResponse>(payerEndpoint.AbsoluteUri, "POST", "application/json", payerSignable);
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
            return tx;
        }
        
        private void Cosigner(string postUrl, JObject signableJObj, FlowTransaction tx, (string IframeUrl, Uri PollingUrl) endpoint)
        {
            var cosigner = _webRequestUtility.GetResponse<SignatureResponse>(postUrl, "POST", "application/json", signableJObj);
            tx.PayloadSignatures.Add(new FlowSignature
                                     {
                                         Address = new FlowAddress(cosigner.SignatureInfo().Address.ToString()),
                                         Signature = cosigner.SignatureInfo().Signature.ToString().StringToBytes().ToArray(),
                                         KeyId = Convert.ToUInt32(cosigner.SignatureInfo().KeyId) 
                                     }); 
        }
        
        private void Noncustodual(string postUrl, JObject signableJObj, FlowTransaction tx, (string IframeUrl, Uri PollingUrl) endpoint)
        {
            var authzResponse = _webRequestUtility.GetResponse<NonCustodialAuthzResponse>(postUrl, "POST", "application/json", signableJObj);
            endpoint = authzResponse.AuthzEndpoint();
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
        
        public async Task<FlowAccount> GetAccount(string address)
        {
            var account = _flowClient.GetAccountAtLatestBlockAsync(address);
            return await account;
        }
        
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