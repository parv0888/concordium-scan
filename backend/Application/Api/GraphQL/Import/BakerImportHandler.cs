﻿using System.Threading.Tasks;
using Application.Api.GraphQL.Bakers;
using Application.Api.GraphQL.EfCore;
using Application.Common.Diagnostics;
using Application.Import;
using ConcordiumSdk.NodeApi.Types;
using ConcordiumSdk.Types;
using Microsoft.EntityFrameworkCore;
using BakerPoolOpenStatus = Application.Api.GraphQL.Bakers.BakerPoolOpenStatus;

namespace Application.Api.GraphQL.Import;

public class BakerImportHandler
{
    private readonly BakerWriter _writer;
    private readonly IMetrics _metrics;
    private readonly ILogger _logger;

    public BakerImportHandler(IDbContextFactory<GraphQlDbContext> dbContextFactory, IMetrics metrics)
    {
        _writer = new BakerWriter(dbContextFactory, metrics);
        _metrics = metrics;
        _logger = Log.ForContext(GetType());
    }

    public async Task<BakerUpdateResults> HandleBakerUpdates(BlockDataPayload payload, RewardsSummary rewardsSummary, ImportState importState)
    {
        using var counter = _metrics.MeasureDuration(nameof(BakerImportHandler), nameof(HandleBakerUpdates));

        var resultBuilder = new BakerUpdateResultsBuilder();
        
        if (payload is GenesisBlockDataPayload)
            await AddGenesisBakers(payload, resultBuilder, importState);
        else
            await ApplyBakerChanges(payload, rewardsSummary, importState, resultBuilder);

        resultBuilder.SetTotalAmountStaked(await _writer.GetTotalAmountStaked());
        return resultBuilder.Build();
    }

    public async Task AddBakerTransactionRelations(TransactionPair[] transactions)
    {
        using var counter = _metrics.MeasureDuration(nameof(BakerImportHandler), nameof(AddBakerTransactionRelations));

        var items = transactions
            .Select(tx =>
            {
                var bakerIds = tx.Source.Result.GetBakerIds().Distinct().ToArray();
                if (bakerIds.Length == 0)
                    return null;
                if (bakerIds.Length == 1)
                    return new BakerTransactionRelation
                    {
                        BakerId = (long)bakerIds.Single(),
                        TransactionId = tx.Target.Id
                    };
                throw new InvalidOperationException("Did not expect multiple baker id's from one transaction");
            })
            .Where(x => x != null)
            .Select(x => x!);

        await _writer.AddBakerTransactionRelations(items);
    }

    private async Task AddGenesisBakers(BlockDataPayload payload, BakerUpdateResultsBuilder resultBuilder, ImportState importState)
    {
        var mapBakerPool = payload.BlockSummary.ProtocolVersion >= 4;
        
        var genesisBakers = payload.AccountInfos.CreatedAccounts
            .Where(x => x.AccountBaker != null)
            .Select(x => x.AccountBaker!)
            .Select(x => CreateGenesisBaker(x, mapBakerPool))
            .ToArray();

        await _writer.AddBakers(genesisBakers);
        
        if (mapBakerPool)
            importState.MigrationToBakerPoolsCompleted = true;
        
        resultBuilder.IncrementBakersAdded(genesisBakers.Length);
    }

    private static Baker CreateGenesisBaker(AccountBaker src, bool mapBakerPool)
    {
        var pool = mapBakerPool ? MapBakerPool(src.BakerPoolInfo) : null;
        
        return CreateNewBaker(src.BakerId, src.StakedAmount, src.RestakeEarnings, pool);
    }

    private static BakerPool MapBakerPool(BakerPoolInfo? src)
    {
        if (src == null) throw new ArgumentNullException(nameof(src), "Did not expect baker pool info to be null when trying to map it!");
        
        return new BakerPool
        {
            OpenStatus = src.OpenStatus.MapToGraphQlEnum(),
            MetadataUrl = src.MetadataUrl,
            CommissionRates = new BakerPoolCommissionRates
            {
                TransactionCommission = src.CommissionRates.TransactionCommission,
                FinalizationCommission = src.CommissionRates.FinalizationCommission,
                BakingCommission = src.CommissionRates.BakingCommission
            }
        };
    }

