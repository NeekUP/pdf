namespace NeekUP.PNG.Chunks;

public struct IHDR : IChunk
{
    public int Length => 13;
    public byte[] Data { get; private set; }
    public byte[] Header => new byte[] { 73, 72, 68, 82 };
    
    public int Width { get; }
    public int Height { get; }
    public int BitDepth { get; private set; }
    public ColorType ColorType { get; private set; }

    public IHDR(int width, int height) : this()
    {
        Width = width;
        Height = height;
    }

    public void SetBitDepth(int value)
    {
        BitDepth = value;
    }

    public void SetColorType(ColorType value)
    {
        ColorType = value;
    }

    public void WriteTo(PngStream stream)
    {
        Data = new byte[Length];
        Helper.Int32BigEndian(Width).CopyTo(Data, 0);
        Helper.Int32BigEndian(Height).CopyTo(Data, 4);
        Data[8] = (byte)BitDepth;
        Data[9] = (byte)ColorType;
        Data[10] = 0;  // Deflate
        Data[11] = 0;  // Adaptive filtering
        Data[12] = 0;  // No interlace
        
        stream.WriteChunk(this);
    }
}