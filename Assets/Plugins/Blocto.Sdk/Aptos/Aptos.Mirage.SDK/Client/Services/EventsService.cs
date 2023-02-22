using System.Collections.Generic;
using System.Threading.Tasks;
using Mirage.Aptos.SDK.DTO;
using Mirage.Aptos.SDK.DTO.ResponsePayloads;

namespace Mirage.Aptos.SDK.Services
{
	public class EventsService : BaseService
	{
		private const string Root = "/accounts";
		private const string StartFieldName = "start";
		private const string LimitFieldName = "limit";

		private readonly string _getEventsByCreationNumberRoute = $@"{Root}/{{0}}/events/{{1}}";
		private readonly string _getEventsByEventHandleRoute = $@"{Root}/{{0}}/events/{{1}}/{{2}}";

		public EventsService(OpenAPIConfig config) : base(config)
		{
		}

		public Task<VersionedEvent> GetEventsByCreationNumber(
			string address,
			ulong creationNumber,
			ulong? start = null,
			ulong? limit = null
		)
		{
			var url = URL + string.Format(_getEventsByCreationNumberRoute, address, creationNumber);
			Dictionary<string, string> query = null;
			if (start != null && limit != null)
			{
				query = new Dictionary<string, string>
				{
					{ StartFieldName, start.ToString() },
					{ LimitFieldName, limit.ToString() }
				};
			}

			return WebHelper.SendGetRequest<VersionedEvent>(url, query: query);
		}
		
		public Task<VersionedEvent> GetEventsByEventHandle(
			string address,
			string eventHandle,
			string fieldName,
			ulong? start = null,
			ulong? limit = null
		)
		{
			var url = URL + string.Format(_getEventsByEventHandleRoute, address, eventHandle, fieldName);
			Dictionary<string, string> query = null;
			if (start != null && limit != null)
			{
				query = new Dictionary<string, string>
				{
					{ StartFieldName, start.ToString() },
					{ LimitFieldName, limit.ToString() }
				};
			}

			return WebHelper.SendGetRequest<VersionedEvent>(url, query: query);
		}
	}
}