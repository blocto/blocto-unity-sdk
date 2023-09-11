using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Blocto.Sdk.Core.Extension;
using Blocto.Sdk.Core.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using UnityEngine.Networking;

namespace Blocto.Sdk.Core.Utility
{
    public class WebRequestUtility : MonoBehaviour
    {
        public string BloctoAppId { get; set; }
        
        public Dictionary<string, string> Headers;

        private Dictionary<string, Action<UnityWebRequest>> _handlers;

        private void Awake()
        {
            Headers = new Dictionary<string, string>();
            _handlers = new Dictionary<string, Action<UnityWebRequest>>
                        {
                            {"400", BadRequestHandler},
                            {"404", NotFoundHandler},
                            {"500", InternalServerError}
                        };
        }
        
        public void SetHeader(KeyValuePair<string, string>[] parameters)
        {
            Headers ??= new Dictionary<string, string>();
            foreach (var item in parameters)
            {
                Headers.Add(item.Key, item.Value);
            }
        }
        
        /// <summary>
        /// Send http request
        /// </summary>
        /// <param name="url">Endpoint url</param>
        /// <param name="method">HTTP method</param>
        /// <param name="contentType">Header content type</param>
        /// <typeparam name="TResponse">Return type</typeparam>
        /// <returns></returns>
        public TResponse GetResponse<TResponse>(string url, HttpMethod method, string contentType)
        {
            Debug.Log($"Get response, url: {url}");
            var client = CreateUnityWebRequest(url, method.ToString(), contentType, new DownloadHandlerBuffer());;
            
            try
            {
                var result = ProcessWebRequest<TResponse>(client);
                return result;
            }
            finally
            {
                client?.Dispose();
            }
        }

        /// <summary>
        /// Send http request
        /// </summary>
        /// <param name="url">Endpoint url</param>
        /// <param name="method">HTTP method</param>
        /// <param name="contentType">Header content type</param>
        /// <param name="parameters">body parameter</param>
        /// <typeparam name="TResponse">Return type</typeparam>
        /// <returns></returns>
        public TResponse GetResponse<TResponse>(string url, string method, string contentType, Dictionary<string, object> parameters)
        {
            Debug.Log($"Get response, url: {url}");
            var client = CreateUnityWebRequest(url, method, contentType, new DownloadHandlerBuffer());;
            if(parameters.Keys.Any())
            {
                var jsonStr = JsonConvert.SerializeObject(parameters);
                var requestBytes = Encoding.UTF8.GetBytes(jsonStr);
                var uploadHandler = new UploadHandlerRaw(requestBytes);
                client.uploadHandler = uploadHandler;
            }
            
            try
            {
                var result = ProcessWebRequest<TResponse>(client);
                return result;
            }
            finally
            {
                client?.Dispose();
            }
        }
        
        /// <summary>
        /// Send http request
        /// </summary>
        /// <param name="url">Endpoint url</param>
        /// <param name="method">HTTP method</param>
        /// <param name="contentType">Header content type</param>
        /// <param name="parameter">body parameter</param>
        /// <typeparam name="TResponse">Return type</typeparam>
        /// <returns></returns>
        public TResponse GetResponse<TResponse>(string url, string method, string contentType, object parameter)
        {
            var client = CreateUnityWebRequest(url, method, contentType, new DownloadHandlerBuffer());;
            var jsonStr = default(string);
            
            jsonStr = parameter is JObject
                          ? parameter.ToString()
                          : JsonConvert.SerializeObject(parameter);
            $"Url: {url}".ToLog();
            $"Request body: {jsonStr}".ToLog();
            var requestBytes = Encoding.UTF8.GetBytes(jsonStr);
            var uploadHandler = new UploadHandlerRaw(requestBytes);
            client.uploadHandler = uploadHandler;
            
            try
            {
                var result = ProcessWebRequest<TResponse>(client);
                return result;
            }
            finally
            {
                client?.Dispose();
            }
        }
        
        /// <summary>
        /// Handling unitywebrequest send request and receive response
        /// </summary>
        /// <param name="unityWebRequest">UnityWebRequest instance</param>
        /// <typeparam name="T">Return type</typeparam>
        /// <returns></returns>
        /// <exception cref="ApiException"></exception>
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
                $"Respone content: {Encoding.UTF8.GetString(tmp)}".ToLog();
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
                $"Request Url: {unityWebRequest.url}".ToLog();
                if(unityWebRequest.downloadHandler.data.Length > 0)
                {
                    $"Error Message: {Encoding.UTF8.GetString(unityWebRequest.downloadHandler.data)}".ToLog();
                }
                
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
            var tmp = unityWebRequest.downloadHandler.data; 
            $"Http 400, Reson: {Encoding.UTF8.GetString(tmp)}".ToLog();
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
            if(contentType != string.Empty)
            {
                unityWebRequest.SetRequestHeader("Content-Type", contentType);
            }
            
            if(url.ToLower().Contains("blocto"))
            {
                unityWebRequest.SetRequestHeader("Blocto-Application-Identifier", BloctoAppId);
                unityWebRequest.SetRequestHeader("Blocto-Request-Source", "sdk_unity");
            }
            
            if(Headers.Count > 0)
            {
                foreach (var header in Headers)
                {
                    $"Set unityWebRequest header: {header.Key}:{header.Value}".ToLog();
                    unityWebRequest.SetRequestHeader(header.Key, header.Value);
                }
                
                Headers.Clear();
            }
            
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