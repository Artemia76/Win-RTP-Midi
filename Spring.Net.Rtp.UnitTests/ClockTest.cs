using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spring.Net.Rtp.Interop;

namespace Spring.Net.Rtp.UnitTests
{
    internal sealed class MockTicksProvider : ITicksProvider
    {
        public long Ticks { get; private set; }

        public void Advance(TimeSpan duration)
        {
            Ticks += (long) Math.Floor(duration.TotalSeconds*TimeSpan.TicksPerSecond);
        }
    }

    [TestClass]
    public class ClockTest
    {
        [TestMethod]
        public void ClockInitialValue()
        {
            const uint timestamp = 0x956a084;
            const int rate = 48000;

            var clock = new RtpMidiClock(new MockTicksProvider(), timestamp, rate);
            Assert.AreEqual(timestamp, clock.Now);
            Assert.AreEqual(0U, clock.Delta);
        }

        [TestMethod]
        public void ClockRatePerSecond()
        {
            const uint timestamp = 0x956a084;
            const int rate = 48000;

            var provider = new MockTicksProvider();
            var clock = new RtpMidiClock(provider, timestamp, rate);

            // advance 30 seconds
            provider.Advance(TimeSpan.FromSeconds(30));

            Assert.AreEqual(timestamp + 30*rate, clock.Now);
            Assert.AreEqual((uint) (30*rate), clock.Delta);
        }

        [TestMethod]
        public void ClockWrapLongValues()
        {
            const uint timestamp = UInt32.MaxValue - 48000;
            const int rate = 48000;

            var provider = new MockTicksProvider();
            var clock = new RtpMidiClock(provider, timestamp, rate);

            // advance 1 second - overflow
            provider.Advance(TimeSpan.FromSeconds(1));

            Assert.AreEqual(0U, clock.Now);
            Assert.AreEqual((uint) (1*48000), clock.Delta);
        }
    }
}