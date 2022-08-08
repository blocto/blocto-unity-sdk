using Flow.FCL.Utility;
using UnityEngine;

namespace Flow.FCL
{
    public class HelperFactory : MonoBehaviour
    {
        public virtual IWebRequestUtils CreateWebRequestHelper()
        {
            return gameObject.AddComponent<WebRequestUtils>();
        }
    }
}