namespace WHICHEATSERVER.Core.Packets
{
    public unsafe partial class NConvert
    {
        public static ushort ToUInt16(void* buffer)
        {
            byte* ptr = (byte*)buffer;
            return (ushort)(ptr[0] << 8 | ptr[1]);
        }

        public static short ToInt16(void* buffer)
        {
            return unchecked((short)NConvert.ToUInt16(buffer));
        }

        public static byte[] GetBytes(ushort value)
        {
            byte[] buffer = new byte[sizeof(ushort)];
            buffer[0] = (byte)(value >> 8);
            buffer[1] = (byte)value;
            return buffer;
        }

        public static byte[] GetBytes(short value)
        {
            return NConvert.GetBytes(unchecked((ushort)value));
        }
    }
}
