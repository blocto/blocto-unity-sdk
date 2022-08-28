using Flow.FCL.Utility;
using Flow.Net.SDK.Extensions;
using UnityEngine;

namespace Flow.FCL
{
    public class UtilFactory : MonoBehaviour
    {
        private IResolveUtils _resolveUtils;
        
        public virtual IWebRequestUtils CreateWebRequestUtil() => gameObject.AddComponent<WebRequestUtils>();
        
        public virtual IResolveUtils CreateResolveUtils()
        {
            if(_resolveUtils != null)
            {
                $"ResolveUtils not null return exist ResolveUtils".ToLog();
                return _resolveUtils;
            }
            
            _resolveUtils = new ResolveUtils();
            return _resolveUtils;
        }
    }
}