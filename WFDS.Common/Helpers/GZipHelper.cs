using System.IO.Compression;

namespace WFDS.Common.Helpers;

public static class GZipHelper
{
    public static Memory<byte> Compress(Memory<byte> bytes)
    {
        using var result = new MemoryStream();
        using var gzip = new GZipStream(result, CompressionMode.Compress);
        gzip.Write(bytes.Span);
        return new Memory<byte>(result.GetBuffer(), 0, (int)result.Length);
    }

    public static unsafe Memory<byte> Decompress(Memory<byte> bytes)
    {
        using var result = new MemoryStream();
        fixed (byte* ptr = bytes.Span)
        {
            using var input = new UnmanagedMemoryStream(ptr, bytes.Length);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            gzip.CopyTo(result);
        }
        return new Memory<byte>(result.GetBuffer(), 0, (int)result.Length);
    }
}