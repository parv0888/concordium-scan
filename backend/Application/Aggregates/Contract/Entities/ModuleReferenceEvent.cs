using Application.Aggregates.Contract.Types;
using Application.Api.GraphQL.Accounts;

namespace Application.Aggregates.Contract.Entities;

/// <summary>
/// Event which is generated when a module reference is created.
/// </summary>
public sealed class ModuleReferenceEvent
{
    public ulong BlockHeight { get; init; }
    public string TransactionHash { get; init; } = null!;
    public ulong TransactionIndex { get; init; }
    public uint EventIndex { get; init; }
    public string ModuleReference { get; init; } = null!;
    public AccountAddress Sender { get; init; } = null!;
    public ImportSource Source { get; init; }
    public DateTimeOffset BlockSlotTime { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTime.UtcNow;
    
    /// <summary>
    /// Needed for EF Core
    /// </summary>
    private ModuleReferenceEvent()
    {}

    public ModuleReferenceEvent(
        ulong blockHeight,
        string transactionHash,
        ulong transactionIndex,
        uint eventIndex,
        string moduleReference,
        AccountAddress sender,
        ImportSource source,
        DateTimeOffset blockSlotTime
    )
    {
        BlockHeight = blockHeight;
        TransactionHash = transactionHash;
        TransactionIndex = transactionIndex;
        EventIndex = eventIndex;
        ModuleReference = moduleReference;
        Sender = sender;
        Source = source;
        BlockSlotTime = blockSlotTime;
    }
}
