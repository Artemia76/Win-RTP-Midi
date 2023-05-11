using System;
using System.Diagnostics;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spring.Net.Diagnostics;
using Spring.Net.Extensions;

namespace Spring.Net.Rtp.UnitTests
{
    [TestClass]
    public class ByteArrayExtensionsTest
    {
        [TestMethod]
        public void ByteArrayGetInt16()
        {
            var bytes = new byte[] {0x00, 0xF1, 0x02, 0x03,};
            Assert.AreEqual(-3838, bytes.GetInt16(1));
        }

        [TestMethod]
        public void ByteArraySetInt16()
        {
            var bytes = new byte[] {0x00, 0xF1, 0x02, 0x03,};

            bytes.SetInt16(1, -4567);

            Assert.AreEqual(238, bytes[1]);
            Assert.AreEqual(41, bytes[2]);
        }

        [TestMethod]
        public void ByteArrayGetUInt16()
        {
            var bytes = new byte[] {0x00, 0x01, 0x02, 0x03,};
            Assert.AreEqual((ushort) 258, bytes.GetUInt16(1));
        }

        [TestMethod]
        public void ByteArraySetUInt16()
        {
            var bytes = new byte[] {0x00, 0xF1, 0x02, 0x03,};

            bytes.SetUInt16(1, 567);

            Assert.AreEqual(2, bytes[1]);
            Assert.AreEqual(55, bytes[2]);
        }

        [TestMethod]
        public void ByteArrayGetInt32()
        {
            var bytes = new byte[] {0x00, 0xF1, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,};
            Assert.AreEqual(-251526396, bytes.GetInt32(1));
        }

        [TestMethod]
        public void ByteArraySetInt32()
        {
            var bytes = new byte[] {0x00, 0xF1, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,};

            bytes.SetInt32(1, -341322312);

            Assert.AreEqual(235, bytes[1]);
            Assert.AreEqual(167, bytes[2]);
            Assert.AreEqual(213, bytes[3]);
            Assert.AreEqual(184, bytes[4]);
        }

        [TestMethod]
        public void ByteArrayGetUInt32()
        {
            var bytes = new byte[] {0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,};
            Assert.AreEqual((uint) 16909060, bytes.GetUInt32(1));
        }

        [TestMethod]
        public void ByteArraySetUInt32()
        {
            var bytes = new byte[] {0x00, 0xF1, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,};

            bytes.SetUInt32(1, 3413223126);

            Assert.AreEqual(203, bytes[1]);
            Assert.AreEqual(113, bytes[2]);
            Assert.AreEqual(166, bytes[3]);
            Assert.AreEqual(214, bytes[4]);
        }

        [TestMethod]
        public void ByteArrayGetInt64()
        {
            var bytes = new byte[] {0x00, 0xF1, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a,};
            Assert.AreEqual(-1080297644816464120, bytes.GetInt64(1));
        }

        [TestMethod]
        public void ByteArraySetInt64()
        {
            var bytes = new byte[] {0x00, 0xF1, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a,};

            bytes.SetInt64(1, -107918634705453020);

            Assert.AreEqual(254, bytes[1]);
            Assert.AreEqual(128, bytes[2]);
            Assert.AreEqual(152, bytes[3]);
            Assert.AreEqual(146, bytes[4]);
            Assert.AreEqual(209, bytes[5]);
            Assert.AreEqual(188, bytes[6]);
            Assert.AreEqual(128, bytes[7]);
            Assert.AreEqual(36, bytes[8]);
        }

        [TestMethod]
        public void ByteArrayGetUInt64()
        {
            var bytes = new byte[] {0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a,};
            Assert.AreEqual((ulong) 72623859790382856, bytes.GetUInt64(1));
        }

        [TestMethod]
        public void ByteArraySetUInt64()
        {
            var bytes = new byte[] {0x00, 0xF1, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a,};

            bytes.SetUInt64(1, 1079186347054530209);

            Assert.AreEqual(14, bytes[1]);
            Assert.AreEqual(250, bytes[2]);
            Assert.AreEqual(10, bytes[3]);
            Assert.AreEqual(67, bytes[4]);
            Assert.AreEqual(206, bytes[5]);
            Assert.AreEqual(162, bytes[6]);
            Assert.AreEqual(254, bytes[7]);
            Assert.AreEqual(161, bytes[8]);
        }

        [TestMethod]
        public void ByteArrayGetValue()
        {
            // 0xBF =  10111111
            // 0xC0 =  11------
            // ==>     10------
            // ==>     2
            var bytes = new byte[] {0xBF, 0xFF, 0xBF, 0xFF};
            Assert.AreEqual(2, bytes.GetVal(0, 0xC0));
            Assert.AreEqual(2, bytes.GetVal(2, 0xC0));
        }

        [TestMethod]
        public void ByteArraySetValue()
        {
            var bytes = new byte[] {0xAA, 0xAA, 0xBF, 0xFF,};

            bytes.SetVal(0, 0x3C, 5);
            bytes.SetVal(2, 0xC0, 3);

            Assert.AreEqual(0x96, bytes[0]);
            Assert.AreEqual(0xFF, bytes[2]);
        }

        [TestMethod]
        public void ByteArrayGetFlag()
        {
            // 0xAA =  10101010
            var bytes = new byte[] {0xAA, 0xAA,};

            Assert.IsTrue(bytes.GetFlag(0, 0x80));
            Assert.IsFalse(bytes.GetFlag(0, 0x40));
            Assert.IsTrue(bytes.GetFlag(1, 0x80));
            Assert.IsFalse(bytes.GetFlag(1, 0x40));
        }

        [TestMethod]
        public void ByteArraySetFlag()
        {
            var bytes = new byte[] {0xAA, 0xAA,};

            // 0xAA 0xAA =  10101010 10101010
            bytes.SetFlag(0, 0x80, false);
            bytes.SetFlag(1, 0x04, true);

            // 0x2A 0xAD =  00101010 11101010
            Assert.AreEqual(0x2A, bytes[0]);
            Assert.AreEqual(0xAE, bytes[1]);
        }
    }

    [TestClass]
    public class DebugAssertTest
    {
        [TestMethod]
        [Conditional("DEBUG")]
        public void HasContiguousBitsSet()
        {
#if DEBUG
            Assert.IsTrue(DebugAssert.HasOnlyContiguousBits(0x78));
            Assert.IsTrue(DebugAssert.HasOnlyContiguousBits(0x3C));
#endif
        }

        [TestMethod]
        [Conditional("DEBUG")]
        public void HasNotContiguousBitsSet()
        {
#if DEBUG
            Assert.IsFalse(DebugAssert.HasOnlyContiguousBits(0x22));
            Assert.IsFalse(DebugAssert.HasOnlyContiguousBits(0x5F));
#endif
        }
    }
}