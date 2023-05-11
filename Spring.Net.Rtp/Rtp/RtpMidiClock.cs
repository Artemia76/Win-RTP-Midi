using System;
using System.Diagnostics.CodeAnalysis;
using Spring.Net.Rtp.Interop;

namespace Spring.Net.Rtp
{
    public sealed class RtpMidiClock : IProvideClockService
    {
        private readonly uint clockRate_;

        private readonly ITicksProvider provider_;
        private readonly long startTime_;
        private readonly uint timestamp_;

        public RtpMidiClock(uint initialTimeStamp, uint rate)
            : this(new RealTimeTicksProvider(), initialTimeStamp, rate)
        {
        }

        public RtpMidiClock(ITicksProvider provider, uint initialTimeStamp, uint rate)
        {
            timestamp_ = initialTimeStamp;
            provider_ = provider;
            clockRate_ = rate;

            startTime_ = provider.Ticks;
        }

        #region IProvideClockService Implementation

        /// <summary>
        ///     Returns an timestamp value suitable for inclusion in a RTP packet header.
        /// </summary>
        public uint Now
        {
            get { return CalculateCurrentTimeStamp(); }
        }

        /// <summary>
        ///     Returns the time spent since the initial clock timestamp value.
        ///     The returned value is expressed in units of "clock pulsations",
        ///     that are equivalent to seconds, scaled by the clock rate.
        ///     i.e: 1 second difference will result in a delta value equals to the clock rate.
        /// </summary>
        public uint Delta
        {
            get { return CalculateTimeSpent(); }
        }

        #endregion

        #region Implementation

        private uint CalculateCurrentTimeStamp()
        {
            var lapse = CalculateTimeSpent();

            // check for potential overflow

            if (timestamp_ + lapse < UInt32.MaxValue)
                return timestamp_ + lapse;

            var remainder = UInt32.MaxValue - timestamp_;
            return lapse - remainder;
        }

        private uint CalculateTimeSpent()
        {
            var ticks = provider_.Ticks - startTime_;
            var seconds = ticks/TimeSpan.TicksPerSecond;

            var lapse = (uint) Math.Floor((double) seconds*clockRate_);
            return lapse;
        }

        #endregion
    }

    internal sealed class RealTimeTicksProvider : ITicksProvider
    {
        public long Ticks
        {
            get { return DateTime.UtcNow.Ticks; }
        }
    }
}