    private async Task ApplyBakerChanges(BlockDataPayload payload, RewardsSummary rewardsSummary,
        ImportState importState, BakerUpdateResultsBuilder resultBuilder)
    {
        await MaybeMigrateToBakerPools(payload, importState);
        await WorkAroundConcordiumNodeBug225(payload.BlockInfo, importState);
        
        await UpdateBakersWithPendingChangesDue(payload.BlockInfo, importState, resultBuilder);

        var allTransactionEvents = payload.BlockSummary.TransactionSummaries
            .Select(tx => tx.Result).OfType<TransactionSuccessResult>()
            .SelectMany(x => x.Events)
            .ToArray();

        var txEvents = allTransactionEvents.Where(x => x
            is ConcordiumSdk.NodeApi.Types.BakerAdded
            or ConcordiumSdk.NodeApi.Types.BakerRemoved
            or ConcordiumSdk.NodeApi.Types.BakerStakeIncreased
            or ConcordiumSdk.NodeApi.Types.BakerStakeDecreased
            or ConcordiumSdk.NodeApi.Types.BakerSetRestakeEarnings
            or ConcordiumSdk.NodeApi.Types.BakerSetOpenStatus
            or ConcordiumSdk.NodeApi.Types.BakerSetMetadataURL
            or ConcordiumSdk.NodeApi.Types.BakerSetTransactionFeeCommission
            or ConcordiumSdk.NodeApi.Types.BakerSetFinalizationRewardCommission
            or ConcordiumSdk.NodeApi.Types.BakerSetBakingRewardCommission);

        await UpdateBakersFromTransactionEvents(txEvents, payload.AccountInfos.BakersWithNewPendingChanges, payload.BlockInfo, importState, resultBuilder);
        await _writer.UpdateStakeIfBakerActiveRestakingEarnings(rewardsSummary.AggregatedAccountRewards);
    }

    private async Task MaybeMigrateToBakerPools(BlockDataPayload payload, ImportState importState)
    {
        // Migrate to baker pool first time a block with protocol version 4 (or greater) is encountered.
        if (importState.MigrationToBakerPoolsCompleted || payload.BlockSummary.ProtocolVersion < 4)
            return;
        
        _logger.Information("Migrating all bakers to baker pools (protocol v4 update)...");

        await _writer.UpdateBakers(
            baker => baker.ActiveState!.Pool = CreateDefaultBakerPool(),
            baker => baker.ActiveState != null);
        
        importState.MigrationToBakerPoolsCompleted = true;
        _logger.Information("Migration completed!");
    }

    private async Task WorkAroundConcordiumNodeBug225(BlockInfo blockInfo, ImportState importState)
    {
        if (blockInfo.GenesisIndex > importState.LastGenesisIndex)
        {
            // Work-around for bug in concordium node: https://github.com/Concordium/concordium-node/issues/225
            // A pending change will be (significantly) prolonged if a change to a baker is pending when a
            // protocol update occurs (causing a new era to start and thus resetting epoch to zero)

            importState.LastGenesisIndex = blockInfo.GenesisIndex;
            _logger.Information("New genesis index detected. Will check for pending baker changes to be prolonged. [BlockSlot:{blockSlot}] [BlockSlotTime:{blockSlotTime:O}]", blockInfo.BlockSlot, blockInfo.BlockSlotTime);
            
            await _writer.UpdateBakersWithPendingChange(DateTimeOffset.MaxValue, baker =>
            {
                var activeState = (ActiveBakerState)baker.State;
                var pendingChange = activeState.PendingChange ?? throw new InvalidOperationException("Pending change was null!");
                if (pendingChange.Epoch.HasValue)
                {
                    activeState.PendingChange = pendingChange with
                    {
                        EffectiveTime = CalculateEffectiveTime(pendingChange.Epoch.Value, blockInfo.BlockSlotTime, blockInfo.BlockSlot)
                    };
                    _logger.Information("Rescheduled pending baker change for baker {bakerId} to {effectiveTime} based on epoch value {epochValue}", baker.BakerId, activeState.PendingChange.EffectiveTime, pendingChange.Epoch.Value);
                }
            });

            importState.NextPendingBakerChangeTime = await _writer.GetMinPendingChangeTime();
            _logger.Information("NextPendingBakerChangeTime set to {value}", importState.NextPendingBakerChangeTime);
        }
    }

