using Flow.FCL.Utility;
using UnityEngine;

namespace Plugins.Flow
{
    public class HelperFactory : MonoBehaviour
    {
        public virtual IWebRequestHelper CreateWebRequestHelper()
        {
            return gameObject.AddComponent<WebRequestHelper>();
        }
    }
}