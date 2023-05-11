using System.Diagnostics;

namespace Spring.Net.Rtp
{
    public sealed class RtpMidiHelper
    {
        /// <summary>
        ///     Returns a byte-array containing a timestamp value suitable for inclusion in a MIDI command list.
        /// </summary>
        /// <param name="delta"></param>
        /// <remarks>
        ///     As per RFC 6295 (RTP-MIDI):
        ///     A MIDI list encodes time using the following compact delta time format:
        ///     One-Octet Delta Time:
        ///     Encoded form: 0ddddddd
        ///     Decoded form: 00000000 00000000 00000000 0ddddddd
        ///     Two-Octet Delta Time:
        ///     Encoded form: 1ccccccc 0ddddddd
        ///     Decoded form: 00000000 00000000 00cccccc cddddddd
        ///     Three-Octet Delta Time:
        ///     Encoded form: 1bbbbbbb 1ccccccc 0ddddddd
        ///     Decoded form: 00000000 000bbbbb bbcccccc cddddddd
        ///     Four-Octet Delta Time:
        ///     Encoded form: 1aaaaaaa 1bbbbbbb 1ccccccc 0ddddddd
        ///     Decoded form: 0000aaaa aaabbbbb bbcccccc cddddddd
        /// </remarks>
        /// <returns></returns>
        public static byte[] GetDeltaTime(uint delta)
        {
            Debug.Assert(delta <= 0x0FFFFFFF);

            if ( /* delta > 0 && */ delta <= 0x7F)
                return new[]
                           {
                               (byte) (delta & 0x7F),
                           };

            else if ( /* delta > 0x7F && */ delta <= 0x3FFF)
                return new[]
                           {
                               (byte) (((delta & 0x3F80) >> 7) | 0x80),
                               (byte) (delta & 0x7F),
                           };

            else if ( /* delta > 0x3FFF && */ delta <= 0x001FFFFF)
                return new[]
                           {
                               (byte) (((delta & 0x1FC000) >> 14) | 0x80),
                               (byte) (((delta & 0x3F80) >> 7) | 0x80),
                               (byte) (delta & 0x7F),
                           };


            else /* if (delta > 0x1FFFFFF && delta <= 0x0FFFFFFFF) */
                return new[]
                           {
                               (byte) (((delta & 0xFE00000) >> 21) | 0x80),
                               (byte) (((delta & 0x1FC000) >> 14) | 0x80),
                               (byte) (((delta & 0x3F80) >> 7) | 0x80),
                               (byte) (delta & 0x7F),
                           };
        }
    }
}