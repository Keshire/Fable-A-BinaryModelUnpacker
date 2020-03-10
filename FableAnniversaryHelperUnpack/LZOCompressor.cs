using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LZOWrapper
{
    /// <summary>
    /// Wrapper class for the highly performant LZO compression library
    /// </summary>
    public class LZO
    {
        private const string LzoDll = @"lzo2_64.dll";
        #region Dll-Imports

        [DllImport(LzoDll)]
        private static extern int __lzo_init_v2(uint v, int s1, int s2, int s3, int s4, int s5, int s6, int s7, int s8, int s9);
        [DllImport(LzoDll)]
        private static extern IntPtr lzo_version_string();
        [DllImport(LzoDll)]
        private static extern string lzo_version_date();
        [DllImport(LzoDll)]
        private static extern int lzo1x_1_compress(byte[] src, int src_len, byte[] dst, ref int dst_len, byte[] wrkmem);
        [DllImport(LzoDll)]
        private static extern int lzo1x_decompress(byte[] src, int src_len, byte[] dst, ref int dst_len, byte[] wrkmem);

        #endregion

        private byte[] _workMemory = new byte[16384L * 4];

        public LZO()
        {
            int init = 0;
            init = __lzo_init_v2(1, -1, -1, -1, -1, -1, -1, -1, -1, -1);

            if (init != 0)
            {
                throw new Exception("Initialization of LZO-Compressor failed !");
            }
        }



        /// <summary>
        /// Version string of the compression library.
        /// </summary>
        public string Version
        {
            get
            {
                IntPtr strPtr;
                strPtr = lzo_version_string();

                string version = Marshal.PtrToStringAnsi(strPtr);
                return version;
            }
        }

        /// <summary>
        /// Version date of the compression library
        /// </summary>
        public string VersionDate{ get{ return lzo_version_date(); } }

        /// <summary>
        /// Compresses a byte array and returns the compressed data in a new
        /// array. You need the original length of the array to decompress it.
        /// </summary>
        /// <param name="src">Source array for compression</param>
        /// <returns>Byte array containing the compressed data</returns>
        public byte[] Compress(byte[] src)
        {
            byte[] dst = new byte[src.Length + src.Length / 64 + 16 + 3 + 4];
            int outlen = 0;

            lzo1x_1_compress(src, src.Length, dst, ref outlen, _workMemory);
            
            byte[] ret = new byte[outlen + 4];
            Array.Copy(dst, 0, ret, 0, outlen);
            byte[] outlenarr = BitConverter.GetBytes(src.Length);
            Array.Copy(outlenarr, 0, ret, outlen, 4);
            return ret;
        }

        /// <summary>
        /// Decompresses compressed data to its original state.
        /// </summary>
        /// <param name="src">Source array to be decompressed</param>
        /// <returns>Decompressed data</returns>
        public byte[] Decompress(byte[] src, int src_len, byte[] dst, int dst_len)
        {
            lzo1x_decompress(src, src_len, dst, ref dst_len, null);
            return dst;
        }
    }
}