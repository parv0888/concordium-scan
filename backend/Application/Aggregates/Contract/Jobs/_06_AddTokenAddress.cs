using System.Threading;
using System.Threading.Tasks;
using Application.Aggregates.Contract.Configurations;
using Application.Api.GraphQL.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Application.Aggregates.Contract.Entities;
using Application.Resilience;

namespace Application.Aggregates.Contract.Jobs;

/// <summary>
/// Adding <see cref="Token.TokenAddress"/> by iterating through all existing tokens.
/// </summary>
public sealed class _06_AddTokenAddress : IStatelessJob
{
    private readonly IDbContextFactory<GraphQlDbContext> _contextFactory;
    private readonly ContractAggregateOptions _options;
    private readonly ILogger _logger;
    
    /// <summary>
    /// WARNING - Do not change this if job already executed on environment, since it will trigger rerun of job.
    /// </summary>
    private const string JobName = "_06_AddTokenAddress";

    public _06_AddTokenAddress(
        IDbContextFactory<GraphQlDbContext> contextFactory,
        IOptions<ContractAggregateOptions> options)
    {
        _contextFactory = contextFactory;
        _options = options.Value;
        _logger = Log.ForContext<_06_AddTokenAddress>();
    }

    public string GetUniqueIdentifier() => JobName;

    public async Task<IEnumerable<int>> GetIdentifierSequence(CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var max = context.Contract.Max(ce => ce.ContractAddressIndex);
        return Enumerable.Range(0, (int)max + 1);
    }

    public ValueTask Setup(CancellationToken token = default) => ValueTask.CompletedTask;

    public async ValueTask Process(int identifier, CancellationToken token = default)
    {
        _logger.Debug($"Start processing {identifier}");
        await Policies.GetTransientPolicy(GetUniqueIdentifier(), _logger, _options.RetryCount, _options.RetryDelay)
            .ExecuteAsync(async () =>
            {
                await using var context = await _contextFactory.CreateDbContextAsync(token);
                var tokens = await context.Tokens
                    .Where(t => (int)t.ContractIndex == identifier)
                    .ToListAsync(token);
                foreach (var cisToken in tokens.Where(cisToken => cisToken.TokenAddress == null))
                {
                    cisToken.TokenAddress =
                        Token.EncodeTokenAddress(cisToken.ContractIndex, cisToken.ContractSubIndex, cisToken.TokenId);
                }
                await context.SaveChangesAsync(token);
            });
        _logger.Debug($"Completed successfully processing {identifier}");
    }

    public bool ShouldNodeImportAwait() => true;
}
