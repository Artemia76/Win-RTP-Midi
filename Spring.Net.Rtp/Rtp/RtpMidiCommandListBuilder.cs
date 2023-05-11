using System;
using System.Collections.Generic;
using System.Linq;
using Spring.Net.Extensions;

namespace Spring.Net.Rtp
{
    public sealed class RtpMidiCommandListBuilder
    {
        private readonly IList<byte[]> buffers_ = new List<byte[]>();

        #region Operations

        public void AddCommand(uint delta, byte[] command)
        {
            var time = RtpMidiHelper.GetDeltaTime(delta);
            buffers_.Add(time);
            buffers_.Add(command);
        }

        public byte[] GetCommandList()
        {
            // TODO : split command list at 4095 octets

            // remove initial delta time if zero

            var z = true;
            if (buffers_[0].Length == 1 && buffers_[0][0] == 0x00)
            {
                buffers_.RemoveAt(0);
                z = false;
            }

            var len = buffers_.Sum(buf => buf.Length);
            var offset = len <= 15 ? 1 : 2;
            var size = offset + len;

            var b = len >= 15;
            var j = false;
            var p = false;

            var buffer = new byte[size];

            buffer.SetFlag(0, 0x80, b);
            buffer.SetFlag(0, 0x40, z); // J flag: no journal
            buffer.SetFlag(0, 0x20, j); // Z flag: initial delta time
            buffer.SetFlag(0, 0x10, p); // Z flag: initial delta time

            if (b)
            {
                buffer.SetVal(0, 0x0F, Convert.ToByte((len & 0x0F00) >> 8));
                buffer.SetVal(1, 0xFF, Convert.ToByte((len & 0x00FF)));
            }
            else
            {
                buffer.SetVal(0, 0x0F, Convert.ToByte(len));
            }

            foreach (var buf in buffers_)
            {
                buf.CopyTo(buffer, offset);
                offset += buf.Length;
            }

            return buffer;
        }

        public void Clear()
        {
            buffers_.Clear();
        }

        #endregion
    }
}