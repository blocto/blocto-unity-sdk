using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Nethereum.Contracts.Standards.ERC721.ContractDefinition
{
    public  class DomainSeparatorFunction : DomainSeparatorFunctionBase { }

    [Function("DOMAIN_SEPARATOR", "bytes32")]
    public class DomainSeparatorFunctionBase : FunctionMessage { }

    public  class ApproveFunction : ApproveFunctionBase { }

    [Function("approve")]
    public class ApproveFunctionBase : FunctionMessage
    {
        [Parameter("address", "to")]
        public virtual string To { get; set; }

        [Parameter("uint256", "tokenId", 2)]
        public virtual BigInteger TokenId { get; set; }
    }

    public  class BalanceOfFunction : BalanceOfFunctionBase { }

    [Function("balanceOf", "uint256")]
    public class BalanceOfFunctionBase : FunctionMessage
    {
        [Parameter("address", "owner")]
        public virtual string Owner { get; set; }
    }

    public  class BurnFunction : BurnFunctionBase { }

    [Function("burn")]
    public class BurnFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "tokenId")]
        public virtual BigInteger TokenId { get; set; }
    }

    public  class DelegateFunction : DelegateFunctionBase { }

    [Function("delegate")]
    public class DelegateFunctionBase : FunctionMessage
    {
        [Parameter("address", "delegatee")]
        public virtual string Delegatee { get; set; }
    }

    public  class DelegateBySigFunction : DelegateBySigFunctionBase { }

    [Function("delegateBySig")]
    public class DelegateBySigFunctionBase : FunctionMessage
    {
        [Parameter("address", "delegatee")]
        public virtual string Delegatee { get; set; }

        [Parameter("uint256", "nonce", 2)]
        public new virtual BigInteger Nonce { get; set; }

        [Parameter("uint256", "expiry", 3)]
        public virtual BigInteger Expiry { get; set; }

        [Parameter("uint8", "v", 4)]
        public virtual byte V { get; set; }

        [Parameter("bytes32", "r", 5)]
        public virtual byte[] R { get; set; }

        [Parameter("bytes32", "s", 6)]
        public virtual byte[] S { get; set; }
    }

    public  class DelegatesFunction : DelegatesFunctionBase { }

    [Function("delegates", "address")]
    public class DelegatesFunctionBase : FunctionMessage
    {
        [Parameter("address", "account")]
        public virtual string Account { get; set; }
    }

    public  class GetApprovedFunction : GetApprovedFunctionBase { }

    [Function("getApproved", "address")]
    public class GetApprovedFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "tokenId")]
        public virtual BigInteger TokenId { get; set; }
    }

    public  class GetPastTotalSupplyFunction : GetPastTotalSupplyFunctionBase { }

    [Function("getPastTotalSupply", "uint256")]
    public class GetPastTotalSupplyFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "blockNumber")]
        public virtual BigInteger BlockNumber { get; set; }
    }

    public  class GetPastVotesFunction : GetPastVotesFunctionBase { }

    [Function("getPastVotes", "uint256")]
    public class GetPastVotesFunctionBase : FunctionMessage
    {
        [Parameter("address", "account")]
        public virtual string Account { get; set; }

        [Parameter("uint256", "blockNumber", 2)]
        public virtual BigInteger BlockNumber { get; set; }
    }

    public  class GetVotesFunction : GetVotesFunctionBase { }

    [Function("getVotes", "uint256")]
    public class GetVotesFunctionBase : FunctionMessage
    {
        [Parameter("address", "account")]
        public virtual string Account { get; set; }
    }

    public  class IsApprovedForAllFunction : IsApprovedForAllFunctionBase { }

    [Function("isApprovedForAll", "bool")]
    public class IsApprovedForAllFunctionBase : FunctionMessage
    {
        [Parameter("address", "owner")]
        public virtual string Owner { get; set; }

        [Parameter("address", "operator", 2)]
        public virtual string Operator { get; set; }
    }

    public  class NameFunction : NameFunctionBase { }

    [Function("name", "string")]
    public class NameFunctionBase : FunctionMessage { }

    public  class NoncesFunction : NoncesFunctionBase { }

    [Function("nonces", "uint256")]
    public class NoncesFunctionBase : FunctionMessage
    {
        [Parameter("address", "owner")]
        public virtual string Owner { get; set; }
    }

    public  class OwnerFunction : OwnerFunctionBase { }

    [Function("owner", "address")]
    public class OwnerFunctionBase : FunctionMessage { }

    public  class OwnerOfFunction : OwnerOfFunctionBase { }

    [Function("ownerOf", "address")]
    public class OwnerOfFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "tokenId")]
        public virtual BigInteger TokenId { get; set; }
    }

    public  class PauseFunction : PauseFunctionBase { }

    [Function("pause")]
    public class PauseFunctionBase : FunctionMessage { }

    public  class PausedFunction : PausedFunctionBase { }

    [Function("paused", "bool")]
    public class PausedFunctionBase : FunctionMessage { }

    public  class RenounceOwnershipFunction : RenounceOwnershipFunctionBase { }

    [Function("renounceOwnership")]
    public class RenounceOwnershipFunctionBase : FunctionMessage { }

    public  class SafeMintFunction : SafeMintFunctionBase { }

    [Function("safeMint")]
    public class SafeMintFunctionBase : FunctionMessage
    {
        [Parameter("address", "to")]
        public virtual string To { get; set; }

        [Parameter("string", "uri", 2)]
        public virtual string Uri { get; set; }
    }

    public  class SafeTransferFromFunction : SafeTransferFromFunctionBase { }

    [Function("safeTransferFrom")]
    public class SafeTransferFromFunctionBase : FunctionMessage
    {
        [Parameter("address", "from")]
        public virtual string From { get; set; }

        [Parameter("address", "to", 2)]
        public virtual string To { get; set; }

        [Parameter("uint256", "tokenId", 3)]
        public virtual BigInteger TokenId { get; set; }
    }

    public  class SafeTransferFrom1Function : SafeTransferFrom1FunctionBase { }

    [Function("safeTransferFrom")]
    public class SafeTransferFrom1FunctionBase : FunctionMessage
    {
        [Parameter("address", "from")]
        public virtual string From { get; set; }

        [Parameter("address", "to", 2)]
        public virtual string To { get; set; }

        [Parameter("uint256", "tokenId", 3)]
        public virtual BigInteger TokenId { get; set; }

        [Parameter("bytes", "_data", 4)]
        public virtual byte[] Data { get; set; }
    }

    public  class SetApprovalForAllFunction : SetApprovalForAllFunctionBase { }

    [Function("setApprovalForAll")]
    public class SetApprovalForAllFunctionBase : FunctionMessage
    {
        [Parameter("address", "operator")]
        public virtual string Operator { get; set; }

        [Parameter("bool", "approved", 2)]
        public virtual bool Approved { get; set; }
    }

    public  class SupportsInterfaceFunction : SupportsInterfaceFunctionBase { }

    [Function("supportsInterface", "bool")]
    public class SupportsInterfaceFunctionBase : FunctionMessage
    {
        [Parameter("bytes4", "interfaceId")]
        public virtual byte[] InterfaceId { get; set; }
    }

    public  class SymbolFunction : SymbolFunctionBase { }

    [Function("symbol", "string")]
    public class SymbolFunctionBase : FunctionMessage { }

    public  class TokenByIndexFunction : TokenByIndexFunctionBase { }

    [Function("tokenByIndex", "uint256")]
    public class TokenByIndexFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "index")]
        public virtual BigInteger Index { get; set; }
    }

    public  class TokenOfOwnerByIndexFunction : TokenOfOwnerByIndexFunctionBase { }

    [Function("tokenOfOwnerByIndex", "uint256")]
    public class TokenOfOwnerByIndexFunctionBase : FunctionMessage
    {
        [Parameter("address", "owner")]
        public virtual string Owner { get; set; }

        [Parameter("uint256", "index", 2)]
        public virtual BigInteger Index { get; set; }
    }

    public  class TokenURIFunction : TokenURIFunctionBase { }

    [Function("tokenURI", "string")]
    public class TokenURIFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "tokenId")]
        public virtual BigInteger TokenId { get; set; }
    }

    public  class TotalSupplyFunction : TotalSupplyFunctionBase { }

    [Function("totalSupply", "uint256")]
    public class TotalSupplyFunctionBase : FunctionMessage { }

    public  class TransferFromFunction : TransferFromFunctionBase { }

    [Function("transferFrom")]
    public class TransferFromFunctionBase : FunctionMessage
    {
        [Parameter("address", "from")]
        public virtual string From { get; set; }

        [Parameter("address", "to", 2)]
        public virtual string To { get; set; }

        [Parameter("uint256", "tokenId", 3)]
        public virtual BigInteger TokenId { get; set; }
    }

    public  class TransferOwnershipFunction : TransferOwnershipFunctionBase { }

    [Function("transferOwnership")]
    public class TransferOwnershipFunctionBase : FunctionMessage
    {
        [Parameter("address", "newOwner")]
        public virtual string NewOwner { get; set; }
    }

    public  class UnpauseFunction : UnpauseFunctionBase { }

    [Function("unpause")]
    public class UnpauseFunctionBase : FunctionMessage { }

    public  class ApprovalEventDTO : ApprovalEventDTOBase { }

    [Event("Approval")]
    public class ApprovalEventDTOBase : IEventDTO
    {
        [Parameter("address", "owner", 1, true)]
        public virtual string Owner { get; set; }

        [Parameter("address", "approved", 2, true)]
        public virtual string Approved { get; set; }

        [Parameter("uint256", "tokenId", 3, true)]
        public virtual BigInteger TokenId { get; set; }
    }

    public  class ApprovalForAllEventDTO : ApprovalForAllEventDTOBase { }

    [Event("ApprovalForAll")]
    public class ApprovalForAllEventDTOBase : IEventDTO
    {
        [Parameter("address", "owner", 1, true)]
        public virtual string Owner { get; set; }

        [Parameter("address", "operator", 2, true)]
        public virtual string Operator { get; set; }

        [Parameter("bool", "approved", 3, false)]
        public virtual bool Approved { get; set; }
    }

    public  class DelegateChangedEventDTO : DelegateChangedEventDTOBase { }

    [Event("DelegateChanged")]
    public class DelegateChangedEventDTOBase : IEventDTO
    {
        [Parameter("address", "delegator", 1, true)]
        public virtual string Delegator { get; set; }

        [Parameter("address", "fromDelegate", 2, true)]
        public virtual string FromDelegate { get; set; }

        [Parameter("address", "toDelegate", 3, true)]
        public virtual string ToDelegate { get; set; }
    }

    public  class DelegateVotesChangedEventDTO : DelegateVotesChangedEventDTOBase { }

    [Event("DelegateVotesChanged")]
    public class DelegateVotesChangedEventDTOBase : IEventDTO
    {
        [Parameter("address", "delegate", 1, true)]
        public virtual string Delegate { get; set; }

        [Parameter("uint256", "previousBalance", 2, false)]
        public virtual BigInteger PreviousBalance { get; set; }

        [Parameter("uint256", "newBalance", 3, false)]
        public virtual BigInteger NewBalance { get; set; }
    }

    public  class OwnershipTransferredEventDTO : OwnershipTransferredEventDTOBase { }

    [Event("OwnershipTransferred")]
    public class OwnershipTransferredEventDTOBase : IEventDTO
    {
        [Parameter("address", "previousOwner", 1, true)]
        public virtual string PreviousOwner { get; set; }

        [Parameter("address", "newOwner", 2, true)]
        public virtual string NewOwner { get; set; }
    }

    public  class PausedEventDTO : PausedEventDTOBase { }

    [Event("Paused")]
    public class PausedEventDTOBase : IEventDTO
    {
        [Parameter("address", "account", 1, false)]
        public virtual string Account { get; set; }
    }

    public  class TransferEventDTO : TransferEventDTOBase { }

    [Event("Transfer")]
    public class TransferEventDTOBase : IEventDTO
    {
        [Parameter("address", "from", 1, true)]
        public virtual string From { get; set; }

        [Parameter("address", "to", 2, true)]
        public virtual string To { get; set; }

        [Parameter("uint256", "tokenId", 3, true)]
        public virtual BigInteger TokenId { get; set; }
    }

    public  class UnpausedEventDTO : UnpausedEventDTOBase { }

    [Event("Unpaused")]
    public class UnpausedEventDTOBase : IEventDTO
    {
        [Parameter("address", "account", 1, false)]
        public virtual string Account { get; set; }
    }

    public  class DomainSeparatorOutputDto : DomainSeparatorOutputDtoBase { }

    [FunctionOutput]
    public class DomainSeparatorOutputDtoBase : IFunctionOutputDTO
    {
        [Parameter("bytes32", "")]
        public virtual byte[] ReturnValue1 { get; set; }
    }

    public  class BalanceOfOutputDTO : BalanceOfOutputDTOBase { }

    [FunctionOutput]
    public class BalanceOfOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("uint256", "")]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public  class DelegatesOutputDTO : DelegatesOutputDTOBase { }

    [FunctionOutput]
    public class DelegatesOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("address", "")]
        public virtual string ReturnValue1 { get; set; }
    }

    public  class GetApprovedOutputDTO : GetApprovedOutputDTOBase { }

    [FunctionOutput]
    public class GetApprovedOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("address", "")]
        public virtual string ReturnValue1 { get; set; }
    }

    public  class GetPastTotalSupplyOutputDTO : GetPastTotalSupplyOutputDTOBase { }

    [FunctionOutput]
    public class GetPastTotalSupplyOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("uint256", "")]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public  class GetPastVotesOutputDTO : GetPastVotesOutputDTOBase { }

    [FunctionOutput]
    public class GetPastVotesOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("uint256", "")]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public  class GetVotesOutputDTO : GetVotesOutputDTOBase { }

    [FunctionOutput]
    public class GetVotesOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("uint256", "")]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public  class IsApprovedForAllOutputDTO : IsApprovedForAllOutputDTOBase { }

    [FunctionOutput]
    public class IsApprovedForAllOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("bool", "")]
        public virtual bool ReturnValue1 { get; set; }
    }

    public  class NameOutputDTO : NameOutputDTOBase { }

    [FunctionOutput]
    public class NameOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("string", "")]
        public virtual string ReturnValue1 { get; set; }
    }

    public  class NoncesOutputDTO : NoncesOutputDTOBase { }

    [FunctionOutput]
    public class NoncesOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("uint256", "")]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public  class OwnerOutputDTO : OwnerOutputDTOBase { }

    [FunctionOutput]
    public class OwnerOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("address", "")]
        public virtual string ReturnValue1 { get; set; }
    }

    public  class OwnerOfOutputDTO : OwnerOfOutputDTOBase { }

    [FunctionOutput]
    public class OwnerOfOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("address", "")]
        public virtual string ReturnValue1 { get; set; }
    }

    public  class PausedOutputDTO : PausedOutputDTOBase { }

    [FunctionOutput]
    public class PausedOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("bool", "")]
        public virtual bool ReturnValue1 { get; set; }
    }

    public  class SupportsInterfaceOutputDTO : SupportsInterfaceOutputDTOBase { }

    [FunctionOutput]
    public class SupportsInterfaceOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("bool", "")]
        public virtual bool ReturnValue1 { get; set; }
    }

    public  class SymbolOutputDTO : SymbolOutputDTOBase { }

    [FunctionOutput]
    public class SymbolOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("string", "")]
        public virtual string ReturnValue1 { get; set; }
    }

    public  class TokenByIndexOutputDTO : TokenByIndexOutputDTOBase { }

    [FunctionOutput]
    public class TokenByIndexOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("uint256", "")]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public  class TokenOfOwnerByIndexOutputDTO : TokenOfOwnerByIndexOutputDTOBase { }

    [FunctionOutput]
    public class TokenOfOwnerByIndexOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("uint256", "")]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public  class TokenURIOutputDTO : TokenURIOutputDTOBase { }

    [FunctionOutput]
    public class TokenURIOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("string", "")]
        public virtual string ReturnValue1 { get; set; }
    }

    public  class TotalSupplyOutputDTO : TotalSupplyOutputDTOBase { }

    [FunctionOutput]
    public class TotalSupplyOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("uint256", "")]
        public virtual BigInteger ReturnValue1 { get; set; }
    }
}