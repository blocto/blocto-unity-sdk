using System.Threading.Tasks;
using Mirage.Aptos.SDK.DTO;

namespace Mirage.Aptos.SDK.Services
{
	public class FaucetService
	{
		private const string FundAccountRoute = "/mint";

		private readonly string _url;
		
		public FaucetService(string faucetUrl)
		{
			_url = faucetUrl;
		}

		public Task<IndexResponse> FundAccount(Account account, uint amount)
		{
			return WebHelper.SendPostRequest<IndexResponse>(_url + FundAccountRoute);
		}
	}
}