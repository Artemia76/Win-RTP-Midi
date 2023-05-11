using System.Diagnostics;
using Spring.Net.Diagnostics;

namespace Spring.Net.Extensions
{
    /// <summary>
    ///     Provides bit-manipulation methods to access and manipulate a byte array.
    /// </summary>
    public static class ByteArrayExtensions
    {
        /// <summary>
        ///     Returns a big-endian signed 16-bits integer value from a specified byte array offset.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static short GetInt16(this byte[] buffer, int offset)
        {
            short result = 0;
            result += (short) (buffer[offset] << 8);
            result += buffer[offset + 1];
            return result;
        }

        /// <summary>
        ///     Returns a big-endian unsigned 16-bits integer value from a specified byte array offset.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static ushort GetUInt16(this byte[] buffer, int offset)
        {
            return (ushort) GetInt16(buffer, offset);
        }

        /// <summary>
        ///     Returns a big-endian signed 32-bits integer value from a specified byte array offset.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static int GetInt32(this byte[] buffer, int offset)
        {
            var result = 0;
            result = buffer[offset] << 24;
            result += buffer[offset + 1] << 16;
            result += buffer[offset + 2] << 8;
            result += buffer[offset + 3];
            return result;
        }

        /// <summary>
        ///     Returns a big-endian unsigned 32-bits integer value from a specified byte array offset.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static uint GetUInt32(this byte[] buffer, int offset)
        {
            return (uint) GetInt32(buffer, offset);
        }

        /// <summary>
        ///     Returns a big-endian signed 64-bits integer value from a specified byte array offset.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static long GetInt64(this byte[] buffer, int offset)
        {
            var result = 0L;
            result = (long) buffer[offset] << 56;
            result += (long) buffer[offset + 1] << 48;
            result += (long) buffer[offset + 2] << 40;
            result += (long) buffer[offset + 3] << 32;
            result += (long) buffer[offset + 4] << 24;
            result += (long) buffer[offset + 5] << 16;
            result += (long) buffer[offset + 6] << 8;
            result += buffer[offset + 7];
            return result;
        }

        /// <summary>
        ///     Returns a big-endian unsigned 64-bits integer value from a specified byte array offset.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static ulong GetUInt64(this byte[] buffer, int offset)
        {
            return (ulong) GetInt64(buffer, offset);
        }

        /// <summary>
        ///     Updates the specified byte array offset with a big-endian signed 16-bit integer value.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="value"></param>
        public static void SetInt16(this byte[] buffer, int offset, short value)
        {
            buffer[offset] = (byte) (value >> 8);
            buffer[offset + 1] = (byte) value;
        }

        /// <summary>
        ///     Updates the specified byte array offset with a big-endian unsigned 16-bit integer value.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="value"></param>
        public static void SetUInt16(this byte[] buffer, int offset, ushort value)
        {
            SetInt16(buffer, offset, (short) value);
        }

        /// <summary>
        ///     Updates the specified byte array offset with a big-endian signed 32-bit integer value.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="value"></param>
        public static void SetInt32(this byte[] buffer, int offset, int value)
        {
            buffer[offset] = (byte) (value >> 24);
            buffer[offset + 1] = (byte) (value >> 16);
            buffer[offset + 2] = (byte) (value >> 8);
            buffer[offset + 3] = (byte) value;
        }

        /// <summary>
        ///     Updates the specified byte array offset with a big-endian unsigned 32-bit integer value.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="value"></param>
        public static void SetUInt32(this byte[] buffer, int offset, uint value)
        {
            SetInt32(buffer, offset, (int) value);
        }

        /// <summary>
        ///     Updates the specified byte array offset with a big-endian signed 64-bit integer value.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="value"></param>
        public static void SetInt64(this byte[] buffer, int offset, long value)
        {
            buffer[offset] = (byte) (value >> 56);
            buffer[offset + 1] = (byte) (value >> 48);
            buffer[offset + 2] = (byte) (value >> 40);
            buffer[offset + 3] = (byte) (value >> 32);
            buffer[offset + 4] = (byte) (value >> 24);
            buffer[offset + 5] = (byte) (value >> 16);
            buffer[offset + 6] = (byte) (value >> 8);
            buffer[offset + 7] = (byte) value;
        }

        /// <summary>
        ///     Updates the specified byte array offset with a big-endian unsigned 64-bit integer value.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="value"></param>
        public static void SetUInt64(this byte[] buffer, int offset, ulong value)
        {
            SetInt64(buffer, offset, (long) value);
        }

        /// <summary>
        ///     Returns whether a specific bit is set at the specified byte array offset.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="mask">Value representing the single bit to check for.</param>
        /// <returns></returns>
        public static bool GetFlag(this byte[] buffer, int offset, byte mask)
        {
            Debug.Assert(mask == (mask & -mask)); // check single bit mask
            return (buffer[offset] & mask) == mask;
        }

        /// <summary>
        ///     Set or clears a specific bit at the specified byte array offset.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="mask">Value representing the single bit to set or clear.</param>
        /// <param name="value">
        ///     If the value is <c>true</c>, the bit is set. Otherwise, the bit is cleared.
        /// </param>
        public static void SetFlag(this byte[] buffer, int offset, byte mask, bool value)
        {
            Debug.Assert(mask == (mask & -mask)); // check single bit mask
            if (value)
                buffer[offset] = (byte) (buffer[offset] | mask);
            else
                buffer[offset] = (byte) (buffer[offset] ^ (buffer[offset] & mask));
        }

        /// <summary>
        ///     Returns a masked unsigned integer value at the specified byte array offset.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="mask"></param>
        /// <returns></returns>
        public static byte GetVal(this byte[] buffer, int offset, byte mask)
        {
            DebugAssert.HasOnlyContiguousBitsSet(mask);
            var shift = GetBitMaskOffset(mask);
            return (byte) ((buffer[offset] & mask) >> shift);
        }

        public static void SetVal(this byte[] buffer, int offset, byte mask, byte value)
        {
            DebugAssert.HasOnlyContiguousBitsSet(mask);
            var complement = (byte) (~mask & 0xFF);
            var shift = GetBitMaskOffset(mask);

            buffer[offset] = (byte) (((value << shift) & mask) | (buffer[offset] & complement));
        }

        #region Implementation

        private static byte GetBitMaskOffset(byte mask)
        {
            byte shift = 0;
            while ((mask & 1) == 0 && shift++ < 8)
                mask >>= 1;
            return shift;
        }

        #endregion
    }
}