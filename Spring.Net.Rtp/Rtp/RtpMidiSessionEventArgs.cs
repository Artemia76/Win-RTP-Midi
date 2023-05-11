using System;

namespace Spring.Net.Rtp
{
    public sealed class RtpMidiSessionEventArgs : EventArgs
    {
        public RtpMidiSession Session { get; internal set; }
    }
}