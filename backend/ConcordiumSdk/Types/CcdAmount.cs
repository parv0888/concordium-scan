﻿using System.Buffers.Binary;

namespace ConcordiumSdk.Types;

public struct CcdAmount
{
    private readonly ulong _microCcd;

    private CcdAmount(ulong microCcd)
    {
        _microCcd = microCcd;
    }

    public ulong MicroCcdValue => _microCcd;

    public static CcdAmount FromMicroCcd(ulong microCcd)
    {
        return new CcdAmount(microCcd);
    }

    public static CcdAmount FromMicroCcd(int microCcd)
    {
        if (microCcd < 0) throw new ArgumentOutOfRangeException(nameof(microCcd), "Cannot represent negative numbers");
        return new CcdAmount(Convert.ToUInt64(microCcd));
    }

    public static CcdAmount FromCcd(int ccd)
    {
        if (ccd < 0) throw new ArgumentOutOfRangeException(nameof(ccd), "Cannot represent negative numbers");
        return new CcdAmount(Convert.ToUInt64(ccd * 1_000_000));
    }

    public byte[] SerializeToBytes()
    {
        var bytes = new byte[8];
        BinaryPrimitives.WriteUInt64BigEndian(new Span<byte>(bytes), _microCcd);
        return bytes;
    }

    public static CcdAmount operator +(CcdAmount a, CcdAmount b)
        => new(a._microCcd + b._microCcd);

    public bool Equals(CcdAmount other)
    {
        return _microCcd == other._microCcd;
    }

    public override bool Equals(object? obj)
    {
        return obj is CcdAmount other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _microCcd.GetHashCode();
    }

    public static bool operator ==(CcdAmount left, CcdAmount right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(CcdAmount left, CcdAmount right)
    {
        return !left.Equals(right);
    }
}