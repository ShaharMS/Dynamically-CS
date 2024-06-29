using System;
using System.Collections;
using System.IO;
using Dynamically.Backend.Interfaces;

namespace Dynamically.Geometry.Basics;

public partial class Vertex : IEncodable<Vertex>
{
    public byte[] Encode()
    {
        var builder = new MemoryStream();
        // Length: 25 bytes (currently). Length "token": 2 bytes.
        builder.Write(BitConverter.GetBytes((ushort)25));
        // Identifier - 2 bytes
        builder.Write(BitConverter.GetBytes(Id));
        // Position - 8 + 8 bytes
        builder.Write(BitConverter.GetBytes(X));
        builder.Write(BitConverter.GetBytes(Y));
        // Other - 8 bytes
        builder.Write(BitConverter.GetBytes(Opacity));

        // Attributes - 1 byte.
        // We will create a bitmap of all changeable vertex-dependent boolean attributes:
        var arr = new BitArray(new[] {Anchored, Hidden, Draggable});
        var byteArray = new byte[arr.Length % 8 == 0 ? arr.Length / 8 : 1 + arr.Length / 8];
        arr.CopyTo(byteArray, 0);
        builder.Write(byteArray);

        return builder.ToArray();
    }

    public static Vertex Decode(byte[] bytes, byte[] version) 
    {
        throw new System.NotImplementedException();
    }
}