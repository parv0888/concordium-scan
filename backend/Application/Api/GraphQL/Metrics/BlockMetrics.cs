﻿using HotChocolate;

namespace Application.Api.GraphQL.Metrics;

public record BlockMetrics(
    [property:GraphQLDescription("The most recent block height (equals the total length of the chain).")]
    long LastBlockHeight, 
    [property:GraphQLDescription("Total number of blocks added in requested period.")]
    int BlocksAdded, 
    [property:GraphQLDescription("The average block time (slot-time difference between two adjacent blocks) in the requested period.")]
    double AvgBlockTime, 
    BlockMetricsBuckets Buckets);
    
public record BlockMetricsBuckets(
    [property:GraphQLDescription("The width (time interval) of each bucket.")]
    TimeSpan BucketWidth,
    [property:GraphQLDescription("Start of the bucket time period. Intended x-axis value.")]
    DateTimeOffset[] X_Time,
    [property:GraphQLDescription("Number of blocks added within the bucket time period. Intended y-axis value.")]
    int[] Y_BlocksAdded,
    [property:GraphQLDescription("The minimum block time (slot-time difference between two adjacent blocks) in the requested period. Intended y-axis value.")]
    double[] Y_BlockTimeMin,
    [property:GraphQLDescription("The average block time (slot-time difference between two adjacent blocks) in the requested period. Intended y-axis value.")]
    double[] Y_BlockTimeAvg,
    [property:GraphQLDescription("The maximum block time (slot-time difference between two adjacent blocks) in the requested period. Intended y-axis value.")]
    double[] Y_BlockTimeMax);