    private async Task UpdateBakersFromTransactionEvents(
        IEnumerable<ConcordiumSdk.NodeApi.Types.TransactionResultEvent> transactionEvents,
        AccountInfo[] accountInfosForBakersWithNewPendingChanges, BlockInfo blockInfo, ImportState importState,
        BakerUpdateResultsBuilder resultBuilder)
    {
        foreach (var txEvent in transactionEvents)
        {
            if (txEvent is ConcordiumSdk.NodeApi.Types.BakerAdded bakerAdded)
            {
                var pool = importState.MigrationToBakerPoolsCompleted ? CreateDefaultBakerPool() : null;
                
                await _writer.AddOrUpdateBaker(bakerAdded,
                    src => src.BakerId,
                    src => CreateNewBaker(src.BakerId, src.Stake, src.RestakeEarnings, pool),
                    (src, dst) =>
                    {
                        dst.State = new ActiveBakerState(src.Stake.MicroCcdValue, src.RestakeEarnings, pool, null);
                    });

                resultBuilder.IncrementBakersAdded();
            }

            if (txEvent is ConcordiumSdk.NodeApi.Types.BakerRemoved bakerRemoved)
                await SetPendingChangeOnBaker(bakerRemoved.Account, accountInfosForBakersWithNewPendingChanges, blockInfo, importState);

            if (txEvent is ConcordiumSdk.NodeApi.Types.BakerStakeDecreased stakeDecreased)
                await SetPendingChangeOnBaker(stakeDecreased.Account, accountInfosForBakersWithNewPendingChanges, blockInfo, importState);

            if (txEvent is ConcordiumSdk.NodeApi.Types.BakerStakeIncreased stakeIncreased)
            {
                await _writer.UpdateBaker(stakeIncreased,
                    src => src.BakerId,
                    (src, dst) =>
                    {
                        var activeState = dst.State as ActiveBakerState ?? throw new InvalidOperationException("Cannot set restake earnings for a baker that is not active!");
                        activeState.StakedAmount = src.NewStake.MicroCcdValue;
                    });
            }

            if (txEvent is ConcordiumSdk.NodeApi.Types.BakerSetRestakeEarnings restakeEarnings)
            {
                await _writer.UpdateBaker(restakeEarnings,
                    src => src.BakerId,
                    (src, dst) =>
                    {
                        var activeState = dst.State as ActiveBakerState ?? throw new InvalidOperationException("Cannot set restake earnings for a baker that is not active!");
                        activeState.RestakeEarnings = src.RestakeEarnings;
                    });
            }
            
            if (txEvent is ConcordiumSdk.NodeApi.Types.BakerSetOpenStatus openStatus)
            {
                await _writer.UpdateBaker(openStatus,
                    src => src.BakerId,
                    (src, dst) =>
                    {
                        var pool = GetPool(dst);
                        pool.OpenStatus = src.OpenStatus.MapToGraphQlEnum();
                    });
            }
            
            if (txEvent is ConcordiumSdk.NodeApi.Types.BakerSetMetadataURL metadataUrl)
            {
                await _writer.UpdateBaker(metadataUrl,
                    src => src.BakerId,
                    (src, dst) =>
                    {
                        var pool = GetPool(dst);
                        pool.MetadataUrl = src.MetadataURL;
                    });
            }
            
            if (txEvent is ConcordiumSdk.NodeApi.Types.BakerSetTransactionFeeCommission transactionFeeCommission)
            {
                await _writer.UpdateBaker(transactionFeeCommission,
                    src => src.BakerId,
                    (src, dst) =>
                    {
                        var pool = GetPool(dst);
                        pool.CommissionRates.TransactionCommission = src.TransactionFeeCommission;
                    });
            }
            
            if (txEvent is ConcordiumSdk.NodeApi.Types.BakerSetFinalizationRewardCommission finalizationRewardCommission)
            {
                await _writer.UpdateBaker(finalizationRewardCommission,
                    src => src.BakerId,
                    (src, dst) =>
                    {
                        var pool = GetPool(dst);
                        pool.CommissionRates.FinalizationCommission = src.FinalizationRewardCommission;
                    });
            }
            
            if (txEvent is ConcordiumSdk.NodeApi.Types.BakerSetBakingRewardCommission bakingRewardCommission)
            {
                await _writer.UpdateBaker(bakingRewardCommission,
                    src => src.BakerId,
                    (src, dst) =>
                    {
                        var pool = GetPool(dst);
                        pool.CommissionRates.BakingCommission = src.BakingRewardCommission;
                    });
            }
        }
    }

