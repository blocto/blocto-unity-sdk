using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Mirage.Aptos.SDK.DTO;
using Newtonsoft.Json;

namespace Mirage.Aptos.SDK
{
	public static class WebHelper
	{
		private const string MimeType = "application/json";

		public static Task<TResultType> SendPostRequest<TPayloadType, TResultType>(
			string url,
			TPayloadType payload,
			Dictionary<string, string> headers = null,
			Dictionary<string, string> query = null
		)
		{
			var payloadJson = JsonConvert.SerializeObject(payload);

			return SendChangeRequest<TResultType>(url, payloadJson, headers, query);
		}

		public static Task<TResultType> SendPostRequest<TResultType>(
			string url,
			Dictionary<string, string> headers = null,
			Dictionary<string, string> query = null
		)
		{
			return SendChangeRequest<TResultType>(url, string.Empty, headers, query);
		}
		
		public static async Task<TResultType> SendGetRequest<TResultType>(
			string url,
			Dictionary<string, string> headers = null,
			Dictionary<string, string> query = null,
			string wrapper = null
		)
		{
			using (var request = new HttpClient())
			{
				if (headers != null)
				{
					AddHeaders(request, headers);
				}

				if (query != null)
				{
					url = BuildURL(url, query);
				}

				request.Timeout = TimeSpan.FromSeconds(20);
				
				var answer = await request.GetAsync(url);

				var retValue = await ParseAnswer<TResultType>(answer, wrapper);
				
				return retValue;
			}
		}

		private static async Task<TResultType> SendChangeRequest<TResultType>(
			string url,
			string payload,
			Dictionary<string, string> headers = null,
			Dictionary<string, string> query = null,
			string wrapper = null
		)
		{
			var content = new StringContent(payload);
			content.Headers.ContentType = new MediaTypeHeaderValue(MimeType);

			using (var client = new HttpClient())
			{
				if (headers != null)
				{
					AddHeaders(client, headers);
				}

				if (query != null)
				{
					url = BuildURL(url, query);
				}

				client.Timeout = TimeSpan.FromSeconds(20);

				var answer = await client.PostAsync(url, content);

				var retValue = await ParseAnswer<TResultType>(answer, wrapper);
				
				return retValue;
			}
		}

		private static Task<TResultType> ParseAnswer<TResultType>(HttpResponseMessage answer, string wrapper = null)
		{
			if (wrapper != null)
			{
				return ParseStringAnswer<TResultType>(answer, wrapper);
			}
			else
			{
				return ParseStreamAnswer<TResultType>(answer);
			}
		}
		
		private static async Task<TResultType> ParseStringAnswer<TResultType>(HttpResponseMessage answer, string wrapper)
		{
			var json = await answer.Content.ReadAsStringAsync();
			if (answer.IsSuccessStatusCode)
			{
				try
				{
					var jsonPayload = json;
					if (wrapper != null)
					{
						jsonPayload = string.Format(wrapper, json);
					}

					var result = JsonConvert.DeserializeObject<TResultType>(jsonPayload);
					return result;
				}
				catch (Exception e)
				{
					Console.WriteLine($"Error while deserializing response: {e.Message}");
					throw e;
				}
			}
			else
			{
				if (string.IsNullOrEmpty(json))
				{
					throw new InvalidOperationException("Unexpected exception: " + json);
				}
				else
				{
					var error = JsonConvert.DeserializeObject<Error>(json);
					Console.WriteLine(AptosException.CreateMessage(error.Message, error.ErrorCode,
						error.VmErrorCode));
					throw new AptosException(error.Message, error.ErrorCode, error.VmErrorCode);
				}
			}
		}
		
		private static async Task<TResultType> ParseStreamAnswer<TResultType>(HttpResponseMessage answer)
		{
			var stream = await answer.Content.ReadAsStreamAsync();
			var streamReader = new StreamReader(stream);
			var jsonReader = new JsonTextReader(streamReader);

			JsonSerializer serializer = new JsonSerializer();

			if (answer.IsSuccessStatusCode)
			{
				try
				{
					return serializer.Deserialize<TResultType>(jsonReader);
				}
				catch (Exception e)
				{
					Console.WriteLine($"Error while deserializing response: {e.Message}");
					throw e;
				}
			}
			else
			{
				if (streamReader.Peek() == -1)
				{
					throw new InvalidOperationException("Unexpected exception: " + streamReader.ReadToEnd());
				}

				var error = serializer.Deserialize<Error>(jsonReader);
				Console.WriteLine(AptosException.CreateMessage(error.Message, error.ErrorCode,
					error.VmErrorCode));
				throw new AptosException(error.Message, error.ErrorCode, error.VmErrorCode);
			}
		}

		private static string BuildURL(string url, Dictionary<string, string> query)
		{
			var queryBuilder = HttpUtility.ParseQueryString(string.Empty);

			foreach (var part in query)
			{
				queryBuilder[part.Key] = part.Value;
			}

			return $"{url}?{queryBuilder}";
		}

		private static void AddHeaders(HttpClient request, Dictionary<string, string> headers)
		{
			foreach (var entryHeader in headers)
			{
				request.DefaultRequestHeaders.Add(entryHeader.Key, entryHeader.Value);
			}
		}
	}
}