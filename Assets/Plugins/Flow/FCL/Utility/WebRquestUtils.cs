using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flow.Net.SDK.Client.Unity.Models.Apis;
using Flow.Net.SDK.Client.Unity.Models.Enums;
using Flow.Net.SDK.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using UnityEngine.Networking;

namespace Flow.FCL.Utility
{
    // 再打 wallet webapp 的時候請帶上 Blocto-Application-Identifier 這個 Header
    public class WebRequestUtils : MonoBehaviour, IWebRequestUtils
    {
        private readonly Dictionary<string, Action<UnityWebRequest>> _handlers;
        
        public WebRequestUtils()
        {
            _handlers = new Dictionary<string, Action<UnityWebRequest>>
                        {
                            {"400", BadRequestHandler},
                            {"404", NotFoundHandler},
                            {"500", InternalServerError}
                        };
        }

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
        
        public TResponse GetResponse<TResponse>(string url, string method, string contentType, object parameter)
        {
            var client = CreateUnityWebRequest(url, method, contentType, new DownloadHandlerBuffer());;
            client.SetRequestHeader("Blocto-Application-Identifier", "12a22f0b-c2ec-47ef-aa24-64115f94f781");
            var jsonStr = default(string);
            
            jsonStr = parameter is JObject
                          ? parameter.ToString()
                          : JsonConvert.SerializeObject(parameter);
            
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

        private T ProcessWebRequest<T>(UnityWebRequest unityWebRequest)
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
                using var streamReader = new System.IO.StreamReader(new MemoryStream(unityWebRequest.downloadHandler.data));
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
            var objectResponse_ = ReadObjectResponseAsync<string>(unityWebRequest);
            throw new ApiException<Error>("Bad Request", (int)unityWebRequest.responseCode, objectResponse_.Text, new Error
                                                                                                                  {
                                                                                                                      Message = objectResponse_.Text
                                                                                                                  }, null); 
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
            // Debug.Log($"DEBUG Send Request: {DateTime.Now:HH:mm:ss.fff}");
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
                if(unityWebRequest.responseCode == 200)
                {
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
                else
                {
                    var responseTxt = streamReader.ReadToEnd();
                    return new ObjectResponseResult<T>(default(T), responseTxt);
                }
            }
            catch (JsonException exception)
            {
                var message = "Could not deserialize the response body stream as " + typeof(T).FullName + ".";
                throw new ApiException(message, (int)unityWebRequest.responseCode, string.Empty, exception);
            }
        }
    }
}