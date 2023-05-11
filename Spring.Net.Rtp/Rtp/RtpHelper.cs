using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Runtime.InteropServices;

using Spring.WinRT.Utils;

using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.System.Profile;

namespace Spring.Net.Rtp
{
    public static class RtpHelper
    {
        /// <summary>
        ///     Generates a random 32-bit quantity using
        ///     an algorithm suggested in RFC 3550 (RTP) Appendix A.6.
        /// </summary>
        /// <remarks>
        ///     Note that this routine produces the same result on
        ///     repeated calls until the value of the system clock changes unless
        ///     different values are supplied for the type argument
        /// </remarks>
        /// <returns></returns>
        public static uint GetRandomUInt32(int type)
        {
            var buffer = GetEntropy(type);
            var hash  = ComputeMd5Hash(buffer);

            var random = 0U;
            for (var index = 0; index < 16; index += 4)
                random ^= BitConverter.ToUInt32(hash, index);

            return random;
        }

        public static int GetRandomInt32(int type)
        {
            return (int) GetRandomUInt32(type);
        }

        #region Implementation

        private static byte[] GetEntropy(int type)
        {
            var entropy = new Entropy
            {
                Type = type,
                Ticks = DateTime.UtcNow.Ticks,
                ProcessorTime = Environment.TickCount,
                HostName = GetMachineName(),
                HostAddress = GetHostIpAddress(),
                UniqueId = GetUniqueId(),
            };

            return entropy.GetBytes();
        }

        private struct Entropy
        {
            public byte[] HostAddress;
            public string HostName;
            public long ProcessorTime;
            public long Ticks;
            public int Type;
            public byte[] UniqueId;

            public byte[] GetBytes()
            {
                var hostAddress = HostAddress;
                var hostName = Encoding.UTF8.GetBytes(HostName);
                var processorTime = BitConverter.GetBytes(ProcessorTime);
                var ticks = BitConverter.GetBytes(Ticks);
                var type = BitConverter.GetBytes(Type);
                var uniqueId = UniqueId;

                var size = hostAddress.Length +
                           hostName.Length +
                           processorTime.Length +
                           ticks.Length +
                           type.Length +
                           uniqueId.Length
                    ;

                var buffer = new byte[size];
                var offset = 0;

                hostAddress.CopyTo(buffer, offset);
                offset += hostAddress.Length;
                hostName.CopyTo(buffer, offset);
                offset += hostName.Length;
                processorTime.CopyTo(buffer, offset);
                offset += processorTime.Length;
                ticks.CopyTo(buffer, offset);
                offset += ticks.Length;
                type.CopyTo(buffer, offset);
                offset += type.Length;
                uniqueId.CopyTo(buffer, offset);
                //offset += uniqueId.Length;

                return buffer;
            }
        };

        public static byte[] ComputeMd5Hash(byte[] buffer)
        {
            var md5 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            var hashBuffer = md5.HashData(CryptographicBuffer.CreateFromByteArray(buffer));

            byte[] hash;
            CryptographicBuffer.CopyToByteArray(hashBuffer, out hash);

            return hash;
        }

        public static byte[] GetHostIpAddress()
        {
            return IPAddressHelper.GetHostIpAddress();
        }

        private static string GetMachineName()
        {
            return EnvironmentHelper.GetMachineName();
        }

        private static byte[] GetUniqueId()
        {
            var token = HardwareIdentification.GetPackageSpecificToken(null);
            var hardwareId = token.Id;
            var dataReader = DataReader.FromBuffer(hardwareId);

            byte[] bytes = new byte[hardwareId.Length];
            dataReader.ReadBytes(bytes);

            return bytes;
        }

        #endregion
    }
}