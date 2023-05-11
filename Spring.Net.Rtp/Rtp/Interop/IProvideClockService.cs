namespace Spring.Net.Rtp.Interop
{
    public interface IProvideClockService
    {
        /// <summary>
        ///     Returns an timestamp value suitable for inclusion in a RTP packet header.
        /// </summary>
        uint Now { get; }

        /// <summary>
        ///     Returns the time spent since the initial clock timestamp value.
        ///     The returned value is expressed in units of "clock pulsations",
        ///     that are equivalent to seconds, scaled by the clock rate.
        ///     i.e: 1 second difference will result in a delta value equals to the clock rate.
        /// </summary>
        uint Delta { get; }
    }
}