using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Blocto.Flow;
using Flow.FCL.Models;
using Flow.FCL.Utility;
using Flow.FCL.WalletProvider;
using Flow.Net.SDK.Client.Unity.Unity;
using Flow.Net.Sdk.Core.Cadence;
using Flow.Net.Sdk.Core.Client;
using Flow.Net.Sdk.Core.Models;
using Flow.Net.SDK.Extensions;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TestTools;

namespace Flow.FCL
{
    public class FlowClientLibrary : MonoBehaviour
    {
        private Config.Config Config { get; set; }

        private IWalletProvider _walletProvider;
        
        private CurrentUser _currentUser;
        
        private IWebRequestUtils _webRequestUtils;
        
        private IResolveUtils _resolveUtils;
        
        private IFlowClient _flowClient;
        
        private CoreModule _coreModule;
        
        private ICadence _response;
        
        private string _errorMessage;
        
        private bool _isSuccessed;
        
        public static FlowClientLibrary CreateClientLibrary(GameObject gameObject, Config.Config config, string mode, IResolveUtils resolveUtils)
        {
            var fcl = gameObject.AddComponent<FlowClientLibrary>();
            var tmpWalletProvider = BloctoWalletProvider.CreateBloctoWalletProvider(gameObject, config.Get("testnet"));
            var flowClient = new FlowUnityWebRequest(gameObject, config.Get("testnet")); 
            var factory = UtilFactory.CreateUtilFactory(gameObject, flowClient);
            
            fcl._resolveUtils = resolveUtils;
            fcl._flowClient = flowClient;
            fcl._walletProvider = tmpWalletProvider;
            fcl._webRequestUtils = factory.CreateWebRequestUtil();
            fcl._coreModule = new CoreModule(
                tmpWalletProvider,
                flowClient,
                resolveUtils,
                factory);
            fcl.Config = config;
            return fcl;
        }
        
        public FlowClientLibrary()
        {
           Config = new Config.Config();
           _response = default(Cadence);
           _errorMessage = string.Empty;
           _isSuccessed = false;
        }

        public CurrentUser CurrentUser()
        {
            return _currentUser;
        }
        
        /// <summary>
        /// Calling this method will authenticate the current user via any wallet that supports FCL.
        /// Once called, FCL will initiate communication with the configured discovery.wallet endpoint which lets the user select a wallet to authenticate with.
        /// Once the wallet provider has authenticated the user,
        /// FCL will set the values on the current user object for future use and authorization.
        /// </summary>
        /// <param name="callback">The callback will be called when the user authenticates and un-authenticates, making it easy to update the UI accordingly.</param>
        public void Authenticate(Action callback = null)
        {
            var url = Config.Get("discovery.wallet");
            _coreModule.Authenticate(url, () => {
                                                           Debug.Log("In internal callback");
                                                           Debug.Log($"WallProvider is null {_walletProvider == null}");
                                                           switch (_walletProvider.PollingResponse.Status)
                                                           {
                                                               case PollingStatusEnum.APPROVED:
                                                                   Debug.Log($"Polling response status APPROVED");
                                                                   _currentUser = new CurrentUser
                                                                                  {
                                                                                      Addr = new FlowAddress(_walletProvider.PollingResponse.Data.Addr),
                                                                                      LoggedIn = true, 
                                                                                      F_type = "USER",
                                                                                      F_vsn = _walletProvider.PollingResponse.FVsn,
                                                                                      Services = _walletProvider.PollingResponse.Data.Services.ToList(),
                                                                                      ExpiresAt = _walletProvider.PollingResponse.Data.Expires 
                                                                                  };
                                                                   
                                                                   _currentUser.SetConfig(Config);
                                                                   _currentUser.SetWalletProvider(_walletProvider);
                                                                   _currentUser.SetCoreModule(_coreModule);
                                                                   _currentUser.SetCurrentUser(_currentUser);
                                                                   Debug.Log($"currentUser service count: {_currentUser.Services.Count}");
                                                                   break;
                                                               case PollingStatusEnum.DECLINED:
                                                                   _currentUser = new CurrentUser
                                                                                  {
                                                                                       LoggedIn = false, 
                                                                                       F_type = "USER",
                                                                                       F_vsn = _walletProvider.PollingResponse.FVsn,
                                                                                       ExpiresAt = _walletProvider.PollingResponse.Data.Expires
                                                                                  };
                                                                   break;
                                                               case PollingStatusEnum.PENDING:
                                                               case PollingStatusEnum.REDIRECT:
                                                               case PollingStatusEnum.NONE:
                                                               default:
                                                                   break;
                                                           }
                                                       }, callback);
        }
        
