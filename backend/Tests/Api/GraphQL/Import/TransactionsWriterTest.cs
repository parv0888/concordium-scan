﻿using Application.Api.GraphQL.Import;
using Application.Api.GraphQL.Transactions;
using ConcordiumSdk.NodeApi.Types;
using ConcordiumSdk.Types;
using Dapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.TestUtilities;
using Tests.TestUtilities.Builders;
using Tests.TestUtilities.Stubs;
using AccountAddress = ConcordiumSdk.Types.AccountAddress;
using AccountCreated = ConcordiumSdk.NodeApi.Types.AccountCreated;
using AlreadyABaker = ConcordiumSdk.NodeApi.Types.AlreadyABaker;
using AmountAddedByDecryption = ConcordiumSdk.NodeApi.Types.AmountAddedByDecryption;
using AmountTooLarge = ConcordiumSdk.NodeApi.Types.AmountTooLarge;
using BakerAdded = ConcordiumSdk.NodeApi.Types.BakerAdded;
using BakerInCooldown = ConcordiumSdk.NodeApi.Types.BakerInCooldown;
using BakerKeysUpdated = ConcordiumSdk.NodeApi.Types.BakerKeysUpdated;
using BakerRemoved = ConcordiumSdk.NodeApi.Types.BakerRemoved;
using BakerSetBakingRewardCommission = ConcordiumSdk.NodeApi.Types.BakerSetBakingRewardCommission;
using BakerSetFinalizationRewardCommission = ConcordiumSdk.NodeApi.Types.BakerSetFinalizationRewardCommission;
using BakerSetMetadataURL = ConcordiumSdk.NodeApi.Types.BakerSetMetadataURL;
using BakerSetOpenStatus = ConcordiumSdk.NodeApi.Types.BakerSetOpenStatus;
using BakerSetRestakeEarnings = ConcordiumSdk.NodeApi.Types.BakerSetRestakeEarnings;
using BakerSetTransactionFeeCommission = ConcordiumSdk.NodeApi.Types.BakerSetTransactionFeeCommission;
using BakerStakeDecreased = ConcordiumSdk.NodeApi.Types.BakerStakeDecreased;
using BakerStakeIncreased = ConcordiumSdk.NodeApi.Types.BakerStakeIncreased;
using ContractAddress = ConcordiumSdk.Types.ContractAddress;
using ContractInitialized = ConcordiumSdk.NodeApi.Types.ContractInitialized;
using CredentialDeployed = ConcordiumSdk.NodeApi.Types.CredentialDeployed;
using CredentialHolderDidNotSign = ConcordiumSdk.NodeApi.Types.CredentialHolderDidNotSign;
using CredentialKeysUpdated = ConcordiumSdk.NodeApi.Types.CredentialKeysUpdated;
using CredentialsUpdated = ConcordiumSdk.NodeApi.Types.CredentialsUpdated;
using DataRegistered = ConcordiumSdk.NodeApi.Types.DataRegistered;
using DelegationAdded = ConcordiumSdk.NodeApi.Types.DelegationAdded;
using DelegationRemoved = ConcordiumSdk.NodeApi.Types.DelegationRemoved;
using DelegationSetDelegationTarget = ConcordiumSdk.NodeApi.Types.DelegationSetDelegationTarget;
using DelegationSetRestakeEarnings = ConcordiumSdk.NodeApi.Types.DelegationSetRestakeEarnings;
using DelegationStakeDecreased = ConcordiumSdk.NodeApi.Types.DelegationStakeDecreased;
using DelegationStakeIncreased = ConcordiumSdk.NodeApi.Types.DelegationStakeIncreased;
using DuplicateAggregationKey = ConcordiumSdk.NodeApi.Types.DuplicateAggregationKey;
using DuplicateCredIds = ConcordiumSdk.NodeApi.Types.DuplicateCredIds;
using EncryptedAmountSelfTransfer = ConcordiumSdk.NodeApi.Types.EncryptedAmountSelfTransfer;
using EncryptedAmountsRemoved = ConcordiumSdk.NodeApi.Types.EncryptedAmountsRemoved;
using EncryptedSelfAmountAdded = ConcordiumSdk.NodeApi.Types.EncryptedSelfAmountAdded;
using ExchangeRate = ConcordiumSdk.NodeApi.Types.ExchangeRate;
using FirstScheduledReleaseExpired = ConcordiumSdk.NodeApi.Types.FirstScheduledReleaseExpired;
using InsufficientBalanceForBakerStake = ConcordiumSdk.NodeApi.Types.InsufficientBalanceForBakerStake;
using InvalidAccountReference = ConcordiumSdk.NodeApi.Types.InvalidAccountReference;
using InvalidAccountThreshold = ConcordiumSdk.NodeApi.Types.InvalidAccountThreshold;
using InvalidContractAddress = ConcordiumSdk.NodeApi.Types.InvalidContractAddress;
using InvalidCredentialKeySignThreshold = ConcordiumSdk.NodeApi.Types.InvalidCredentialKeySignThreshold;
using InvalidCredentials = ConcordiumSdk.NodeApi.Types.InvalidCredentials;
using InvalidEncryptedAmountTransferProof = ConcordiumSdk.NodeApi.Types.InvalidEncryptedAmountTransferProof;
using InvalidIndexOnEncryptedTransfer = ConcordiumSdk.NodeApi.Types.InvalidIndexOnEncryptedTransfer;
using InvalidInitMethod = ConcordiumSdk.NodeApi.Types.InvalidInitMethod;
using InvalidModuleReference = ConcordiumSdk.NodeApi.Types.InvalidModuleReference;
using InvalidProof = ConcordiumSdk.NodeApi.Types.InvalidProof;
using InvalidReceiveMethod = ConcordiumSdk.NodeApi.Types.InvalidReceiveMethod;
using InvalidTransferToPublicProof = ConcordiumSdk.NodeApi.Types.InvalidTransferToPublicProof;
using KeyIndexAlreadyInUse = ConcordiumSdk.NodeApi.Types.KeyIndexAlreadyInUse;
using ModuleHashAlreadyExists = ConcordiumSdk.NodeApi.Types.ModuleHashAlreadyExists;
using ModuleNotWf = ConcordiumSdk.NodeApi.Types.ModuleNotWf;
using NewEncryptedAmount = ConcordiumSdk.NodeApi.Types.NewEncryptedAmount;
using NonExistentCredentialId = ConcordiumSdk.NodeApi.Types.NonExistentCredentialId;
using NonExistentCredIds = ConcordiumSdk.NodeApi.Types.NonExistentCredIds;
using NonExistentRewardAccount = ConcordiumSdk.NodeApi.Types.NonExistentRewardAccount;
using NonIncreasingSchedule = ConcordiumSdk.NodeApi.Types.NonIncreasingSchedule;
using NotABaker = ConcordiumSdk.NodeApi.Types.NotABaker;
using NotAllowedMultipleCredentials = ConcordiumSdk.NodeApi.Types.NotAllowedMultipleCredentials;
using NotAllowedToHandleEncrypted = ConcordiumSdk.NodeApi.Types.NotAllowedToHandleEncrypted;
using NotAllowedToReceiveEncrypted = ConcordiumSdk.NodeApi.Types.NotAllowedToReceiveEncrypted;
using OutOfEnergy = ConcordiumSdk.NodeApi.Types.OutOfEnergy;
using PassiveDelegationTarget = ConcordiumSdk.NodeApi.Types.PassiveDelegationTarget;
using RejectedInit = ConcordiumSdk.NodeApi.Types.RejectedInit;
using RejectedReceive = ConcordiumSdk.NodeApi.Types.RejectedReceive;
using RemoveFirstCredential = ConcordiumSdk.NodeApi.Types.RemoveFirstCredential;
using RuntimeFailure = ConcordiumSdk.NodeApi.Types.RuntimeFailure;
using ScheduledSelfTransfer = ConcordiumSdk.NodeApi.Types.ScheduledSelfTransfer;
using SerializationFailure = ConcordiumSdk.NodeApi.Types.SerializationFailure;
using StakeUnderMinimumThresholdForBaking = ConcordiumSdk.NodeApi.Types.StakeUnderMinimumThresholdForBaking;
using TimestampedAmount = ConcordiumSdk.NodeApi.Types.TimestampedAmount;
using TransactionRejectReason = ConcordiumSdk.NodeApi.Types.TransactionRejectReason;
using TransferMemo = ConcordiumSdk.NodeApi.Types.TransferMemo;
using Transferred = ConcordiumSdk.NodeApi.Types.Transferred;
using TransferredWithSchedule = ConcordiumSdk.NodeApi.Types.TransferredWithSchedule;
using ZeroScheduledAmount = ConcordiumSdk.NodeApi.Types.ZeroScheduledAmount;

