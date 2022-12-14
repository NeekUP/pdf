namespace NeekUP.PNG.Chunks;

public struct tRNS : IChunk
{
    public int Length => Data.Length;
    public byte[] Data { get; }
    public byte[] Header => new byte[] { 116, 82, 78, 83 };
    
    public tRNS(byte[] alphaTable)
    {
        Data = alphaTable;
    }

    public void WriteTo(PngStream stream)
    {
        if (Data == null)
            return;
        
        stream.WriteChunk(this);
    }


}