    private async Task SetPendingChangeOnBaker(ConcordiumSdk.Types.AccountAddress bakerAccountAddress, AccountInfo[] accountInfos, BlockInfo blockInfo, ImportState importState)
    {
        var accountBaker = accountInfos
            .SingleOrDefault(x => x.AccountAddress == bakerAccountAddress)?
            .AccountBaker ?? throw new InvalidOperationException("AccountInfo not included for baker -OR- was not a baker!");

        var updatedBaker = await _writer.UpdateBaker(accountBaker, src => src.BakerId,
            (src, dst) => SetPendingChange(dst, src, blockInfo));

        var effectiveTime = ((ActiveBakerState)updatedBaker.State).PendingChange!.EffectiveTime;
        if (!importState.NextPendingBakerChangeTime.HasValue ||
            importState.NextPendingBakerChangeTime.Value > effectiveTime)
            importState.NextPendingBakerChangeTime = effectiveTime;
    }

    private void SetPendingChange(Baker destination, AccountBaker source, BlockInfo blockInfo)
    {
        if (source.PendingChange == null) throw new ArgumentException("Pending change must not be null");
        
        var activeState = destination.State as ActiveBakerState ?? throw new InvalidOperationException("Cannot set a pending change for a baker that is not active!");
        activeState.PendingChange = source.PendingChange switch
        {
            AccountBakerRemovePendingV0 x => new PendingBakerRemoval(CalculateEffectiveTime(x.Epoch, blockInfo.BlockSlotTime, blockInfo.BlockSlot), x.Epoch), 
            AccountBakerRemovePendingV1 x => new PendingBakerRemoval(x.EffectiveTime), 
            AccountBakerReduceStakePendingV0 x => new PendingBakerReduceStake(CalculateEffectiveTime(x.Epoch, blockInfo.BlockSlotTime, blockInfo.BlockSlot), x.NewStake.MicroCcdValue, x.Epoch),
            AccountBakerReduceStakePendingV1 x => new PendingBakerReduceStake(x.EffectiveTime, x.NewStake.MicroCcdValue),
            _ => throw new NotImplementedException($"Mapping not implemented for '{source.PendingChange.GetType().Name}'")
        };
    }

