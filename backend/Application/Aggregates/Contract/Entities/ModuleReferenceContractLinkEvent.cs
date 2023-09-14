using Application.Aggregates.Contract.Types;
using Application.Api.GraphQL;
using Application.Api.GraphQL.Accounts;

namespace Application.Aggregates.Contract.Entities;

/// <summary>
/// Event which is generated when a module reference and smart contract is linked.
///
/// This is either at contract initialization or contract upgrade.
/// </summary>
public sealed class ModuleReferenceContractLinkEvent
{
    public ulong BlockHeight { get; init; }
    public string TransactionHash { get; init; } = null!;
    public ulong TransactionIndex { get; init; }
    public uint EventIndex { get; init; }
    public string ModuleReference { get; init; } = null!;
    public ulong ContractAddressIndex { get; init; }
    public ulong ContractAddressSubIndex { get; init; }
    public AccountAddress Sender { get; init; } = null!;
    public ImportSource Source { get; init; }
    public ModuleReferenceContractLinkAction LinkAction { get; init; }
    public DateTimeOffset BlockSlotTime { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Needed for EF Core
    /// </summary>
    private ModuleReferenceContractLinkEvent()
    {}

    public ModuleReferenceContractLinkEvent(
        ulong blockHeight,
        string transactionHash,
        ulong transactionIndex,
        uint eventIndex,
        string moduleReference,
        ContractAddress contractAddress,
        AccountAddress sender,
        ImportSource source, 
        ModuleReferenceContractLinkAction linkAction,
        DateTimeOffset blockSlotTime
    )
    {
        BlockHeight = blockHeight;
        TransactionHash = transactionHash;
        TransactionIndex = transactionIndex;
        EventIndex = eventIndex;
        ModuleReference = moduleReference;
        ContractAddressIndex = contractAddress.Index;
        ContractAddressSubIndex = contractAddress.SubIndex;
        Sender = sender;
        Source = source;
        LinkAction = linkAction;
        BlockSlotTime = blockSlotTime;
    }
    
    /// <summary>
    /// Identifies if the event add- or removes a link between a <see cref="Contract"/>
    /// and <see cref="ModuleReferenceEvent"/>.
    /// </summary>
    public enum ModuleReferenceContractLinkAction
    {
        Added,
        Removed
    }
}