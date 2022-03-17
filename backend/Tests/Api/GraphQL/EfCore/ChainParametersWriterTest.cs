﻿using Application.Api.GraphQL;
using Application.Api.GraphQL.EfCore;
using ConcordiumSdk.Types;
using Dapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.TestUtilities;
using Tests.TestUtilities.Builders;

namespace Tests.Api.GraphQL.EfCore;

[Collection("Postgres Collection")]
public class ChainParametersWriterTest : IClassFixture<DatabaseFixture>
{
    private readonly GraphQlDbContextFactoryStub _dbContextFactory;
    private readonly ChainParametersWriter _target;
    
    public ChainParametersWriterTest(DatabaseFixture dbFixture)
    {
        _dbContextFactory = new GraphQlDbContextFactoryStub(dbFixture.DatabaseSettings);
        _target = new ChainParametersWriter(_dbContextFactory);

        using var connection = dbFixture.GetOpenConnection();
        connection.Execute("TRUNCATE TABLE graphql_chain_parameters");
    }
   
    [Fact]
    public async Task GetOrCreateChainParameters_DatabaseEmpty()
    {
        var blockSummary = new BlockSummaryBuilder()
            .WithChainParameters(new ChainParametersBuilder()
                .WithElectionDifficulty(0.1m)
                .WithEuroPerEnergy(1, 3)
                .WithMicroGtuPerEuro(2, 5)
                .WithBakerCooldownEpochs(4)
                .WithCredentialsPerBlockLimit(6)
                .WithRewardParameters(new RewardParametersBuilder()
                    .WithMintDistribution(0.2m, 0.3m, 0.4m)
                    .WithTransactionFeeDistribution(0.5m, 0.6m)
                    .WithGasRewards(0.7m, 0.8m, 0.9m, 0.95m)
                    .Build())
                .WithFoundationAccountIndex(7)
                .WithMinimumThresholdForBaking(CcdAmount.FromMicroCcd(848482929))
                .Build())
            .Build();
        
        var returnedResult = await _target.GetOrCreateChainParameters(blockSummary);
        
        var dbContext = _dbContextFactory.CreateDbContext();
        var persistedResults = await dbContext.ChainParameters.ToArrayAsync();
        persistedResults.Length.Should().Be(1);
        
        var persistedResult = persistedResults.Single();
        persistedResult.Id.Should().BeGreaterThan(0);
        persistedResult.ElectionDifficulty.Should().Be(0.1m);
        persistedResult.EuroPerEnergy.Should().Be(new ExchangeRate { Numerator = 1, Denominator = 3 });
        persistedResult.MicroCcdPerEuro.Should().Be(new ExchangeRate { Numerator = 2, Denominator = 5 });
        persistedResult.BakerCooldownEpochs.Should().Be(4);
        persistedResult.CredentialsPerBlockLimit.Should().Be(6);
        persistedResult.RewardParameters.MintDistribution.MintPerSlot.Should().Be(0.2m);
        persistedResult.RewardParameters.MintDistribution.BakingReward.Should().Be(0.3m);
        persistedResult.RewardParameters.MintDistribution.FinalizationReward.Should().Be(0.4m);
        persistedResult.RewardParameters.TransactionFeeDistribution.Baker.Should().Be(0.5m);
        persistedResult.RewardParameters.TransactionFeeDistribution.GasAccount.Should().Be(0.6m);
        persistedResult.RewardParameters.GasRewards.Baker.Should().Be(0.7m);
        persistedResult.RewardParameters.GasRewards.FinalizationProof.Should().Be(0.8m);
        persistedResult.RewardParameters.GasRewards.AccountCreation.Should().Be(0.9m);
        persistedResult.RewardParameters.GasRewards.ChainUpdate.Should().Be(0.95m);
        persistedResult.FoundationAccountId.Should().Be(7);
        persistedResult.MinimumThresholdForBaking.Should().Be(848482929);

        returnedResult.Should().Be(persistedResult);
    }
}