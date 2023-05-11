using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

using Windows.Foundation;
using Windows.Networking;

using Spring.Net.Extensions;
using Spring.Net.Rtp.Interop;
using Spring.Net.Rtp.Protocols;
using Spring.Net.Threading;
using Spring.Net.Udp;

using Spring.WinRT.Utils;

namespace Spring.Net.Rtp.AppleMidi
{
    public sealed class AppleMidiSessionListener : INetworkChannel, IDisposable
    {
        public readonly IProvideClockService clock_;
        private UdpListener control_ = null;
        private UdpListener rtp_ = null;

        private readonly uint mySsrc_ = RtpHelper.GetRandomUInt32(0x53535243 /* 'SSRC' */);
        private readonly string party_;

        private readonly List<RtpMidiSession> sessions_ = new List<RtpMidiSession>();
        private readonly ConsumerProducerQueue<RtpPacketInfo> pending_ = new ConsumerProducerQueue<RtpPacketInfo>();

        private readonly ManualResetEvent stopping_ = new ManualResetEvent(false);
        private readonly ManualResetEvent stopped_ = new ManualResetEvent(false);
        private IAsyncAction processor_;

        private uint latency_ = 0;

        private const uint CLOCK_RATE = 10000; // 10 kHz

        public AppleMidiSessionListener(string party, string address, int port)
        {
            party_ = party;

            clock_ = InitializeClockService(CLOCK_RATE);
            InitializeNetworkChannels(address, port);
        }

        #region Events

        public EventHandler<RtpMidiSessionEventArgs> OnSessionClosing;
        public EventHandler<RtpMidiSessionEventArgs> OnSessionCreated;

        private void RaiseSessionEvent(EventHandler<RtpMidiSessionEventArgs> handler, RtpMidiSession session)
        {
            if (handler != null)
                handler(this, new RtpMidiSessionEventArgs {Session = session,});
        }

        #endregion

        #region Attributes

        /// <summary>
        /// Returns the clock rate, i.e. the timestamp increment value over a one second period.
        /// </summary>
        public uint ClockRate
        {
            get { return CLOCK_RATE; }
        }

        /// <summary>
        /// Returns the latest observed media delay (latency) in milliseconds.
        /// </summary>
        public ulong Latency
        {
            get { return Convert.ToUInt64(Math.Ceiling((double) (latency_ * CLOCK_RATE) / 1000)); }
        }

        #endregion

        #region Operations

        public async Task StartAsync()
        {
            // TODO: throw if already started
            InitiatePacketProcessor();
            await OpenNetworkChannelsAsync();
        }

        public void Stop()
        {
            // TODO: throw if not started
            CancelPacketProcessor();
            CloseNetworkChannels();
            CloseSessions();
        }

        #endregion

        #region Network Channel Data Receivers

        private void OnControlChannelReceived(byte[] buffer, HostName remoteAddress, string remotePort)
        {
            WinRT.Utils.Console.WriteLine("OnControlChannelReceived");

            var offset = 0;

            var signature = buffer.GetInt16(offset);
            offset += 2;

            Debug.Assert(signature == AppleMidiConstant.Signature);

            var command = (AppleMidiCommand) buffer.GetInt16(offset);
            offset += 2;

            switch (command)
            {
                case AppleMidiCommand.Invitation:
                    {
                        var version = buffer.GetInt32(offset);
                        offset += 4;
                        var token = buffer.GetInt32(offset);
                        offset += 4;
                        var ssrc = buffer.GetUInt32(offset);
                        offset += 4;

                        var len = buffer.Length - offset;
                        var party = String.Empty;

                        if (len > 0)
                            party = Encoding.UTF8.GetString(buffer, offset, len - 1);

                        WinRT.Utils.Console.WriteLine("<< REQUEST: Invitation: peer = \"{0}\"", party);
                        OnControlChannelInvitation(ssrc, party, token, remoteAddress, remotePort);
                    }
                    break;

                case AppleMidiCommand.EndSession:
                    {
                        var version = buffer.GetInt32(offset);
                        offset += 4;
                        var token = buffer.GetInt32(offset);
                        offset += 4;
                        var ssrc = buffer.GetUInt32(offset);
                        offset += 4;

                        var len = buffer.Length - offset;
                        var party = String.Empty;

                        if (len > 0)
                            party = Encoding.UTF8.GetString(buffer, offset, len - 1);

                        WinRT.Utils.Console.WriteLine("<< REQUEST: End Session.");
                        OnControlChannelEndSession(ssrc);
                    }
                    break;

                case AppleMidiCommand.ReceiverFeedback:
                    {
                        var ssrc = buffer.GetUInt32(offset);
                        offset += 4;

                        // tobias: Apple includes a 32bit sequence-number, but the RTP-packet only specifies 16 bits.
                        // in rtpMIDI, only a ushort is included !

                        var sequence = buffer.GetUInt16(offset);
                        offset += 2;

                        WinRT.Utils.Console.WriteLine("<< REQUEST: Receiver Feedback: sequence = {0}", sequence);
                        OnControlChannelReceiverFeedback(ssrc, sequence);
                    }
                    break;

                case AppleMidiCommand.InvitationAccepted:
                case AppleMidiCommand.InvitationRejected:
                case AppleMidiCommand.Synchronization:
                case AppleMidiCommand.BitRateReceiveLimit:
                    WinRT.Utils.Console.WriteLine("Command {0} not supported.", command);
                    break;

                default:
                    WinRT.Utils.Console.WriteLine("Unknown RTCP packet.");
                    break;
            }
        }

