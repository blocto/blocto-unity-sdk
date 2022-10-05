using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Blocto.Sdk.Core.Model;
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
        private static string Thumbnail;
        
        /// <summary>
        /// System Title
        /// </summary>
        private static string Title;

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
        
        /// <summary>
        /// Android instance
        /// </summary>
        private AndroidJavaObject _pluginInstance = null;
        
        private IResolveUtility _resolveUtility;
        
        private IFlowClient _flowClient;
        
        private bool _isCancelRequest;
        
        private Guid _bloctoAppIdentifier;
        
        private string _appSdkDomain = "https://staging.blocto.app/sdk?";
        
        private string _backedApiDomain = "https://api.blocto.app";
        
        public string _address = "default";
        
        private Dictionary<bool, Func<FlowTransaction, PreAuthzAdapterResponse, Action<string>, FlowTransaction>> _transactionModeMapper;
        
        private Dictionary<bool, Action<FclService, FlowTransaction, Action<string>>> _transactionSdkMapper;

        public bool _isInstalledApp = false;
        
        public static string UniversalLink = "default";
        
        public Dictionary<string, string> _requestIdActionMapper;
        
        private List<Func<string, (int Index, string Name, string Value)>> _authnReturnParsers;
        
        private Action<object> _authenticateCallback;
        
        private Action<ExecuteResult<List<FlowSignature>>> _signmessageCallback;
        
        private Action<string> _transactionCallback;

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
                                                             provider._resolveUtility = resolveUtility;
                                                             provider._flowClient = flowClient;
                                                             provider._requestIdActionMapper = new Dictionary<string, string>();
                                                             
                                                             return provider;
                                                         });
            
            bloctoWalletProvider.gameObject.name = "bloctowalletprovider";
            bloctoWalletProvider._isCancelRequest = false;
            bloctoWalletProvider._bloctoAppIdentifier = bloctoAppIdentifier;
            bloctoWalletProvider.WebRequestUtility.BloctoAppId = bloctoAppIdentifier.ToString();
            
            if(env.ToLower() == "testnet")
            {
                bloctoWalletProvider._backedApiDomain = bloctoWalletProvider._backedApiDomain.Replace("api", "api-dev");
            } 
            
            if(Application.platform == RuntimePlatform.Android)
            {
                bloctoWalletProvider.InitializePlugins("com.blocto.unity.PluginActivity");
            }
            
            bloctoWalletProvider._isInstalledApp = bloctoWalletProvider.IsInstalledApp();
            
            return bloctoWalletProvider;
        }

        public void Awake()
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
            if(_isInstalledApp)
            {
                _authnReturnParsers = new List<Func<string, (int Index, string Name, string Value)>>
                                      {
                                          AddressParser,
                                          SignatureParser
                                      };
            }
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
            if(_requestIdActionMapper.ContainsKey(item.RequestId))
            {
                var action = _requestIdActionMapper[item.RequestId];
                switch (action)
                {
                    case "authn":
                        var tmp = UniversalLinkAuthnHandler(item.RemainContent);
                        _address = tmp.Address;
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
                                                           Services = new FclService[]{ 
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
                        
                        _authenticateCallback.Invoke(authnResponse);
                        break;
                    case "signmessage":
                        var flowSignatures = UniversalLinkSignMessageHandler(item.RemainContent); 
                        var result = new ExecuteResult<List<FlowSignature>>
                                     {
                                         Data = flowSignatures,
                                         IsSuccessed = true,
                                         Message = string.Empty
                                     };
                        
                        _signmessageCallback?.Invoke(result);
                        break;
                    case "transaction":
                        var tx = UniversalLinkTransactionHandler(item.RemainContent);
                        _transactionCallback.Invoke(tx);
                        break;
                }
            }
        }

        /// <summary>
        /// Check specify app installed
        /// </summary>
        /// <returns></returns>
        private bool IsInstalledApp()
        {
            
            var isInstallApp = false;
            var testDomain = "blocto://open";
            if(FlowClientLibrary.Config.Get("flow.network", "testnet") == "testnet")
            {
                testDomain = $"blocto-staging://open";
            }
            
            $"InstalledApp.".ToLog();
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    $"In android mehtod".ToLog();
                    var number = _pluginInstance.Call<int>("add", 1, 3);
                    $"Number: {number}".ToLog();
                    var content = _pluginInstance.Call<string>("log", "Method test.");
                    $"Content: {content}".ToLog();
                    isInstallApp = _pluginInstance.Call<bool>("isInstalledApp", "com.portto.blocto.staging"); 
                    break;
                case RuntimePlatform.IPhonePlayer:
                    isInstallApp = BloctoWalletProvider.IsInstalled(testDomain);
                    break;
            }
            
            return isInstallApp;
        }

        /// <summary>
        /// Check specify app installed
        /// </summary>
        /// <returns></returns>
        private bool IsInstalledApp()
        {
            var isInstallApp = false;
            var testDomain = "blocto://open";
            if(FlowClientLibrary.Config.Get("flow.network", "testnet") == "testnet")
            {
                testDomain = $"blocto-staging://open";
            }
            
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    break;
                case RuntimePlatform.IPhonePlayer:
                    $"App domain: {_appSdkDomain}".ToLog();
                
                    isInstallApp = BloctoWalletProvider.IsInstalled(testDomain);
                    break;
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
            if(_isInstalledApp)
            {
                var requestId = Guid.NewGuid();
                _requestIdActionMapper.Add(requestId.ToString(), "authn");
                
                var sb = new StringBuilder(_appSdkDomain);
                sb.Append($"app_id={_bloctoAppIdentifier}" + "&")
                  .Append($"request_id={requestId}" + "&")
                  .Append("blockchain=flow" + "&")
                  .Append("method=authn" + "&");
                    
                if(parameters.ContainsKey("accountProofIdentifier") && parameters.ContainsKey("accountProofNonce"))
                {
                    sb.Append($"flow_app_id={parameters["accountProofIdentifier"]}" + "&")
                      .Append($"flow_nonce={parameters["accountProofNonce"]}");
                }
                
                $"Nonce: {parameters["accountProofNonce"].ToString()}".ToLog();
                
                _authenticateCallback = internalCallback;
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
        
        /// <summary>
        /// Send transaction
        /// </summary>
        /// <param name="service">fcl service for web sdk</param>
        /// <param name="tx">flow transaction data</param>
        /// <param name="internalCallback">complete transaction internal callback</param>
        public virtual void SendTransaction(FclService service, FlowTransaction tx, Action<string> internalCallback)
        {
            var action = _transactionSdkMapper[_isInstalledApp];
            action.Invoke(service, tx, internalCallback);
        }

        /// <summary>
        /// Send transaction with web sdk
        /// </summary>
        /// <param name="service"></param>
        /// <param name="tx"></param>
        /// <param name="internalCallback"></param>
        private void SendTransactionWithWebSdk(FclService service, FlowTransaction tx, Action<string> internalCallback)
        {
            var url = service.PreAuthzEndpoint();
            var preSignableJObj = _resolveUtility.ResolvePreSignable(ref tx);
            var preAuthzResponse = WebRequestUtility.GetResponse<PreAuthzAdapterResponse>(url, "POST", "application/json", preSignableJObj);
            var isNonCustodial = preAuthzResponse.AuthorizerData.Authorizations.Any(p => p.Endpoint.ToLower().Contains("cosigner") || p.Endpoint.ToLower().Contains("non-custodial"));
            var tmpAccount = GetAccount(preAuthzResponse.AuthorizerData.Proposer.Identity.Address).ConfigureAwait(false).GetAwaiter().GetResult();
            tx.ProposalKey = GetProposerKey(tmpAccount, preAuthzResponse.AuthorizerData.Proposer.Identity.KeyId);
            var action = _transactionModeMapper[isNonCustodial];
            tx = action.Invoke(tx, preAuthzResponse, internalCallback);
        }

        /// <summary>
        /// Send transaction use app sdk
        /// </summary>
        /// <param name="service"></param>
        /// <param name="tx"></param>
        /// <param name="internalCallback"></param>
        /// <exception cref="Exception"></exception>
        private void SendTransactionWithAppSdk(FclService service, FlowTransaction tx, Action<string> internalCallback)
        {
            var proposer = _flowClient.GetAccountAtLatestBlockAsync(_address).ConfigureAwait(false).GetAwaiter().GetResult();
            var key = proposer.Keys.FirstOrDefault(p => p.Weight == 999 && p.Revoked == false);
            if (key == null)
            {
                throw new Exception("Can't find user key.");
            }

            if (!tx.SignerList.ContainsKey(_address.AddHexPrefix()) && !tx.SignerList.ContainsKey(_address.RemoveHexPrefix()))
            {
                tx.SignerList.Add(_address.RemoveHexPrefix(), tx.SignerList.Count + 1);
            }

            tx.ProposalKey = new FlowProposalKey
                             {
                                 Address = new FlowAddress(_address),
                                 KeyId = Convert.ToUInt32(key.Index),
                                 SequenceNumber = key.SequenceNumber
                             };

            var response = WebRequestUtility.GetResponse<JObject>($"{_backedApiDomain}/flow/feePayer", "GET", "", new Dictionary<string, object>());
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
                tx.Authorizers.Add(new FlowAddress(_address));
            }

            var tmp = EncodeUtility.GetRlpEncodeCollection(tx);
            var dataCollection = new List<object>
                                 {
                                     tmp,
                                     new List<List<byte>>(),
                                     new List<List<byte>>(),
                                 };

            var encode = RLP.RlpEncode(dataCollection).ToArray().BytesToHex();

            var requestId = Guid.NewGuid();
            _requestIdActionMapper.Add(requestId.ToString(), "transaction");
            var sb = new StringBuilder(_appSdkDomain);
            sb.Append($"app_id={_bloctoAppIdentifier}" + "&").Append($"request_id={requestId}" + "&").Append("blockchain=flow" + "&").Append("method=flow_send_transaction" + "&").Append($"from={_address}" + "&").Append($"flow_transaction={encode}");

            $"Url: {sb.ToString()}".ToLog();
            _transactionCallback = internalCallback;
            StartCoroutine(OpenUrl(sb.ToString()));
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
            var signableJObj = _resolveUtility.ResolveSignable(ref tx, preAuthzResponse.AuthorizerData, authorize).First();
            var authzResponse = WebRequestUtility.GetResponse<AuthzAdapterResponse>(postUrl, "POST", "application/json", signableJObj);
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

            return tx;
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
        public void SignMessage(string message, FclService signService, Action<ExecuteResult<List<FlowSignature>>> callback = null)
        {
            if(_isInstalledApp)
            {
                var requestId = Guid.NewGuid();
                _requestIdActionMapper.Add(requestId.ToString(), "signmessage");
                
                var sb = new StringBuilder(_appSdkDomain);
                sb.Append($"app_id={_bloctoAppIdentifier}" + "&")
                  .Append($"request_id={requestId}" + "&")
                  .Append("blockchain=flow" + "&")
                  .Append("method=user_signature" + "&")
                  .Append($"from={signService.Addr}" + "&")
                  .Append($"message={Uri.EscapeUriString(Uri.EscapeUriString(message))}"); 
                
                _signmessageCallback = callback;
                StartCoroutine(OpenUrl(sb.ToString()));
            }
            else
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
                                                                                         var result = new ExecuteResult<List<FlowSignature>>
                                                                                                      {
                                                                                                          Data = new List<FlowSignature>
                                                                                                                 {
                                                                                                                     new FlowSignature
                                                                                                                     {
                                                                                                                         Address = new FlowAddress(addr),
                                                                                                                         KeyId = keyId,
                                                                                                                         Signature = Encoding.UTF8.GetBytes(signature!)
                                                                                                                     }
                                                                                                                 },
                                                                                                          IsSuccessed = true,
                                                                                                          Message = string.Empty
                                                                                                      };
                                                                                         
                                                                                         callback?.Invoke(result);
                                                                                     }));
            }
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
                yield return new WaitForSeconds(0.5f);
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
                switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                        _pluginInstance.Call("openSDK", "com.portto.blocto.staging", url, url, new AndroidCallback(), "bloctowalletprovider", "DeeplinkHandler");
                        break;
                    case RuntimePlatform.IPhonePlayer:
                        BloctoWalletProvider.OpenUrl("bloctowalletprovider", "DeeplinkHandler", url, url);
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
            return data.MatchContent.First();
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