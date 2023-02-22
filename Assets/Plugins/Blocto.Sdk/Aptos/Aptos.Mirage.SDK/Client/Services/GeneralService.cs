using System.Threading.Tasks;
using Mirage.Aptos.SDK.DTO;

namespace Mirage.Aptos.SDK.Services
{
	public class GeneralService : BaseService
	{
		private const string GetLedgerInfoRoute = "/";
		
		public GeneralService(OpenAPIConfig config) : base(config)
		{
		}

		public Task<IndexResponse> GetLedgerInfo()
		{
			return WebHelper.SendGetRequest<IndexResponse>(URL + GetLedgerInfoRoute);
		}
	}
}