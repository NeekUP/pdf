namespace NeekUP.PNG;

// x 	the byte being filtered;
// a 	the byte corresponding to x in the pixel immediately before the pixel containing x (or the byte immediately before x, when the bit depth is less than 8);
// b 	the byte corresponding to x in the previous scanline;
// c 	the byte corresponding to b in the pixel immediately before the pixel containing b (or the byte immediately before b, when the bit depth is less than 8)
//
// Bytes
// ---------
// | c | b |
// | a | x |
// --------
//
public static class Filters
{
    public static void ApplyFilter(ReadOnlySpan<byte> current, ReadOnlySpan<byte> previous, Span<byte> buffer, int bpp)
    {
        var sums = new []
        {
            FilterNoneSum(current),
            FilterSubSum(current, bpp),
            FilterUpSum(current, previous),
            FilterAvgSum(current, previous, bpp),
            FilterPaethSum(current, previous, bpp)
        };
        
        var type = (FilterType)Array.IndexOf(sums, sums.Max());
        switch (type)
        {
            case FilterType.None: FilterNone(current, buffer); break;
            case FilterType.Sub: FilterSub(current, buffer, bpp); break;
            case FilterType.Up: FilterUp(current, previous, buffer); break;
            case FilterType.Avg: FilterAvg(current, previous, buffer, bpp); break;
            case FilterType.Paeth: FilterPaeth(current, previous, buffer, bpp); break;
            default: throw new ArgumentOutOfRangeException($"Unexpected {nameof(FilterType)}: {type}");
        }
    }

    private static void FilterNone(ReadOnlySpan<byte> current, Span<byte> buffer)
    {
        buffer[0] = (byte)FilterType.None;
        for (var i = 1; i < buffer.Length; i++)
        {
            buffer[i] = current[i - 1];
        }
    }

    private static void FilterSub(ReadOnlySpan<byte> current, Span<byte> buffer, int bpp)
    {
        var i = 0;
        buffer[i++] = (byte)FilterType.Sub;

        for (; i <= bpp; i++)
            buffer[i] = current[i - 1];

        for (; i < buffer.Length; i++)
            buffer[i] = (byte)(current[i - 1] - current[i - 1 - bpp]);
    }

    private static void FilterUp(ReadOnlySpan<byte> current, ReadOnlySpan<byte> previous, Span<byte> buffer)
    {
        var i = 0;
        buffer[i++] = (byte)FilterType.Up;

        for (; i < buffer.Length; i++)
        {
            var up = !previous.IsEmpty ? previous[i - 1] : 0;
            buffer[i] = (byte)(current[i - 1] - up);
        }
    }

    private static void FilterAvg(ReadOnlySpan<byte> current, ReadOnlySpan<byte> previous, Span<byte> buffer, int bpp)
    {
        var i = 0;
        buffer[i++] = (byte)FilterType.Avg;

        for (; i < buffer.Length; i++)
        {
            var left = i > bpp ? current[i - 1 - bpp] : 0;
            var up = !previous.IsEmpty ? previous[i - 1] : 0;
            buffer[i] = (byte)(current[i - 1] - ((left + up) >> 1));
        }
    }
    
    private static void FilterPaeth(ReadOnlySpan<byte> current, ReadOnlySpan<byte> previous, Span<byte> buffer, int bpp)
    {
        var i = 0;
        buffer[i++] = (byte)FilterType.Paeth;

        for (; i < buffer.Length; i++)
        {
            var left = i > bpp ? current[i - 1 - bpp] : 0;
            var up = !previous.IsEmpty ? previous[i - 1] : 0;
            var upleft = !previous.IsEmpty && i > bpp ? previous[i - 1 - bpp] : 0;
            buffer[i] = (byte)(current[i - 1] - PaethPredictor(left, up, upleft));
        }
    }
    
    private static int FilterNoneSum(ReadOnlySpan<byte> current)
    {
        var sum = 0;
        for (var i = 1; i < current.Length; i++)
            sum += Math.Abs(current[i]);
    
        return sum;
    }

    private static int FilterSubSum(ReadOnlySpan<byte> current, int bpp)
    {
        var sum = 0;
        var i = 0;
        
        for (; i <= bpp; i++)
            sum += current[i];

        for (; i < current.Length; i++)
            sum += Math.Abs(current[i] - current[i - bpp]);
 
        return sum;
    }
    
    private static int FilterUpSum(ReadOnlySpan<byte> current, ReadOnlySpan<byte> previous)
    {
        var sum = 0;
        for (var i = 0; i < current.Length; i++)
        {
            var up = !previous.IsEmpty ? previous[i] : 0;
            sum += Math.Abs(current[i] - up);
        }
        return sum;
    }
    
    private static int FilterAvgSum(ReadOnlySpan<byte> current, ReadOnlySpan<byte> previous, int bpp)
    {
        var sum = 0;
        for (var i = 0; i < current.Length; i++)
        {
            var left = i > bpp ? current[i - bpp] : 0;
            var up = !previous.IsEmpty ? previous[i] : 0;
            sum += Math.Abs(current[i] - ((left + up) >> 1));
        }
        return sum;
    }
    
    private static int FilterPaethSum(ReadOnlySpan<byte> current, ReadOnlySpan<byte> previous, int bpp)
    {
        var sum = 0;
        for (var i = 0; i < current.Length; i++)
        {
            var left = i > bpp ? current[i - bpp] : 0;
            var up = !previous.IsEmpty ? previous[i] : 0;
            var upleft = !previous.IsEmpty && i > bpp ? previous[i - bpp] : 0;
            sum += Math.Abs(current[i] - PaethPredictor(left, up, upleft));
        }
        return sum;
    }

    private static byte PaethPredictor(int left, int above, int upperLeft)
    {
        var p = left + above - upperLeft;
        var pa = Math.Abs(p - left);
        var pb = Math.Abs(p - above);
        var pc = Math.Abs(p - upperLeft);
        if (pa <= pb && pa <= pc)
            return (byte)left;
        return (byte)(pb <= pc ? above : upperLeft);
    }
}