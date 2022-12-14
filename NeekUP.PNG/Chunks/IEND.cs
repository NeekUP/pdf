namespace NeekUP.PNG.Chunks;

public struct IEND : IChunk
{
    public int Length => 0;
    public byte[] Data => Array.Empty<byte>();
    public byte[] Header => new byte[] { 73, 69, 78, 68 };
    
    public void WriteTo(PngStream stream)
    {
        stream.WriteChunk(this);
    }
}