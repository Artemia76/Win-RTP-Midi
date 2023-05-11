namespace Spring.Net.Rtp.Protocols
{
    public sealed class RtpMidiPacket : RtpPacketBase
    {
        private byte[] command_ = new byte[] {};
        private byte[] journal_ = new byte[] {};

        public RtpMidiPacket()
        {
            PayloadType = 97; // rtp-midi
            Marker = false;
        }

        public void SetCommandList(byte[] buffer)
        {
            command_ = buffer;
            SetPayload();
        }

        public void SetRecoveryJournal(byte[] buffer)
        {
            journal_ = buffer;
            SetPayload();
        }

        #region Implementation

        public void SetPayload()
        {
            var payload = new byte[command_.Length + journal_.Length];
            var offset = 0;
            command_.CopyTo(payload, offset);
            offset += command_.Length;

            journal_.CopyTo(payload, offset);
            // offset += journal_.Length;

            SetPayload(payload);
        }

        #endregion
    }
}