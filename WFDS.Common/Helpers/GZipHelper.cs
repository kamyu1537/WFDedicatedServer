using System.IO.Compression;

namespace WFDS.Common.Helpers;

public static class GZipHelper
{
    public static byte[] Compress(byte[] bytes)
    {
        using var result = new MemoryStream();
        using var gzip = new GZipStream(result, CompressionMode.Compress);
        gzip.Write(bytes);
        return result.ToArray();
    }

    public static byte[] Decompress(byte[] bytes)
    {
        using var result = new MemoryStream();
        using var input = new MemoryStream(bytes);
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        gzip.CopyTo(result);
        return result.ToArray();
    }
}