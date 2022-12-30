using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.Contracts.Services;
using Nethereum.Contracts.Standards.ENS.Registrar.ContractDefinition;
using Nethereum.RPC.Eth.DTOs;

namespace Nethereum.Contracts.Standards.ENS
{
    public partial class RegistrarService
    {
        public string ContractAddress { get; }

        public ContractHandler ContractHandler { get; }

        public RegistrarService(IEthApiContractService ethApiContractService, string contractAddress)
        {
            ContractAddress = contractAddress;
#if !DOTNET35
            ContractHandler = ethApiContractService.GetContractHandler(contractAddress);
#endif
        }
#if !DOTNET35
        public Task<string> ReleaseDeedRequestAsync(ReleaseDeedFunction releaseDeedFunction)
        {
             return ContractHandler.SendRequestAsync(releaseDeedFunction);
        }

        public Task<TransactionReceipt> ReleaseDeedRequestAndWaitForReceiptAsync(ReleaseDeedFunction releaseDeedFunction, CancellationToken cancellationToken = default)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(releaseDeedFunction, cancellationToken);
        }

        public Task<string> ReleaseDeedRequestAsync(byte[] hash)
        {
            var releaseDeedFunction = new ReleaseDeedFunction();
                releaseDeedFunction.Hash = hash;
            
             return ContractHandler.SendRequestAsync(releaseDeedFunction);
        }

        public Task<TransactionReceipt> ReleaseDeedRequestAndWaitForReceiptAsync(byte[] hash, CancellationToken cancellationToken = default)
        {
            var releaseDeedFunction = new ReleaseDeedFunction();
                releaseDeedFunction.Hash = hash;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(releaseDeedFunction, cancellationToken);
        }

        public Task<BigInteger> GetAllowedTimeQueryAsync(GetAllowedTimeFunction getAllowedTimeFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetAllowedTimeFunction, BigInteger>(getAllowedTimeFunction, blockParameter);
        }

        
        public Task<BigInteger> GetAllowedTimeQueryAsync(byte[] hash, BlockParameter blockParameter = null)
        {
            var getAllowedTimeFunction = new GetAllowedTimeFunction();
                getAllowedTimeFunction.Hash = hash;
            
            return ContractHandler.QueryAsync<GetAllowedTimeFunction, BigInteger>(getAllowedTimeFunction, blockParameter);
        }



        public Task<string> InvalidateNameRequestAsync(InvalidateNameFunction invalidateNameFunction)
        {
             return ContractHandler.SendRequestAsync(invalidateNameFunction);
        }

