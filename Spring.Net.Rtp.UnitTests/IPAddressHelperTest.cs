using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Windows.Networking;
using Windows.Networking.Connectivity;

using Spring.WinRT.Utils;

namespace Spring.Net.Rtp8.UnitTests
{
    [TestClass]
    public class IPAddressHelperTest
    {
        [TestMethod]
        public void ParseIpv4Address()
        {
            var expected = new byte[]{ 192, 168, 1, 2, };
            var actual = new byte[] { };
            var result = IPAddressHelper.TryParseIpv4Address("192.168.1.2", out actual);

            Assert.IsTrue(result);
            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void ParseCanonicalIpv6Address()
        {
            var expected = new byte[] {
                32, 1,
                0, 0,
                65, 55,
                158, 118,
                20, 112,
                55, 240,
                63, 87,
                254, 241,
            };
            var actual = new byte[] { };
            var result = IPAddressHelper.TryParseIpv6Address("2001:0:4137:9e76:1470:37f0:3f57:fef1", out actual);

            Assert.IsTrue(result);
            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void ParseCanonicalIpv6AddressCompressedZeroes()
        {
            var expected = new byte[] {
                32, 1,
                0, 0,
                65, 55,
                0, 0,
                0, 0,
                0, 0,
                0, 0,
                254, 241,
            };
            var actual = new byte[] { };
            var result = IPAddressHelper.TryParseIpv6Address("2001:0:4137::fef1", out actual);

            Assert.IsTrue(result);
            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void ParseCanonicalIpv6AddressCompressedLeadingZeroes()
        {
            var expected = new byte[] {
                0, 0,
                0, 0,
                0, 0,
                32, 1,
                65, 55,
                0, 0,
                0, 0,
                254, 241,
            };
            var actual = new byte[] { };
            var result = IPAddressHelper.TryParseIpv6Address("::2001:4137:0:0:fef1", out actual);
        }

        [TestMethod]
        public void ParseCanonicalIpv6AddressCompressedTrailingZeroes()
        {
            // compressed zeroes are in the middle

            var expected = new byte[] {
                32, 1,
                0, 0,
                65, 55,
                0, 0,
                254, 241,
                0, 0,
                0, 0,
                0, 0,
            };
            var actual = new byte[] { };
            var result = IPAddressHelper.TryParseIpv6Address("2001:0:4137:0:fef1::", out actual);
        }

        [TestMethod]
        public void ParseIpv4MappedIpv6Address()
        {
            // compressed zeroes are in the middle

            var expected = new byte[] {
                0, 0,
                0, 0,
                0, 0,
                0, 0,
                32, 1,
                254, 241,
                10, 254,
                12, 1,
            };
            var actual = new byte[] { };
            var result = IPAddressHelper.TryParseIpv6Address("::2001:fef1:10.254.12.1", out actual);

            Assert.IsTrue(result);
            Assert.IsTrue(expected.SequenceEqual(actual));

            expected = new byte[] {
                32, 1,
                0, 0,
                0, 0,
                0, 0,
                32, 1,
                254, 241,
                10, 254,
                12, 1,
            };
            actual = new byte[] { };
            result = IPAddressHelper.TryParseIpv6Address("2001::2001:fef1:10.254.12.1", out actual);

            Assert.IsTrue(result);
            Assert.IsTrue(expected.SequenceEqual(actual));

            expected = new byte[] {
                32, 1,
                32, 1,
                0, 0,
                0, 0,
                32, 1,
                254, 241,
                10, 254,
                12, 1,
            };
            actual = new byte[] { };
            result = IPAddressHelper.TryParseIpv6Address("2001:2001::2001:fef1:10.254.12.1", out actual);

            Assert.IsTrue(result);
            Assert.IsTrue(expected.SequenceEqual(actual));

            Assert.IsTrue(result);
            Assert.IsTrue(expected.SequenceEqual(actual));

            expected = new byte[] {
                32, 1,
                254, 241,
                0, 0,
                0, 0,
                0, 0,
                0, 0,
                10, 254,
                12, 1,
            };
            actual = new byte[] { };
            result = IPAddressHelper.TryParseIpv6Address("2001:fef1::10.254.12.1", out actual);

            Assert.IsTrue(result);
            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void ParseMalformedIpv6Addresses()
        {
            var actual = new byte[] { };

            // no colon separators
            Assert.IsFalse(IPAddressHelper.TryParseIpv6Address("2001|0|4137||fef1", out actual));
            Assert.AreEqual(0, actual.Length);

            // not valid hexadecimal values
            Assert.IsFalse(IPAddressHelper.TryParseIpv6Address("2z001:0:4i37::fef1", out actual));
            Assert.AreEqual(0, actual.Length);

            // too many compressed sequences
            Assert.IsFalse(IPAddressHelper.TryParseIpv6Address("2001::0::4137", out actual));
            Assert.AreEqual(0, actual.Length);

            // invalid ipv4 mapped address
            Assert.IsFalse(IPAddressHelper.TryParseIpv6Address("::fef1:a.b.c.d", out actual));
            Assert.AreEqual(0, actual.Length);
        }
    }
}
