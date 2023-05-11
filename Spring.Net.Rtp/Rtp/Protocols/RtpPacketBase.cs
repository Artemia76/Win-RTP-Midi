using System;
using Spring.Net.Extensions;

namespace Spring.Net.Rtp.Protocols
{
    // http://tools.ietf.org/html/rfc3550
    // TODO: howto handle padding?

    /// <summary>
    ///     Represents a generic RTP packet.
    /// </summary>
    public abstract class RtpPacketBase
    {
        #region Constants

        private const int RTP_VERSION = 2;

        private const int HEADER_SIZE = 12;

        private const int VER_INDEX = 0;
        private const int PAD_INDEX = 0;
        private const int EXT_INDEX = 0;
        private const int CSRCC_INDEX = 0;

        private const int MPT_INDEX = 1;
        private const int MRK_INDEX = 1;

        private const int SEQ_INDEX = 2;

        private const int TS_INDEX = 4;

        private const int SSRC_INDEX = 8;

        private const byte VER_MASK = 0xC0;
        private const byte PAD_MASK = 0x20;
        private const byte EXT_MASK = 0x10;
        private const byte CSRCC_MASK = 0x0F;

        private const byte MRK_MASK = 0x80;
        private const byte MPT_MASK = 0x7F;

        #endregion

        private byte[] header_ = new byte[] {};
        private byte[] padding_ = new byte[] {};
        private byte[] payload_ = new byte[] {};

        protected RtpPacketBase()
            : this(HEADER_SIZE)
        {
        }

        protected RtpPacketBase(int headerSize)
        {
            headerSize = Math.Min(HEADER_SIZE, headerSize);
            header_ = new byte[headerSize];

            Version = RTP_VERSION;
            Extension = false;
            Marker = false;
        }

        protected virtual int HeaderSize
        {
            get { return HEADER_SIZE; }
        }

        public int Version
        {
            get { return header_.GetVal(VER_INDEX, VER_MASK); }
            private set { header_.SetVal(VER_INDEX, VER_MASK, (byte) value); }
        }

        public bool Padding
        {
            get { return header_.GetFlag(PAD_INDEX, PAD_MASK); }
            protected set
            {
                throw new NotImplementedException();
                header_.SetFlag(PAD_INDEX, PAD_MASK, value);
            }
        }

        public bool Extension
        {
            get { return header_.GetFlag(EXT_INDEX, EXT_MASK); }
            protected set { header_.SetFlag(EXT_INDEX, EXT_MASK, value); }
        }

        public int CsrcCount
        {
            get { return header_.GetVal(CSRCC_INDEX, CSRCC_MASK); }
            protected set
            {
                throw new NotImplementedException();
                header_.SetVal(CSRCC_INDEX, CSRCC_MASK, (byte) value);
            }
        }

        public bool Marker
        {
            get { return header_.GetFlag(MRK_INDEX, MRK_MASK); }
            protected set { header_.SetFlag(MRK_INDEX, MRK_MASK, value); }
        }

        public ushort PayloadType
        {
            get { return header_.GetVal(MPT_INDEX, MPT_MASK); }
            protected set { header_.SetVal(MPT_INDEX, MPT_MASK, (byte) value); }
        }

        public ushort Sequence
        {
            get { return header_.GetUInt16(SEQ_INDEX); }
            set { header_.SetUInt16(SEQ_INDEX, value); }
        }

        public uint Timestamp
        {
            get { return header_.GetUInt32(TS_INDEX); }
            set { header_.SetUInt32(TS_INDEX, value); }
        }

        public uint SsrcId
        {
            get { return header_.GetUInt32(SSRC_INDEX); }
            set { header_.SetUInt32(SSRC_INDEX, value); }
        }

        public uint[] CsrcCollection
        {
            get { return new uint[] {}; }
        }

        #region Operations

        public byte[] GetBuffer()
        {
            var buffer = new byte[header_.Length + payload_.Length + padding_.Length];

            var offset = 0;
            header_.CopyTo(buffer, offset);
            offset += header_.Length;

            payload_.CopyTo(buffer, offset);
            offset += payload_.Length;

            padding_.CopyTo(buffer, offset);
            // offset += padding_.Length;

            return buffer;
        }

        #endregion

        #region Implementation

        protected void SetPayload(byte[] buffer)
        {
            payload_ = buffer;
        }

        protected void SetPadding(byte[] buffer)
        {
            throw new NotImplementedException();
            Padding = true;
        }

        #endregion
    }
}