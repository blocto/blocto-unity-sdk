using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Flow.Net.SDK.Client.Unity.Models.Apis;
using Flow.Net.SDK.Client.Unity.Models.Enums;
using Flow.Net.SDK.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using UnityEngine.Networking;

namespace Blocto.Sdk.Core.Utility
{
    public class WebRequestUtility : MonoBehaviour
    {
        private readonly Dictionary<string, Action<UnityWebRequest>> _handlers;
        
        public WebRequestUtility()
        {
            _handlers = new Dictionary<string, Action<UnityWebRequest>>
                        {
                            {"400", BadRequestHandler},
                            {"404", NotFoundHandler},
                            {"500", InternalServerError}
                        };
        }
        
        /// <summary>
        /// Handling unitywebrequest send request and receive response
        /// </summary>
        /// <param name="unityWebRequest">UnityWebRequest instance</param>
        /// <typeparam name="T">Return type</typeparam>
        /// <returns></returns>
        /// <exception cref="ApiException{Error}"></exception>
        /// <exception cref="ApiException"></exception>
        public T ProcessWebRequest<T>(UnityWebRequest unityWebRequest)
        {
            StartCoroutine(SendRequest(unityWebRequest));
            while (!unityWebRequest.isDone)
            {
                Task.Yield();
            }
                
            if (unityWebRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                throw new ApiException<Error>("Bad Request", (int)unityWebRequest.responseCode, "", null, null);
            }
                
            var headers_ = unityWebRequest.GetResponseHeaders();
            var status = ((int)unityWebRequest.responseCode).ToString();
            if(status is "200" or "204")
            {
                var tmp = unityWebRequest.downloadHandler.data;
                $"Response content: {Encoding.UTF8.GetString(tmp)}".ToLog();
                var objectResponse_ = ReadObjectResponseAsync<T>(unityWebRequest);
                unityWebRequest.Dispose();
                return objectResponse_.Object;  
            }
                
            if(_handlers.ContainsKey(status))
            {
                _handlers[status].Invoke(unityWebRequest);
            }
            else
            {
                $"Url: {unityWebRequest.url}".ToLog();
                throw new ApiException("The HTTP status code of the response was not expected (" + (int)unityWebRequest.responseCode + ").", (int)unityWebRequest.responseCode, "", null); 
            } 
            
            return default;
        }
        
        /// <summary>
        /// Handler web response status code is 400
        /// </summary>
        /// <param name="unityWebRequest">UnityWebRequest</param>
        /// <exception cref="ApiException{Error}"></exception>
        private void BadRequestHandler(UnityWebRequest unityWebRequest)
        {
            var objectResponse_ = ReadObjectResponseAsync<Error>(unityWebRequest);
            throw new ApiException<Error>("Bad Request", (int)unityWebRequest.responseCode, objectResponse_.Text, objectResponse_.Object, null); 
        }
        
        /// <summary>
        /// Handler web response status code is 404
        /// </summary>
        /// <param name="unityWebRequest">UnityWebRequest</param>
        /// <exception cref="ApiException{Error}"></exception>
        private void NotFoundHandler(UnityWebRequest unityWebRequest)
        {
            var objectResponse_ = ReadObjectResponseAsync<Error>(unityWebRequest);
            throw new ApiException<Error>("Not Found", (int)unityWebRequest.responseCode, objectResponse_.Text, objectResponse_.Object, null); 
        }
        
        /// <summary>
        /// Handler web response status code is 500
        /// </summary>
        /// <param name="unityWebRequest">UnityWebRequest</param>
        /// <exception cref="ApiException{Error}"></exception>
        private void InternalServerError(UnityWebRequest unityWebRequest)
        {
            var objectResponse_ = ReadObjectResponseAsync<Error>(unityWebRequest);
            throw new ApiException<Error>("Internal Server Error", (int)unityWebRequest.responseCode, objectResponse_.Text, objectResponse_.Object, null); 
        }
        
        public UnityWebRequest CreateUnityWebRequest(string url, string method, string contentType, DownloadHandlerBuffer downloadHandlerBuffer, UploadHandlerRaw uploadHandlerRaw = null)
        {
            var uri = new Uri(url, UriKind.RelativeOrAbsolute);
            var unityWebRequest = new UnityWebRequest(uri, method);
            unityWebRequest.SetRequestHeader("Content-Type", contentType);
            unityWebRequest.SetRequestHeader("Blocto-Application-Identifier", "12a22f0b-c2ec-47ef-aa24-64115f94f781");
            
            if(uploadHandlerRaw != null)
            {
                unityWebRequest.uploadHandler = uploadHandlerRaw;
            }
            
            unityWebRequest.downloadHandler = downloadHandlerBuffer;
            
            return unityWebRequest;
        }
        
        private IEnumerator SendRequest(UnityWebRequest unityRequest)
        {
            yield return unityRequest.SendWebRequest();
        }
        
        protected virtual ObjectResponseResult<T> ReadObjectResponseAsync<T>(UnityWebRequest unityWebRequest)
        {
            if (unityWebRequest.downloadHandler.data == null)
            {
                return new ObjectResponseResult<T>(default, string.Empty);
            }

            try
            {
                using var streamReader = new System.IO.StreamReader(new MemoryStream(unityWebRequest.downloadHandler.data));
                using var jsonTextReader = new JsonTextReader(streamReader);
                var ser = new JsonSerializer
                          {
                              ContractResolver = new CamelCasePropertyNamesContractResolver(),
                              NullValueHandling = NullValueHandling.Ignore,
                          };
                
                ser.Converters.Add(new StringEnumConverter());
                var typedBody = ser.Deserialize<T>(jsonTextReader);
                return new ObjectResponseResult<T>(typedBody, string.Empty);
            }
            catch (JsonException exception)
            {
                var message = "Could not deserialize the response body stream as " + typeof(T).FullName + ".";
                throw new ApiException(message, (int)unityWebRequest.responseCode, string.Empty, exception);
            }
        }
    }
}