namespace Mirage.Aptos.SDK.Constants
{
	public static class FunctionTypes
	{
		public const string Transfer = "0x1::coin::transfer";
		public const string CreateCollectionScript = "0x3::token::create_collection_script";
		public const string CreateTokenScript = "0x3::token::create_token_script";
		public const string OfferScript = "0x3::token_transfers::offer_script";
		public const string ClaimScript = "0x3::token_transfers::claim_script";
	}
}