using Blocto.Sdk.Core.Utility;
using Flow.FCL.Utility;
using Flow.Net.Sdk.Core.Client;
using Flow.Net.SDK.Extensions;
using UnityEngine;

namespace Flow.FCL
{
    public class UtilFactory : MonoBehaviour
    {
        private IResolveUtils _resolveUtils;
        
        private IFlowClient _flowClient;
        
        private ResolveUtility _resolveUtility;
        
        private IWebRequestUtils _webRequestUtils;
        
        public static UtilFactory CreateUtilFactory(GameObject gameObject, IFlowClient flowClient)
        {
            var factory = gameObject.AddComponent<UtilFactory>();
            factory._webRequestUtils = gameObject.AddComponent<WebRequestUtils>();
            factory._flowClient = flowClient;
            return factory;
        }
        
        public virtual IWebRequestUtils CreateWebRequestUtil() => _webRequestUtils;
        
        public virtual ResolveUtility CreateResolveUtility()
        {
            if(_resolveUtility != null)
            {
                return _resolveUtility;
            }
            
            _resolveUtility = new ResolveUtility(_flowClient);
            return _resolveUtility;
        }
        
        public virtual AppUtils CreateAppUtil(string env) => new AppUtils(_flowClient, env);
    }
}