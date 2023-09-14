using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Blocto.Sdk.Core.Model;
using Blocto.Sdk.Core.Extension;
using Blocto.Sdk.Core.Utility;
using Blocto.Sdk.Flow.Model;
using Blocto.Sdk.Flow.Utility;
using Flow.FCL;
using Flow.FCL.Extensions;
using Flow.FCL.Models;
using Flow.FCL.Models.Authn;
using Flow.FCL.Models.Authz;
using Flow.FCL.Utility;
using Flow.FCL.WalletProvider;
using Flow.Net.Sdk.Core;
using Flow.Net.Sdk.Core.Client;
using Flow.Net.Sdk.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Object = System.Object;

namespace Blocto.Sdk.Flow
{
    public class BloctoWalletProvider : MonoBehaviour, IBloctoWalletProvider, IWalletProvider
    {
        private static string env;
        
        /// <summary>
        /// System Thumbnail
        /// </summary>
        protected static string Thumbnail;
        
        /// <summary>
        /// System Title
        /// </summary>
        protected static string Title;

        /// <summary>
        /// HTTP utility
        /// </summary>
        private WebRequestUtility WebRequestUtility;
        
        /// <summary>
        /// Forced use of WebView
        /// </summary>
        public bool ForcedUseWebView { get; set; }

        public string  ConnectedWalletAddress = "default";
        
        #if UNITY_IOS
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
        /// Check specify app installed
        /// </summary>
        /// <param name="appUrl"></param>
        /// <returns></returns>
        [DllImport ("__Internal")]
        private static extern bool IsInstalled(string appUrl);
        
        /// <summary>
        /// Get universal link data
        /// </summary>
        /// <returns></returns>
        [DllImport("__Internal")]
        private static extern string UniversalLink_GetURL();

        /// <summary>
        /// Reset universal link on iOS code
        /// </summary>
        /// <returns></returns>
        [DllImport("__Internal")]
        private static extern string UniversalLink_Reset(); 
        #endif
        
        
        protected IResolveUtility ResolveUtility;
        
        protected Dictionary<string, string> RequestIdActionMapper;
        
        protected Action<object> AuthenticateCallback;
        
        protected Action<ExecuteResult<List<FlowSignature>>> SignmessageCallback;
        
        protected Action<string> TransactionCallback;
        
        protected bool IsInstalledApp = false;

        protected Guid BloctoAppIdentifier;

        protected Guid RequestId;
        
        private IFlowClient _flowClient;
        
        private int _maxRetryCount = 10;
        
        private bool _isCancelRequest;
        
        /// <summary>
        /// Android instance
        /// </summary>
        private AndroidJavaObject _pluginInstance = null;
        
        private string _appSdkDomain = "https://blocto.app/sdk?";
        
        private string _backedApiDomain = "https://api.blocto.app";
        
        private string _androidPackageName = "com.portto.blocto";
        
        private Dictionary<bool, Func<FlowTransaction, PreAuthzAdapterResponse, Action<string>, FlowTransaction>> _transactionModeMapper;
        
        private Dictionary<bool, Action<FclService, FlowTransaction, Action<string>>> _transactionSdkMapper;
        
        private List<Func<string, (int Index, string Name, string Value)>> _authnReturnParsers;

