using System;
using System.Threading.Tasks;
using Mirage.Aptos.SDK.Constants;
using Mirage.Aptos.SDK.DTO;
using Mirage.Aptos.Constants;
using Mirage.Aptos.SDK.DTO.ResponsePayloads;

namespace Mirage.Aptos.SDK
{
	public class TokenClient : SpecificClient
	{
		/// <summary>
		/// Creates new TokenClient instance.
		/// </summary>
		/// <param name="client"><see cref="Client"/> instance.</param>
		public TokenClient(Client client) : base(client, ABIs.GetTokenABIs())
		{
		}

		/// <summary>
		/// Creates a new NFT collection within the specified account.
		/// </summary>
		/// <param name="account">Account where collection will be created.</param>
		/// <param name="name">Collection name.</param>
		/// <param name="description">Collection description.</param>
		/// <param name="uri">URL to additional info about collection.</param>
		/// <param name="maxAmount">Maximum number of `token_data` allowed within this collection.</param>
		/// <param name="extraArgs">Extra args for checking the balance.</param>
		/// <returns>The transaction submitted to the API.</returns>
		public Task<PendingTransactionPayload> CreateCollection(
			Account account,
			string name,
			string description,
			string uri,
			long maxAmount = UInt32.MaxValue,
			OptionalTransactionArgs extraArgs = null
		)
		{
			var payload = new EntryFunctionPayload
			{
				Type = TransactionPayloadTypes.EntryFunction,
				Function = FunctionTypes.CreateCollectionScript,
				TypeArguments = Array.Empty<string>(),
				Arguments = new object[]
					{ name, description, uri, maxAmount.ToString(), new bool[] { false, false, false } }
			};

			return SubmitSignedTransaction(account, payload, extraArgs);
		}

		/// <summary>
		/// Creates a new NFT collection within the specified account.
		/// </summary>
		/// <param name="account">Account where token will be created.</param>
		/// <param name="collectionName">Name of collection, that token belongs to.</param>
		/// <param name="name">Token name.</param>
		/// <param name="description">Token description.</param>
		/// <param name="supply">Token supply.</param>
		/// <param name="uri">URL to additional info about token.</param>
		/// <param name="max">The maxium of tokens can be minted from this token.</param>
		/// <param name="royaltyPayeeAddress">The address to receive the royalty, the address can be a shared account address.</param>
		/// <param name="royaltyPointsDenominator">The denominator for calculating royalty.</param>
		/// <param name="royaltyPointsNumerator">The numerator for calculating royalty.</param>
		/// <param name="propertyKeys">The property keys for storing on-chain properties.</param>
		/// <param name="propertyValues">The property values to be stored on-chain.</param>
		/// <param name="propertyTypes">The type of property values.</param>
		/// <param name="extraArgs">Extra args for checking the balance.</param>
		/// <returns>The transaction submitted to the API.</returns>
		public Task<PendingTransactionPayload> CreateToken(
			Account account,
			string collectionName,
			string name,
			string description,
			ulong supply,
			string uri,
			ulong max = UInt64.MaxValue,
			string royaltyPayeeAddress = null,
			int royaltyPointsDenominator = 0,
			int royaltyPointsNumerator = 0,
			string[] propertyKeys = null,
			string[] propertyValues = null,
			string[] propertyTypes = null,
			OptionalTransactionArgs extraArgs = null
		)
		{
			var payload = CreateTokenPayload(
				account,
				collectionName,
				name,
				description,
				supply,
				uri,
				max,
				royaltyPayeeAddress,
				royaltyPointsDenominator,
				royaltyPointsNumerator,
				propertyKeys,
				propertyValues,
				propertyTypes
			);

			return SubmitSignedTransaction(account, payload, extraArgs);
		}

