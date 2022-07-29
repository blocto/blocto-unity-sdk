using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Blocto.Flow.Client.Http.Models;
using Blocto.Flow.Client.Http.Models.Apis;
using Newtonsoft.Json;
using UnityEngine.Networking;
using UnityEngine;
using Error = Blocto.Flow.Client.Http.Models.Apis.Error;

namespace Blocto.Flow.Client.Http.Unity 
{

    public partial class FlowApiV1 : MonoBehaviour
    {
        public string BaseUrl { get; set; } = "";
        
        public bool ReadResponseAsString { get; set; }

        private JsonSerializerSettings JsonSerializerSettings => _jsonSettings.Value;

        private readonly Lazy<JsonSerializerSettings> _jsonSettings;
        
        private readonly Dictionary<string, Action<UnityWebRequest>> _handlers;

        public FlowApiV1()
        {
            _jsonSettings = new Lazy<JsonSerializerSettings>(() => {
                                                                 var settings = new JsonSerializerSettings();
                                                                 settings.Converters.Add(new ScriptValueConverter());
                                                                 UpdateJsonSerializerSettings(settings);
                                                                 return settings;
                                                             });
            
            _handlers = new Dictionary<string, Action<UnityWebRequest>>
                        {
                            {"400", BadRequestHandler},
                            {"404", NotFoundHandler},
                            {"500", InternalServerError}
                        };
        }

        partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings);
        
