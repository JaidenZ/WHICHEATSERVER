namespace WHICHEATSERVER.Core.Packets
{
    public unsafe static class NString
    {
        public static string To(byte* ptr, int ofs, int size)
        {
            if (size < 0)
            {
                return null;
            }
            byte[] buffer = new byte[size];
            ptr += ofs;
            for (int i = 0; i < size; i++)
            {
                buffer[i] = *ptr++;
            }
            return PacketTextEncoder.GetEncoding(buffer);
        }
    }
}
