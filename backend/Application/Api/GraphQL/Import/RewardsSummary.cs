﻿using Application.Api.GraphQL.Accounts;
using Application.Api.GraphQL.Bakers;
using ConcordiumSdk.NodeApi.Types;

namespace Application.Api.GraphQL.Import;

public class RewardsSummary
{
    public AccountRewardSummary[] AggregatedAccountRewards { get; }

    public RewardsSummary(AccountRewardSummary[] aggregatedAccountRewards)
    {
        AggregatedAccountRewards = aggregatedAccountRewards;
    }

    public static RewardsSummary Create(BlockSummaryBase blockSummary, IAccountLookup accountLookup)
    {
        var rewards = blockSummary.SpecialEvents.SelectMany(se => se.GetAccountBalanceUpdates());

        var aggregatedRewards = rewards
            .Select(x => new
            {
                BaseAddress = x.AccountAddress.GetBaseAddress().AsString,
                Amount = x.AmountAdjustment,
                x.BalanceUpdateType
            })
            .GroupBy(x => x.BaseAddress)
            .Select(addressGroup => new
            {
                BaseAddress = addressGroup.Key,
                TotalAmount = addressGroup.Aggregate(0L, (acc, item) => acc + item.Amount),
                TotalAmountByType = addressGroup
                    .GroupBy(x => x.BalanceUpdateType)
                    .Select(rewardTypeGroup =>
                        new RewardTypeAmount(rewardTypeGroup.Key.ToRewardType(),
                            rewardTypeGroup.Aggregate(0L, (acc, item) => acc + item.Amount))
                    )
                    .ToArray()
            })
            .ToArray();

        var baseAddresses = aggregatedRewards.Select(x => x.BaseAddress);
        var accountIdMap = accountLookup.GetAccountIdsFromBaseAddresses(baseAddresses);
        
        var accountRewards = aggregatedRewards
            .Where(x => accountIdMap.ContainsKey(x.BaseAddress) && accountIdMap[x.BaseAddress] is not null)
            .Select(x =>
            {
                var accountId = accountIdMap[x.BaseAddress] ?? throw new InvalidOperationException("Attempt at updating account that does not exist!");
                return new AccountRewardSummary(accountId, x.TotalAmount, x.TotalAmountByType);
            });

        return new RewardsSummary(accountRewards.ToArray());
    }
}

public record AccountRewardSummary(
    long AccountId, 
    long TotalAmount,
    RewardTypeAmount[] TotalAmountByType);

public record RewardTypeAmount(
    RewardType RewardType, 
    long TotalAmount);
