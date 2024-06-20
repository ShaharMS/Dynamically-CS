using Dynamically.Backend.Interfaces;
using Dynamically.Containers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Files;

public class Geom : IFileFormat 
{ 
    public static uint MagicNumber => 0xFF030E00;

    public static string Extension => "geom";

    public static byte[] Save(Board board)
    {
        var builder = new MemoryStream();

        // Write the expected amount of bytes at position 0.
        builder.Write(BitConverter.GetBytes(0x00_00_00_00_00_00_00_00));

        // Mark the file as a GEOM file
        builder.Write(BitConverter.GetBytes(MagicNumber));

        // Write the encoder version
        builder.WriteByte(0x01);
        builder.WriteByte(0x00);
        builder.WriteByte(0x00); // 1.0.0 Currently

        // Next: Board settings:
        // Unimplemented

        // Then: Board data
        // Unimplemented

        return builder.ToArray();
    }


    /// <summary>
    /// Loads a saved board from a file encoded using Dynamically GEOM file encoder
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static Board Load(byte[] bytes)
    {
        throw new NotImplementedException();
    }


    static Board __Load__1_0_0(byte[] bytes)
    {
        throw new NotImplementedException();
    }
}
