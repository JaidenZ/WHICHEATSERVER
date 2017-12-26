namespace WHICHEATSERVER.Core.Packets
{
    using System;

    public class PacketDataEncoder
    {
        private static byte[] Init(string key, int bit)
        {
            byte[] box = new byte[bit];
            for (int i = 0; i < bit; i++)
            {
                box[i] = (byte)i;
            }
            for (int i = 0, j = 0; i < bit; i++)
            {
                j = (j + box[i] + key[i % key.Length]) % bit;
                byte b = box[i];
                box[i] = box[j];
                box[j] = b;
            }
            return box;
        }

        public static byte[] To(string key, byte[] value, int bit)
        {
            NBuffer buffer = new NBuffer(value, 0, value.Length);
            PacketDataEncoder.To(key, buffer, bit);
            return value;
        }

        public unsafe static NBuffer To(string key, NBuffer value, int bit)
        {
            if (bit > 0)
            {
                fixed (byte* pinned = value.buffer)
                {
                    byte[] box = PacketDataEncoder.Init(key, bit);
                    byte* buffer = value.GetBuffer();
                    for (int i = 0, low = 0, high = 0, mid; i < value.size; i++)
                    {
                        low = (low + key.Length) % bit;
                        high = (high + box[i % bit]) % bit;

                        byte b = box[low];
                        box[low] = box[high];
                        box[high] = b;

                        mid = (box[low] + box[high]) % bit;
                        buffer[i] ^= box[mid];
                    }
                }
            }
            return value;
        }
    }
}
