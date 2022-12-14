using System.IO.Compression;
using NeekUP.PNG.Chunks;

namespace NeekUP.PNG;

public class Encoder
{
    private static readonly byte[] Signature = { 137, 80, 78, 71, 13, 10, 26, 10 };

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pixels">byte[]{R,G,B, R,G,B...} or byte[]{R,G,B,A, R,G,B,A...}</param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="output"></param>
    /// <exception cref="NotImplementedException"></exception>
    public static void Encode(ReadOnlySpan<byte> pixels, int width, int height, Stream output)
    {
        var bpp = GetBpp(pixels, width, height);
        var ihdr = new IHDR(width, height);
        var plte = new PLTE(pixels, bpp);
        var iend = new IEND();
        tRNS trns = default;
        IDAT idat;

        ihdr.SetColorType(plte.GetColorType());
        ihdr.SetBitDepth(8);
        
        switch (ihdr.ColorType)
        {
            case ColorType.IndexedColour:
                idat = IDAT.FromIndexed(plte, ihdr.Width, ihdr.Height, pixels, bpp);
                trns = new tRNS(plte.AlphaTable);
                break;
            case ColorType.Truecolour:
            case ColorType.TruecolourA:
                idat = IDAT.FromTruecolour(ihdr.Width, ihdr.Height, pixels, bpp);
                break;
            case ColorType.Greyscale:
            case ColorType.GreyscaleA:
                idat = IDAT.FromGrayscale(ihdr.Width, ihdr.Height, pixels, bpp);
                break;
            default:
                throw new NotImplementedException($"{nameof(ihdr.ColorType)} has unexpected value: {ihdr.ColorType}");
        }
        
        var stream = new PngStream(output);
        stream.Write(Signature, 0, Signature.Length);
        ihdr.WriteTo(stream);
        plte.WriteTo(stream);
        trns.WriteTo(stream);
        idat.WriteTo(stream);
        iend.WriteTo(stream);

        stream.Flush();
    }

    private static int GetBpp(ReadOnlySpan<byte> pixels, int width, int height)
    {
        int bpp;
        if (pixels.Length == width * height * 3)
            bpp = 3;
        else if (pixels.Length == width * height * 4)
            bpp = 4;
        else
            throw new Exception($"{nameof(pixels)} length must be {nameof(width)} * {nameof(height)} * (3 or 4(alpha))");
        return bpp;
    }
}