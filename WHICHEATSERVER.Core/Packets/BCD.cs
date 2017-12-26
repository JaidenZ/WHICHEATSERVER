namespace WHICHEATSERVER.Core.Packets
{
    using System;
    using System.Text;

    /// <summary>
    /// 表示BCD码
    /// </summary>
    public unsafe partial struct BCD
    {
        public static byte From(byte value) // 算数左右移
        {
            byte sal = (byte)(value / 10); // 高四位  
            byte sar = (byte)(value % 10); // 低四位  
            return (byte)((sal << 4) | sar);
        }

        public static byte To(byte bcd) // 算数左右移
        {
            byte sal = (byte)((bcd >> 4) & 0xF); // 高四位  
            byte sar = (byte)(bcd & 0xF); // 低四位  
            return (byte)(sal * 10 + sar);  
        }

        public static byte[] From(string src, int len)
        {
            byte[] buffer = new byte[len];
            string str = src.PadLeft(len * 2, '0');
            for (int i = 0; i < len; i++)
            {
                buffer[i] = Convert.ToByte("0x" + str.Substring(i * 2, 2), 0x10);
            }
            return buffer;
        }

        public static string To(byte[] data, int ofs, int len)
        {
            fixed (byte* ptr = data)
            {
                return To(ptr, ofs, len);
            }
        }

        /// <summary>
        /// BCD码到字符串
        /// </summary>
        /// <param name="len"></param>
        /// <returns></returns>
        public static string To(byte* data, int ofs, int len)
        {
            unchecked
            {
                StringBuilder sb = new StringBuilder(len * 2);
                for (int i = 0; i < len; i++)
                {
                    sb.Append(data[i + ofs].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// BCD转字符串
        /// </summary>
        /// <param name="data"></param>
        /// <param name="startIndex"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static string BCDToString(byte[] data, int startIndex, int len)
        {
            StringBuilder sbResault = new StringBuilder();
            for (int index = 0; index < len; index++)
            {
                sbResault.Append(data[index + startIndex].ToString("X2"));
                
            }
            return sbResault.ToString();
        }
    }
}
