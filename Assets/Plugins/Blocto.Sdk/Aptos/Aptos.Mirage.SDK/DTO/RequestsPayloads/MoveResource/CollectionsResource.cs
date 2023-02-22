using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mirage.Aptos.SDK.DTO
{
	public class CollectionsResource
	{
		[JsonProperty(PropertyName = "collection_data")]
		public DataStorePointer CollectionData;
		[JsonProperty(PropertyName = "create_collection_events")]
		public ResourceEvent CreateCollectionEvents;
		[JsonProperty(PropertyName = "create_token_data_events")]
		public ResourceEvent CreateTokenDataEvents;
		[JsonProperty(PropertyName = "mint_token_events")]
		public ResourceEvent MintTokenEvents;
		[JsonProperty(PropertyName = "token_data")]
		public DataStorePointer TokenData;
	}
}