using System;
using System.Threading.Tasks;
using System.Net;
using Spring.Net.Rtp.Interop;
using Spring.Net.Rtp.Protocols;

using Windows.Networking;

using Spring.Net.Udp;
using Spring.WinRT.Utils;

namespace Spring.Net.Rtp
{
    public sealed class TS__
    {
        private readonly long origin_ = TS__.RawNow;
        private static readonly TS__ instance_ = new TS__();
        private TS__(){}
        public static TS__ Get() { return instance_; }
        public long Now { get { return TS__.RawNow - origin_; } }
        private static long RawNow { get { return DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond; } }
    }

    public sealed class RtpMidiSession : IProvideSequenceNumber
    {
        private readonly IProvideClockService clock_;
        private readonly INetworkChannel channel_;
        private readonly HostName remoteAddress_;
        private readonly string remotePort_;

        private readonly uint mySsrc_;
        private readonly uint peerSsrc_;
        private readonly string peer_;

        private ulong feedback_;
        private ulong sequence_ = RtpHelper.GetRandomUInt32(0x5345514E /* SEQN */);

        internal RtpMidiSession
            (
            uint ssrc, uint peerSsrc, string peer,
            INetworkChannel channel, HostName remoteAddress, string remotePort,
            IProvideClockService clock
            )
        {
            channel_ = channel;
            remoteAddress_ = remoteAddress;
            remotePort_ = remotePort;

            mySsrc_ = ssrc;
            peerSsrc_ = peerSsrc;
            peer_ = peer;

            clock_ = clock;
        }

        #region Attributes

        public HostName RemoteAddress
        {
            get { return remoteAddress_; }
        }

        public string RemotePort
        {
            get { return remotePort_; }
        }

        public uint PeerSsrc
        {
            get { return peerSsrc_; }
        }

        public string Peer
        {
            get { return peer_; }
        }

        #endregion

        #region Operations

        public void TransmitCommands(System.Collections.Generic.IEnumerable<byte[]> commands)
        {
            // create an RTP-MIDI packet *without* a sequence number
            // the sequence number will be calculated retrieved just
            // before sending the packet. This ensures that multiple
            // interleaved sessions do not cause sequence numbers to
            // be transmitted out of order.

            var packet = new RtpMidiPacket();
            packet.SsrcId = mySsrc_;
            packet.Sequence = 0;
            packet.Timestamp = clock_.Now;

            var builder = new RtpMidiCommandListBuilder();
            foreach (var command in commands)
                builder.AddCommand(0, command);

            var buffer = builder.GetCommandList();

            packet.SetCommandList(buffer);

            WinRT.Utils.Console.WriteLine(">> {0}ms SENDING RTP-MIDI PACKET : Sequence = {1}.", TS__.Get().Now, "N/A");
            channel_.SendPacket(this as IProvideSequenceNumber, packet, remoteAddress_, remotePort_);
        }

        #endregion

        #region IProvideSequenceNumber Implementation

        void IProvideSequenceNumber.ConfirmLastSequence(ulong sequence)
        {
            feedback_ = sequence;

            // TODO: trim recovery journal
        }

        ulong IProvideSequenceNumber.GetNextSequence()
        {
            return sequence_++;
        }

        #endregion

        #region Implementation

        #endregion
    }
}