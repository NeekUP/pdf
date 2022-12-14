namespace NeekUP.PNG;

public class PngStream : Stream
{
    private readonly Stream _stream;
    public override bool CanRead => _stream.CanRead;
    public override bool CanSeek => _stream.CanSeek;
    public override bool CanWrite => _stream.CanWrite;
    public override long Length => _stream.Length;

    public override long Position
    {
        get => _stream.Position;
        set => _stream.Position = value;
    }

    public PngStream(Stream inner)
    {
        _stream = inner;
    }

    public void WriteChunk<T>(T chunk) where T : struct, IChunk 
    {
        _stream.Write(Helper.Int32BigEndian(chunk.Length), 0, 4);
        _stream.Write(chunk.Header, 0, chunk.Header.Length);
        _stream.Write(chunk.Data, 0, chunk.Data.Length);
        
        var hd = new byte[chunk.Header.Length + chunk.Data.Length];
        chunk.Header.CopyTo(hd, 0);
        chunk.Data.CopyTo(hd, chunk.Header.Length);
        _stream.Write(Helper.Int32BigEndian((int)Crc.Calc(hd)), 0, 4);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return _stream.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return _stream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        _stream.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _stream.Write(buffer, offset, count);
    }

    public override void Flush() => _stream.Flush();
}