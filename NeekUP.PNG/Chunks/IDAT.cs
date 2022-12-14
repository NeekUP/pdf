using System.IO.Compression;

namespace NeekUP.PNG.Chunks;

public struct IDAT : IChunk
{
    public byte[] Data { get; }
    public byte[] Header => new byte[] {73, 68, 65, 84 };
    public int Length => Data.Length;
    
    private IDAT(byte[] data)
    {
        Data = data;
    }
 
    public static IDAT FromIndexed(PLTE plte,int width, int heigth, ReadOnlySpan<byte> pixels, int bpp)
    {
        var indexedData = plte.Index(width, heigth, pixels, bpp);
        var data = Compress(new ReadOnlySpan<byte>(indexedData, 0, indexedData.Length), CompressionLevel.Optimal);
        return new IDAT(data);
    }

    public static IDAT FromTruecolour(int width, int heigth, ReadOnlySpan<byte> pixels, int bpp)
    {
        var filtered = new byte[pixels.Length * bpp + heigth];
        var sLength = width * bpp;
        var fLength = sLength + 1;

        for (var i = 0; i < heigth; i++)
        {
            var source = pixels.Slice(i * sLength, sLength);
            var buffer = new Span<byte>(filtered, i * fLength, fLength);
            var previous = i > 0
                ? pixels.Slice((i - 1) * sLength, sLength)
                : Array.Empty<byte>();

            Filters.ApplyFilter(source, previous, buffer, bpp);
        }

        var data = Compress(new ReadOnlySpan<byte>(filtered, 0, filtered.Length), CompressionLevel.Optimal);
        return new IDAT(data);
    }
    
    public static IDAT FromGrayscale(int width, int heigth, ReadOnlySpan<byte> pixels, int bpp)
    {
        var pxc = width * heigth;
        var data = new byte[bpp == 3 ? pxc : pxc * 2];
        var ix = 0;
        if (bpp == 3)
        {
            for (var i = 0; i < pixels.Length; i += bpp)
                data[ix++] = pixels[i];
        }
        else
        {
            for (var i = 0; i < pixels.Length; i += bpp)
            {
                data[ix++] = pixels[i];
                data[ix++] = pixels[i + 3];
            }
        }

        return new IDAT(Compress(new ReadOnlySpan<byte>(data, 0, data.Length), CompressionLevel.Optimal));
    }

    private static byte[] Compress(ReadOnlySpan<byte> input, CompressionLevel compressionLevel)
    {
        const int headerLength = 2;
        const int checksumLength = 4;

        using var ms = new MemoryStream();
        using var compressor = new DeflateStream(ms, compressionLevel, true);

        compressor.Write(input);
        compressor.Close();

        var output = new byte[headerLength + ms.Length + checksumLength];
        output[0] = 120; // zlib header
        output[1] = 1; // checksum bits

        ms.Seek(0, SeekOrigin.Begin);

        int value;
        var i = 0;
        while ((value = ms.ReadByte()) != -1)
            output[headerLength + i++] = (byte)value;

        var checksum = Checksum(input, input.Length);
        var offset = headerLength + ms.Length;

        output[offset++] = (byte)(checksum >> 24);
        output[offset++] = (byte)(checksum >> 16);
        output[offset++] = (byte)(checksum >> 8);
        output[offset] = (byte)(checksum >> 0);

        return output;
    }

    public void WriteTo(PngStream stream)
    {
        stream.WriteChunk(this);
    }
    
    private static int Checksum(ReadOnlySpan<byte> data, int length = -1)
    {
        var a = 1;
        var b = 0;
        var count = 0;

        foreach (var val in data)
        {
            if (length > 0 && count == length)
                break;

            a = (a + val) % 65521;
            b = (a + b) % 65521;
            count++;
        }

        return b * 65536 + a;
    }
}