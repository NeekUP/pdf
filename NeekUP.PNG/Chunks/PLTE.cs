namespace NeekUP.PNG.Chunks;

public struct PLTE : IChunk
{
    private const int Capacity = 256;

    public int Length => PaletteTable.Length;
    public byte[] Data => PaletteTable;
    public byte[] Header => new byte[] { 80, 76, 84, 69 };
    
    public byte[] AlphaTable { get; }
    /// <summary>
    /// If the number of distinct pixel values is 256 or less,
    /// and the RGB sample depths are not greater than 8,
    /// and the alpha channel is absent or exactly 8 bits deep or every pixel is either fully transparent or fully opaque,
    /// then an alternative representation called indexed-colour may be more efficient for encoding.
    /// Each pixel is replaced by an index into a palette.
    /// </summary>
    public byte[] PaletteTable { get; }
    
    public bool HasAlpha { get; }
    
    public bool IsGreyscale { get; }
    
    private readonly Dictionary<uint, int> _colors = new ();

    public PLTE(ReadOnlySpan<byte> pixels, int bpp)
    {
        HasAlpha = bpp == 4;
        IsGreyscale = true;
        var pixelsIx = 0;
        for (var i = 0; i < pixels.Length; i += bpp)
        {
            var px = pixels.Slice(i, bpp);
            var hash = bpp == 3 ? Span3ToUint(px) : Span4ToUint(px);
            
            if (!_colors.ContainsKey(hash))
                _colors.Add(hash, pixelsIx++);
            
            if (IsGreyscale)
                IsGreyscale &= px[0] == px[1] && px[1] == px[2];
            
            if(pixelsIx > Capacity)
                break;
        }

        if (pixelsIx is > 0 and <= Capacity)
        {
            PaletteTable = new byte[_colors.Count * 3];
            AlphaTable = HasAlpha ? new byte[_colors.Count] : Array.Empty<byte>();

            var ix = 0;
            foreach (var b in _colors.Select(px => BitConverter.GetBytes(px.Key)))
            {
                if (HasAlpha)
                    AlphaTable[ix / 3] = b[3];
                
                PaletteTable[ix++] = b[0];
                PaletteTable[ix++] = b[1];
                PaletteTable[ix++] = b[2];
            }
        }
        else
        {
            PaletteTable = Array.Empty<byte>();
            AlphaTable = Array.Empty<byte>();
        }
    }
    
    public byte[] Index(int width, int height, ReadOnlySpan<byte> pixels, int bpp)
    {
        var data = new byte[width * height + height];
        
        var ix = 0;
        for (var row = 0; row < height; row++)
        {
            data[ix++] = (byte)FilterType.None;
            for (var col = 0; col < width; col++)
            {
                var start = (row * width * bpp) + col * bpp;
                var px = pixels.Slice(start, bpp);
                var hash = bpp == 3 ? Span3ToUint(px) : Span4ToUint(px);
                data[ix++] = (byte)_colors[hash];
            }
        }

        return data;
    }
    
    public ColorType GetColorType()
    {
        if (IsGreyscale)
            return HasAlpha ? ColorType.GreyscaleA : ColorType.Greyscale;
        
        if (_colors.Count <= Capacity)
            return ColorType.IndexedColour;

        return HasAlpha ? ColorType.TruecolourA : ColorType.Truecolour;
    }

    public void WriteTo(PngStream stream)
    {
        if (GetColorType() != ColorType.IndexedColour)
            return;
        
        stream.WriteChunk(this);
    }
    
    static uint Span4ToUint(ReadOnlySpan<byte> array)
    {
        return BitConverter.ToUInt32(array);
    }
    
    static uint Span3ToUint(ReadOnlySpan<byte> array)
    {
        return array[0] | (uint)array[1] << 8 | (uint)array[2] << 16;
    }
}