namespace Tests.Api.GraphQL.Import;

[Collection("Postgres Collection")]
public class TransactionsWriterTest : IClassFixture<DatabaseFixture>
{
    private readonly TransactionWriter _target;
    private readonly GraphQlDbContextFactoryStub _dbContextFactory;
    private readonly BlockSummaryV0Builder _blockSummaryBuilder = new();
    private readonly DateTimeOffset _anyBlockSlotTime = new DateTimeOffset(2020, 11, 7, 17, 13, 0, 331, TimeSpan.Zero);

    public TransactionsWriterTest(DatabaseFixture dbFixture)
    {
        _dbContextFactory = new GraphQlDbContextFactoryStub(dbFixture.DatabaseSettings);
        _target = new TransactionWriter(_dbContextFactory, new NullMetrics());

        using var connection = dbFixture.GetOpenConnection();
        connection.Execute("TRUNCATE TABLE graphql_transactions");
        connection.Execute("TRUNCATE TABLE graphql_transaction_events");
    }
    
    [Fact]
    public async Task Transactions_BasicInformation_AllValuesNonNull()
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithIndex(0)
                .WithSender(new("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd"))
                .WithTransactionHash(new("42b83d2be10b86bd6df5c102c4451439422471bc4443984912a832052ff7485b"))
                .WithCost(CcdAmount.FromMicroCcd(45872))
                .WithEnergyCost(399)
                .Build());
        
        await WriteData(133);

        await using var dbContext = _dbContextFactory.CreateDbContext();
        var transaction = dbContext.Transactions.Single();
        transaction.Id.Should().BeGreaterThan(0);
        transaction.BlockId.Should().Be(133);
        transaction.TransactionIndex.Should().Be(0);
        transaction.TransactionHash.Should().Be("42b83d2be10b86bd6df5c102c4451439422471bc4443984912a832052ff7485b");
        transaction.SenderAccountAddress!.AsString.Should().Be("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd");
        transaction.CcdCost.Should().Be(45872);
        transaction.EnergyCost.Should().Be(399);
    }
    
    [Fact]
    public async Task Transactions_BasicInformation_AllNullableValuesNull()
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithSender(null)
                .Build());
        
        await WriteData();

        await using var dbContext = _dbContextFactory.CreateDbContext();
        var transaction = dbContext.Transactions.Single();
        transaction.SenderAccountAddress.Should().BeNull();
    }

    [Theory]
    [InlineData(AccountTransactionType.AddBaker)]
    [InlineData(AccountTransactionType.EncryptedTransfer)]
    [InlineData(AccountTransactionType.SimpleTransfer)]
    [InlineData(AccountTransactionType.TransferWithSchedule)]
    [InlineData(AccountTransactionType.InitializeSmartContractInstance)]
    public async Task Transactions_TransactionType_AccountTransactionTypes(AccountTransactionType transactionType)
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithType(TransactionType.Get(transactionType))
                .Build());

        await WriteData();
        
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var transaction = dbContext.Transactions.Single();
        transaction.TransactionType.Should().BeOfType<AccountTransaction>()
            .Which.AccountTransactionType.Should().Be(transactionType);
    }
    
    [Theory]
    [InlineData(CredentialDeploymentTransactionType.Initial)]
    [InlineData(CredentialDeploymentTransactionType.Normal)]
    public async Task Transactions_TransactionType_CredentialDeploymentTransactionTypes(CredentialDeploymentTransactionType transactionType)
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithType(TransactionType.Get(transactionType))
                .Build());

        await WriteData();
        
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var transaction = dbContext.Transactions.Single();
        transaction.TransactionType.Should().BeOfType<CredentialDeploymentTransaction>()
            .Which.CredentialDeploymentTransactionType.Should().Be(transactionType);
    }
    
    [Theory]
    [InlineData(UpdateTransactionType.UpdateProtocol)]
    [InlineData(UpdateTransactionType.UpdateLevel1Keys)]
    [InlineData(UpdateTransactionType.UpdateAddIdentityProvider)]
    [InlineData(UpdateTransactionType.UpdateMicroGtuPerEuro)]
    public async Task Transactions_TransactionType_UpdateTransactionTypes(UpdateTransactionType transactionType)
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithType(TransactionType.Get(transactionType))
                .Build());

        await WriteData();
        
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var transaction = dbContext.Transactions.Single();
        transaction.TransactionType.Should().BeOfType<UpdateTransaction>()
            .Which.UpdateTransactionType.Should().Be(transactionType);
    }

    [Fact]
    public async Task TransactionEvents_TransactionIdAndIndex()
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(
                        new CredentialDeployed("b5e170bfd468a55bb2bf593e7d1904936436679f448779a67d3f8632b92b1c7e7e037bf9175c257f6893d7a80f8b317d", new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd")),
                        new AccountCreated(new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd")))
                    .Build())
                .Build());
        
        await WriteData();

        await using var dbContext = _dbContextFactory.CreateDbContext();
        var transaction = dbContext.Transactions.Single();

        var result = dbContext.TransactionResultEvents.ToArray();
        result.Length.Should().Be(2);
        result[0].TransactionId.Should().Be(transaction.Id);
        result[0].Index.Should().Be(0);
        result[0].Entity.Should().BeOfType<Application.Api.GraphQL.Transactions.CredentialDeployed>();
        result[1].TransactionId.Should().Be(transaction.Id);
        result[1].Index.Should().Be(1);
        result[1].Entity.Should().BeOfType<Application.Api.GraphQL.Transactions.AccountCreated>();
    }
    
    [Fact]
    public async Task TransactionEvents_Transferred()
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new Transferred(CcdAmount.FromMicroCcd(458382), new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd"), new ContractAddress(234, 32)))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<Application.Api.GraphQL.Transactions.Transferred>();
        result.Amount.Should().Be(458382);
        result.To.Should().Be(new Application.Api.GraphQL.Accounts.AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd"));
        result.From.Should().Be(new Application.Api.GraphQL.ContractAddress(234, 32));
    }
    
    [Fact]
    public async Task TransactionEvents_AccountCreated()
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new AccountCreated(new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd")))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<Application.Api.GraphQL.Transactions.AccountCreated>();
        result.AccountAddress.AsString.Should().Be("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd");
    }
    
    [Fact]
    public async Task TransactionEvents_CredentialDeployed()
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new CredentialDeployed("b5e170bfd468a55bb2bf593e7d1904936436679f448779a67d3f8632b92b1c7e7e037bf9175c257f6893d7a80f8b317d", new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd")))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<Application.Api.GraphQL.Transactions.CredentialDeployed>();
        result.RegId.Should().Be("b5e170bfd468a55bb2bf593e7d1904936436679f448779a67d3f8632b92b1c7e7e037bf9175c257f6893d7a80f8b317d");
        result.AccountAddress.AsString.Should().Be("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd");
    }

    [Fact]
    public async Task TransactionEvents_BakerAdded()
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new BakerAdded(CcdAmount.FromMicroCcd(12551), true, 17, new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd"), "418dd98d0a42b972b974298e357132214b2821796159bfce86ffeacee567195c", "dd90b72a8044e1f82443d1531c55078516c912bf3e21633ad7a30309d781cf88", "823050dc33bd7e94ef46221f45909a2811cb99eef3a41fd9a81a622f1abdc4ef60bac6477bab0f37d000cb077b5cc61f0fa7ffc401ed14f90765d2bea15ea9c2a60010eb0aa8e702ac24f8c25dabe97a53d2d506794e552896f12e43496589f1"))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<Application.Api.GraphQL.Transactions.BakerAdded>();
        result.StakedAmount.Should().Be(12551);
        result.RestakeEarnings.Should().BeTrue();
        result.BakerId.Should().Be(17);
        result.AccountAddress.AsString.Should().Be("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd");
        result.SignKey.Should().Be("418dd98d0a42b972b974298e357132214b2821796159bfce86ffeacee567195c");
        result.ElectionKey.Should().Be("dd90b72a8044e1f82443d1531c55078516c912bf3e21633ad7a30309d781cf88");
        result.AggregationKey.Should().Be("823050dc33bd7e94ef46221f45909a2811cb99eef3a41fd9a81a622f1abdc4ef60bac6477bab0f37d000cb077b5cc61f0fa7ffc401ed14f90765d2bea15ea9c2a60010eb0aa8e702ac24f8c25dabe97a53d2d506794e552896f12e43496589f1");
    }

    [Fact]
    public async Task TransactionEvents_BakerKeysUpdated()
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new BakerKeysUpdated(19, new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd"), "418dd98d0a42b972b974298e357132214b2821796159bfce86ffeacee567195c", "dd90b72a8044e1f82443d1531c55078516c912bf3e21633ad7a30309d781cf88", "823050dc33bd7e94ef46221f45909a2811cb99eef3a41fd9a81a622f1abdc4ef60bac6477bab0f37d000cb077b5cc61f0fa7ffc401ed14f90765d2bea15ea9c2a60010eb0aa8e702ac24f8c25dabe97a53d2d506794e552896f12e43496589f1"))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<Application.Api.GraphQL.Transactions.BakerKeysUpdated>();
        result.BakerId.Should().Be(19);
        result.AccountAddress.AsString.Should().Be("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd");
        result.SignKey.Should().Be("418dd98d0a42b972b974298e357132214b2821796159bfce86ffeacee567195c");
        result.ElectionKey.Should().Be("dd90b72a8044e1f82443d1531c55078516c912bf3e21633ad7a30309d781cf88");
        result.AggregationKey.Should().Be("823050dc33bd7e94ef46221f45909a2811cb99eef3a41fd9a81a622f1abdc4ef60bac6477bab0f37d000cb077b5cc61f0fa7ffc401ed14f90765d2bea15ea9c2a60010eb0aa8e702ac24f8c25dabe97a53d2d506794e552896f12e43496589f1");
    }

    [Fact]
    public async Task TransactionEvents_BakerRemoved()
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new BakerRemoved(new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd"), 21))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<Application.Api.GraphQL.Transactions.BakerRemoved>();
        result.BakerId.Should().Be(21);
        result.AccountAddress.AsString.Should().Be("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd");
    }

    [Fact]
    public async Task TransactionEvents_BakerSetRestakeEarnings()
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new BakerSetRestakeEarnings(23, new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd"), true))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<Application.Api.GraphQL.Transactions.BakerSetRestakeEarnings>();
        result.BakerId.Should().Be(23);
        result.AccountAddress.AsString.Should().Be("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd");
        result.RestakeEarnings.Should().BeTrue();
    }

    [Fact]
    public async Task TransactionEvents_BakerStakeDecreased()
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new BakerStakeDecreased(23, new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd"), CcdAmount.FromMicroCcd(34786451)))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<Application.Api.GraphQL.Transactions.BakerStakeDecreased>();
        result.BakerId.Should().Be(23);
        result.AccountAddress.AsString.Should().Be("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd");
        result.NewStakedAmount.Should().Be(34786451);
    }

    [Fact]
    public async Task TransactionEvents_BakerStakeIncreased()
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new BakerStakeIncreased(23, new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd"), CcdAmount.FromMicroCcd(34786451)))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<Application.Api.GraphQL.Transactions.BakerStakeIncreased>();
        result.BakerId.Should().Be(23);
        result.AccountAddress.AsString.Should().Be("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd");
        result.NewStakedAmount.Should().Be(34786451);
    }

    [Fact]
    public async Task TransactionEvents_AmountAddedByDecryption()
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new AmountAddedByDecryption(CcdAmount.FromMicroCcd(2362462), new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd")))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<Application.Api.GraphQL.Transactions.AmountAddedByDecryption>();
        result.Amount.Should().Be(2362462);
        result.AccountAddress.AsString.Should().Be("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd");
    }

    [Fact]
    public async Task TransactionEvents_EncryptedAmountsRemoved()
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new EncryptedAmountsRemoved(new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd"), "8127cc7b219f268461b83c2397573b41815a4c4246b03e17184275ea158561d68bb526a2b5f69eb3ef5c5400927a6c528c461717287f5ec5f31bc0469f1f562f08a270f194963adf814e20fa632782de005efb59014490a2d7a726f2b626d12ab4e23198006317c29cbe3882030ba8f561ba52e6684408ea6e4471871f2f4e043cb2e036bc8e1d53b8d784b61c4cba5ca60c4a8172d9c50f5d56c16640f46f08f1f3224d8fbfa56482547af30b60a21cc24392c1e68df8dcba86bda4e3088fd2", "acde243d9f17432a12a04bd553846a9464ecd6c59be5bc3fd6b58d608b002c725c7f495f3c9fe80510d52a739bc5b67280b612dec5a2212bdb3257136fbe5703a3c159a3cda1e70aed0ce69245c8dc6f7c3f374bde1f7584dce9c90b288d3eef8b48cd548dfdeac5d58b0c32585d26c181f142f1e47f9c6695a6abe6a008a7bce1bc02f71f880e198acb03550c50de8daf1e25967487a5f1a9d0ee1afdee9f50c4d2a9fc849d5b234dd47a3af95a7a4e2df78923e39e60ac55d60fd90b4e9074", 789))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<Application.Api.GraphQL.Transactions.EncryptedAmountsRemoved>();
        result.AccountAddress.AsString.Should().Be("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd");
        result.NewEncryptedAmount.Should().Be("8127cc7b219f268461b83c2397573b41815a4c4246b03e17184275ea158561d68bb526a2b5f69eb3ef5c5400927a6c528c461717287f5ec5f31bc0469f1f562f08a270f194963adf814e20fa632782de005efb59014490a2d7a726f2b626d12ab4e23198006317c29cbe3882030ba8f561ba52e6684408ea6e4471871f2f4e043cb2e036bc8e1d53b8d784b61c4cba5ca60c4a8172d9c50f5d56c16640f46f08f1f3224d8fbfa56482547af30b60a21cc24392c1e68df8dcba86bda4e3088fd2");
        result.InputAmount.Should().Be("acde243d9f17432a12a04bd553846a9464ecd6c59be5bc3fd6b58d608b002c725c7f495f3c9fe80510d52a739bc5b67280b612dec5a2212bdb3257136fbe5703a3c159a3cda1e70aed0ce69245c8dc6f7c3f374bde1f7584dce9c90b288d3eef8b48cd548dfdeac5d58b0c32585d26c181f142f1e47f9c6695a6abe6a008a7bce1bc02f71f880e198acb03550c50de8daf1e25967487a5f1a9d0ee1afdee9f50c4d2a9fc849d5b234dd47a3af95a7a4e2df78923e39e60ac55d60fd90b4e9074");
        result.UpToIndex.Should().Be(789);
    }

    [Fact]
    public async Task TransactionEvents_EncryptedSelfAmountAdded()
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new EncryptedSelfAmountAdded(new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd"), "8127cc7b219f268461b83c2397573b41815a4c4246b03e17184275ea158561d68bb526a2b5f69eb3ef5c5400927a6c528c461717287f5ec5f31bc0469f1f562f08a270f194963adf814e20fa632782de005efb59014490a2d7a726f2b626d12ab4e23198006317c29cbe3882030ba8f561ba52e6684408ea6e4471871f2f4e043cb2e036bc8e1d53b8d784b61c4cba5ca60c4a8172d9c50f5d56c16640f46f08f1f3224d8fbfa56482547af30b60a21cc24392c1e68df8dcba86bda4e3088fd2", CcdAmount.FromMicroCcd(23446)))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<Application.Api.GraphQL.Transactions.EncryptedSelfAmountAdded>();
        result.AccountAddress.AsString.Should().Be("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd");
        result.NewEncryptedAmount.Should().Be("8127cc7b219f268461b83c2397573b41815a4c4246b03e17184275ea158561d68bb526a2b5f69eb3ef5c5400927a6c528c461717287f5ec5f31bc0469f1f562f08a270f194963adf814e20fa632782de005efb59014490a2d7a726f2b626d12ab4e23198006317c29cbe3882030ba8f561ba52e6684408ea6e4471871f2f4e043cb2e036bc8e1d53b8d784b61c4cba5ca60c4a8172d9c50f5d56c16640f46f08f1f3224d8fbfa56482547af30b60a21cc24392c1e68df8dcba86bda4e3088fd2");
        result.Amount.Should().Be(23446);
    }

    [Fact]
    public async Task TransactionEvents_NewEncryptedAmount()
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new NewEncryptedAmount(new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd"), 155, "8127cc7b219f268461b83c2397573b41815a4c4246b03e17184275ea158561d68bb526a2b5f69eb3ef5c5400927a6c528c461717287f5ec5f31bc0469f1f562f08a270f194963adf814e20fa632782de005efb59014490a2d7a726f2b626d12ab4e23198006317c29cbe3882030ba8f561ba52e6684408ea6e4471871f2f4e043cb2e036bc8e1d53b8d784b61c4cba5ca60c4a8172d9c50f5d56c16640f46f08f1f3224d8fbfa56482547af30b60a21cc24392c1e68df8dcba86bda4e3088fd2"))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<Application.Api.GraphQL.Transactions.NewEncryptedAmount>();
        result.AccountAddress.AsString.Should().Be("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd");
        result.NewIndex.Should().Be(155);
        result.EncryptedAmount.Should().Be("8127cc7b219f268461b83c2397573b41815a4c4246b03e17184275ea158561d68bb526a2b5f69eb3ef5c5400927a6c528c461717287f5ec5f31bc0469f1f562f08a270f194963adf814e20fa632782de005efb59014490a2d7a726f2b626d12ab4e23198006317c29cbe3882030ba8f561ba52e6684408ea6e4471871f2f4e043cb2e036bc8e1d53b8d784b61c4cba5ca60c4a8172d9c50f5d56c16640f46f08f1f3224d8fbfa56482547af30b60a21cc24392c1e68df8dcba86bda4e3088fd2");
    }

    [Fact]
    public async Task TransactionEvents_CredentialKeysUpdated()
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new CredentialKeysUpdated("b5e170bfd468a55bb2bf593e7d1904936436679f448779a67d3f8632b92b1c7e7e037bf9175c257f6893d7a80f8b317d"))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<Application.Api.GraphQL.Transactions.CredentialKeysUpdated>();
        result.CredId.Should().Be("b5e170bfd468a55bb2bf593e7d1904936436679f448779a67d3f8632b92b1c7e7e037bf9175c257f6893d7a80f8b317d");
    }

    [Fact]
    public async Task TransactionEvents_CredentialsUpdated()
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new CredentialsUpdated(new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd"), new []{"b5e170bfd468a55bb2bf593e7d1904936436679f448779a67d3f8632b92b1c7e7e037bf9175c257f6893d7a80f8b317d"}, new string[0], 123))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<Application.Api.GraphQL.Transactions.CredentialsUpdated>();
        result.AccountAddress.AsString.Should().Be("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd");
        result.NewCredIds.Should().Equal("b5e170bfd468a55bb2bf593e7d1904936436679f448779a67d3f8632b92b1c7e7e037bf9175c257f6893d7a80f8b317d");
        result.RemovedCredIds.Should().BeEmpty();
        result.NewThreshold.Should().Be(123);
    }

    [Fact]
    public async Task TransactionEvents_ContractInitialized()
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new ContractInitialized(new ModuleRef("2ff7af94aa3e338912d398309531578bd8b7dc903c974111c8d63f4b7098cecb"), new ContractAddress(1423, 1), CcdAmount.FromMicroCcd(5345462), "init_CIS1-singleNFT", new []{ BinaryData.FromHexString("fe00010000000000000000736e8b0e5f740321883ee1cf6a75e2d9ba31d3c33cfaf265807b352db91a53c4"), BinaryData.FromHexString("fb00160068747470733a2f2f636f6e636f726469756d2e636f6d00")}))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<Application.Api.GraphQL.Transactions.ContractInitialized>();
        result.ModuleRef.Should().Be("2ff7af94aa3e338912d398309531578bd8b7dc903c974111c8d63f4b7098cecb");
        result.ContractAddress.Should().Be(new Application.Api.GraphQL.ContractAddress(1423, 1));
        result.Amount.Should().Be(5345462);
        result.InitName.Should().Be("init_CIS1-singleNFT");
        result.EventsAsHex.Should().Equal("fe00010000000000000000736e8b0e5f740321883ee1cf6a75e2d9ba31d3c33cfaf265807b352db91a53c4", "fb00160068747470733a2f2f636f6e636f726469756d2e636f6d00");
    }

    [Fact]
    public async Task TransactionEvents_ContractModuleDeployed()
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new ModuleDeployed(new ModuleRef("2ff7af94aa3e338912d398309531578bd8b7dc903c974111c8d63f4b7098cecb")))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<ContractModuleDeployed>();
        result.ModuleRef.Should().Be("2ff7af94aa3e338912d398309531578bd8b7dc903c974111c8d63f4b7098cecb");
    }

    [Fact]
    public async Task TransactionEvents_ContractUpdated()
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new Updated(
                        new ContractAddress(1423, 1),
                        new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd"),
                        CcdAmount.FromMicroCcd(15674371),
                        BinaryData.FromHexString("080000d671a4d50101c0196da50d25f71a236ec71cedc9ba2d49c8c6fc9fa98df7475d3bfbc7612c32"), 
                        "inventory.transfer", 
                        new []
                        {
                            BinaryData.FromHexString("05080000d671a4d501aa3a794db185bb8ac998abe33146301afcb53f78d58266c6417cb9d859c90309c0196da50d25f71a236ec71cedc9ba2d49c8c6fc9fa98df7475d3bfbc7612c32"),
                            BinaryData.FromHexString("01080000d671a4d50101aa3a794db185bb8ac998abe33146301afcb53f78d58266c6417cb9d859c9030901c0196da50d25f71a236ec71cedc9ba2d49c8c6fc9fa98df7475d3bfbc7612c32")
                        }))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<ContractUpdated>();
        result.ContractAddress.Should().Be(new Application.Api.GraphQL.ContractAddress(1423, 1));
        result.Instigator.Should().Be(new Application.Api.GraphQL.Accounts.AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd"));
        result.Amount.Should().Be(15674371);
        result.MessageAsHex.Should().Be("080000d671a4d50101c0196da50d25f71a236ec71cedc9ba2d49c8c6fc9fa98df7475d3bfbc7612c32");
        result.ReceiveName.Should().Be("inventory.transfer");
        result.EventsAsHex.Should().Equal("05080000d671a4d501aa3a794db185bb8ac998abe33146301afcb53f78d58266c6417cb9d859c90309c0196da50d25f71a236ec71cedc9ba2d49c8c6fc9fa98df7475d3bfbc7612c32", "01080000d671a4d50101aa3a794db185bb8ac998abe33146301afcb53f78d58266c6417cb9d859c9030901c0196da50d25f71a236ec71cedc9ba2d49c8c6fc9fa98df7475d3bfbc7612c32");
    }

    [Fact]
    public async Task TransactionEvents_TransferredWithSchedule()
    {
        var baseTimestamp = new DateTimeOffset(2010, 10, 01, 12, 0, 0, TimeSpan.Zero);
        
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new TransferredWithSchedule(
                        new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd"), 
                        new AccountAddress("3rAsvTuH2gQawenRgwJQzrk9t4Kd2Y1uZYinLqJRDAHZKJKEeH"), 
                        new []
                        {
                            new TimestampedAmount(baseTimestamp.AddHours(10), CcdAmount.FromMicroCcd(1000)),
                            new TimestampedAmount(baseTimestamp.AddHours(20), CcdAmount.FromMicroCcd(3333)),
                            new TimestampedAmount(baseTimestamp.AddHours(30), CcdAmount.FromMicroCcd(2111)),
                        }))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<Application.Api.GraphQL.Transactions.TransferredWithSchedule>();
        result.FromAccountAddress.AsString.Should().Be("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd");
        result.ToAccountAddress.AsString.Should().Be("3rAsvTuH2gQawenRgwJQzrk9t4Kd2Y1uZYinLqJRDAHZKJKEeH");
        result.AmountsSchedule.Should().Equal(
            new Application.Api.GraphQL.TimestampedAmount(baseTimestamp.AddHours(10), 1000),
            new Application.Api.GraphQL.TimestampedAmount(baseTimestamp.AddHours(20), 3333),
            new Application.Api.GraphQL.TimestampedAmount(baseTimestamp.AddHours(30), 2111));
    }
    
    [Fact]
    public async Task TransactionEvents_DataRegistered()
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new DataRegistered(RegisteredData.FromHexString("784747502d3030323a32636565666132633339396239353639343138353532363032623063383965376665313935303465336438623030333035336339616435623361303365353863")))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<Application.Api.GraphQL.Transactions.DataRegistered>();
        result.DataAsHex.Should().Be("784747502d3030323a32636565666132633339396239353639343138353532363032623063383965376665313935303465336438623030333035336339616435623361303365353863");
    }

    [Fact]
    public async Task TransactionEvents_TransferMemo()
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new TransferMemo(Memo.CreateFromHex("704164616d2042696c6c696f6e61697265")))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<Application.Api.GraphQL.Transactions.TransferMemo>();
        result.RawHex.Should().Be("704164616d2042696c6c696f6e61697265");
    }

    [Fact]
    public async Task TransactionEvents_ChainUpdateEnqueued_MicroGtuPerEuroPayload() 
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new UpdateEnqueued(new UnixTimeSeconds(1624630671), new MicroGtuPerEuroUpdatePayload(new ExchangeRate(1, 2))))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<ChainUpdateEnqueued>();
        result.EffectiveTime.Should().Be(DateTimeOffset.FromUnixTimeSeconds(1624630671));
        var item = Assert.IsType<MicroCcdPerEuroChainUpdatePayload>(result.Payload);
        item.ExchangeRate.Numerator.Should().Be(1);
        item.ExchangeRate.Denominator.Should().Be(2);
    }

    [Fact]
    public async Task TransactionEvents_ChainUpdateEnqueued_CooldownParametersUpdatePayload() 
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new UpdateEnqueued(new UnixTimeSeconds(1624630671), new CooldownParametersUpdatePayload(new CooldownParameters(20, 40))))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<ChainUpdateEnqueued>();
        result.EffectiveTime.Should().Be(DateTimeOffset.FromUnixTimeSeconds(1624630671));
        var item = Assert.IsType<CooldownParametersChainUpdatePayload>(result.Payload);
        item.PoolOwnerCooldown.Should().Be(20);
        item.DelegatorCooldown.Should().Be(40);
    }

    [Fact]
    public async Task TransactionEvents_ChainUpdateEnqueued_PoolParametersUpdatePayload() 
    {
        var payload = new PoolParametersUpdatePayload(new PoolParameters(
            0.1m, 0.2m, 0.3m, 
            new InclusiveRange<decimal>(1.0m, 1.2m),
            new InclusiveRange<decimal>(2.0m, 2.2m),
            new InclusiveRange<decimal>(3.0m, 3.2m),
            12000UL, 3.0m, new LeverageFactor(13, 17)));
        
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new UpdateEnqueued(new UnixTimeSeconds(1624630671), payload))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<ChainUpdateEnqueued>();
        result.EffectiveTime.Should().Be(DateTimeOffset.FromUnixTimeSeconds(1624630671));
        var item = Assert.IsType<PoolParametersChainUpdatePayload>(result.Payload);
        item.PassiveFinalizationCommission.Should().Be(0.1m);
        item.PassiveBakingCommission.Should().Be(0.2m);
        item.PassiveTransactionCommission.Should().Be(0.3m);
        item.FinalizationCommissionRange.Min.Should().Be(1.0m);
        item.FinalizationCommissionRange.Max.Should().Be(1.2m);
        item.BakingCommissionRange.Min.Should().Be(2.0m);
        item.BakingCommissionRange.Max.Should().Be(2.2m);
        item.TransactionCommissionRange.Min.Should().Be(3.0m);
        item.TransactionCommissionRange.Max.Should().Be(3.2m);
        item.MinimumEquityCapital.Should().Be(12000UL);
        item.CapitalBound.Should().Be(3.0m);
        item.LeverageBound.Numerator.Should().Be(13);
        item.LeverageBound.Denominator.Should().Be(17);
    }

    [Fact]
    public async Task TransactionEvents_ChainUpdateEnqueued_TimeParametersUpdatePayload() 
    {
        var payload = new TimeParametersUpdatePayload(new TimeParameters(170, 4.2m));
        
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new UpdateEnqueued(new UnixTimeSeconds(1624630671), payload))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<ChainUpdateEnqueued>();
        result.EffectiveTime.Should().Be(DateTimeOffset.FromUnixTimeSeconds(1624630671));
        var item = Assert.IsType<TimeParametersChainUpdatePayload>(result.Payload);
        item.RewardPeriodLength.Should().Be(170);
        item.MintPerPayday.Should().Be(4.2m);
    }

    [Fact]
    public async Task TransactionEvents_ChainUpdateEnqueued_MintDistributionV1UpdatePayload() 
    {
        var payload = new MintDistributionV1UpdatePayload(new MintDistributionV1(1.1m, 0.5m));
        
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new UpdateEnqueued(new UnixTimeSeconds(1624630671), payload))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<ChainUpdateEnqueued>();
        result.EffectiveTime.Should().Be(DateTimeOffset.FromUnixTimeSeconds(1624630671));
        var item = Assert.IsType<MintDistributionV1ChainUpdatePayload>(result.Payload);
        item.BakingReward.Should().Be(1.1m);
        item.FinalizationReward.Should().Be(0.5m);
    }

    [Theory]
    [InlineData(BakerPoolOpenStatus.OpenForAll, Application.Api.GraphQL.Bakers.BakerPoolOpenStatus.OpenForAll)]
    [InlineData(BakerPoolOpenStatus.ClosedForNew, Application.Api.GraphQL.Bakers.BakerPoolOpenStatus.ClosedForNew)]
    [InlineData(BakerPoolOpenStatus.ClosedForAll, Application.Api.GraphQL.Bakers.BakerPoolOpenStatus.ClosedForAll)]
    public async Task TransactionEvents_BakerSetOpenStatus(BakerPoolOpenStatus inputStatus, Application.Api.GraphQL.Bakers.BakerPoolOpenStatus expectedStatus) 
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new BakerSetOpenStatus(42, new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd"), inputStatus))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<Application.Api.GraphQL.Transactions.BakerSetOpenStatus>();
        result.BakerId.Should().Be(42);
        result.AccountAddress.AsString.Should().Be("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd");
        result.OpenStatus.Should().Be(expectedStatus);
    }

    [Fact]
    public async Task TransactionEvents_BakerSetTransactionFeeCommission() 
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new BakerSetTransactionFeeCommission(42, new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd"), 0.9m))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<Application.Api.GraphQL.Transactions.BakerSetTransactionFeeCommission>();
        result.BakerId.Should().Be(42);
        result.AccountAddress.AsString.Should().Be("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd");
        result.TransactionFeeCommission.Should().Be(0.9m);
    }

    [Fact]
    public async Task TransactionEvents_BakerSetMetadataURL() 
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new BakerSetMetadataURL(42, new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd"), "https://ccd.bakers.com/metadata"))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<Application.Api.GraphQL.Transactions.BakerSetMetadataURL>();
        result.BakerId.Should().Be(42);
        result.AccountAddress.AsString.Should().Be("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd");
        result.MetadataUrl.Should().Be("https://ccd.bakers.com/metadata");
    }

    [Fact]
    public async Task TransactionEvents_BakerSetBakingRewardCommission() 
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new BakerSetBakingRewardCommission(42, new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd"), 0.9m))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<Application.Api.GraphQL.Transactions.BakerSetBakingRewardCommission>();
        result.BakerId.Should().Be(42);
        result.AccountAddress.AsString.Should().Be("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd");
        result.BakingRewardCommission.Should().Be(0.9m);
    }

    [Fact]
    public async Task TransactionEvents_BakerSetFinalizationRewardCommission() 
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new BakerSetFinalizationRewardCommission(42, new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd"), 0.9m))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<Application.Api.GraphQL.Transactions.BakerSetFinalizationRewardCommission>();
        result.BakerId.Should().Be(42);
        result.AccountAddress.AsString.Should().Be("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd");
        result.FinalizationRewardCommission.Should().Be(0.9m);
    }

    [Fact]
    public async Task TransactionEvents_DelegationAdded() 
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new DelegationAdded(42, new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd")))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<Application.Api.GraphQL.Transactions.DelegationAdded>();
        result.DelegatorId.Should().Be(42);
        result.AccountAddress.AsString.Should().Be("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd");
    }

    [Fact]
    public async Task TransactionEvents_DelegationRemoved() 
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new DelegationRemoved(42, new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd")))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<Application.Api.GraphQL.Transactions.DelegationRemoved>();
        result.DelegatorId.Should().Be(42);
        result.AccountAddress.AsString.Should().Be("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd");
    }

    [Fact]
    public async Task TransactionEvents_DelegationStakeIncreased() 
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new DelegationStakeIncreased(42, new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd"), CcdAmount.FromMicroCcd(758111)))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<Application.Api.GraphQL.Transactions.DelegationStakeIncreased>();
        result.DelegatorId.Should().Be(42);
        result.AccountAddress.AsString.Should().Be("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd");
        result.NewStakedAmount.Should().Be(758111);
    }

    [Fact]
    public async Task TransactionEvents_DelegationStakeDecreased() 
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new DelegationStakeDecreased(42, new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd"), CcdAmount.FromMicroCcd(758111)))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<Application.Api.GraphQL.Transactions.DelegationStakeDecreased>();
        result.DelegatorId.Should().Be(42);
        result.AccountAddress.AsString.Should().Be("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd");
        result.NewStakedAmount.Should().Be(758111);
    }

    [Fact]
    public async Task TransactionEvents_DelegationSetRestakeEarnings() 
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new DelegationSetRestakeEarnings(42, new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd"), true))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<Application.Api.GraphQL.Transactions.DelegationSetRestakeEarnings>();
        result.DelegatorId.Should().Be(42);
        result.AccountAddress.AsString.Should().Be("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd");
        result.RestakeEarnings.Should().BeTrue();
    }

    [Fact]
    public async Task TransactionEvents_DelegationSetDelegationTarget() 
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionSuccessResultBuilder()
                    .WithEvents(new DelegationSetDelegationTarget(42, new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd"), new PassiveDelegationTarget()))
                    .Build())
                .Build());
        
        await WriteData();

        var result = await ReadSingleTransactionEventType<Application.Api.GraphQL.Transactions.DelegationSetDelegationTarget>();
        result.DelegatorId.Should().Be(42);
        result.AccountAddress.AsString.Should().Be("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd");
        result.DelegationTarget.Should().BeOfType<Application.Api.GraphQL.PassiveDelegationTarget>();
    }

    [Fact]
    public async Task TransactionRejectReason_ModuleNotWf()
    {
        var inputReason = new ModuleNotWf();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.ModuleNotWf>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task TransactionRejectReason_ModuleHashAlreadyExists()
    {
        var inputReason = new ModuleHashAlreadyExists(new ModuleRef("2ff7af94aa3e338912d398309531578bd8b7dc903c974111c8d63f4b7098cecb"));
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.ModuleHashAlreadyExists>();
        result.ModuleRef.Should().Be("2ff7af94aa3e338912d398309531578bd8b7dc903c974111c8d63f4b7098cecb");
    }

    [Fact]
    public async Task TransactionRejectReason_InvalidAccountReference()
    {
        var inputReason = new InvalidAccountReference(new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd"));
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.InvalidAccountReference>();
        result.AccountAddress.AsString.Should().Be("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd");
    }

    [Fact]
    public async Task TransactionRejectReason_InvalidInitMethod()
    {
        var inputReason = new InvalidInitMethod(new ModuleRef("2ff7af94aa3e338912d398309531578bd8b7dc903c974111c8d63f4b7098cecb"), "trader.init");
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.InvalidInitMethod>();
        result.ModuleRef.Should().Be("2ff7af94aa3e338912d398309531578bd8b7dc903c974111c8d63f4b7098cecb");
        result.InitName.Should().Be("trader.init");
    }

    [Fact]
    public async Task TransactionRejectReason_InvalidReceiveMethod()
    {
        var inputReason = new InvalidReceiveMethod(new ModuleRef("2ff7af94aa3e338912d398309531578bd8b7dc903c974111c8d63f4b7098cecb"), "trader.receive");
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.InvalidReceiveMethod>();
        result.ModuleRef.Should().Be("2ff7af94aa3e338912d398309531578bd8b7dc903c974111c8d63f4b7098cecb");
        result.ReceiveName.Should().Be("trader.receive");
    }

    [Fact]
    public async Task TransactionRejectReason_InvalidModuleReference()
    {
        var inputReason = new InvalidModuleReference(new ModuleRef("2ff7af94aa3e338912d398309531578bd8b7dc903c974111c8d63f4b7098cecb"));
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.InvalidModuleReference>();
        result.ModuleRef.Should().Be("2ff7af94aa3e338912d398309531578bd8b7dc903c974111c8d63f4b7098cecb");
    }

    [Fact]
    public async Task TransactionRejectReason_InvalidContractAddress()
    {
        var inputReason = new InvalidContractAddress(new ContractAddress(187, 22));
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.InvalidContractAddress>();
        result.ContractAddress.Should().Be(new Application.Api.GraphQL.ContractAddress(187, 22));
    }

    [Fact]
    public async Task TransactionRejectReason_RuntimeFailure()
    {
        var inputReason = new RuntimeFailure();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.RuntimeFailure>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task TransactionRejectReason_AmountTooLarge()
    {
        var inputReason = new AmountTooLarge(new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd"), CcdAmount.FromMicroCcd(34656));
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.AmountTooLarge>();
        result.Address.Should().Be(new Application.Api.GraphQL.Accounts.AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd"));
        result.Amount.Should().Be(34656);
    }

    [Fact]
    public async Task TransactionRejectReason_SerializationFailure()
    {
        var inputReason = new SerializationFailure();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.SerializationFailure>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task TransactionRejectReason_OutOfEnergy()
    {
        var inputReason = new OutOfEnergy();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.OutOfEnergy>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task TransactionRejectReason_RejectedInit()
    {
        var inputReason = new RejectedInit(-48518);
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.RejectedInit>();
        result.RejectReason.Should().Be(-48518);
    }

    [Fact]
    public async Task TransactionRejectReason_RejectedReceive()
    {
        var inputReason = new RejectedReceive(
            -48518,
            new ContractAddress(187, 22),
            "trader.dostuff",
            BinaryData.FromHexString("fb00160068747470733a2f2f636f6e636f726469756d2e636f6d00"));
        
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.RejectedReceive>();
        result.RejectReason.Should().Be(-48518);
        result.ContractAddress.Should().Be(new Application.Api.GraphQL.ContractAddress(187, 22));
        result.ReceiveName.Should().Be("trader.dostuff");
        result.MessageAsHex.Should().Be("fb00160068747470733a2f2f636f6e636f726469756d2e636f6d00");
    }

    [Fact]
    public async Task TransactionRejectReason_NonExistentRewardAccount()
    {
        var inputReason = new NonExistentRewardAccount(new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd"));
        
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.NonExistentRewardAccount>();
        result.AccountAddress.AsString.Should().Be("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd");
    }

    [Fact]
    public async Task TransactionRejectReason_InvalidProof()
    {
        var inputReason = new InvalidProof();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.InvalidProof>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task TransactionRejectReason_AlreadyABaker()
    {
        var inputReason = new AlreadyABaker(45);
        
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.AlreadyABaker>();
        result.BakerId.Should().Be(45);
    }

    [Fact]
    public async Task TransactionRejectReason_NotABaker()
    {
        var inputReason = new NotABaker(new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd"));
        
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.NotABaker>();
        result.AccountAddress.AsString.Should().Be("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd");
    }

    [Fact]
    public async Task TransactionRejectReason_InsufficientBalanceForBakerStake()
    {
        var inputReason = new InsufficientBalanceForBakerStake();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.InsufficientBalanceForBakerStake>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task TransactionRejectReason_StakeUnderMinimumThresholdForBaking()
    {
        var inputReason = new StakeUnderMinimumThresholdForBaking();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.StakeUnderMinimumThresholdForBaking>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task TransactionRejectReason_BakerInCooldown()
    {
        var inputReason = new BakerInCooldown();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.BakerInCooldown>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task TransactionRejectReason_DuplicateAggregationKey()
    {
        var inputReason = new DuplicateAggregationKey("98528ef89dc117f102ef3f089c81b92e4d945d22c0269269af6ef9f876d79e828b31b8b4b8cc3d9234c30e83bd79e20a0a807bc110f0ac9babae90cb6a8c6d0deb2e5627704b41bdd646a547895fd1f9f2a7b0dd4fb4e138356e91d002a28f83");
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.DuplicateAggregationKey>();
        result.AggregationKey.Should().Be("98528ef89dc117f102ef3f089c81b92e4d945d22c0269269af6ef9f876d79e828b31b8b4b8cc3d9234c30e83bd79e20a0a807bc110f0ac9babae90cb6a8c6d0deb2e5627704b41bdd646a547895fd1f9f2a7b0dd4fb4e138356e91d002a28f83");
    }

    [Fact]
    public async Task TransactionRejectReason_NonExistentCredentialId()
    {
        var inputReason = new NonExistentCredentialId();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.NonExistentCredentialId>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task TransactionRejectReason_KeyIndexAlreadyInUse()
    {
        var inputReason = new KeyIndexAlreadyInUse();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.KeyIndexAlreadyInUse>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task TransactionRejectReason_InvalidAccountThreshold()
    {
        var inputReason = new InvalidAccountThreshold();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.InvalidAccountThreshold>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task TransactionRejectReason_InvalidCredentialKeySignThreshold()
    {
        var inputReason = new InvalidCredentialKeySignThreshold();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.InvalidCredentialKeySignThreshold>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task TransactionRejectReason_InvalidEncryptedAmountTransferProof()
    {
        var inputReason = new InvalidEncryptedAmountTransferProof();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.InvalidEncryptedAmountTransferProof>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task TransactionRejectReason_InvalidTransferToPublicProof()
    {
        var inputReason = new InvalidTransferToPublicProof();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.InvalidTransferToPublicProof>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task TransactionRejectReason_EncryptedAmountSelfTransfer()
    {
        var inputReason = new EncryptedAmountSelfTransfer(new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd"));
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.EncryptedAmountSelfTransfer>();
        result.AccountAddress.AsString.Should().Be("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd");
    }

    [Fact]
    public async Task TransactionRejectReason_InvalidIndexOnEncryptedTransfer()
    {
        var inputReason = new InvalidIndexOnEncryptedTransfer();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.InvalidIndexOnEncryptedTransfer>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task TransactionRejectReason_ZeroScheduledAmount()
    {
        var inputReason = new ZeroScheduledAmount();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.ZeroScheduledAmount>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task TransactionRejectReason_NonIncreasingSchedule()
    {
        var inputReason = new NonIncreasingSchedule();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.NonIncreasingSchedule>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task TransactionRejectReason_FirstScheduledReleaseExpired()
    {
        var inputReason = new FirstScheduledReleaseExpired();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.FirstScheduledReleaseExpired>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task TransactionRejectReason_ScheduledSelfTransfer()
    {
        var inputReason = new ScheduledSelfTransfer(new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd"));
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.ScheduledSelfTransfer>();
        result.AccountAddress.AsString.Should().Be("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd");
    }

    [Fact]
    public async Task TransactionRejectReason_InvalidCredentials()
    {
        var inputReason = new InvalidCredentials();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.InvalidCredentials>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task TransactionRejectReason_DuplicateCredIds()
    {
        var inputReason = new DuplicateCredIds(new[] { "b9a35cfb9556b897d3c1e81ab8247e916762755a7673bd493a2062a6988033e6a37d88c366a89109fa6e26ba7a317b7f" });
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.DuplicateCredIds>();
        result.CredIds.Should().ContainSingle().Which.Should().Be("b9a35cfb9556b897d3c1e81ab8247e916762755a7673bd493a2062a6988033e6a37d88c366a89109fa6e26ba7a317b7f");
    }

    [Fact]
    public async Task TransactionRejectReason_NonExistentCredIds()
    {
        var inputReason = new NonExistentCredIds(new[] { "b9a35cfb9556b897d3c1e81ab8247e916762755a7673bd493a2062a6988033e6a37d88c366a89109fa6e26ba7a317b7f" });
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.NonExistentCredIds>();
        result.CredIds.Should().ContainSingle().Which.Should().Be("b9a35cfb9556b897d3c1e81ab8247e916762755a7673bd493a2062a6988033e6a37d88c366a89109fa6e26ba7a317b7f");
    }

    [Fact]
    public async Task TransactionRejectReason_RemoveFirstCredential()
    {
        var inputReason = new RemoveFirstCredential();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.RemoveFirstCredential>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task TransactionRejectReason_CredentialHolderDidNotSign()
    {
        var inputReason = new CredentialHolderDidNotSign();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.CredentialHolderDidNotSign>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task TransactionRejectReason_NotAllowedMultipleCredentials()
    {
        var inputReason = new NotAllowedMultipleCredentials();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.NotAllowedMultipleCredentials>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task TransactionRejectReason_NotAllowedToReceiveEncrypted()
    {
        var inputReason = new NotAllowedToReceiveEncrypted();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.NotAllowedToReceiveEncrypted>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task TransactionRejectReason_NotAllowedToHandleEncrypted()
    {
        var inputReason = new NotAllowedToHandleEncrypted();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.NotAllowedToHandleEncrypted>();
        result.Should().NotBeNull();
    }
    
    [Fact]
    public async Task TransactionRejectReason_MissingBakerAddParameters()
    {
        var inputReason = new global::ConcordiumSdk.NodeApi.Types.MissingBakerAddParameters();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.MissingBakerAddParameters>();
        result.Should().NotBeNull();
    }
    
    [Fact]
    public async Task TransactionRejectReason_FinalizationRewardCommissionNotInRange()
    {
        var inputReason = new global::ConcordiumSdk.NodeApi.Types.FinalizationRewardCommissionNotInRange();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.FinalizationRewardCommissionNotInRange>();
        result.Should().NotBeNull();
    }
    
    [Fact]
    public async Task TransactionRejectReason_BakingRewardCommissionNotInRange()
    {
        var inputReason = new global::ConcordiumSdk.NodeApi.Types.BakingRewardCommissionNotInRange();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.BakingRewardCommissionNotInRange>();
        result.Should().NotBeNull();
    }
    
    [Fact]
    public async Task TransactionRejectReason_TransactionFeeCommissionNotInRange()
    {
        var inputReason = new global::ConcordiumSdk.NodeApi.Types.TransactionFeeCommissionNotInRange();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.TransactionFeeCommissionNotInRange>();
        result.Should().NotBeNull();
    }
    
    [Fact]
    public async Task TransactionRejectReason_AlreadyADelegator()
    {
        var inputReason = new global::ConcordiumSdk.NodeApi.Types.AlreadyADelegator();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.AlreadyADelegator>();
        result.Should().NotBeNull();
    }
    
    [Fact]
    public async Task TransactionRejectReason_InsufficientBalanceForDelegationStake()
    {
        var inputReason = new global::ConcordiumSdk.NodeApi.Types.InsufficientBalanceForDelegationStake();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.InsufficientBalanceForDelegationStake>();
        result.Should().NotBeNull();
    }
    
    [Fact]
    public async Task TransactionRejectReason_MissingDelegationAddParameters()
    {
        var inputReason = new global::ConcordiumSdk.NodeApi.Types.MissingDelegationAddParameters();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.MissingDelegationAddParameters>();
        result.Should().NotBeNull();
    }
    
    [Fact]
    public async Task TransactionRejectReason_InsufficientDelegationStake()
    {
        var inputReason = new global::ConcordiumSdk.NodeApi.Types.InsufficientDelegationStake();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.InsufficientDelegationStake>();
        result.Should().NotBeNull();
    }
    
    [Fact]
    public async Task TransactionRejectReason_DelegatorInCooldown()
    {
        var inputReason = new global::ConcordiumSdk.NodeApi.Types.DelegatorInCooldown();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.DelegatorInCooldown>();
        result.Should().NotBeNull();
    }
    
    [Fact]
    public async Task TransactionRejectReason_NotADelegator()
    {
        var inputReason = new global::ConcordiumSdk.NodeApi.Types.NotADelegator(new AccountAddress("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd"));
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.NotADelegator>();
        result.Should().NotBeNull();
        result.AccountAddress.AsString.Should().Be("31JA2dWnv6xHrdP73kLKvWqr5RMfqoeuJXG2Mep1iyQV9E5aSd");
    }
    
    [Fact]
    public async Task TransactionRejectReason_DelegationTargetNotABaker()
    {
        var inputReason = new global::ConcordiumSdk.NodeApi.Types.DelegationTargetNotABaker(42UL);
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.DelegationTargetNotABaker>();
        result.Should().NotBeNull();
        result.BakerId.Should().Be(42UL);
    }
    
    [Fact]
    public async Task TransactionRejectReason_StakeOverMaximumThresholdForPool()
    {
        var inputReason = new global::ConcordiumSdk.NodeApi.Types.StakeOverMaximumThresholdForPool();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.StakeOverMaximumThresholdForPool>();
        result.Should().NotBeNull();
    }
    
    [Fact]
    public async Task TransactionRejectReason_PoolWouldBecomeOverDelegated()
    {
        var inputReason = new global::ConcordiumSdk.NodeApi.Types.PoolWouldBecomeOverDelegated();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.PoolWouldBecomeOverDelegated>();
        result.Should().NotBeNull();
    }
    
    [Fact]
    public async Task TransactionRejectReason_PoolClosed()
    {
        var inputReason = new global::ConcordiumSdk.NodeApi.Types.PoolClosed();
        await WriteSingleRejectedTransaction(inputReason);
        
        var result = await ReadSingleRejectedTransactionRejectReason<Application.Api.GraphQL.Transactions.PoolClosed>();
        result.Should().NotBeNull();
    }
    
    private async Task WriteData(long blockId = 42)
    {
        var blockSummary = _blockSummaryBuilder.Build();
        await _target.AddTransactions(blockSummary, blockId, _anyBlockSlotTime);
    }
    
    private async Task<T> ReadSingleTransactionEventType<T>()
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var result = await dbContext.TransactionResultEvents.SingleAsync();
        return result.Entity.Should().BeOfType<T>().Subject;
    }
    
    private async Task WriteSingleRejectedTransaction(TransactionRejectReason rejectReason)
    {
        _blockSummaryBuilder
            .WithTransactionSummaries(new TransactionSummaryBuilder()
                .WithResult(new TransactionRejectResultBuilder()
                    .WithRejectReason(rejectReason)
                    .Build())
                .Build());

        await WriteData();
    }
    
    private async Task<T> ReadSingleRejectedTransactionRejectReason<T>() where T : Application.Api.GraphQL.Transactions.TransactionRejectReason
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var transaction = await dbContext.Transactions.SingleAsync();
        var rejected = Assert.IsType<Rejected>(transaction.Result);
        return Assert.IsType<T>(rejected.Reason);
    }
}