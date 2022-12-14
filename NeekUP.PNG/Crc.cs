namespace NeekUP.PNG;

// https://www.w3.org/TR/2003/REC-PNG-20031110/#D-CRCAppendix
static class Crc
{
    private static readonly uint[] CrcTable = new uint[256];

    static Crc()
    {
        uint c;
        int n, k;
   
        for (n = 0; n < 256; n++) 
        {
            c = (uint) n;
            for (k = 0; k < 8; k++) {
                if ((c & 1) != 0)
                    c = 0xEDB88320 ^ (c >> 1);
                else
                    c >>= 1;
            }
            CrcTable[n] = c;
        }
    }
    
    static uint Update(uint crc, IReadOnlyList<byte> buf)
    {
        uint c = crc;
        int n;
        
        for (n = 0; n < buf.Count; n++) {
            c = CrcTable[(c ^ buf[n]) & 0xff] ^ (c >> 8);
        }
        return c;
    }
    
    public static uint Calc(IReadOnlyList<byte> buf)
    {
        return Update(0xffffffff, buf) ^ 0xffffffff;
    }
}