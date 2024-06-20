namespace Dynamically.Backend.Interfaces;

public interface IEncodable<T>
{
    public static T? Decode(byte[] input) { return default; }
    public byte[] Encode();
}