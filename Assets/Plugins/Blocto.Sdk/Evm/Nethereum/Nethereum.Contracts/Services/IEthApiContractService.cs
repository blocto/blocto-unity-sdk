﻿using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts.Constants;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.Contracts.CQS;
using Nethereum.Contracts.QueryHandlers.MultiCall;
using Nethereum.Contracts.Standards.ENS;
using Nethereum.Contracts.Standards.ERC1155;
using Nethereum.Contracts.Standards.ERC1271;
using Nethereum.Contracts.Standards.ERC20;
using Nethereum.Contracts.Standards.ERC721;
using Nethereum.Contracts.Standards.ProofOfHumanity;
using Nethereum.RPC;
using Nethereum.RPC.Eth.Transactions;

namespace Nethereum.Contracts.Services
{
    public interface IEthApiContractService: IEthApiService
    {
        IDeployContract DeployContract { get; }
        Contract GetContract(string abi, string contractAddress);
        Contract GetContract<TContractMessage>(string contractAddress);
        Event<TEventType> GetEvent<TEventType>() where TEventType : IEventDTO, new();
        Event<TEventType> GetEvent<TEventType>(string contractAddress) where TEventType : IEventDTO, new();

#if !DOTNET35
        IContractDeploymentTransactionHandler<TContractDeploymentMessage> GetContractDeploymentHandler<TContractDeploymentMessage>() where TContractDeploymentMessage : ContractDeploymentMessage, new();
        ContractHandler GetContractHandler(string contractAddress);
        IContractQueryHandler<TContractFunctionMessage> GetContractQueryHandler<TContractFunctionMessage>() where TContractFunctionMessage : FunctionMessage, new();

        /// <summary>
        /// Creates a multi query handler, to enable execute a single request combining multiple queries to multiple contracts using the multicall contract https://github.com/makerdao/multicall/blob/master/src/Multicall.sol
        /// This is deployed at https://etherscan.io/address/0xcA11bde05977b3631167028862bE2a173976CA11#code
        /// </summary>
        /// <param name="multiContractAdress">The address of the deployed multicall contract</param>
        MultiQueryHandler GetMultiQueryHandler(string multiContractAdress = CommonAddresses.MULTICALL_ADDRESS);
        
        /// <summary>
        /// ERC20 Standard Token Service to interact with smart contracts compliant with the standard interface
        /// https://ethereum.org/en/developers/docs/standards/tokens/erc-20/
        /// </summary>
        ERC20Service ERC20 { get; }
        IContractTransactionHandler<TContractFunctionMessage> GetContractTransactionHandler<TContractFunctionMessage>() where TContractFunctionMessage : FunctionMessage, new();
        IEthGetContractTransactionErrorReason GetContractTransactionErrorReason { get; }
        /// <summary>
        /// ERC721 NFT - Non Fungible Token Standard Service to interact with smart contracts compliant with the standard interface
        /// https://ethereum.org/en/developers/docs/standards/tokens/erc-721
        /// </summary>
        ERC721Service ERC721 { get; }

        /// <summary>
        /// ERC1155 Multi token standard Service to interact with smart contracts compliant with the standard interface
        /// https://ethereum.org/en/developers/docs/standards/tokens/erc-1155/
        /// </summary>
        ERC1155Service ERC1155 { get; }
        /// <summary>
        /// ERC1271: Standard Signature Validation Method for Contracts, Service to interact with smart contracts compliant with the standard interface
        /// This enables to validate if a signature is valid for a smart contract
        /// https://eips.ethereum.org/EIPS/eip-1271
        /// </summary>
        ERC1271Service ERC1271 { get; }
        ENSService GetEnsService(string ensRegistryAddress = CommonAddresses.ENS_REGISTRY_ADDRESS);

        /// <summary>
        /// Service to interact with the Proof of Humanity registry smart contract
        /// </summary>
        ProofOfHumanityService ProofOfHumanity { get; }
        EthTLSService GetEnsEthTlsService(string ensRegistryAddress = CommonAddresses.ENS_REGISTRY_ADDRESS);
#endif


    }
}