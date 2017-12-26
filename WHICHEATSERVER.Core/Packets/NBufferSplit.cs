namespace WHICHEATSERVER.Core.Packets
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// 缓冲区分割
    /// </summary>
    public unsafe class NBufferSplit
    {
        /// <summary>
        /// 最大缓冲区块大小
        /// </summary>
        public const int MAX_BUFFER_BLOCK_SIZE = 1400;

        public static IList<byte[]> Split(byte[] buffer)
        {
            if (buffer.Length < NBufferSplit.MAX_BUFFER_BLOCK_SIZE)
            {
                return new byte[][] { buffer };
            }
            int size = buffer.Length / NBufferSplit.MAX_BUFFER_BLOCK_SIZE;
            int block = buffer.Length % NBufferSplit.MAX_BUFFER_BLOCK_SIZE;
            if (block != 0)
            {
                size += 1;
            }
            byte[][] splits = new byte[size][];
            for (int i = 0, len = splits.Length - 1; i <= len; i++)
            {
                int ofs = NBufferSplit.MAX_BUFFER_BLOCK_SIZE;
                if (i >= len && block > 0)
                {
                    ofs = block;
                }
                byte[] split = new byte[ofs];
                {
                    ofs = i * NBufferSplit.MAX_BUFFER_BLOCK_SIZE;
                    Buffer.BlockCopy(buffer, ofs, split, 0, split.Length);
                }
                splits[i] = split;
            }
            return splits;
        }
    }
}
