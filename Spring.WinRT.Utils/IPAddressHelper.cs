using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using Windows.Networking;
using Windows.Networking.Connectivity;

namespace Spring.WinRT.Utils
{
    public static class IPAddressHelper
    {
        public static string GetHostAddress()
        {
            var profile = NetworkInformation.GetInternetConnectionProfile();

            if (profile != null && profile.NetworkAdapter != null)
            {
                Func<HostName, bool> isAssociatedWithNetworkAdapter =
                    (host) =>
                    {
                        return host.IPInformation != null &&
                            host.IPInformation.NetworkAdapter != null &&
                            host.IPInformation.NetworkAdapter.NetworkAdapterId ==
                            profile.NetworkAdapter.NetworkAdapterId
                            ;
                    };

                IReadOnlyList<HostName> hostnames = NetworkInformation.GetHostNames();

                //HostName hostname = hostnames.SingleOrDefault(isAssociatedWithNetworkAdapter);
                HostName hostname = hostnames.FirstOrDefault(isAssociatedWithNetworkAdapter);
                if (hostname != null)
                    return hostname.CanonicalName;
            }
                return String.Empty;
        }

        /// <summary>
        /// Returns the local host IP address as a byte array.
        /// </summary>
        /// <returns></returns>
        public static byte[] GetHostIpAddress()
        {
            var address = GetHostAddress();
            if (!String.IsNullOrEmpty(address))
            {
                var buffer = new byte[] { };
                if (TryParseIpAddress(address, out buffer))
                    return buffer;
            }

            return new byte[] { };
        }        

        /// <summary>
        /// Parses the string representation of an IP address into a byte array.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static bool TryParseIpAddress(string address, out byte[] buffer)
        {
            return
                TryParseIpv6Address(address, out buffer) ||
                TryParseIpv4Address(address, out buffer)
                    ;
        }

        /// <summary>
        /// Parses the dotted decimal string representation
        /// of an IPv4 address into a byte array.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static bool TryParseIpv4Address(string address, out byte[] bytes)
        {
            bytes = new byte[] { };

            var components = address.Split(new[] { '.' });
            if (components.Length != 4)
                return false;

            var ipv4 = new byte[components.Length];
            for (var index = 0; index < components.Length; index++)
            {
                byte b = 0;
                if (!Byte.TryParse(components[index], out b))
                    return false;
                ipv4[index] = b;
            }

            bytes = ipv4;
            return true;
        }

        /// <summary>
        /// Parses the colon-separated hexadecimal canonical string representation
        /// of an IPv6 address into a byte array.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static bool TryParseIpv6Address(string address, out byte[] bytes)
        {
            // http://tools.ietf.org/html/rfc5952

            bytes = new byte[] { };

            var components = address.Split(new[] { ':' });

            if (components.Length < 3 || components.Length > 8)
                return false;

            var ipv6 = new byte[16];

            var start = 0;
            var end = components.Length;
            var count = 8;

            // handle IP v4 mapped addresses

            var ipv4 = new byte[4];
            if (TryParseIpv4Address(components[components.Length - 1], out ipv4))
            {
                ipv4.CopyTo(ipv6, 12);
                end = components.Length - 1;
                count = 6;
            }

            // leading or trailing  double-colon are split into two empty fields
            // we only need one so shorten the range of components as necessary

            if (components[0].Length == 0 &&
                components[1].Length == 0)
            {
                start++;
            }
            else if (
                components.Length > 2 &&
                components[components.Length - 2].Length == 0 &&
                components[components.Length - 1].Length == 0)
            {
                end--;
            }

            for (int index = start, target = 0; index < end; index++, target++)
            {
                if (components[index].Length == 0)
                {
                    target += (count - (end - start));
                    if (target > 6)
                        return false;
                    continue;
                }

                ushort u;
                if (!UInt16.TryParse(components[index], NumberStyles.HexNumber, null, out u))
                    return false;

                ipv6[(target * 2) + 0] = Convert.ToByte((u & 0xFF00) >> 8);
                ipv6[(target * 2) + 1] = Convert.ToByte(u & 0x00FF);
            }

            bytes = ipv6;
            return true;
        }
    }
}