        private void OnSessionChannelDataReceived(byte[] buffer, HostName remoteAddress, string remotePort)
        {
            WinRT.Utils.Console.WriteLine("OnSessionChannelDataReceived");

            var offset = 0;

            var signature = buffer.GetInt16(offset);
            offset += 2;

            Debug.Assert(signature == AppleMidiConstant.Signature);

            var command = (AppleMidiCommand) buffer.GetInt16(offset);
            offset += 2;

            switch (command)
            {
                case AppleMidiCommand.Invitation:
                    {
                        var version = buffer.GetInt32(offset);
                        offset += 4;
                        var token = buffer.GetInt32(offset);
                        offset += 4;
                        var ssrc = buffer.GetUInt32(offset);
                        offset += 4;

                        var len = buffer.Length - offset;
                        var party = String.Empty;

                        if (len > 0)
                            party = Encoding.UTF8.GetString(buffer, offset, len - 1);

                        WinRT.Utils.Console.WriteLine("<< REQUEST: Invitation: peer = \"{0}\"", party);
                        OnSessionChannelInvitation(ssrc, party, token, remoteAddress, remotePort);
                    }
                    break;

                case AppleMidiCommand.Synchronization:
                    {
                        var ssrc = buffer.GetUInt32(offset);
                        offset += 4;

                        var count = buffer[offset];
                        offset += 1;
                        offset += 3; // padding

                        var timeStamp1 = buffer.GetInt64(offset);
                        offset += 8;
                        var timeStamp2 = buffer.GetInt64(offset);
                        offset += 8;
                        var timeStamp3 = buffer.GetInt64(offset);
                        offset += 8;

                        if (count == 0 || count == 2)
                            WinRT.Utils.Console.WriteLine("<< REQUEST: Synchronization: count = {0}", count);
                        else
                            WinRT.Utils.Console.WriteLine("<< RESPONSE: Synchronization: count = {0}", count);

                        var timestamps = new[]
                                             {
                                                 timeStamp1,
                                                 timeStamp2,
                                                 timeStamp3,
                                             };

                        OnSessionSynchronize(ssrc, count, timestamps);
                    }
                    break;

                case AppleMidiCommand.EndSession:
                case AppleMidiCommand.InvitationAccepted:
                case AppleMidiCommand.InvitationRejected:
                case AppleMidiCommand.BitRateReceiveLimit:
                case AppleMidiCommand.ReceiverFeedback:
                    WinRT.Utils.Console.WriteLine("Command {0} not supported.", command);
                    break;

                default:
                    WinRT.Utils.Console.WriteLine("Unknown RTCP packet.");
                    break;
            }
        }

        #endregion

        #region Control Command Requests Handlers

        private void OnControlChannelInvitation(uint ssrc, string party, int token, HostName remoteAddress, string remotePort)
        {
            // TODO: check session does not already exist - unlikely
            // TODO: probably stop accepting sessions when maximum count is reached

            AcceptInvitation(token, control_, remoteAddress, remotePort);
        }

