using System.IO.Compression;

namespace WFDS.Common.Helpers;

public static class GZipHelper
{
    public static byte[] Compress(byte[] bytes)
    {
        using var result = new MemoryStream();
        using var gzip = new GZipStream(result, CompressionMode.Compress);
        gzip.Write(bytes, 0, bytes.Length);
        gzip.Close();
        return result.ToArray();
    }

    public static byte[] Decompress(byte[] bytes, int size)
    {
        using var result = new MemoryStream();
        using var gzip = new GZipStream(new MemoryStream(bytes, 0, size), CompressionMode.Decompress);
        gzip.CopyTo(result);
        return result.ToArray();
    }
}