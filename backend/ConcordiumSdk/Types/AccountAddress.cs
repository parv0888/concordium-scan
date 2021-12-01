using System.Linq;
using NBitcoin.DataEncoders;

namespace ConcordiumSdk.Types;

public class AccountAddress
{
    private static readonly Base58CheckEncoder EncoderInstance = new();
    private readonly byte[] _bytes;

    /// <summary>
    /// Creates an instance from a 32 byte address (ie. excluding the version byte).
    /// </summary>
    public AccountAddress(byte[] bytes)
    {
        _bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));

        if (_bytes.Length != 32) throw new ArgumentException("Expected length to be exactly 32 bytes");
        
        var bytesToEncode = new byte[33];
        bytesToEncode[0] = 1;
        bytes.CopyTo(bytesToEncode, 1);
        AsString = EncoderInstance.EncodeData(bytesToEncode);
    }
    
    /// <summary>
    /// Creates an instance from a base58-check encoded string
    /// </summary>
    public AccountAddress(string base58CheckEncodedAddress)
    {
        AsString = base58CheckEncodedAddress ?? throw new ArgumentNullException(nameof(base58CheckEncodedAddress));

        var decodedBytes = EncoderInstance.DecodeData(base58CheckEncodedAddress);
        _bytes = decodedBytes.Skip(1).ToArray(); // Remove version byte
    }

    /// <summary>
    /// Gets the address as a byte array (without leading version byte).
    /// Will always be 32 bytes. 
    /// </summary>
    public byte[] AsBytes => _bytes;
    
    /// <summary>
    /// Gets the address as a base58-check encoded string.
    /// </summary>
    public string AsString { get; }

    public override string ToString()
    {
        return AsString;
    }
}