        private void OnControlChannelEndSession(uint ssrc)
        {
            // find session - transmit end request

            var session = sessions_.SingleOrDefault(s => s.PeerSsrc == ssrc);
            if (session == null)
            {
                WinRT.Utils.Console.WriteLine("WARNING: Unable to find target session!");
                return;
            }

            RaiseSessionEvent(OnSessionClosing, session);

            sessions_.Remove(session);
        }

        private void OnControlChannelReceiverFeedback(uint ssrc, ulong sequence)
        {
            // find session - transmit sequence number

            var session = sessions_.SingleOrDefault(s => s.PeerSsrc == ssrc);
            if (session == null)
            {
                WinRT.Utils.Console.WriteLine("WARNING: Unable to find target session!");
                return;
            }

            (session as IProvideSequenceNumber).ConfirmLastSequence(sequence);
        }

        #endregion

        #region Session Channel Command Requests Helpers

        private void OnSessionChannelInvitation(uint ssrc, string party, int token, HostName remoteAddress, string remotePort)
        {
            // TODO: check session already exists

            var session = new RtpMidiSession(mySsrc_, ssrc, party, this as INetworkChannel, remoteAddress, remotePort, clock_);
            sessions_.Add(session);

            AcceptInvitation(token, rtp_, remoteAddress, remotePort);
            RaiseSessionEvent(OnSessionCreated, session);
        }

        private void OnSessionSynchronize(uint ssrc, int count, long[] timestamps)
        {
            // find session - synchronize

            var session = sessions_.SingleOrDefault(s => s.PeerSsrc == ssrc);
            if (session == null)
                return;

            // TODO: how to ensure this is never called for non-existing sessions

            if (count == 0)
            {
                count = 1;
                timestamps[count] = clock_.Now;
                Synchronize(count, timestamps, session.RemoteAddress, session.RemotePort);
                WinRT.Utils.Console.WriteLine(">> RESPONSE: Synchronization: count = {0}", count);
            }

            else if (count == 1)
            {
                // compute media delay
                latency_ = Convert.ToUInt32((double) (clock_.Now - timestamps[0]) / 2);

                count = 2;
                timestamps[count] = clock_.Now;
                Synchronize(count, timestamps, session.RemoteAddress, session.RemotePort);
                WinRT.Utils.Console.WriteLine(">> REQUEST: Synchronization: count = {0}", count);
            }

            else if (count == 2)
            {
                // compute media delay
                latency_ = Convert.ToUInt32(Math.Ceiling((double) (timestamps[2] - timestamps[0]) / 2));

                count = 0;
                timestamps[count] = clock_.Now;
                Synchronize(count, timestamps, session.RemoteAddress, session.RemotePort);
                WinRT.Utils.Console.WriteLine(">> REQUEST: Synchronization: count = {0}", count);
            }

            Debug.WriteLine("MEDIA DELAY: {0}ms. ({1})", Latency, latency_);
        }

        #endregion

        #region Command Responses Helpers

        private async void AcceptInvitation(int token, UdpListener channel, HostName remoteAddress, string remotePort)
        {
            WinRT.Utils.Console.WriteLine(">> RESPONSE: Invitation Accepted: peer = \"{0}\"", party_);
            var buffer = AppleMidiHelper.CreateInvitationCommand(AppleMidiCommand.InvitationAccepted, mySsrc_, party_, token);
            await channel.SendAsync(buffer, remoteAddress, remotePort);
        }

        private async void Synchronize(int count, long[] timestamps, HostName remoteAddress, string remotePort)
        {
            var buffer = AppleMidiHelper.CreateSynchronizeCommand(AppleMidiCommand.Synchronization, mySsrc_, count, timestamps);
            await rtp_.SendAsync(buffer, remoteAddress, remotePort);
        }

        #endregion

        #region Implementation

        private static IProvideClockService InitializeClockService(uint rate)
        {
            var initialTimestamp_ = RtpHelper.GetRandomUInt32(0x54535450 /* TSTP */);
            var clockService = new RtpMidiClock(initialTimestamp_, rate);

            var provider = ServiceProvider.GetProvider();
            provider.RegisterService<IProvideClockService>(clockService);

            return clockService;
        }

        private void InitializeNetworkChannels(string address, int port)
        {
            control_ = new UdpListener(address, port);
            control_.ReceiveHandler += OnControlChannelReceived;

            rtp_ = new UdpListener(address, port + 1);
            rtp_.ReceiveHandler += OnSessionChannelDataReceived;
        }

