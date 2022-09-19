using Flow.FCL.Utility;
using Flow.Net.Sdk.Core.Client;
using UnityEngine;

namespace Flow.FCL
{
    public class UtilFactory : MonoBehaviour
    {
        private IFlowClient _flowClient;
        
        private IResolveUtility _resolveUtility;
        
        private IWebRequestUtils _webRequestUtility;
        
        public static UtilFactory CreateUtilFactory(GameObject gameObject, IFlowClient flowClient, IResolveUtility resolveUtility)
        {
            var factory = gameObject.AddComponent<UtilFactory>();
            factory._webRequestUtility = gameObject.AddComponent<WebRequestUtils>();
            factory._flowClient = flowClient;
            factory._resolveUtility = resolveUtility;
            return factory;
        }
        
        public virtual IWebRequestUtils CreateWebRequestUtil() => _webRequestUtility;
        
        public virtual IResolveUtility CreateResolveUtility() => _resolveUtility;
        
        public virtual AppUtility CreateAppUtil(string env) => new AppUtility(_flowClient);
    }
}