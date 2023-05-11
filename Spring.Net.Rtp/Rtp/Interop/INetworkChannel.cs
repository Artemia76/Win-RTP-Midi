using Spring.Net.Rtp.Protocols;

using Windows.Networking;

namespace Spring.Net.Rtp.Interop
{
    public interface INetworkChannel
    {
        /// <summary>
        /// Returns the clock rate, i.e. the timestamp increment value over a one second period.
        /// </summary>
        uint ClockRate { get; }

        /// <summary>
        /// Returns the latest observed media delay (latency) in milliseconds.
        /// </summary>
        ulong Latency { get; }

        /// <summary>
        /// Schedules a raw RTP-MIDI packets for transmission to the target host.
        /// </summary>
        /// <param name="sequenceHandler"></param>
        /// <param name="packet"></param>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        void SendPacket(IProvideSequenceNumber sequenceHandler, RtpMidiPacket packet, HostName hostname, string port);
    }
}