using Mirage.Aptos.SDK.DTO;

namespace Mirage.Aptos.SDK.Services
{
	public abstract class BaseService
	{
		protected string URL;
		
		public BaseService(OpenAPIConfig config)
		{
			URL = $"{config.Base}/{config.Version}";
		}
	}
}