    public static DateTimeOffset CalculateEffectiveTime(ulong epoch, DateTimeOffset blockSlotTime, int blockSlot)
    {
        // TODO: Prior to protocol update 4, the effective time must be calculated in this cumbersome way
        //       We should be able to change this once we switch to concordium node v4 or greater!
        //
        // BUILT-IN ASSUMPTIONS (that can change but probably wont):
        //       Block time is 250ms
        //       Epoch duration is 1 hour
        
        var millisecondsSinceEraGenesis = (long)blockSlot * 250; // cast to long to avoid overflow!
        var eraGenesisTime = blockSlotTime.AddMilliseconds(-1 * millisecondsSinceEraGenesis);
        var effectiveTime = eraGenesisTime.AddHours(epoch);
        
        return effectiveTime;
    }

    private async Task UpdateBakersWithPendingChangesDue(BlockInfo blockInfo, ImportState importState, BakerUpdateResultsBuilder resultBuilder)
    {
        if (blockInfo.BlockSlotTime >= importState.NextPendingBakerChangeTime)
        {
            await _writer.UpdateBakersWithPendingChange(blockInfo.BlockSlotTime, baker => ApplyPendingChange(baker, resultBuilder));

            importState.NextPendingBakerChangeTime = await _writer.GetMinPendingChangeTime();
            _logger.Information("NextPendingBakerChangeTime set to {value}", importState.NextPendingBakerChangeTime);
        }
    }

    private void ApplyPendingChange(Baker baker, BakerUpdateResultsBuilder resultBuilder)
    {
        var activeState = baker.State as ActiveBakerState ?? throw new InvalidOperationException("Applying pending change to a baker that was not active!");
        if (activeState.PendingChange is PendingBakerRemoval pendingRemoval)
        {
            _logger.Information("Baker with id {bakerId} will be removed.", baker.Id);
            baker.State = new RemovedBakerState(pendingRemoval.EffectiveTime);
            resultBuilder.IncrementBakersRemoved();
        }
        else if (activeState.PendingChange is PendingBakerReduceStake reduceStake)
        {
            _logger.Information("Baker with id {bakerId} will have its stake reduced to {newStake}.", baker.Id, reduceStake.NewStakedAmount);
            activeState.PendingChange = null;
            activeState.StakedAmount = reduceStake.NewStakedAmount;
        }
        else throw new NotImplementedException("Applying this pending change is not implemented!");
    }

    private static Baker CreateNewBaker(ulong bakerId, CcdAmount stakedAmount, bool restakeEarnings, BakerPool? pool)
    {
        return new Baker
        {
            Id = (long)bakerId,
            State = new ActiveBakerState(stakedAmount.MicroCcdValue, restakeEarnings, pool, null)
        };
    }

    private BakerPool CreateDefaultBakerPool()
    {
        return new BakerPool
        {
            OpenStatus = BakerPoolOpenStatus.ClosedForAll,
            MetadataUrl = "",
            CommissionRates = new BakerPoolCommissionRates
            {
                TransactionCommission = 0.0m,
                FinalizationCommission = 0.0m,
                BakingCommission = 0.0m
            }
        };
    }

    private static BakerPool GetPool(Baker dst)
    {
        var activeState = dst.State as ActiveBakerState ?? throw new InvalidOperationException("Cannot set open status for a baker that is not active!");
        return activeState.Pool ?? throw new InvalidOperationException("Cannot set open status for a baker where pool is null!");
    }

    private class BakerUpdateResultsBuilder
    {
        private ulong _totalAmountStaked = 0;
        private int _bakersAdded = 0;
        private int _bakersRemoved = 0;

        public void SetTotalAmountStaked(ulong totalAmountStaked)
        {
            _totalAmountStaked = totalAmountStaked;
        }

        public BakerUpdateResults Build()
        {
            return new BakerUpdateResults(_totalAmountStaked, _bakersAdded, _bakersRemoved);
        }

        public void IncrementBakersAdded(int incrementValue = 1)
        {
            _bakersAdded += incrementValue;
        }

        public void IncrementBakersRemoved()
        {
            _bakersRemoved += 1;
        }
    }
}

public record BakerUpdateResults(
    ulong TotalAmountStaked,
    int BakersAdded,
    int BakersRemoved);