        /// <summary>Gets Blocks by Height</summary>
        /// <param name="height">A comma-separated list of block heights to get. This parameter is incompatible with `start_height` and `end_height`.</param>
        /// <param name="start_height">The start height of the block range to get. Must be used together with `end_height`. This parameter is incompatible with `height`.</param>
        /// <param name="end_height">The ending height of the block range to get. Must be used together with `start_height`. This parameter is incompatible with `height`.</param>
        /// <param name="expand">A comma-separated list indicating which properties of the content to expand.</param>
        /// <param name="select">A comma-separated list indicating which properties of the content to return.</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public ICollection<Block> BlocksAllAsync(IEnumerable<string> height, string start_height, string end_height, IEnumerable<string> expand, IEnumerable<string> select)
        {
            return BlocksAll(height, start_height, end_height, expand, select, System.Threading.CancellationToken.None);
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Gets Blocks by Height</summary>
        /// <param name="height">A comma-separated list of block heights to get. This parameter is incompatible with `start_height` and `end_height`.</param>
        /// <param name="start_height">The start height of the block range to get. Must be used together with `end_height`. This parameter is incompatible with `height`.</param>
        /// <param name="end_height">The ending height of the block range to get. Must be used together with `start_height`. This parameter is incompatible with `height`.</param>
        /// <param name="expand">A comma-separated list indicating which properties of the content to expand.</param>
        /// <param name="select">A comma-separated list indicating which properties of the content to return.</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public ICollection<Block> BlocksAll(IEnumerable<string> height, string start_height, string end_height, IEnumerable<string> expand, IEnumerable<string> select, System.Threading.CancellationToken cancellationToken)
        {
            var urlBuilder = new StringBuilder();
            urlBuilder.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/blocks?");
            IsAppendQueryWithHeight(height, urlBuilder);
            IsAppendQueryWithStartHeight(start_height, urlBuilder);
            IsAppendQueryWithEndHeight(end_height, urlBuilder);
            IsAppendQueryWithExpand(expand, urlBuilder);
            IsAppendQueryWithSelect(select, urlBuilder);
            urlBuilder.Length--;
            
            var uri = new Uri(urlBuilder.ToString(), UriKind.RelativeOrAbsolute);
            var client = CreateUnityWebRequestWithGet(uri, "application/json", new DownloadHandlerBuffer());
            try
            {
                // StartCoroutine(SendRequest(client));
                // while (!client.isDone)
                // {
                //     Task.Yield();
                // }
                //
                // if (client.result == UnityWebRequest.Result.ConnectionError)
                // {
                //     throw new ApiException<Error>("Bad Request", (int)client.responseCode, "", null, null);
                // }
                //
                // var headers_ = client.GetResponseHeaders();
                // var status = ((int)client.responseCode).ToString();
                // if(status is "200" or "204")
                // {
                //     var tmp = client.downloadHandler.data;
                //     var objectResponse_ = ReadObjectResponseAsync<ICollection<Block>>(client);
                //     Debug.Log($"return object: {DateTime.Now:HH:mm:ss.fff}");
                //     return objectResponse_.Object;  
                // }
                //
                // if(_handlers.ContainsKey(status))
                // {
                //     _handlers[status].Invoke(client);
                // }
                // else
                // {
                //     throw new ApiException("The HTTP status code of the response was not expected (" + (int)client.responseCode + ").", (int)client.responseCode, "", null); 
                // }
                
                var result = ProcessWebRequest<ICollection<Block>>(client);
                return result;
            }
            finally
            {
                    client?.Dispose();
            }
            
            return default;
        }
        
        /// <summary>Get a Transaction by ID.</summary>
        /// <param name="id">The ID of the transaction to get.</param>
        /// <param name="expand">A comma-separated list indicating which properties of the content to expand.</param>
        /// <param name="select">A comma-separated list indicating which properties of the content to return.</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public Transaction Transactions(string id, IEnumerable<string> expand, IEnumerable<string> select)
        {
            return Transactions(id, expand, select, System.Threading.CancellationToken.None);
        }
        
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Get a Transaction by ID.</summary>
        /// <param name="id">The ID of the transaction to get.</param>
        /// <param name="expand">A comma-separated list indicating which properties of the content to expand.</param>
        /// <param name="select">A comma-separated list indicating which properties of the content to return.</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public Transaction Transactions(string id, IEnumerable<string> expand, IEnumerable<string> select, System.Threading.CancellationToken cancellationToken)
        {
            if (id == null)
                throw new ArgumentNullException("id");

            var urlBuilder = new StringBuilder();
            urlBuilder.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/transactions/{id}?");
            urlBuilder.Replace("{id}", Uri.EscapeDataString(ConvertToString(id, CultureInfo.InvariantCulture)));
            IsAppendQueryWithExpand(expand, urlBuilder);
            IsAppendQueryWithSelect(select, urlBuilder);
            urlBuilder.Length--;

            var uri = new Uri(urlBuilder.ToString(), UriKind.RelativeOrAbsolute);
            var client = CreateUnityWebRequestWithGet(uri, "application/json", new DownloadHandlerBuffer());
            try
            {
                var result = ProcessWebRequest<Transaction>(client);
                return result;
            }
            finally
            {
                client?.Dispose();
            }
            
            return default;
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
                var objectResponse_ = ReadObjectResponseAsync<T>(unityWebRequest);
                Debug.Log($"return object: {DateTime.Now:HH:mm:ss.fff}");
                return objectResponse_.Object;  
            }
                
            if(_handlers.ContainsKey(status))
            {
                _handlers[status].Invoke(unityWebRequest);
            }
            else
            {
                throw new ApiException("The HTTP status code of the response was not expected (" + (int)client.responseCode + ").", (int)client.responseCode, "", null); 
            } 
            
            return default;
        }
        
        private void BadRequestHandler(UnityWebRequest unityWebRequest)
        {
            var objectResponse_ = ReadObjectResponseAsync<Error>(unityWebRequest);
            throw new ApiException<Error>("Bad Request", (int)unityWebRequest.responseCode, objectResponse_.Text, objectResponse_.Object, null); 
        }
        
        private void NotFoundHandler(UnityWebRequest unityWebRequest)
        {
            var objectResponse_ = ReadObjectResponseAsync<Error>(unityWebRequest);
            throw new ApiException<Error>("Not Found", (int)unityWebRequest.responseCode, objectResponse_.Text, objectResponse_.Object, null); 
        }
        
        private void InternalServerError(UnityWebRequest unityWebRequest)
        {
            var objectResponse_ = ReadObjectResponseAsync<Error>(unityWebRequest);
            throw new ApiException<Error>("Internal Server Error", (int)unityWebRequest.responseCode, objectResponse_.Text, objectResponse_.Object, null); 
        }

