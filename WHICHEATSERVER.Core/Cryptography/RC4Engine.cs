namespace WHICHEATSERVER.Core.Cryptography
{
    /// <summary>
    /// 一个用于安全的RC4线性加密转换引擎服务
    /// </summary>
    public static class RC4Engine
    {
        private static byte[] RC4Init(string key)
        {
            byte[] box = new byte[byte.MaxValue];
            for (int i = 0; i < byte.MaxValue; i++)
            {
                box[i] = (byte)i;
            }
            for (int i = 0, j = 0; i < byte.MaxValue; i++)
            {
                j = (j + box[i] + key[i % key.Length]) % byte.MaxValue;
                byte b = box[i];
                box[i] = box[j];
                box[j] = b;
            }
            return box;
        }

        private static string RC4Create(string key, string value)
        {
            char[] buffer = value.ToCharArray();
            byte[] box = RC4Engine.RC4Init(key);
            for (int i = 0, low = 0, high = 0, mid; i < buffer.Length; i++)
            {
                low = (low + key.Length) % byte.MaxValue;
                high = (high + box[i % byte.MaxValue]) % byte.MaxValue;

                byte b = box[low];
                box[low] = box[high];
                box[high] = b;

                mid = (box[low] + box[high]) % byte.MaxValue;
                buffer[i] ^= (char)box[mid];
            }
            return new string(buffer);
        }
        /// <summary>
        /// 将有效的RC4编码字符串转换到等效字符串表示形式。
        /// </summary>
        /// <param name="value">欲被转换的字符串</param>
        /// <returns></returns>
        public static string FromRC4String(this string value, string key)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }
            return RC4Engine.RC4Create(key, value);
        }
        /// <summary>
        /// 将有效的字符串转换为 RC4 编码的等效字符串表示形式。
        /// </summary>
        /// <param name="value">欲被转换的字符串</param>
        /// <returns></returns>
        public static string ToRC4String(this string value, string key)
        {
            return RC4Engine.FromRC4String(value, key);
        }
    }
}
