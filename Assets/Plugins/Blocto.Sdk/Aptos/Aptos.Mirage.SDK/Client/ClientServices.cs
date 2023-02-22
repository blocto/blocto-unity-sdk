using Mirage.Aptos.SDK.DTO;
using Mirage.Aptos.SDK.Services;

namespace Mirage.Aptos.SDK
{
	public class ClientServices
	{
		private const string DefaultBase = "https://fullnode.devnet.aptoslabs.com";

		private OpenAPIConfig _config;

		public readonly TransactionsService TransactionsService;
		public readonly GeneralService GeneralService;
		public readonly AccountsService AccountsService;
		public readonly TableService TableService;
		public readonly EventsService EventsService;

		protected static OpenAPIConfig EnhanceConfig(OpenAPIConfig config)
		{
			return new OpenAPIConfig
			{
				Base = config.Base ?? DefaultBase,
				Version = config.Version ?? "v1",
				WithCredentials = config.WithCredentials ?? false,
				Credentials = config.Credentials ?? "include",
				Token = config.Token,
				Username = config.Username,
				Password = config.Password,
				Headers = config.Headers
			};
		}

		public ClientServices(OpenAPIConfig config)
		{
			config = EnhanceConfig(config);
			TransactionsService = new TransactionsService(config);
			GeneralService = new GeneralService(config);
			AccountsService = new AccountsService(config);
			TableService = new TableService(config);
			EventsService = new EventsService(config);
		}
	}
}