        /// <summary>
        /// Create blocto wallet provider instance
        /// </summary>
        /// <param name="initialFun">Blocto initial func</param>
        /// <param name="env">Env</param>
        /// <param name="bloctoAppIdentifier">Blocto sdk appId</param>
        /// <returns>BloctoWalletProvider</returns>
        public static BloctoWalletProvider CreateBloctoWalletProvider(Func<Func<GameObject, IFlowClient, IResolveUtility, BloctoWalletProvider>, BloctoWalletProvider> initialFun, string env, Guid bloctoAppIdentifier)
        {
            var bloctoWalletProvider = initialFun.Invoke((gameObject, flowClient, resolveUtility) => {
                                                             var provider = gameObject.AddComponent<BloctoWalletProvider>();
                                                             provider.WebRequestUtility = gameObject.AddComponent<WebRequestUtility>();
                                                             provider.ResolveUtility = resolveUtility;
                                                             provider._flowClient = flowClient;
                                                             provider.RequestIdActionMapper = new Dictionary<string, string>();
                                                             provider.ForcedUseWebView = false;
                                                             
                                                             return provider;
                                                         });
            
            bloctoWalletProvider.gameObject.name = "bloctowalletprovider";
            bloctoWalletProvider._isCancelRequest = false;
            bloctoWalletProvider.BloctoAppIdentifier = bloctoAppIdentifier;
            bloctoWalletProvider.WebRequestUtility.BloctoAppId = bloctoAppIdentifier.ToString();
            BloctoWalletProvider.env = env.ToLower();
            
            if(env.ToLower() == "testnet")
            {
                bloctoWalletProvider._backedApiDomain = bloctoWalletProvider._backedApiDomain.Replace("api", "api-dev");
                bloctoWalletProvider._androidPackageName = $"{bloctoWalletProvider._androidPackageName}.dev";
                bloctoWalletProvider._appSdkDomain = bloctoWalletProvider._appSdkDomain.Replace("blocto.app", "dev.blocto.app");
            } 
            
            if(Application.platform == RuntimePlatform.Android)
            {
                bloctoWalletProvider.InitializePlugins("com.blocto.unity.UtilityActivity");
            }
            
            bloctoWalletProvider.IsInstalledApp = bloctoWalletProvider.CheckInstalledApp();
            return bloctoWalletProvider;
        }

        public void Awake()
        {
            SetUp();
        }

        protected virtual void SetUp()
        {
            _transactionModeMapper = new Dictionary<bool, Func<FlowTransaction, PreAuthzAdapterResponse, Action<string>, FlowTransaction>>
            {
                { false, CustodialHandler},
                { true, NonCustodialHandler}
            };
            
            _transactionSdkMapper = new Dictionary<bool, Action<FclService, FlowTransaction, Action<string>>>
            {
                { true, SendTransactionWithAppSdk },
                { false, SendTransactionWithWebSdk },
            };
            
            if(IsInstalledApp)
            {
                _authnReturnParsers = new List<Func<string, (int Index, string Name, string Value)>>
                {
                    AddressParser,
                    SignatureParser
                };
            } 
        }

        /// <summary>
        /// Check specify app installed
        /// </summary>
        /// <returns></returns>
        private bool CheckInstalledApp()
        {
            IsInstalledApp = false;
            var testDomain = "blocto://open";
            if(FlowClientLibrary.Config.Get("flow.network", "testnet") == "testnet")
            {
                testDomain = "blocto-dev://open";
            }
            
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    IsInstalledApp = _pluginInstance.Call<bool>("isInstalledApp", _androidPackageName); 
                    break;
                case RuntimePlatform.IPhonePlayer:
                    #if UNITY_IOS
                    isInstallApp = BloctoWalletProvider.IsInstalled(testDomain);
                    #endif
                    break;
                case RuntimePlatform.OSXEditor:
                    break;
            }
            
            $"Is installed app: {IsInstalledApp}".ToLog();
            return IsInstalledApp;
        }
        
        public void UnAuthenticate()
        {
            var isLogout = false;
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    isLogout = _pluginInstance.Call<bool>("webViewLogout"); 
                    break;
                case RuntimePlatform.IPhonePlayer:
                    #if UNITY_IOS
                    throw new NotSupportedException("iOS does not support disconnect wallet.");
                    #endif
                case RuntimePlatform.OSXEditor:
                    break;
            } 
            
