using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Blocto.Flow.Client.Http;
using Flow.Net.SDK.Client.Http.Models.Apis;
using Flow.Net.SDK.Client.Http.Models.Enums;
using Newtonsoft.Json;
using UnityEngine.Networking;
using UnityEngine;
// ReSharper disable TooManyArguments
// ReSharper disable MethodTooLong

namespace Flow.Net.SDK.Client.Http.Unity
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
            
            var client = CreateUnityWebRequestWithGet(urlBuilder, "application/json", new DownloadHandlerBuffer());
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
        }
        
        /// <summary>Get Blocks by ID.</summary>
        /// <param name="id">A block ID or comma-separated list of block IDs.</param>
        /// <param name="expand">A comma-separated list indicating which properties of the content to expand.</param>
        /// <param name="select">A comma-separated list indicating which properties of the content to return.</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public ICollection<Block> Blocks(IEnumerable<string> id, IEnumerable<string> expand, IEnumerable<string> select)
        {
            return Blocks(id, expand, select, System.Threading.CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Get Blocks by ID.</summary>
        /// <param name="id">A block ID or comma-separated list of block IDs.</param>
        /// <param name="expand">A comma-separated list indicating which properties of the content to expand.</param>
        /// <param name="select">A comma-separated list indicating which properties of the content to return.</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public ICollection<Block> Blocks(IEnumerable<string> id, IEnumerable<string> expand, IEnumerable<string> select, System.Threading.CancellationToken cancellationToken)
        {
            if (id == null)
                throw new ArgumentNullException("id");

            var urlBuilder = new StringBuilder();
            urlBuilder.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/blocks/{id}?");
            urlBuilder.Replace("{id}", Uri.EscapeDataString(string.Join(",", id.Select(s_ => ConvertToString(s_, CultureInfo.InvariantCulture)))));
            IsAppendQueryWithExpand(expand, urlBuilder);
            IsAppendQueryWithSelect(select, urlBuilder);
            urlBuilder.Length--;

            var client = CreateUnityWebRequestWithGet(urlBuilder, "application/json", new DownloadHandlerBuffer());
            try
            {
                var result = ProcessWebRequest<ICollection<Block>>(client);
                return result;
            }
            finally
            {
                client?.Dispose();
            }
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

            var client = CreateUnityWebRequestWithGet(urlBuilder, "application/json", new DownloadHandlerBuffer());
            try
            {
                var result = ProcessWebRequest<Transaction>(client);
                return result;
            }
            finally
            {
                client?.Dispose();
            }
        }
        
        /// <summary>Submit a Transaction</summary>
        /// <param name="body">The transaction to submit.</param>
        /// <returns>Created</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public Transaction SendTransaction(TransactionBody body)
        {
            return SendTransaction(body, System.Threading.CancellationToken.None);
        }
        
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Submit a Transaction</summary>
        /// <param name="body">The transaction to submit.</param>
        /// <returns>Created</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public Transaction SendTransaction(TransactionBody body, System.Threading.CancellationToken cancellationToken)
        {
            var urlBuilder = new StringBuilder();
            urlBuilder.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/transactions");

            var client = CreateUnityWebRequestWithGet(urlBuilder, "application/json", new DownloadHandlerBuffer());
            try
            {
                var result = ProcessWebRequest<Transaction>(client);
                return result; 
            }
            finally
            {
                client?.Dispose();
            }
        }
        
        /// <summary>Gets a Collection by ID</summary>
        /// <param name="id">The collection ID.</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public Collection Collections(string id, IEnumerable<string> expand)
        {
            return Collections(id, expand, System.Threading.CancellationToken.None);
        }
        
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Gets a Collection by ID</summary>
        /// <param name="id">The collection ID.</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public Collection Collections(string id, IEnumerable<string> expand, System.Threading.CancellationToken cancellationToken)
        {
            if (id == null)
                throw new ArgumentNullException("id");

            var urlBuilder = new StringBuilder();
            urlBuilder.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/collections/{id}?");
            urlBuilder.Replace("{id}", Uri.EscapeDataString(ConvertToString(id, CultureInfo.InvariantCulture)));
            IsAppendQueryWithExpand(expand, urlBuilder);
            urlBuilder.Length--;

            var client = CreateUnityWebRequestWithGet(urlBuilder, "application/json", new DownloadHandlerBuffer());
            try
            {
                var result = ProcessWebRequest<Collection>(client);
                return result;
            }
            finally
            {
                client?.Dispose();
            }
        }
        
        /// <summary>Get an Account By Address</summary>
        /// <param name="address">The address of the account.</param>
        /// <param name="block_height">The block height to query for the account details at the "sealed" is used by default.</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public Account Accounts(string address, string block_height, IEnumerable<string> expand)
        {
            return Accounts(address, block_height, expand, System.Threading.CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Get an Account By Address</summary>
        /// <param name="address">The address of the account.</param>
        /// <param name="block_height">The block height to query for the account details at the "sealed" is used by default.</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public Account Accounts(string address, string block_height, IEnumerable<string> expand, System.Threading.CancellationToken cancellationToken)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            var urlBuilder = new StringBuilder();
            urlBuilder.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/accounts/{address}?");
            urlBuilder.Replace("{address}", Uri.EscapeDataString(ConvertToString(address, CultureInfo.InvariantCulture)));
            IsAppendQueryWithBlockHeight(block_height, urlBuilder);
            IsAppendQueryWithExpand(expand, urlBuilder);
            urlBuilder.Length--;

            var client = CreateUnityWebRequestWithGet(urlBuilder, "application/json", new DownloadHandlerBuffer());
            try
            {
                var result = ProcessWebRequest<Account>(client);
                return result;
            }
            finally
            {
                client?.Dispose();
            }
        }

        /// <summary>Execute a Cadence Script</summary>
        /// <param name="block_id">The ID of the block to execute the script against. For a specific block height, use `block_height` instead.</param>
        /// <param name="block_height">The height of the block to execute the script against. This parameter is incompatible with `block_id`.</param>
        /// <param name="body">The script to execute.</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public Response Scripts(string block_id, string block_height, ScriptBody body)
        {
            return Scripts(block_id, block_height, body, System.Threading.CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Execute a Cadence Script</summary>
        /// <param name="block_id">The ID of the block to execute the script against. For a specific block height, use `block_height` instead.</param>
        /// <param name="block_height">The height of the block to execute the script against. This parameter is incompatible with `block_id`.</param>
        /// <param name="body">The script to execute.</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public Response Scripts(string block_id, string block_height, ScriptBody body, System.Threading.CancellationToken cancellationToken)
        {
            var urlBuilder = new StringBuilder();
            urlBuilder.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/scripts?");
            IsAppendQueryWithBlockId(block_id, urlBuilder);
            IsAppendQueryWithBlockHeight(block_height, urlBuilder);
            urlBuilder.Length--;

            var client = CreateUnityWebRequestWithGet(urlBuilder, "application/json", new DownloadHandlerBuffer());
            try
            {
                var result = ProcessWebRequest<Response>(client);
                return result;
            }
            finally
            {
                client?.Dispose();
            }
        }
        
        /// <summary>Get Events</summary>
        /// <param name="type">The event type is [identifier of the event as defined here](https://docs.onflow.org/core-contracts/flow-token/#events).</param>
        /// <param name="start_height">The start height of the block range for events. Must be used together with `end_height`. This parameter is incompatible with `block_ids`.</param>
        /// <param name="end_height">The end height of the block range for events. Must be used together with `start_height`. This parameter is incompatible with `block_ids`.</param>
        /// <param name="block_ids">List of block IDs. Either provide this parameter or both height parameters. This parameter is incompatible with heights parameters.</param>
        /// <param name="select">A comma-separated list indicating which properties of the content to return.</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public ICollection<BlockEvents> Events(string type, string start_height, string end_height, IEnumerable<string> block_ids, IEnumerable<string> select)
        {
            return Events(type, start_height, end_height, block_ids, select, System.Threading.CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Get Events</summary>
        /// <param name="type">The event type is [identifier of the event as defined here](https://docs.onflow.org/core-contracts/flow-token/#events).</param>
        /// <param name="start_height">The start height of the block range for events. Must be used together with `end_height`. This parameter is incompatible with `block_ids`.</param>
        /// <param name="end_height">The end height of the block range for events. Must be used together with `start_height`. This parameter is incompatible with `block_ids`.</param>
        /// <param name="block_ids">List of block IDs. Either provide this parameter or both height parameters. This parameter is incompatible with heights parameters.</param>
        /// <param name="select">A comma-separated list indicating which properties of the content to return.</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public ICollection<BlockEvents> Events(string type, string start_height, string end_height, IEnumerable<string> block_ids, IEnumerable<string> select, System.Threading.CancellationToken cancellationToken)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            var urlBuilder = new StringBuilder();
            urlBuilder.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/events?");
            urlBuilder.Append(Uri.EscapeDataString("type") + "=").Append(Uri.EscapeDataString(ConvertToString(type, CultureInfo.InvariantCulture))).Append("&");
            IsAppendQueryWithStartHeight(start_height, urlBuilder);
            IsAppendQueryWithEndHeight(end_height, urlBuilder);
            IsAppendQueryWithBlockIds(block_ids, urlBuilder);
            IsAppendQueryWithSelect(select, urlBuilder);
            urlBuilder.Length--;

            var client = CreateUnityWebRequestWithGet(urlBuilder, "application/json", new DownloadHandlerBuffer());
            try
            {
                var result = ProcessWebRequest<ICollection<BlockEvents>>(client);
                return result;
            }
            finally
            {
                client?.Dispose();
            }
        }

        /// <summary>Get a Transaction Result by ID.</summary>
        /// <param name="transaction_id">The transaction ID of the transaction result.</param>
        /// <param name="expand">A comma-separated list indicating which properties of the content to expand.</param>
        /// <param name="select">A comma-separated list indicating which properties of the content to return.</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public TransactionResult Results(string transaction_id, IEnumerable<string> expand, IEnumerable<string> select)
        {
            return Results(transaction_id, expand, select, System.Threading.CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Get a Transaction Result by ID.</summary>
        /// <param name="transaction_id">The transaction ID of the transaction result.</param>
        /// <param name="expand">A comma-separated list indicating which properties of the content to expand.</param>
        /// <param name="select">A comma-separated list indicating which properties of the content to return.</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public TransactionResult Results(string transaction_id, IEnumerable<string> expand, IEnumerable<string> select, System.Threading.CancellationToken cancellationToken)
        {
            if (transaction_id == null)
                throw new ArgumentNullException("transaction_id");

            var urlBuilder = new StringBuilder();
            urlBuilder.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/transaction_results/{transaction_id}?");
            urlBuilder.Replace("{transaction_id}", Uri.EscapeDataString(ConvertToString(transaction_id, CultureInfo.InvariantCulture)));
            IsAppendQueryWithExpand(expand, urlBuilder);
            IsAppendQueryWithSelect(select, urlBuilder);
            urlBuilder.Length--;

            var client = CreateUnityWebRequestWithGet(urlBuilder, "application/json", new DownloadHandlerBuffer());
            try
            {
                var result = ProcessWebRequest<TransactionResult>(client);
                return result;
            }
            finally
            {
                client?.Dispose();
            }
        }
        
        /// <summary>Get Execution Results by Block ID</summary>
        /// <param name="block_id">Single ID or comma-separated list of block IDs.</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public ICollection<ExecutionResult> ResultsAll(IEnumerable<string> block_id)
        {
            return ResultsAll(block_id, System.Threading.CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Get Execution Results by Block ID</summary>
        /// <param name="block_id">Single ID or comma-separated list of block IDs.</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public ICollection<ExecutionResult> ResultsAll(IEnumerable<string> block_id, System.Threading.CancellationToken cancellationToken)
        {
            if (block_id == null)
                throw new ArgumentNullException(nameof(block_id));

            var urlBuilder = new StringBuilder();
            urlBuilder.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/execution_results?");
            IsAppendQueryWithBlockIds(block_id, urlBuilder);
            urlBuilder.Length--;

            var client = CreateUnityWebRequestWithGet(urlBuilder, "application/json", new DownloadHandlerBuffer());
            try
            {
                var result = ProcessWebRequest<ICollection<ExecutionResult>>(client);
                return result;
            }
            finally
            {
                client?.Dispose();
            }
        }
        
        /// <summary>Get Execution Result by ID</summary>
        /// <param name="id">The ID of the execution result.</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public ExecutionResult Results(string id)
        {
            return Results(id, System.Threading.CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Get Execution Result by ID</summary>
        /// <param name="id">The ID of the execution result.</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public ExecutionResult Results(string id, System.Threading.CancellationToken cancellationToken)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var urlBuilder = new StringBuilder();
            urlBuilder.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/execution_results/{id}");
            urlBuilder.Replace("{id}", Uri.EscapeDataString(ConvertToString(id, CultureInfo.InvariantCulture)));

            var client = CreateUnityWebRequestWithGet(urlBuilder, "application/json", new DownloadHandlerBuffer());;
            try
            {
                var result = ProcessWebRequest<ExecutionResult>(client);
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

        /// <summary>
        /// Confirm whether need to append select query parameter
        /// </summary>
        /// <param name="select">selects parameter</param>
        /// <param name="urlBuilder">Url string builder</param>
        private void IsAppendQueryWithSelect(IEnumerable<string> select, StringBuilder urlBuilder)
        {
            if (select == null)
            {
                return;
            }

            urlBuilder.Append(Uri.EscapeDataString("select") + "=");
            foreach (var item_ in select)
            {
                urlBuilder.Append(Uri.EscapeDataString(ConvertToString(item_, CultureInfo.InvariantCulture))).Append(",");
            }

            urlBuilder.Length--;
            urlBuilder.Append("&");
        }
        
        /// <summary>
        /// Confirm whether need to append expand query parameter 
        /// </summary>
        /// <param name="expand">Expand parameter</param>
        /// <param name="urlBuilder">Url string builder</param>
        private void IsAppendQueryWithExpand(IEnumerable<string> expand, StringBuilder urlBuilder)
        {
            if (expand == null)
            {
                return;
            }

            urlBuilder.Append(Uri.EscapeDataString("expand") + "=");
            foreach (var item_ in expand)
            {
                urlBuilder.Append(Uri.EscapeDataString(ConvertToString(item_, CultureInfo.InvariantCulture))).Append(",");
            }

            urlBuilder.Length--;
            urlBuilder.Append("&");
        }

        /// <summary>
        /// Confirm whether need to append end height query parameter 
        /// </summary>
        /// <param name="end_height">end height parameter</param>
        /// <param name="urlBuilder">Url string builder</param>
        private void IsAppendQueryWithEndHeight(string end_height, StringBuilder urlBuilder)
        {
            if (end_height != null)
            {
                urlBuilder.Append(Uri.EscapeDataString("end_height") + "=").Append(Uri.EscapeDataString(ConvertToString(end_height, CultureInfo.InvariantCulture))).Append("&");
            }
        }

        /// <summary>
        /// Confirm whether need to append start height query parameter 
        /// </summary>
        /// <param name="start_height">start height parameter</param>
        /// <param name="urlBuilder">Url string builder</param>
        private void IsAppendQueryWithStartHeight(string start_height, StringBuilder urlBuilder)
        {
            if (start_height != null)
            {
                urlBuilder.Append(Uri.EscapeDataString("start_height") + "=").Append(Uri.EscapeDataString(ConvertToString(start_height, CultureInfo.InvariantCulture))).Append("&");
            }
        }

        /// <summary>
        /// Confirm whether need to append height query parameter 
        /// </summary>
        /// <param name="height">height parameter</param>
        /// <param name="urlBuilder">Url string builder</param>
        private void IsAppendQueryWithHeight(IEnumerable<string> height, StringBuilder urlBuilder)
        {
            if (height == null)
            {
                return;
            }

            urlBuilder.Append(Uri.EscapeDataString("height") + "=");
            foreach (var item_ in height)
            {
                urlBuilder.Append(Uri.EscapeDataString(ConvertToString(item_, CultureInfo.InvariantCulture))).Append(",");
            }

            urlBuilder.Length--;
            urlBuilder.Append("&");
        }

        /// <summary>
        /// Confirm whether need to append block height query parameter 
        /// </summary>
        /// <param name="block_height">block height parameter</param>
        /// <param name="urlBuilder">Url string builder</param>
        private void IsAppendQueryWithBlockHeight(string block_height, StringBuilder urlBuilder)
        {
            if (block_height != null)
            {
                urlBuilder.Append(Uri.EscapeDataString("block_height") + "=").Append(Uri.EscapeDataString(ConvertToString(block_height, CultureInfo.InvariantCulture))).Append("&");
            }
        }

        /// <summary>
        /// Confirm whether need to append block id query parameter 
        /// </summary>
        /// <param name="block_id">block id parameter</param>
        /// <param name="urlBuilder">Url string builder</param>
        private void IsAppendQueryWithBlockId(string block_id, StringBuilder urlBuilder)
        {
            if (block_id != null)
            {
                urlBuilder.Append(Uri.EscapeDataString("block_id") + "=").Append(Uri.EscapeDataString(ConvertToString(block_id, CultureInfo.InvariantCulture))).Append("&");
            }
        }
        
        /// <summary>
        /// Confirm whether need to append block ids query parameter 
        /// </summary>
        /// <param name="block_ids">block ids parameter</param>
        /// <param name="urlBuilder">Url string builder</param>
        private void IsAppendQueryWithBlockIds(IEnumerable<string> block_ids, StringBuilder urlBuilder)
        {
            if (block_ids == null)
            {
                return;
            }

            foreach (var item_ in block_ids)
            {
                urlBuilder.Append(Uri.EscapeDataString("block_ids") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, CultureInfo.InvariantCulture))).Append("&");
            }
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
        
        private UnityWebRequest CreateUnityWebRequestWithGet(StringBuilder urlBuilder, string contentType, DownloadHandlerBuffer downloadHandlerBuffer, UploadHandlerRaw uploadHandlerRaw = null)
        {
            var uri = new Uri(urlBuilder.ToString(), UriKind.RelativeOrAbsolute);
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