        private void IsAppendQueryWithSelect(IEnumerable<string> select, StringBuilder urlBuilder_)
        {
            if (select == null)
            {
                return;
            }

            urlBuilder_.Append(Uri.EscapeDataString("select") + "=");
            foreach (var item_ in select)
            {
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(item_, CultureInfo.InvariantCulture))).Append(",");
            }

            urlBuilder_.Length--;
            urlBuilder_.Append("&");
        }
        
        private void IsAppendQueryWithExpand(IEnumerable<string> expand, StringBuilder urlBuilder_)
        {
            if (expand == null)
            {
                return;
            }

            urlBuilder_.Append(Uri.EscapeDataString("expand") + "=");
            foreach (var item_ in expand)
            {
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(item_, CultureInfo.InvariantCulture))).Append(",");
            }

            urlBuilder_.Length--;
            urlBuilder_.Append("&");
        }

        private void IsAppendQueryWithEndHeight(string end_height, StringBuilder urlBuilder_)
        {
            if (end_height != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("end_height") + "=").Append(Uri.EscapeDataString(ConvertToString(end_height, CultureInfo.InvariantCulture))).Append("&");
            }
        }

        private void IsAppendQueryWithStartHeight(string start_height, StringBuilder urlBuilder_)
        {
            if (start_height != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("start_height") + "=").Append(Uri.EscapeDataString(ConvertToString(start_height, CultureInfo.InvariantCulture))).Append("&");
            }
        }

        private void IsAppendQueryWithHeight(IEnumerable<string> height, StringBuilder urlBuilder_)
        {
            if (height == null)
            {
                return;
            }

            urlBuilder_.Append(Uri.EscapeDataString("height") + "=");
            foreach (var item_ in height)
            {
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(item_, CultureInfo.InvariantCulture))).Append(",");
            }

            urlBuilder_.Length--;
            urlBuilder_.Append("&");
        }

        private string ConvertToString(object value, IFormatProvider cultureInfo)
        {
            switch (value)
            {
                case Enum: {
                    var name = Enum.GetName(value.GetType(), value);
                    if (name == null)
                    {
                        return Convert.ToString(value, cultureInfo);
                    }

                    var field = System.Reflection.IntrospectionExtensions.GetTypeInfo(value.GetType()).GetDeclaredField(name);
                    if (field != null)
                    {
                        if (System.Reflection.CustomAttributeExtensions.GetCustomAttribute(field, typeof(EnumMemberAttribute)) is EnumMemberAttribute attribute)
                        {
                            return attribute.Value ?? name;
                        }
                    }

                    break;
                }
                case bool:
                    return Convert.ToString(value, cultureInfo)?.ToLowerInvariant();
                case byte[] bytes:
                    return Convert.ToBase64String(bytes);
                default: {
                    if (value != null && value.GetType().IsArray)
                    {
                        var array = ((Array)value).OfType<object>();
                        return string.Join(",", array.Select(o => ConvertToString(o, cultureInfo)));
                    }

                    break;
                }
            }

            return Convert.ToString(value, cultureInfo);
        }
        
        private UnityWebRequest CreateUnityWebRequestWithGet(Uri uri, string contentType, DownloadHandlerBuffer downloadHandlerBuffer, UploadHandlerRaw uploadHandlerRaw = null)
        {
            var unityWebRequest = new UnityWebRequest(uri, "GET");
            unityWebRequest.SetRequestHeader("Content-Type", contentType);
            if(uploadHandlerRaw != null)
            {
                unityWebRequest.uploadHandler = uploadHandlerRaw;
            }
            
            unityWebRequest.downloadHandler = downloadHandlerBuffer;
            
            return unityWebRequest;
        }
        
        private UploadHandler CreateUploadHandler(string body, string contentType)
        {
            var requestBytes = Encoding.UTF8.GetBytes(body); 
            var uploadHandler = new UploadHandlerRaw(requestBytes);
            uploadHandler.contentType = contentType;
            return uploadHandler;
        }
        
        private IEnumerator SendRequest(UnityWebRequest unityRequest)
        {
            Debug.Log($"DEBUG Send Request: {DateTime.Now:HH:mm:ss.fff}");
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
                var serializer = JsonSerializer.Create(_jsonSettings.Value);
                var typedBody = serializer.Deserialize<T>(jsonTextReader);
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

