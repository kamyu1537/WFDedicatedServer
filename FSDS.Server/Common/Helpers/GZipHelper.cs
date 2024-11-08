using System.IO.Compression;

namespace FSDS.Server.Common.Helpers;

public static class GZipHelper
{
    public static byte[] Compress(byte[] bytes)
    {
        using var memoryStream = new MemoryStream();
        using var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress);
        gZipStream.Write(bytes, 0, bytes.Length);
        gZipStream.Close();
        return memoryStream.ToArray();
    }

    public static byte[] Decompress(byte[] bytes)
    {
        using var memoryStream = new MemoryStream();
        using var gZipStream = new GZipStream(new MemoryStream(bytes), CompressionMode.Decompress);
        gZipStream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }
}