		/// <summary>
		/// Transfers specified amount of tokens from account to receiver.
		/// </summary>
		/// <param name="account">Account where token from which tokens will be transfered.</param>
		/// <param name="receiver">Hex-encoded 32 byte Aptos account address to which tokens will be transfered.</param>
		/// <param name="creator">Hex-encoded 32 byte Aptos account address to which created tokens.</param>
		/// <param name="collectionName">Name of collection where token is stored.</param>
		/// <param name="name">Token name.</param>
		/// <param name="amount">Amount of tokens which will be transfered.</param>
		/// <param name="propertyVersion">The version of token PropertyMap with a default value 0.</param>
		/// <param name="extraArgs">Extra args for checking the balance.</param>
		/// <returns>The transaction submitted to the API.</returns>
		public Task<PendingTransactionPayload> OfferToken(
			Account account,
			string receiver,
			string creator,
			string collectionName,
			string name,
			long amount,
			long propertyVersion = 0,
			OptionalTransactionArgs extraArgs = null
		)
		{
			var payload = new EntryFunctionPayload
			{
				Type = TransactionPayloadTypes.EntryFunction,
				Function = FunctionTypes.OfferScript,
				TypeArguments = Array.Empty<string>(),
				Arguments = new object[]
					{ receiver, creator, collectionName, name, propertyVersion.ToString(), amount.ToString() }
			};

			return SubmitSignedTransaction(account, payload, extraArgs);
		}

		/// <summary>
		/// Claims a token on specified account.
		/// </summary>
		/// <param name="account">Account which will claim token.</param>
		/// <param name="sender">Hex-encoded 32 byte Aptos account address which holds a token.</param>
		/// <param name="creator">Hex-encoded 32 byte Aptos account address which created a token.</param>
		/// <param name="collectionName">Name of collection where token is stored.</param>
		/// <param name="name">Token name.</param>
		/// <param name="propertyVersion">The version of token PropertyMap with a default value 0.</param>
		/// <param name="extraArgs">Extra args for checking the balance.</param>
		/// <returns>The transaction submitted to the API.</returns>
		public Task<PendingTransactionPayload> ClaimToken(
			Account account,
			string sender,
			string creator,
			string collectionName,
			string name,
			long propertyVersion = 0,
			OptionalTransactionArgs extraArgs = null
		)
		{
			var payload = new EntryFunctionPayload
			{
				Type = TransactionPayloadTypes.EntryFunction,
				Function = FunctionTypes.ClaimScript,
				TypeArguments = Array.Empty<string>(),
				Arguments = new object[]
					{ sender, creator, collectionName, name, propertyVersion.ToString() }
			};

			return SubmitSignedTransaction(account, payload, extraArgs);
		}

		public async Task<CollectionPayload> GetCollectionData(string creator, string collectionName)
		{
			var collections = await _client.GetAccountResource(creator, ResourcesTypes.Collections);
			var collectionData = collections.Data.ToObject<CollectionsResource>();
			var handle = collectionData.CollectionData.Handle;

			var request = new TableItemRequest
			{
				KeyType = KeyTypes.String,
				ValueType = ValueTypes.CollectionData,
				Key = collectionName
			};

			return await _client.GetTableItem<CollectionPayload>(handle, request);
		}

		public async Task<TokenPayload> GetTokenData(string creator, string collectionName, string tokenName)
		{
			var collections = await _client.GetAccountResource(creator, ResourcesTypes.Collections);
			var collectionData = collections.Data.ToObject<CollectionsResource>();
			var handle = collectionData.TokenData.Handle;

			var request = new TableItemRequest
			{
				KeyType = KeyTypes.TokenDataId,
				ValueType = ValueTypes.TokenData,
				Key = new TokenDataId
				{
					Creator = creator,
					Collection = collectionName,
					Name = tokenName
				}
			};

			var tableItem = await _client.GetTableItem<TokenPayload>(handle, request);

			return tableItem;
		}

		public Task<TokenFromAccount> GetToken(
			string creator,
			string collectionName,
			string tokenName,
			long propertyVersion
		)
		{
			var tokenId = new TokenId
			{
				TokenDataId = new TokenDataId
				{
					Creator = creator,
					Collection = collectionName,
					Name = tokenName
				},
				PropertyVersion = propertyVersion.ToString()
			};

			return GetTokenForAccount(creator, tokenId);
		}

