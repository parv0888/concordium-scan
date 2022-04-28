﻿using System.Collections.Generic;
using Application.Api.GraphQL.Import;
using Application.Database;
using Dapper;
using FluentAssertions;
using Npgsql;
using Tests.TestUtilities;
using Tests.TestUtilities.Builders.GraphQL;
using Tests.TestUtilities.Stubs;

namespace Tests.Api.GraphQL.Import;

[Collection("Postgres Collection")]
public class MetricsWriterTest : IClassFixture<DatabaseFixture>
{
    private readonly MetricsWriter _target;
    private readonly DatabaseSettings _databaseSettings;
    private readonly DateTimeOffset _anyDateTimeOffset = new(2010, 10, 1, 12, 23, 34, 124, TimeSpan.Zero);

    public MetricsWriterTest(DatabaseFixture dbFixture)
    {
        _databaseSettings = dbFixture.DatabaseSettings;
        _target = new MetricsWriter(_databaseSettings, new NullMetrics());
        
        using var connection = dbFixture.GetOpenConnection();
        connection.Execute("TRUNCATE TABLE metrics_blocks");
        connection.Execute("TRUNCATE TABLE metrics_bakers");
        connection.Execute("TRUNCATE TABLE metrics_rewards");
    }

    [Fact]
    public async Task AddBlockMetrics()
    {
        await _target.AddBlockMetrics(new BlockBuilder()
            .WithBlockHeight(42)
            .WithBlockStatistics(new BlockStatisticsBuilder()
                .WithBlockTime(10.1d)
                .Build())
            .WithBalanceStatistics(new BalanceStatisticsBuilder()
                .WithTotalAmount(10000)
                .WithTotalAmountReleased(9000)
                .WithTotalAmountEncrypted(1000)
                .WithTotalAmountStaked(2500)
                .Build())
            .Build());
        
        var result = QuerySingle(@"
            select time, block_height, block_time_secs, finalization_time_secs, total_microccd, total_microccd_released, total_microccd_encrypted, total_microccd_staked
            from metrics_blocks");

        Assert.Equal(42, result.block_height);
        Assert.Equal(10.1d, result.block_time_secs);
        Assert.Null(result.finalization_time_secs);
        Assert.Equal(10000, result.total_microccd);
        Assert.Equal(9000, result.total_microccd_released);
        Assert.Equal(1000, result.total_microccd_encrypted);
        Assert.Equal(2500, result.total_microccd_staked);
    }

    [Theory]
    [InlineData(10000UL, 10000UL, 1.0d)]
    [InlineData(10000UL, 5000UL, 0.5d)]
    [InlineData(333UL, 311UL, 0.9339339339d)]
    [InlineData(ulong.MaxValue, 0UL, 0d)]
    [InlineData(ulong.MaxValue, ulong.MaxValue/100, 0.01d)]
    [InlineData(10000UL, null, null)]
    public async Task AddBlockMetrics_PercentageTotalAmountReleased(ulong totalAmount, ulong? totalAmountReleased, double? expectedResult)
    {
        await _target.AddBlockMetrics(new BlockBuilder()
            .WithBalanceStatistics(new BalanceStatisticsBuilder()
                .WithTotalAmount(totalAmount)
                .WithTotalAmountReleased(totalAmountReleased)
                .Build())
            .Build());
        
        var result = QuerySingle(@"
            select total_percentage_released
            from metrics_blocks");

        Assert.Equal(expectedResult, result.total_percentage_released);
    }
    
    [Theory]
    [InlineData(10000UL, 10000UL, 1.0d)]
    [InlineData(10000UL, 5000UL, 0.5d)]
    [InlineData(333UL, 311UL, 0.9339339339d)]
    [InlineData(ulong.MaxValue, 0UL, 0d)]
    [InlineData(ulong.MaxValue, ulong.MaxValue/100, 0.01d)]
    public async Task AddBlockMetrics_PercentageTotalAmountStaked(ulong totalAmount, ulong totalAmountStaked, double? expectedResult)
    {
        await _target.AddBlockMetrics(new BlockBuilder()
            .WithBalanceStatistics(new BalanceStatisticsBuilder()
                .WithTotalAmount(totalAmount)
                .WithTotalAmountStaked(totalAmountStaked)
                .Build())
            .Build());
        
        var result = QuerySingle(@"
            select total_percentage_staked
            from metrics_blocks");

        Assert.Equal(expectedResult, result.total_percentage_staked);
    }
    
    [Theory]
    [InlineData(10000UL, 10000UL, 1.0d)]
    [InlineData(10000UL, 5000UL, 0.5d)]
    [InlineData(333UL, 311UL, 0.9339339339d)]
    [InlineData(ulong.MaxValue, 0UL, 0d)]
    [InlineData(ulong.MaxValue, ulong.MaxValue/100, 0.01d)]
    public async Task AddBlockMetrics_PercentageTotalAmountEncrypted(ulong totalAmount, ulong totalAmountEncrypted, double? expectedResult)
    {
        await _target.AddBlockMetrics(new BlockBuilder()
            .WithBalanceStatistics(new BalanceStatisticsBuilder()
                .WithTotalAmount(totalAmount)
                .WithTotalAmountEncrypted(totalAmountEncrypted)
                .Build())
            .Build());
        
        var result = QuerySingle(@"
            select total_percentage_encrypted
            from metrics_blocks");

        Assert.Equal(expectedResult, result.total_percentage_encrypted);
    }
    
    [Fact]
    public async Task AddBakerMetrics_NoChanges()
    {
        var input = new BakerUpdateResultsBuilder()
            .WithBakersAdded(0)
            .WithBakersRemoved(0)
            .Build();

        var importState = new ImportStateBuilder()
            .WithTotalBakerCount(10)
            .Build();
        
        await _target.AddBakerMetrics(_anyDateTimeOffset, input, importState);
        
        var result = Query(@"
            select time, total_baker_count, bakers_added, bakers_removed
            from metrics_bakers");

        result.Should().BeEmpty();
    }
    
    [Theory]
    [InlineData(10, 5, 15)]
    [InlineData(2, 0, 12)]
    [InlineData(0, 4, 6)]
    public async Task AddBakerMetrics_Changes(int bakersAdded, int bakersRemoved, int expectedTotalCount)
    {
        var input = new BakerUpdateResultsBuilder()
            .WithBakersAdded(bakersAdded)
            .WithBakersRemoved(bakersRemoved)
            .Build();

        var importState = new ImportStateBuilder()
            .WithTotalBakerCount(10)
            .Build();
        
        await _target.AddBakerMetrics(_anyDateTimeOffset, input, importState);
        
        var result = Query(@"
            select time, total_baker_count, bakers_added, bakers_removed
            from metrics_bakers");

        var item = Assert.Single(result);
        Assert.Equal(expectedTotalCount, item.total_baker_count);
        Assert.Equal(bakersAdded, item.bakers_added);
        Assert.Equal(bakersRemoved, item.bakers_removed);
        
        Assert.Equal(expectedTotalCount, importState.TotalBakerCount);
    }

    [Fact]
    public void AddRewardMetrics()
    {
        var input = new RewardsSummary(new[]
        {
            new AccountReward(10, 1000),
            new AccountReward(421, 24100)
        });
        _target.AddRewardMetrics(_anyDateTimeOffset, input);

        var result = Query(@"
            select time, account_id, amount
            from metrics_rewards
            order by account_id").ToArray();
        
        Assert.Equal(2, result.Length);
        Assert.Equal(10, result[0].account_id);
        Assert.Equal(1000, result[0].amount);
        Assert.Equal(421, result[1].account_id);
        Assert.Equal(24100, result[1].amount);
    }
    
    private IEnumerable<dynamic> Query(string sql)
    {
        using var conn = new NpgsqlConnection(_databaseSettings.ConnectionString);
        conn.Open();

        var result = conn.Query(sql);
        conn.Close();
        return result;
    }

    private dynamic QuerySingle(string sql)
    {
        using var conn = new NpgsqlConnection(_databaseSettings.ConnectionString);
        conn.Open();

        var result = conn.QuerySingle(sql);
        conn.Close();
        return result;
    }
}