        public Task<TransactionReceipt> InvalidateNameRequestAndWaitForReceiptAsync(InvalidateNameFunction invalidateNameFunction, CancellationToken cancellationToken = default)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(invalidateNameFunction, cancellationToken);
        }

        public Task<string> InvalidateNameRequestAsync(string unhashedName)
        {
            var invalidateNameFunction = new InvalidateNameFunction();
                invalidateNameFunction.UnhashedName = unhashedName;
            
             return ContractHandler.SendRequestAsync(invalidateNameFunction);
        }

        public Task<TransactionReceipt> InvalidateNameRequestAndWaitForReceiptAsync(string unhashedName, CancellationToken cancellationToken = default)
        {
            var invalidateNameFunction = new InvalidateNameFunction();
                invalidateNameFunction.UnhashedName = unhashedName;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(invalidateNameFunction, cancellationToken);
        }

        public Task<byte[]> ShaBidQueryAsync(ShaBidFunction shaBidFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<ShaBidFunction, byte[]>(shaBidFunction, blockParameter);
        }

        
        public Task<byte[]> ShaBidQueryAsync(byte[] hash, string owner, BigInteger value, byte[] salt, BlockParameter blockParameter = null)
        {
            var shaBidFunction = new ShaBidFunction();
                shaBidFunction.Hash = hash;
                shaBidFunction.Owner = owner;
                shaBidFunction.Value = value;
                shaBidFunction.Salt = salt;
            
            return ContractHandler.QueryAsync<ShaBidFunction, byte[]>(shaBidFunction, blockParameter);
        }



        public Task<string> CancelBidRequestAsync(CancelBidFunction cancelBidFunction)
        {
             return ContractHandler.SendRequestAsync(cancelBidFunction);
        }

        public Task<TransactionReceipt> CancelBidRequestAndWaitForReceiptAsync(CancelBidFunction cancelBidFunction, CancellationToken cancellationToken = default)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(cancelBidFunction, cancellationToken);
        }

        public Task<string> CancelBidRequestAsync(string bidder, byte[] seal)
        {
            var cancelBidFunction = new CancelBidFunction();
                cancelBidFunction.Bidder = bidder;
                cancelBidFunction.Seal = seal;
            
             return ContractHandler.SendRequestAsync(cancelBidFunction);
        }

        public Task<TransactionReceipt> CancelBidRequestAndWaitForReceiptAsync(string bidder, byte[] seal, CancellationToken cancellationToken = default)
        {
            var cancelBidFunction = new CancelBidFunction();
                cancelBidFunction.Bidder = bidder;
                cancelBidFunction.Seal = seal;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(cancelBidFunction, cancellationToken);
        }

        public Task<EntriesOutputDTO> EntriesQueryAsync(EntriesFunction entriesFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<EntriesFunction, EntriesOutputDTO>(entriesFunction, blockParameter);
        }

        
        public Task<EntriesOutputDTO> EntriesQueryAsync(byte[] hash, BlockParameter blockParameter = null)
        {
            var entriesFunction = new EntriesFunction();
                entriesFunction.Hash = hash;
            
            return ContractHandler.QueryDeserializingToObjectAsync<EntriesFunction, EntriesOutputDTO>(entriesFunction, blockParameter);
        }



        public Task<string> EnsQueryAsync(EnsFunction ensFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<EnsFunction, string>(ensFunction, blockParameter);
        }

        
        public Task<string> EnsQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<EnsFunction, string>(null, blockParameter);
        }



        public Task<string> UnsealBidRequestAsync(UnsealBidFunction unsealBidFunction)
        {
             return ContractHandler.SendRequestAsync(unsealBidFunction);
        }

        public Task<TransactionReceipt> UnsealBidRequestAndWaitForReceiptAsync(UnsealBidFunction unsealBidFunction, CancellationToken cancellationToken = default)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(unsealBidFunction, cancellationToken);
        }

        public Task<string> UnsealBidRequestAsync(byte[] hash, BigInteger value, byte[] salt)
        {
            var unsealBidFunction = new UnsealBidFunction();
                unsealBidFunction.Hash = hash;
                unsealBidFunction.Value = value;
                unsealBidFunction.Salt = salt;
            
             return ContractHandler.SendRequestAsync(unsealBidFunction);
        }

        public Task<TransactionReceipt> UnsealBidRequestAndWaitForReceiptAsync(byte[] hash, BigInteger value, byte[] salt, CancellationToken cancellationToken = default)
        {
            var unsealBidFunction = new UnsealBidFunction();
                unsealBidFunction.Hash = hash;
                unsealBidFunction.Value = value;
                unsealBidFunction.Salt = salt;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(unsealBidFunction, cancellationToken);
        }

        public Task<string> TransferRegistrarsRequestAsync(TransferRegistrarsFunction transferRegistrarsFunction)
        {
             return ContractHandler.SendRequestAsync(transferRegistrarsFunction);
        }

        public Task<TransactionReceipt> TransferRegistrarsRequestAndWaitForReceiptAsync(TransferRegistrarsFunction transferRegistrarsFunction, CancellationToken cancellationToken = default)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(transferRegistrarsFunction, cancellationToken);
        }

        public Task<string> TransferRegistrarsRequestAsync(byte[] hash)
        {
            var transferRegistrarsFunction = new TransferRegistrarsFunction();
                transferRegistrarsFunction.Hash = hash;
            
             return ContractHandler.SendRequestAsync(transferRegistrarsFunction);
        }

        public Task<TransactionReceipt> TransferRegistrarsRequestAndWaitForReceiptAsync(byte[] hash, CancellationToken cancellationToken = default)
        {
            var transferRegistrarsFunction = new TransferRegistrarsFunction();
                transferRegistrarsFunction.Hash = hash;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(transferRegistrarsFunction, cancellationToken);
        }

        public Task<string> SealedBidsQueryAsync(SealedBidsFunction sealedBidsFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<SealedBidsFunction, string>(sealedBidsFunction, blockParameter);
        }

        
        public Task<string> SealedBidsQueryAsync(string returnValue1, byte[] returnValue2, BlockParameter blockParameter = null)
        {
            var sealedBidsFunction = new SealedBidsFunction();
                sealedBidsFunction.ReturnValue1 = returnValue1;
                sealedBidsFunction.ReturnValue2 = returnValue2;
            
            return ContractHandler.QueryAsync<SealedBidsFunction, string>(sealedBidsFunction, blockParameter);
        }



        public Task<byte> StateQueryAsync(StateFunction stateFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<StateFunction, byte>(stateFunction, blockParameter);
        }

        
        public Task<byte> StateQueryAsync(byte[] hash, BlockParameter blockParameter = null)
        {
            var stateFunction = new StateFunction();
                stateFunction.Hash = hash;
            
            return ContractHandler.QueryAsync<StateFunction, byte>(stateFunction, blockParameter);
        }



        public Task<string> TransferRequestAsync(TransferFunction transferFunction)
        {
             return ContractHandler.SendRequestAsync(transferFunction);
        }

        public Task<TransactionReceipt> TransferRequestAndWaitForReceiptAsync(TransferFunction transferFunction, CancellationToken cancellationToken = default)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(transferFunction, cancellationToken);
        }

        public Task<string> TransferRequestAsync(byte[] hash, string newOwner)
        {
            var transferFunction = new TransferFunction();
                transferFunction.Hash = hash;
                transferFunction.NewOwner = newOwner;
            
             return ContractHandler.SendRequestAsync(transferFunction);
        }

        public Task<TransactionReceipt> TransferRequestAndWaitForReceiptAsync(byte[] hash, string newOwner, CancellationToken cancellationToken = default)
        {
            var transferFunction = new TransferFunction();
                transferFunction.Hash = hash;
                transferFunction.NewOwner = newOwner;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(transferFunction, cancellationToken);
        }

        public Task<bool> IsAllowedQueryAsync(IsAllowedFunction isAllowedFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<IsAllowedFunction, bool>(isAllowedFunction, blockParameter);
        }

        
        public Task<bool> IsAllowedQueryAsync(byte[] hash, BigInteger timestamp, BlockParameter blockParameter = null)
        {
            var isAllowedFunction = new IsAllowedFunction();
                isAllowedFunction.Hash = hash;
                isAllowedFunction.Timestamp = timestamp;
            
            return ContractHandler.QueryAsync<IsAllowedFunction, bool>(isAllowedFunction, blockParameter);
        }



        public Task<string> FinalizeAuctionRequestAsync(FinalizeAuctionFunction finalizeAuctionFunction)
        {
             return ContractHandler.SendRequestAsync(finalizeAuctionFunction);
        }

        public Task<TransactionReceipt> FinalizeAuctionRequestAndWaitForReceiptAsync(FinalizeAuctionFunction finalizeAuctionFunction, CancellationToken cancellationToken = default)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(finalizeAuctionFunction, cancellationToken);
        }

        public Task<string> FinalizeAuctionRequestAsync(byte[] hash)
        {
            var finalizeAuctionFunction = new FinalizeAuctionFunction();
                finalizeAuctionFunction.Hash = hash;
            
             return ContractHandler.SendRequestAsync(finalizeAuctionFunction);
        }

        public Task<TransactionReceipt> FinalizeAuctionRequestAndWaitForReceiptAsync(byte[] hash, CancellationToken cancellationToken = default)
        {
            var finalizeAuctionFunction = new FinalizeAuctionFunction();
                finalizeAuctionFunction.Hash = hash;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(finalizeAuctionFunction, cancellationToken);
        }

        public Task<BigInteger> RegistryStartedQueryAsync(RegistryStartedFunction registryStartedFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<RegistryStartedFunction, BigInteger>(registryStartedFunction, blockParameter);
        }

        
        public Task<BigInteger> RegistryStartedQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<RegistryStartedFunction, BigInteger>(null, blockParameter);
        }



        public Task<uint> LaunchLengthQueryAsync(LaunchLengthFunction launchLengthFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<LaunchLengthFunction, uint>(launchLengthFunction, blockParameter);
        }

        
        public Task<uint> LaunchLengthQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<LaunchLengthFunction, uint>(null, blockParameter);
        }



        public Task<string> NewBidRequestAsync(NewBidFunction newBidFunction)
        {
             return ContractHandler.SendRequestAsync(newBidFunction);
        }

        public Task<TransactionReceipt> NewBidRequestAndWaitForReceiptAsync(NewBidFunction newBidFunction, CancellationToken cancellationToken = default)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(newBidFunction, cancellationToken);
        }

        public Task<string> NewBidRequestAsync(byte[] sealedBid)
        {
            var newBidFunction = new NewBidFunction();
                newBidFunction.SealedBid = sealedBid;
            
             return ContractHandler.SendRequestAsync(newBidFunction);
        }

        public Task<TransactionReceipt> NewBidRequestAndWaitForReceiptAsync(byte[] sealedBid, CancellationToken cancellationToken = default)
        {
            var newBidFunction = new NewBidFunction();
                newBidFunction.SealedBid = sealedBid;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(newBidFunction, cancellationToken);
        }

        public Task<string> EraseNodeRequestAsync(EraseNodeFunction eraseNodeFunction)
        {
             return ContractHandler.SendRequestAsync(eraseNodeFunction);
        }

        public Task<TransactionReceipt> EraseNodeRequestAndWaitForReceiptAsync(EraseNodeFunction eraseNodeFunction, CancellationToken cancellationToken = default)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(eraseNodeFunction, cancellationToken);
        }

        public Task<string> EraseNodeRequestAsync(List<byte[]> labels)
        {
            var eraseNodeFunction = new EraseNodeFunction();
                eraseNodeFunction.Labels = labels;
            
             return ContractHandler.SendRequestAsync(eraseNodeFunction);
        }

        public Task<TransactionReceipt> EraseNodeRequestAndWaitForReceiptAsync(List<byte[]> labels, CancellationToken cancellationToken = default)
        {
            var eraseNodeFunction = new EraseNodeFunction();
                eraseNodeFunction.Labels = labels;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(eraseNodeFunction, cancellationToken);
        }

        public Task<string> StartAuctionsRequestAsync(StartAuctionsFunction startAuctionsFunction)
        {
             return ContractHandler.SendRequestAsync(startAuctionsFunction);
        }

        public Task<TransactionReceipt> StartAuctionsRequestAndWaitForReceiptAsync(StartAuctionsFunction startAuctionsFunction, CancellationToken cancellationToken = default)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(startAuctionsFunction, cancellationToken);
        }

        public Task<string> StartAuctionsRequestAsync(List<byte[]> hashes)
        {
            var startAuctionsFunction = new StartAuctionsFunction();
                startAuctionsFunction.Hashes = hashes;
            
             return ContractHandler.SendRequestAsync(startAuctionsFunction);
        }

        public Task<TransactionReceipt> StartAuctionsRequestAndWaitForReceiptAsync(List<byte[]> hashes, CancellationToken cancellationToken = default)
        {
            var startAuctionsFunction = new StartAuctionsFunction();
                startAuctionsFunction.Hashes = hashes;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(startAuctionsFunction, cancellationToken);
        }

        public Task<string> AcceptRegistrarTransferRequestAsync(AcceptRegistrarTransferFunction acceptRegistrarTransferFunction)
        {
             return ContractHandler.SendRequestAsync(acceptRegistrarTransferFunction);
        }

        public Task<TransactionReceipt> AcceptRegistrarTransferRequestAndWaitForReceiptAsync(AcceptRegistrarTransferFunction acceptRegistrarTransferFunction, CancellationToken cancellationToken = default)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(acceptRegistrarTransferFunction, cancellationToken);
        }

        public Task<string> AcceptRegistrarTransferRequestAsync(byte[] hash, string deed, BigInteger registrationDate)
        {
            var acceptRegistrarTransferFunction = new AcceptRegistrarTransferFunction();
                acceptRegistrarTransferFunction.Hash = hash;
                acceptRegistrarTransferFunction.Deed = deed;
                acceptRegistrarTransferFunction.RegistrationDate = registrationDate;
            
             return ContractHandler.SendRequestAsync(acceptRegistrarTransferFunction);
        }

        public Task<TransactionReceipt> AcceptRegistrarTransferRequestAndWaitForReceiptAsync(byte[] hash, string deed, BigInteger registrationDate, CancellationToken cancellationToken = default)
        {
            var acceptRegistrarTransferFunction = new AcceptRegistrarTransferFunction();
                acceptRegistrarTransferFunction.Hash = hash;
                acceptRegistrarTransferFunction.Deed = deed;
                acceptRegistrarTransferFunction.RegistrationDate = registrationDate;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(acceptRegistrarTransferFunction, cancellationToken);
        }

        public Task<string> StartAuctionRequestAsync(StartAuctionFunction startAuctionFunction)
        {
             return ContractHandler.SendRequestAsync(startAuctionFunction);
        }

        public Task<TransactionReceipt> StartAuctionRequestAndWaitForReceiptAsync(StartAuctionFunction startAuctionFunction, CancellationToken cancellationToken = default)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(startAuctionFunction, cancellationToken);
        }

        public Task<string> StartAuctionRequestAsync(byte[] hash)
        {
            var startAuctionFunction = new StartAuctionFunction();
                startAuctionFunction.Hash = hash;
            
             return ContractHandler.SendRequestAsync(startAuctionFunction);
        }

        public Task<TransactionReceipt> StartAuctionRequestAndWaitForReceiptAsync(byte[] hash, CancellationToken cancellationToken = default)
        {
            var startAuctionFunction = new StartAuctionFunction();
                startAuctionFunction.Hash = hash;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(startAuctionFunction, cancellationToken);
        }

        public Task<byte[]> RootNodeQueryAsync(RootNodeFunction rootNodeFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<RootNodeFunction, byte[]>(rootNodeFunction, blockParameter);
        }

        
        public Task<byte[]> RootNodeQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<RootNodeFunction, byte[]>(null, blockParameter);
        }



        public Task<string> StartAuctionsAndBidRequestAsync(StartAuctionsAndBidFunction startAuctionsAndBidFunction)
        {
             return ContractHandler.SendRequestAsync(startAuctionsAndBidFunction);
        }

        public Task<TransactionReceipt> StartAuctionsAndBidRequestAndWaitForReceiptAsync(StartAuctionsAndBidFunction startAuctionsAndBidFunction, CancellationToken cancellationToken = default)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(startAuctionsAndBidFunction, cancellationToken);
        }

        public Task<string> StartAuctionsAndBidRequestAsync(List<byte[]> hashes, byte[] sealedBid)
        {
            var startAuctionsAndBidFunction = new StartAuctionsAndBidFunction();
                startAuctionsAndBidFunction.Hashes = hashes;
                startAuctionsAndBidFunction.SealedBid = sealedBid;
            
             return ContractHandler.SendRequestAsync(startAuctionsAndBidFunction);
        }

        public Task<TransactionReceipt> StartAuctionsAndBidRequestAndWaitForReceiptAsync(List<byte[]> hashes, byte[] sealedBid, CancellationToken cancellationToken = default)
        {
            var startAuctionsAndBidFunction = new StartAuctionsAndBidFunction();
                startAuctionsAndBidFunction.Hashes = hashes;
                startAuctionsAndBidFunction.SealedBid = sealedBid;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(startAuctionsAndBidFunction, cancellationToken);
        }
#endif
    }
}
