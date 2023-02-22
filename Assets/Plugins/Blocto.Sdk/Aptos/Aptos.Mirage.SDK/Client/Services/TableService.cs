using System.Collections.Generic;
using System.Threading.Tasks;
using Mirage.Aptos.SDK.DTO;

namespace Mirage.Aptos.SDK.Services
{
	public class TableService : BaseService
	{
		private const string Root = "/tables";
		private const string LedgerVersionField = "ledger_version";

		private readonly string _getTableItemRoute = $@"{Root}/{{0}}/item";

		public TableService(OpenAPIConfig config) : base(config)
		{
		}

		public Task<TReturn> GetTableItem<TReturn>(
			string tableHandle,
			TableItemRequest requestBody,
			ulong? ledgerVersion = null
		)
		{
			var url = URL + string.Format(_getTableItemRoute, tableHandle);
			Dictionary<string, string> query = null;
			if (ledgerVersion != null)
			{
				query = new Dictionary<string, string>
				{
					{ LedgerVersionField, ledgerVersion.ToString() }
				};
			}

			return WebHelper.SendPostRequest<TableItemRequest, TReturn>(url, requestBody, query: query);
		}
	}
}