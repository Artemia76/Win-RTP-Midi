using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spring.Net.Rtp.Protocols;

namespace Spring.Net.Rtp.UnitTests
{
    internal sealed class MockRtpPacket : RtpPacketBase
    {
        public MockRtpPacket()
        {
            SetPayloadType(123);
            SetMarker(false);
            SetExtension(false);
        }

        public void SetMarker(bool value)
        {
            base.Marker = value;
        }

        public void SetExtension(bool value)
        {
            base.Extension = value;
        }

        public void SetPayloadType(ushort type)
        {
            base.PayloadType = type;
        }
    }

    [TestClass]
    public class RtpPacketBaseTest
    {
        [TestMethod]
        public void RtpPacketVersionIsTwo()
        {
            var packet = new MockRtpPacket();
            Assert.AreEqual(2, packet.Version);
        }

        [TestMethod]
        public void RtpPacketGetMarker()
        {
            var packet = new MockRtpPacket();
            Assert.IsFalse(packet.Marker);
        }

        [TestMethod]
        public void RtpPacketSetMarker()
        {
            var packet = new MockRtpPacket();
            packet.SetMarker(true);
            Assert.IsTrue(packet.Marker);
        }

        [TestMethod]
        public void RtpPacketGetExtension()
        {
            var packet = new MockRtpPacket();
            Assert.IsFalse(packet.Extension);
        }

        [TestMethod]
        public void RtpPacketSetExtension()
        {
            var packet = new MockRtpPacket();
            packet.SetExtension(true);
            Assert.IsTrue(packet.Extension);
        }

        [TestMethod]
        public void RtpPacketGetPayloadType()
        {
            var packet = new MockRtpPacket();
            Assert.AreEqual(123, packet.PayloadType);
        }

        [TestMethod]
        public void RtpPacketSetPayloadType()
        {
            var packet = new MockRtpPacket();
            var marker = packet.Marker;
            packet.SetPayloadType(45);
            Assert.AreEqual(marker, packet.Marker);
        }

        [TestMethod]
        public void RtpPacketSetTimeStamp()
        {
            var packet = new MockRtpPacket();
            const uint expected = (uint) Int32.MaxValue;
            packet.Timestamp = expected;
            var actual = packet.Timestamp;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RtpPacketTimeStampUInt32()
        {
            var packet = new MockRtpPacket();
            const uint expected = UInt32.MaxValue;
            packet.Timestamp = expected;
            var actual = packet.Timestamp;

            Assert.AreEqual(expected, actual);
        }
    }
}