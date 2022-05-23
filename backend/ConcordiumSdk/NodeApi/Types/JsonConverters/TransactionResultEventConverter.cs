﻿using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using ConcordiumSdk.Utilities;

namespace ConcordiumSdk.NodeApi.Types.JsonConverters;

public class TransactionResultEventConverter : JsonConverter<TransactionResultEvent>
{
    private readonly IDictionary<string, Type> _tagValueToTypeMap;
    private readonly IDictionary<Type, string> _typeToTagValueMap;

    public TransactionResultEventConverter()
    {
        _tagValueToTypeMap = new Dictionary<string, Type>()
        {
            { "ModuleDeployed", typeof(ModuleDeployed) },
            { "ContractInitialized", typeof(ContractInitialized) },
            { "Updated", typeof(Updated) },
            { "Transferred", typeof(Transferred) },
            { "AccountCreated", typeof(AccountCreated) },
            { "CredentialDeployed", typeof(CredentialDeployed) },
            { "BakerAdded", typeof(BakerAdded) },
            { "BakerRemoved", typeof(BakerRemoved) },
            { "BakerStakeIncreased", typeof(BakerStakeIncreased) },
            { "BakerStakeDecreased", typeof(BakerStakeDecreased) },
            { "BakerSetRestakeEarnings", typeof(BakerSetRestakeEarnings) },
            { "BakerKeysUpdated", typeof(BakerKeysUpdated) },
            { "CredentialKeysUpdated", typeof(CredentialKeysUpdated) },
            { "NewEncryptedAmount", typeof(NewEncryptedAmount) },
            { "EncryptedAmountsRemoved", typeof(EncryptedAmountsRemoved) },
            { "AmountAddedByDecryption", typeof(AmountAddedByDecryption) },
            { "EncryptedSelfAmountAdded", typeof(EncryptedSelfAmountAdded) },
            { "UpdateEnqueued", typeof(UpdateEnqueued) },
            { "TransferredWithSchedule", typeof(TransferredWithSchedule) },
            { "CredentialsUpdated", typeof(CredentialsUpdated) },
            { "TransferMemo", typeof(TransferMemo) },
            { "DataRegistered", typeof(DataRegistered) },
            { "Interrupted", typeof(Interrupted) },
            { "Resumed", typeof(Resumed) },
            { "BakerSetOpenStatus", typeof(BakerSetOpenStatus) },
            { "BakerSetMetadataURL", typeof(BakerSetMetadataURL) },
            { "BakerSetTransactionFeeCommission", typeof(BakerSetTransactionFeeCommission) },
            { "BakerSetBakingRewardCommission", typeof(BakerSetBakingRewardCommission) },
            { "BakerSetFinalizationRewardCommission", typeof(BakerSetFinalizationRewardCommission) },
            { "DelegationAdded", typeof(DelegationAdded) },
            { "DelegationRemoved", typeof(DelegationRemoved) },
            { "DelegationStakeIncreased", typeof(DelegationStakeIncreased) },
            { "DelegationStakeDecreased", typeof(DelegationStakeDecreased) },
            { "DelegationSetRestakeEarnings", typeof(DelegationSetRestakeEarnings) },
            { "DelegationSetDelegationTarget", typeof(DelegationSetDelegationTarget) },
        };
        
        _typeToTagValueMap = _tagValueToTypeMap
            .ToDictionary(x => x.Value, x => x.Key);
    }

    public override TransactionResultEvent? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var tagValue = reader.ReadString("tag")!;
        if (!_tagValueToTypeMap.TryGetValue(tagValue, out var tagType))
            throw new NotImplementedException($"Deserialization of '{tagValue}' is not implemented.");
        return (TransactionResultEvent)JsonSerializer.Deserialize(ref reader, tagType, options)!;
    }

    public override void Write(Utf8JsonWriter writer, TransactionResultEvent value, JsonSerializerOptions options)
    {
        var valueType = value.GetType();
        var serializedBytes = (Span<byte>)JsonSerializer.SerializeToUtf8Bytes(value, valueType, options);
        var serializedBytesWithoutStartAndEndObjectTag = serializedBytes.Slice(1, serializedBytes.Length - 2);

        if (!_typeToTagValueMap.TryGetValue(valueType, out var tagValue))
            throw new InvalidOperationException($"Type {valueType.Name} is not represented in mapping dictionary.");
        
        writer.WriteStartObject();
        writer.WritePropertyName("tag");
        writer.WriteStringValue(tagValue);
        writer.WriteRawValue(serializedBytesWithoutStartAndEndObjectTag, true);
        writer.WriteEndObject();
    }
}