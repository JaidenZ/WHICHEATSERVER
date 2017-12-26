namespace WHICHEATSERVER.Core.Packets
{
    using System.Runtime.InteropServices;

    public unsafe partial class NBuffer
    {
        public byte[] buffer;
        public int size;
        public int offset;

        public byte* GetBuffer()
        {
            if (this.buffer == null)
            {
                return null;
            }
            return (byte*)Marshal.UnsafeAddrOfPinnedArrayElement(buffer, offset);
        }

        public static implicit operator byte*(NBuffer value)
        {
            if (value == null)
            {
                return null;
            }
            return value.GetBuffer();
        }

        public NBuffer(int ofs, int size)
            : this(new byte[size], ofs, size)
        {

        }

        public NBuffer(byte[] buffer, int ofs, int size)
        {
            this.buffer = buffer;
            this.size = size;
            this.offset = ofs;
        }

        public override string ToString()
        {
            return string.Format("size: {0}, offset: {1}", this.size, this.offset);
        }
    }
}