            $"WebView status: {isLogout}".ToLog();
        }

        /// <summary>
        /// User connect wallet get account
        /// </summary>
        /// <param name="url">fcl authn url</param>
        /// <param name="parameters">parameter of authn</param>
        /// <param name="internalCallback">After, get endpoint response internal callback.</param>
        public void Authenticate(string url, Dictionary<string, object> parameters, Action<object> internalCallback = null)
        {
            if(IsInstalledApp && ForcedUseWebView == false)
            {
                var deeplink = CreateAuthenticateUrlForAppSdk(parameters);
                $"Url: {deeplink.Url}".ToLog();
                AuthenticateCallback = internalCallback;
                StartCoroutine(OpenUrl(deeplink.Url));
                return;
            }
            
            var endpoint = CreateAuthenticateUrlForWebSdk(url, parameters);

            $"Url: {endpoint.IframeUrl}".ToLog();
            StartCoroutine(OpenUrl(endpoint.IframeUrl, endpoint.PollingUrl.AbsoluteUri));
            StartCoroutine(GetService<AuthenticateResponse>(endpoint.PollingUrl, internalCallback));
        }

        /// <summary>
        /// SignMessage
        /// </summary>
        /// <param name="message">Original message </param>
        /// <param name="signService">FCL signature service</param>
        /// <param name="callback">After, get endpoint response callback.</param>
        public void SignMessage(string message, FclService signService, Action<ExecuteResult<List<FlowSignature>>> callback = null)
        {
            if(IsInstalledApp && ForcedUseWebView == false)
            {
                var sb = CreateSignMessageUrlForAppSdk(message, signService);
                SignmessageCallback = callback;
                StartCoroutine(OpenUrl(sb.ToString()));
                return;
            }
            
            var endpoint = CreateSignMessageUrlForWebSdk(message, signService);
            $"IframeUrl: {endpoint.IframeUrl}".ToLog();
            
            StartCoroutine(OpenUrl(endpoint.IframeUrl, endpoint.PollingUrl.AbsoluteUri));
            StartCoroutine(GetService<SignMessageResponse>(endpoint.PollingUrl, signMessageResponse => {
                                                                                    var flowSignatures = signMessageResponse.Data.Select(item => new FlowSignature
                                                                                                                                                 {
                                                                                                                                                     Address = new FlowAddress(item.Address()),
                                                                                                                                                     KeyId = Convert.ToUInt32(item.KeyId()),
                                                                                                                                                     Signature = Encoding.UTF8.GetBytes(item.SignatureStr())
                                                                                                                                                 })
                                                                                                                                                 .ToList();

                                                                                    var result = new ExecuteResult<List<FlowSignature>>
                                                                                                 {
                                                                                                     Data = flowSignatures,
                                                                                                     IsSuccessed = true,
                                                                                                     Message = string.Empty
                                                                                                 };
                                                                                     
                                                                                    callback?.Invoke(result);
                                                                                }));
        }

        protected virtual (string IframeUrl, Uri PollingUrl) CreateSignMessageUrlForWebSdk(string message, FclService signService)
        {
            $"Fcl service: {JsonConvert.SerializeObject(signService)}".ToLog();
            var signUrl = signService.SignMessageAdapterEndpoint();
            $"signUrl: {signUrl}".ToLog();

            var hexMessage = message.StringToHex();
            var payload = ResolveUtility.ResolveSignMessage(hexMessage, signService.PollingParams.SessionId());
            $"Payload: {JsonConvert.SerializeObject(payload)}".ToLog();
            
            var response = SendWebRequest<JObject, AuthnAdapterResponse>((signUrl, "POST", "application/json"), payload);
            $"Authn adapter response: {JsonConvert.SerializeObject(response)}".ToLog();
            
            var endpoint = response.SignMessageEndpoint();
            var webSb = new StringBuilder(endpoint.IframeUrl);
            webSb.Append("&")
                .Append(Uri.EscapeDataString("thumbnail") + "=")
                .Append(Thumbnail + "&")
                .Append(Uri.EscapeDataString("title") + "=")
                .Append(Title);
            endpoint.IframeUrl = webSb.ToString();
            $"IFrame url: {endpoint.IframeUrl}, Polling url: {endpoint.PollingUrl}".ToLog();
            
            return endpoint;
        }

        protected virtual TResponse SendData<TPayload, TResponse>((string SignUrl, string RequestMethod, string ContentTYpe) requestInfo, TPayload payload) where TResponse : class where TPayload : class
        {
            var response = WebRequestUtility.GetResponse<TResponse>(requestInfo.SignUrl, requestInfo.RequestMethod, requestInfo.ContentTYpe, payload);
            return response;
        }

        protected virtual StringBuilder CreateSignMessageUrlForAppSdk(string message, FclService signService)
        {
            var requestId = CreateGuid();
            RequestIdActionMapper.Add(requestId.ToString(), "signmessage");

            var sb = new StringBuilder(_appSdkDomain);
            sb.Append($"app_id={BloctoAppIdentifier}" + "&")
                .Append($"request_id={requestId}" + "&")
                .Append("blockchain=flow" + "&")
                .Append("method=user_signature" + "&")
                .Append($"from={signService.Addr}" + "&")
                .Append($"message={Uri.EscapeUriString(Uri.EscapeUriString(message))}");
            return sb;
        }

        /// <summary>
        /// Send transaction
        /// </summary>
        /// <param name="service">fcl service for web sdk</param>
        /// <param name="tx">flow transaction data</param>
        /// <param name="internalCallback">complete transaction internal callback</param>
        public virtual void SendTransaction(FclService service, FlowTransaction tx, Action<string> internalCallback)
        {
            if(ForcedUseWebView)
            {
                _transactionSdkMapper[false].Invoke(service, tx, internalCallback);
                return;
            }
            
            var action = _transactionSdkMapper[IsInstalledApp];
            action.Invoke(service, tx, internalCallback);
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
            if (!RequestIdActionMapper.TryGetValue(item.RequestId, out var action))
            {
                return;
            }

            switch (action)
            {
                case "authn":
                    var tmp = UniversalLinkAuthnHandler(item.RemainContent);
                    ConnectedWalletAddress = tmp.Address;
                    var signatures = tmp.Signatures.Select(signature => new JObject
                                                                        {
                                                                            new JProperty("keyId", signature.KeyId),
                                                                            new JProperty("signature", Encoding.UTF8.GetString(signature.Signature))
                                                                        }).ToList();

                    var authnResponse = new AuthenticateResponse
                                        {
                                            Data = new AuthenticateData
                                                   {
                                                       Addr = tmp.Address,
                                                       Services = new[]{ 
                                                                           new FclService
                                                                           {
                                                                               Type = ServiceTypeEnum.AccountProof, 
                                                                               Data = new FclServiceData
                                                                                      {
                                                                                          Address = tmp.Address,
                                                                                          FVsn = string.Empty,
                                                                                          Signatures = signatures
                                                                                      }
                                                                           },
                                                                           new FclService
                                                                           {
                                                                               Type = ServiceTypeEnum.USERSIGNATURE,
                                                                               Addr = tmp.Address,
                                                                               Data = new FclServiceData
                                                                                      {
                                                                                          Address = tmp.Address
                                                                                      }
                                                                           },
                                                                           new FclService
                                                                           {
                                                                               Type = ServiceTypeEnum.PREAUTHZ,
                                                                                              
                                                                           }
                                                                       }
                                                   }
                                        };
                        
                    AuthenticateCallback.Invoke(authnResponse);
                    break;
                case "signmessage":
                    var flowSignatures = UniversalLinkSignMessageHandler(item.RemainContent); 
                    var result = new ExecuteResult<List<FlowSignature>>
                                 {
                                     Data = flowSignatures,
                                     IsSuccessed = true,
                                     Message = string.Empty
                                 };
                        
                    SignmessageCallback?.Invoke(result);
                    break;
                case "transaction":
                    var tx = UniversalLinkTransactionHandler(item.RemainContent);
                    TransactionCallback.Invoke(tx);
                    break;
            }
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
            var retryCount = 0;
            _isCancelRequest = true;

            //// Polling requests are only sent on Android or iOS, because an error action in the editor can cause too many requests to be sent
            if(Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                _isCancelRequest = false;
            }
            
            while (isApprove == false && _isCancelRequest == false)
            {
                if (retryCount > _maxRetryCount)
                {
                    _isCancelRequest = true;
                }
                
                var webRequest = WebRequestUtility.CreateUnityWebRequest(pollingUri.AbsoluteUri, "GET", "application/json", new DownloadHandlerBuffer());
                response = WebRequestUtility.ProcessWebRequest<TResponse>(webRequest);
                isApprove = response!.ResponseStatus is ResponseStatusEnum.APPROVED or ResponseStatusEnum.DECLINED;
                
                $"IsApprove: {isApprove}, IsCancelRequest: {_isCancelRequest}".ToLog();
                retryCount++;
                yield return new WaitForSeconds(0.5f);
            }
            
            if(response is null)
            {
                yield break;
            }

            if (response!.ResponseStatus == ResponseStatusEnum.PENDING || _isCancelRequest)
            {
                yield break;
            }

            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    break;
                case RuntimePlatform.IPhonePlayer:
                    #if UNITY_IOS && !UNITY_EDITOR
                    BloctoWalletProvider.CloseWindow();
                    #endif
                    break;
            }

            internalCallback.Invoke(response);
        }
        
        protected virtual Guid CreateGuid()
        {
            return Guid.NewGuid();
        }
        
        protected virtual (string Url, Uri PollingUrl) CreateAuthenticateUrlForAppSdk(Dictionary<string, object> parameters)
        {
            RequestId = CreateGuid();
            RequestIdActionMapper.Add(RequestId.ToString(), "authn");
                
            var sb = new StringBuilder(_appSdkDomain);
            sb.Append($"app_id={BloctoAppIdentifier}" + "&")
                .Append($"request_id={RequestId}" + "&")
                .Append("blockchain=flow" + "&")
                .Append("method=authn" + "&");
                    
            if(parameters.ContainsKey("accountProofIdentifier") && parameters.ContainsKey("accountProofNonce"))
            {
                sb.Append($"flow_app_id={parameters["accountProofIdentifier"]}" + "&")
                    .Append($"flow_nonce={parameters["accountProofNonce"]}");
            }
                
            $"Url: {sb}".ToLog();
            return (sb.ToString(), null);
        }

        protected virtual (string IframeUrl, Uri PollingUrl) CreateAuthenticateUrlForWebSdk(string url, Dictionary<string, object> parameters)
        {
            var payload = CreateConnectWalletPayload(parameters);
            var authnResponse = SendWebRequest<ConnectWalletPayload ,AuthnAdapterResponse>((url,"POST", "application/json"), payload);
            var endpoint = authnResponse.AuthnEndpoint(BloctoAppIdentifier.ToString());
            
            $"Authn response: {JsonConvert.SerializeObject(authnResponse)}, Url: {endpoint.IframeUrl}, PollingUrl: {endpoint.PollingUrl.AbsoluteUri}".ToLog();
            return endpoint;
        }

        protected virtual ConnectWalletPayload CreateConnectWalletPayload(Dictionary<string, object> parameters)
        {
            var payload = new ConnectWalletPayload(parameters, BloctoAppIdentifier.ToString());
            return payload;
        }

        protected virtual THttpResponse SendWebRequest<TPayload, THttpResponse>((string Url, string RequestMethod, string ContentType) requestInfo, TPayload payload) where THttpResponse : class where TPayload : class
        {
            THttpResponse response;
            if (payload is Dictionary<string, object> tmpPayload)
            {
                response = WebRequestUtility.GetResponse<THttpResponse>(requestInfo.Url, requestInfo.RequestMethod, requestInfo.ContentType, tmpPayload);
                return response;
            }

            response = WebRequestUtility.GetResponse<THttpResponse>(requestInfo.Url, requestInfo.RequestMethod, requestInfo.ContentType, payload);
            return response;
        }

        /// <summary>
        /// Get authorizer signature
        /// </summary>
        /// <param name="iframeUrl">User approve page</param>
        /// <param name="pollingUri">Service endpoint url</param>
        /// <param name="internalCallback">After, get endpoint response internal callback.</param>
        private void Authz<TResponse>(string iframeUrl, Uri pollingUri, Action<TResponse> internalCallback) where TResponse : IResponse
        {
            StartCoroutine(OpenUrl(iframeUrl, pollingUri.AbsoluteUri));
            StartCoroutine(GetService(pollingUri, internalCallback));
        }
        
         /// <summary>
        /// Send transaction with web sdk
        /// </summary>
        /// <param name="service"></param>
        /// <param name="tx">Transaction data</param>
        /// <param name="internalCallback"></param>
        private void SendTransactionWithWebSdk(FclService service, FlowTransaction tx, Action<string> internalCallback)
        {
            var url = service.PreAuthzEndpoint();
            var preSignableJObj = ResolveUtility.ResolvePreSignable(ref tx);
            var preAuthzResponse = WebRequestUtility.GetResponse<PreAuthzAdapterResponse>(url, "POST", "application/json", preSignableJObj);
            var isNonCustodial = preAuthzResponse.AuthorizerData.Authorizations.Any(p => p.Endpoint.ToLower().Contains("cosigner") || p.Endpoint.ToLower().Contains("non-custodial"));
            var tmpAccount = GetAccount(preAuthzResponse.AuthorizerData.Proposer.Identity.Address).ConfigureAwait(false).GetAwaiter().GetResult();
            tx.ProposalKey = GetProposerKey(tmpAccount, preAuthzResponse.AuthorizerData.Proposer.Identity.KeyId);
            var action = _transactionModeMapper[isNonCustodial];
            action.Invoke(tx, preAuthzResponse, internalCallback);
        }

        /// <summary>
        /// Send transaction use app sdk
        /// </summary>
        /// <param name="service"></param>
        /// <param name="tx"></param>
        /// <param name="internalCallback"></param>
        /// <exception cref="Exception"></exception>
        protected void SendTransactionWithAppSdk(FclService service, FlowTransaction tx, Action<string> internalCallback)
        {
            var sb = CreateSendTransactionUrlForAppSdk(tx);

            $"Url: {sb}".ToLog();
            TransactionCallback = internalCallback;
            StartCoroutine(OpenUrl(sb.ToString()));
        }

        protected virtual StringBuilder CreateSendTransactionUrlForAppSdk(FlowTransaction tx)
        {
            var proposer = GetFlowAccount();
            var key = proposer.Keys.FirstOrDefault(p => p.Weight == 999 && p.Revoked == false);
            if (key == null)
            {
                throw new Exception("Can't find user key.");
            }

            if (!tx.SignerList.ContainsKey(ConnectedWalletAddress.AddHexPrefix()) &&
                !tx.SignerList.ContainsKey(ConnectedWalletAddress.RemoveHexPrefix()))
            {
                tx.SignerList.Add(ConnectedWalletAddress.RemoveHexPrefix(), tx.SignerList.Count + 1);
            }

            tx.ProposalKey = new FlowProposalKey
            {
                Address = new FlowAddress(ConnectedWalletAddress),
                KeyId = Convert.ToUInt32(key.Index),
                SequenceNumber = key.SequenceNumber
            };

            var response = SendWebRequest<Dictionary<string, object>, JObject>(($"{_backedApiDomain}/flow/feePayer", "GET", ""), new Dictionary<string, object>());
            var feePayerAddr = response.GetValue("address")?.ToString();
            tx.Payer = new FlowAddress(feePayerAddr);
            if (tx.SignerList.Any())
            {
                foreach (var item in tx.SignerList)
                {
                    tx.Authorizers.Add(new FlowAddress(item.Key));
                }

                tx.SignerList.Clear();
            }
            else
            {
                tx.Authorizers.Add(new FlowAddress(ConnectedWalletAddress));
            }

            var tmp = EncodeUtility.GetRlpEncodeCollection(tx);
            var dataCollection = new List<object>
            {
                tmp,
                new List<List<byte>>(),
                new List<List<byte>>()
            };

            var encode = RLP.RlpEncode(dataCollection).ToArray().BytesToHex();
            var requestId = CreateGuid();
            RequestIdActionMapper.Add(requestId.ToString(), "transaction");
            var sb = new StringBuilder(_appSdkDomain);
            sb.Append($"app_id={BloctoAppIdentifier}" + "&").Append($"request_id={requestId}" + "&")
                .Append("blockchain=flow" + "&").Append("method=flow_send_transaction" + "&")
                .Append($"from={ConnectedWalletAddress}" + "&").Append($"flow_transaction={encode}");
            
            $"Sb: {sb}".ToLog();
            return sb;
        }

        protected virtual FlowAccount GetFlowAccount()
        {
            var proposer = _flowClient.GetAccountAtLatestBlockAsync(ConnectedWalletAddress).ConfigureAwait(false).GetAwaiter()
                .GetResult();
            return proposer;
        }

        /// <summary>
        /// Handle custodial mode transaction
        /// </summary>
        /// <param name="tx">transaction data</param>
        /// <param name="preAuthzResponse">pre authz response data</param>
        /// <param name="callback"></param>
        /// <returns>Flow transaction data</returns>
        private FlowTransaction CustodialHandler(FlowTransaction tx, PreAuthzAdapterResponse preAuthzResponse, Action<string> callback)
        {
            var authorization = preAuthzResponse.AuthorizerData.Authorizations.First();
            var postUrl = authorization.AuthzAdapterEndpoint();
            var authorize = authorization.ConvertToFlowAccount();
            var signableJObj = ResolveUtility.ResolveSignable(ref tx, preAuthzResponse.AuthorizerData, authorize).First();
            var authzResponse = WebRequestUtility.GetResponse<AuthzAdapterResponse>(postUrl, "POST", "application/json", signableJObj);
            var endpoint = authzResponse.AuthzEndpoint();
            Authz<AuthzAdapterResponse>(endpoint.IframeUrl, endpoint.PollingUrl, item => {
                                                                                     var response = item;
                                                                                     var signInfo = response.SignatureInfo();
                                                                                     if (signInfo.Signature != null)
                                                                                     {
                                                                                         var payloadSignature = tx.PayloadSignatures.First(p => p.Address.Address == signInfo.Address?.ToString().RemoveHexPrefix());
                                                                                         payloadSignature.Signature = signInfo.Signature?.ToString().StringToBytes().ToArray();
                                                                                     }

                                                                                     var payerEndpoint = preAuthzResponse.PayerEndpoint();
                                                                                     var payerSignable = ResolveUtility.ResolvePayerSignable(ref tx, signableJObj);
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

            return tx;
        }

        /// <summary>
        /// Handle non-custodial mode transaction
        /// </summary>
        /// <param name="tx">transaction data</param>
        /// <param name="preAuthzResponse">pre authz response data</param>
        /// <param name="callback"></param>
        /// <returns>Flow transaction data</returns>
        private FlowTransaction NonCustodialHandler(FlowTransaction tx, PreAuthzAdapterResponse preAuthzResponse, Action<string> callback)
        {
            var signableJObj = default(JObject);
            var endpoint = default((string IframeUrl, Uri PollingUrl));
            var authorization = preAuthzResponse.AuthorizerData.Authorizations.First();
            var authorize = authorization.ConvertToFlowAccount();
            var signableJObjs = ResolveUtility.ResolveSignable(ref tx, preAuthzResponse.AuthorizerData, authorize);
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
                                                                                  var payerSignable = ResolveUtility.ResolvePayerSignable(ref tx, signableJObj);
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

            return tx;
        }
       
        /// <summary>
        /// Open webview
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private IEnumerator OpenUrl(string url, string pollingUrl = "")
        {
            try
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                        if(IsInstalledApp && ForcedUseWebView == false)
                        {
                            _pluginInstance.Call("openSDK", _androidPackageName, url, url, new AndroidCallback(), "bloctowalletprovider", "DeeplinkHandler");
                        }
                        else
                        {
                            $"Call android webview".ToLog();
                            _pluginInstance.Call("webview", url, new AndroidCallback(), pollingUrl, "flow");
                        }
                        
                        break;
                    case RuntimePlatform.IPhonePlayer:
                        #if UNITY_IOS
                        BloctoWalletProvider.OpenUrl("bloctowalletprovider", "DeeplinkHandler", url, url);
                        #endif
                        break;
                    default:
                        throw new Exception("This platform does not support open webview.");
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Ex: {e.Message}");
            } 
            
            yield return new WaitForSeconds(0.5f);
        }
        
        private (string Address, List<FlowSignature> Signatures) UniversalLinkAuthnHandler(string link)
        {
            var address = default(string);
            var signatures = default(List<FlowSignature>);
            var keywords = new List<string>{ "address=", "account_proof" };
            var index = 0;
            var data = (MatchContents: new List<string>(), RemainContent: link);
            while (data.RemainContent.Length > 0)
            {
                var keyword = keywords[index];
                data = CheckContent(data.RemainContent, keyword);
                switch (keyword)
                {
                    case "address=":
                        address = AddressParser(data.MatchContents.FirstOrDefault()).Value;
                        break;
                    case "account_proof":
                        signatures = SignatureProcess(data);
                        break;
                }
                
                index++;
            }
            
            return (address, signatures);
        }
        
        private List<FlowSignature> UniversalLinkSignMessageHandler(string link)
        {
            var data = CheckContent(link, "user_signature");
            var signatures = SignatureProcess(data);
            return signatures;
        }
        
        private string UniversalLinkTransactionHandler(string link)
        {
            var data = CheckContent(link, "tx_hash");
            var tx = data.MatchContent.First().Split("=")[1];
            return tx;
        }

        private List<FlowSignature> SignatureProcess((List<string> MatchContents, string RemainContent) data)
        {
            var sort = 0;
            var signature = new FlowSignature();
            var signatures = new List<FlowSignature>();
            foreach (var result in data.MatchContents.Select(SignatureParser))
            {
                if (sort != result.Index)
                {
                    sort += 1;
                    signatures.Add(signature);
                    signature = new FlowSignature();
                }

                switch (result.Name)
                {
                    case "address":
                        signature.Address = new FlowAddress(result.Value);
                        break;
                    case "key_id":
                        signature.KeyId = Convert.ToUInt32(result.Value);
                        break;
                    case "signature":
                        signature.Signature = Encoding.UTF8.GetBytes(result.Value);
                        break;
                }
            }
            
            signatures.Add(signature);
            return signatures;
        }
        
        private (List<string> MatchContent, string RemainContent) CheckContent(string text, string keyword)
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
        
        private (int Index, string Name, string Value) AddressParser(string text)
        {
            var value = text.Split("=")[1];
            return (0, "address", value);
        }
        
        private (int Index, string Name, string Value) SignatureParser(string text)
        {
            var keyValue = text.Split("=");
            var propertiesPattern = @"(?<=\[)(.*)(?=\])";

            var match = Regex.Match(keyValue[0], propertiesPattern);
            if (!match.Success)
            {
                throw new Exception("App sdk return value format error");
            }

            var elements = match.Captures.FirstOrDefault()?.Value.Split("][");
            return (Convert.ToInt32(elements?[0]), elements?[1], keyValue[1]);
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
        /// Initial android instance
        /// </summary>
        /// <param name="pluginName"></param>
        private void InitializePlugins(string pluginName)
        {
            try
            {
                $"Init android plugin, plugin name: {pluginName}".ToLog();
                _pluginInstance = new AndroidJavaObject(pluginName);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                throw;
            }
        }
    }
}