namespace NeekUP.PNG;

public interface IChunk
{
    void WriteTo(PngStream stream);
    int Length { get; }
    byte[] Data { get; }
    byte[] Header { get; }
}