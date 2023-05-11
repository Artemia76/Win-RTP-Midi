using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Networking;
using Windows.Networking.Connectivity;

namespace Spring.WinRT.Utils
{
    public static class EnvironmentHelper
    {
        public static string GetMachineName()
        {
            Func<HostName, bool> isDomainName =
                (host) => host.Type == HostNameType.DomainName
                    ;

            var hostNames = NetworkInformation.GetHostNames();
            var hostName = hostNames.First(isDomainName);
            return hostName.DisplayName;
        }
    }
}
