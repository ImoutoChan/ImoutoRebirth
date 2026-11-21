using System.Buffers.Binary;
using System.IO;

namespace ImoutoRebirth.Navigator.Model;

internal static class PngAnimation
{
    public static bool IsAnimated(string path)
    {
        using var stream = File.OpenRead(path);
        Span<byte> header = stackalloc byte[8];

        if (stream.Length < header.Length)
            return false;

        stream.ReadExactly(header);
        if (BinaryPrimitives.ReadUInt64BigEndian(header) != 0x89504E470D0A1A0A)
            return false;

        while (stream.Length - stream.Position >= header.Length)
        {
            stream.ReadExactly(header);
            var chunkLength = BinaryPrimitives.ReadUInt32BigEndian(header);
            var chunkType = header[4..];
            var reachedImageDataOrEnd = chunkType.SequenceEqual("IDAT"u8) || chunkType.SequenceEqual("IEND"u8);

            if (reachedImageDataOrEnd || chunkLength + 4L > stream.Length - stream.Position)
                return false;

            if (chunkType.SequenceEqual("acTL"u8))
                return chunkLength == 8;

            stream.Seek(chunkLength + 4L, SeekOrigin.Current);
        }

        return false;
    }
}