        private async Task OpenNetworkChannelsAsync()
        {
            await control_.StartAsync();
            await rtp_.StartAsync();
        }

        private void CloseNetworkChannels()
        {
            rtp_.Stop();
            control_.Stop();
        }

        private void ReleaseNetworkChannels()
        {
            if (rtp_ != null)
                rtp_.Dispose();
            rtp_ = null;
            if (control_ != null)
                control_.Dispose();
            control_ = null;
        }

        private sealed class RtpPacketInfo
        {
            public IProvideSequenceNumber SequenceHandler { get; set; }
            public RtpMidiPacket Packet { get; set; }
            public HostName RemoteHost { get; set; }
            public String RemotePort { get; set; }
        }

        /// <summary>
        /// Schedules a raw RTP-MIDI packets for transmission to the target host.
        /// </summary>
        /// <param name="sequenceHandler"></param>
        /// <param name="packet"></param>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        void INetworkChannel.SendPacket(IProvideSequenceNumber sequenceHandler, RtpMidiPacket packet, HostName remoteHost, String remotePort)
        {
            pending_.Enqueue(new RtpPacketInfo() {
                SequenceHandler = sequenceHandler,
                Packet = packet,
                RemoteHost = remoteHost,
                RemotePort = remotePort,
            }
            );
        }

        private void InitiatePacketProcessor()
        {
            WinRT.Utils.Console.WriteLine("====== RTP PACKET PROCESSING WORKER THREAD STARTING ==========");

            stopping_.Reset();
            stopped_.Reset();

            Debug.Assert(processor_ == null);
            processor_ = Windows.System.Threading.ThreadPool.RunAsync(PacketProcessor);
            processor_.Completed += (asyncInfo, asyncStatus) => { stopped_.Set(); };
        }

        private void CancelPacketProcessor()
        {
            stopping_.Set();
            stopped_.WaitOne(5000 /* safety timeout */);
            processor_ = null;
            WinRT.Utils.Console.WriteLine("====== RTP PACKET PROCESSING WORKER THREAD SHUTDOWN ==========");
        }

        private void PacketProcessor(IAsyncAction target)
        {
            const int stopRequested = 0;

            while (true)
            {
                if (WaitHandle.WaitAny(new[] {stopping_, pending_.Published,}) == stopRequested)
                    break;

                RtpPacketInfo packetInfo;
                while (pending_.TryDequeue(out packetInfo))
                {
                    // update packet sequence number

                    var packet = packetInfo.Packet;
                    var sequence = packetInfo.SequenceHandler.GetNextSequence();
                    packet.Sequence = Convert.ToUInt16(sequence & 0x0000FFFF);

                    WinRT.Utils.Console.WriteLine("Processing RTP packet: Sequence = {0}", sequence);

                    // cannot use async in a thread pool work item
                    // http://msdn.microsoft.com/en-us/library/windows/apps/jj248672.aspx
                    
                    // so we need to wait on the task instead

                    var host = packetInfo.RemoteHost;
                    var port = packetInfo.RemotePort;

                    WinRT.Utils.Console.WriteLine(">> {0}ms TRANSMITTING RTP-MIDI PACKET : Sequence = {1}.", TS__.Get().Now, "N/A");

                    rtp_
                        .SendAsync(packet.GetBuffer(), host, port)
                        .Wait();

                    WinRT.Utils.Console.WriteLine(">> {0}ms RTP-MIDI PACKET TRANSMITTEED : Sequence = {1}.", TS__.Get().Now, "N/A");

                }
            }
        }

        private void CloseSessions()
        {
            while (sessions_.Count > 0)
            {
                var session = sessions_[0];
                sessions_.RemoveAt(0);
                RaiseSessionEvent(OnSessionClosing, session);
            }
        }

        #endregion

        #region IDisposable Implementation

        private bool disposed_;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposed_) return;
            disposed_ = true;

            if (disposing)
            {
                ReleaseNetworkChannels();

                stopped_.Dispose();
                stopping_.Dispose();
            }
        }

        private void ThrowIfDisposed()
        {
            if (disposed_)
                throw new ObjectDisposedException(GetType().FullName);
        }

        #endregion
    }
}