namespace NeekUP.PNG;

public static class Helper
{
    public static byte[] Int32BigEndian(int value)
    {
        var intBytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(intBytes);
        return intBytes;
    }
}