using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Flow.FCL.Utility
{
    public interface IWebRequestUtils
    {
        public TResponse GetResponse<TResponse>(string url, string method, string contentType, Dictionary<string, object> parameters);
        
        public TResponse GetResponse<TResponse>(string url, string method, string contentType, object parameter);

        public UnityWebRequest CreateUnityWebRequest(string url, string method, string contentType, DownloadHandlerBuffer downloadHandlerBuffer, UploadHandlerRaw uploadHandlerRaw = null);
    }
}