        public void PreAuthz(FlowTransaction tx, Action callback)
        {
            var service = _currentUser.Services.First(p => p.Type == ServiceTypeEnum.PREAUTHZ);
            var urlBuilder = new StringBuilder();
            urlBuilder.Append(service.Endpoint.AbsoluteUri + "?")
                      .Append(Uri.EscapeUriString("sessionId") + "=")
                      .Append(Uri.EscapeDataString(service.PollingParams.SessionId));
            
            Debug.Log($"Pre authz url: {urlBuilder.ToString()}");
            _coreModule.SendTransaction(service, tx, () => {}, callback);
        }
        
        public void Mutate(FlowTransaction tx, Action callback)
        {
            var service = _currentUser.Services.First(p => p.Type == ServiceTypeEnum.PREAUTHZ);
            tx = _coreModule.SendTransaction(service, tx, () => {}, callback);
        }
        
        public async Task<QueryResult> Query(FlowScript flowScript)
        {
            await ExecuteScript(flowScript);
            var result = new QueryResult
                         {
                             Data = _response,
                             IsSuccessed = _isSuccessed,
                             Message = _errorMessage
                         };
            _isSuccessed = false;
            _errorMessage = string.Empty;
            _response = default(Cadence);
            return result;
        }
        
        private List<(string Type, string Value, string Name)> GetValue(ICadence cadence, string fieldName = "")
        {
            var fields = new List<(string Type, string Value, string Name)>();
            switch (cadence.Type)
            {
                case "Struct":
                case "Resource":
                case "Event":
                case "Contract":
                case "Enum":
                    var tmp =  new CadenceComposite((CadenceCompositeType)Enum.Parse(typeof(CadenceCompositeType), cadence.Type));
                    foreach (var temp in tmp.Value.Fields)
                    {
                         fields.AddRange(GetValue(temp.Value, temp.Name));
                    }
                    
                    return fields;
                    break;
                case "Int":
                case "UInt":
                case "Int8":
                case "UInt8":
                case "Int16":
                case "UInt16":
                case "Int32":
                case "UInt32":
                case "Int64":
                case "UInt64":
                case "Int128":
                case "UInt128":
                case "Int256":
                case "UInt256":
                case "Word8":
                case "Word16":
                case "Word32":
                case "Word64":
                case "Fix64":
                case "UFix64":
                    return new List<(string Type, string Value, string Name)>
                           {
                                          (cadence.Type, cadence.As<CadenceNumber>().Value, fieldName)
                                      };
                case "Address":
                    return new List<(string Type, string Value, string Name)>
                           {
                                          (cadence.Type, cadence.As<CadenceAddress>().Value, fieldName)
                                      };
                case "String":
                    return new List<(string Type, string Value, string Name)>
                           {
                                          (cadence.Type, cadence.As<CadenceString>().Value, fieldName)
                                      };
                case "Bool":
                    return new List<(string Type, string Value, string Name)>
                           {
                                          (cadence.Type, cadence.As<CadenceBool>().Value.ToString(), fieldName)
                                      };
                default:
                    var cadenceType = _response.GetType();
                    var realTypeName = cadenceType.FullName;
                    var realType = Type.GetType(realTypeName!);
                    var item = Convert.ChangeType(_response, realType!);
                    return new List<(string Type, string Value, string Name)>();
                    break;
            }
        }
        
        /// <summary>
        /// Logs out the current user and sets the values on the current user object to null.
        /// </summary>
        /// <param name="callback">The callback will be called when the user authenticates and un-authenticates, making it easy to update the UI accordingly.</param>
        public void UnAuthenticate(Action callback = null)
        {
            this._currentUser = null;
            callback?.Invoke();
        }
        
        private async Task ExecuteScript(FlowScript flowScript)
        {
            try
            {
                _response = await _flowClient.ExecuteScriptAtLatestBlockAsync(flowScript); 
                _isSuccessed = true;
            }
            catch (Exception e)
            {
                $"Execute script error: {e.Message}".ToLog();
                _errorMessage = e.Message;
            }
        }
        
        private IEnumerator WaitForSeconds(float time)
        {
            yield return new WaitForSeconds(time);
        }
        
        public async Task<FlowAccount> GetAccount(string address)
        {
            var account = _coreModule.GetAccount(address);
            return await account;
        }
    }
}