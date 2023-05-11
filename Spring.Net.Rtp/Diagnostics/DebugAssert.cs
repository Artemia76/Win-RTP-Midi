using System.Diagnostics;

namespace Spring.Net.Diagnostics
{
    public static class DebugAssert
    {
        /// <summary>
        ///     Returns <c>true</c> if the specified mask contains only contiguous bits set.
        /// </summary>
        /// <param name="mask"></param>
        /// <remarks>
        ///     By contiguous bits set, we mean values like:
        ///     0x78 => 0111 1000,
        ///     0x3C => 0011 1100,
        ///     <para>
        ///         In contrast, the following values are not considered valid:
        ///         0x22 => 0010 0010,
        ///         0x5F => 0110 1111,
        ///         etc.
        ///     </para>
        /// </remarks>
        [Conditional("DEBUG")]
        public static void HasOnlyContiguousBitsSet(byte mask)
        {
#if DEBUG
            // cannot apply the Conditional attribute
            // on a method if its return type is not void
            Debug.Assert(HasOnlyContiguousBits(mask));
#endif
        }

#if DEBUG
        public static bool HasOnlyContiguousBits(byte mask)
        {
            var bits = 0;
            while ((mask & 1) == 0 && bits++ < 8)
                mask >>= 1;

            return
                mask == 0x01 ||
                mask == 0x03 ||
                mask == 0x07 ||
                mask == 0x0F ||
                mask == 0x1F ||
                mask == 0x3F ||
                mask == 0x7F ||
                mask == 0xFF
                ;
        }
#endif
    }
}