using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Spring.Net.Rtp.UnitTests
{
    [TestClass]
    public class RtpMidiHelperTest
    {
        [TestMethod]
        public void DeltaTimeOneOctet()
        {
            // advance 1 second - delta time = 127 = 0x7F
            // encoded in 1 byte

            const uint delta = 127;

            // One-Octet Delta Time:
            // Encoded form: 0ddddddd
            // Decoded form: 00000000 00000000 00000000 0ddddddd

            var time = RtpMidiHelper.GetDeltaTime(delta);

            Assert.AreEqual(1, time.Length);
            Assert.AreEqual((uint) 0x7F, time[0]);
        }

        [TestMethod]
        public void DeltaTimeTwoOctets()
        {
            // advance 1 second - delta time = 16383 = 0x3FFF
            // encoded in two bytes

            const uint delta = 16383;

            // Two-Octet Delta Time:
            // Encoded form: 1ccccccc 0ddddddd
            // Decoded form: 00000000 00000000 00cccccc cddddddd

            var time = RtpMidiHelper.GetDeltaTime(delta);

            Assert.AreEqual(2, time.Length);
            Assert.AreEqual((uint) 0xFF, time[0]);
            Assert.AreEqual((uint) 0x7F, time[1]);
        }

        [TestMethod]
        public void DeltaTimeThreeOctets()
        {
            // advance 1 second - delta time = 2097151 = 0x1FFFFF
            // encoded in three bytes

            const uint delta = 2097151;

            // Three-Octet Delta Time:
            // Encoded form: 1bbbbbbb 1ccccccc 0ddddddd
            // Decoded form: 00000000 000bbbbb bbcccccc cddddddd

            var time = RtpMidiHelper.GetDeltaTime(delta);

            Assert.AreEqual(3, time.Length);
            Assert.AreEqual((uint) 0xFF, time[0]);
            Assert.AreEqual((uint) 0xFF, time[1]);
            Assert.AreEqual((uint) 0x7F, time[2]);
        }

        [TestMethod]
        public void DeltaTimeFourOctets()
        {
            // advance 1 second - delta time = 268435455 = 0x0FFFFFFF
            // encoded in four bytes

            const uint delta = 268435455;

            // Four-Octet Delta Time:
            // Encoded form: 1aaaaaaa 1bbbbbbb 1ccccccc 0ddddddd
            // Decoded form: 0000aaaa aaabbbbb bbcccccc cddddddd

            var time = RtpMidiHelper.GetDeltaTime(delta);

            Assert.AreEqual(4, time.Length);
            Assert.AreEqual((uint) 0xFF, time[0]);
            Assert.AreEqual((uint) 0xFF, time[1]);
            Assert.AreEqual((uint) 0xFF, time[2]);
            Assert.AreEqual((uint) 0x7F, time[3]);
        }

        [TestMethod]
        public void DeltaTimeKnownValues()
        {
            Assert.AreEqual((uint) 0x00, RtpMidiHelper.GetDeltaTime(0)[0]);

            Assert.AreEqual((uint) 0xE8, RtpMidiHelper.GetDeltaTime(13419)[0]);
            Assert.AreEqual((uint) 0x6B, RtpMidiHelper.GetDeltaTime(13419)[1]);

            Assert.AreEqual((uint) 0x82, RtpMidiHelper.GetDeltaTime(44100)[0]);
            Assert.AreEqual((uint) 0xD8, RtpMidiHelper.GetDeltaTime(44100)[1]);
            Assert.AreEqual((uint) 0x44, RtpMidiHelper.GetDeltaTime(44100)[2]);

            Assert.AreEqual((uint) 0x81, RtpMidiHelper.GetDeltaTime(2222222)[0]);
            Assert.AreEqual((uint) 0x87, RtpMidiHelper.GetDeltaTime(2222222)[1]);
            Assert.AreEqual((uint) 0xD1, RtpMidiHelper.GetDeltaTime(2222222)[2]);
            Assert.AreEqual((uint) 0x0E, RtpMidiHelper.GetDeltaTime(2222222)[3]);
        }
    }
}