		public async Task<TokenFromAccount> GetTokenForAccount(string creator, TokenId tokenId)
		{
			TokenResource collectionData = null;
			try
			{
				var collections = await _client.GetAccountResource(creator, ResourcesTypes.TokenStore);
				collectionData = collections.Data.ToObject<TokenResource>();
			}
			catch (AptosException e)
			{
				if (e.ErrorCode == "resource_not_found")
				{
					return CreateEmptyToken(tokenId);
				}
			}

			var handle = collectionData.Tokens.Handle;
			Console.WriteLine($"handle = {handle}");

			var request = new TableItemRequest
			{
				KeyType = KeyTypes.TokenId,
				ValueType = ValueTypes.Token,
				Key = tokenId
			};

			try
			{
				return await _client.GetTableItem<TokenFromAccount>(handle, request);
			}
			catch (AptosException e)
			{
				Console.WriteLine(e.ErrorCode);
				if (e.ErrorCode == "table_item_not_found")
				{
					return CreateEmptyToken(tokenId);
				}

				throw;
			}
		}
		
		/// <summary>
		/// Generates and submits a transaction to the transaction simulation
		/// endpoint. For this we generate a transaction with a fake signature.
		/// </summary>
		/// <param name="account">Account which will claim transaction.</param>
		/// <param name="payload">Transaction payload.</param>
		/// <param name="extraArgs">Extra args for checking the balance.</param>
		/// <param name="estimateMaxGasAmount">If set to true, the max gas value in the transaction will be ignored and the maximum possible gas will be used.</param>
		/// <param name="estimateGasUnitPrice">If set to true, the gas unit price in the transaction will be ignored and the estimated value will be used.</param>
		/// <param name="estimatePrioritizedGasUnitPrice">If set to true, the transaction will use a higher price than the original estimate.</param>
		/// <returns>The BCS encoded signed transaction, which you should then provide.</returns>
		public async Task<UserTransaction> SimulateTransaction(
			Account account,
			EntryFunctionPayload payload,
			OptionalTransactionArgs extraArgs = null,
			bool? estimateMaxGasAmount = null,
			bool? estimateGasUnitPrice = null,
			bool? estimatePrioritizedGasUnitPrice = null
		)
		{
			var transaction = await PrepareTransaction(account, payload, extraArgs);

			var raw = transaction.GetRaw();
			var signature = _signatureBuilder.GetSimulatedSignature(account, raw);
			var request = transaction.GetRequest(payload, signature);

			var simulatedTransaction = await _client.SimulateTransaction(
				request,
				estimateMaxGasAmount,
				estimateGasUnitPrice,
				estimatePrioritizedGasUnitPrice
			);

			return simulatedTransaction;
		}

		private TokenFromAccount CreateEmptyToken(TokenId tokenId)
		{
			return new TokenFromAccount
			{
				Id = tokenId,
				Amount = "0"
			};
		}

		private async Task<PendingTransactionPayload> SubmitSignedTransaction(
			Account account,
			EntryFunctionPayload payload,
			OptionalTransactionArgs extraArgs = null
		)
		{
			var transaction = await PrepareTransaction(account, payload, extraArgs);

			var raw = transaction.GetRaw();
			var signature = _signatureBuilder.GetSignature(account, raw);
			var request = transaction.GetRequest(payload, signature);

			var receipt = await _client.SubmitTransaction(request);

			return receipt;
		}

		private EntryFunctionPayload CreateTokenPayload(
			Account account,
			string collectionName,
			string name,
			string description,
			ulong supply,
			string uri,
			ulong max = UInt64.MaxValue,
			string royaltyPayeeAddress = null,
			int royaltyPointsDenominator = 0,
			int royaltyPointsNumerator = 0,
			string[] propertyKeys = null,
			string[] propertyValues = null,
			string[] propertyTypes = null
		)
		{
			if (royaltyPayeeAddress == null)
			{
				royaltyPayeeAddress = account.Address;
			}

			if (propertyKeys == null)
			{
				propertyKeys = Array.Empty<string>();
			}

			if (propertyValues == null)
			{
				propertyValues = Array.Empty<string>();
			}

			if (propertyTypes == null)
			{
				propertyTypes = Array.Empty<string>();
			}

			return new EntryFunctionPayload
			{
				Type = TransactionPayloadTypes.EntryFunction,
				Function = FunctionTypes.CreateTokenScript,
				TypeArguments = Array.Empty<string>(),
				Arguments = new object[]
				{
					collectionName,
					name,
					description,
					supply.ToString(),
					max.ToString(),
					uri,
					royaltyPayeeAddress,
					royaltyPointsDenominator.ToString(),
					royaltyPointsNumerator.ToString(),
					new bool[] { false, false, false, false, false },
					propertyKeys,
					propertyValues,
					propertyTypes
				}
			};
		}
	}
}