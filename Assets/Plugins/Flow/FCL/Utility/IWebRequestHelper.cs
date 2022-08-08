using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Flow.FCL.Utility
{
    public interface IWebRequestHelper
    {
        public TResponse GetResponse<TResponse>(string url);

        public UnityWebRequest CreateUnityWebRequest(string url, string method, string contentType, DownloadHandlerBuffer downloadHandlerBuffer, UploadHandlerRaw uploadHandlerRaw = null);
    }
}