using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using Windows.Networking;
using Windows.Networking.Connectivity;

namespace Spring.WinRT.Utils
{
    public static class Console
    {
        public static void WriteLine(string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(format, args);
        }
    }
}
