using System.Text;
using Spring.Net.Extensions;

namespace Spring.Net.Rtp.AppleMidi
{
    public static class AppleMidiHelper
    {
        /// <summary>
        ///     Creates a byte array suitable for an AppleMIDI Invitation command.
        /// </summary>
        /// <param name="command">
        ///     One of the following constants:
        ///     AppleMidiCommand.Invitation,
        ///     AppleMidiCommand.InvitationAccepted,
        ///     AppleMidiCommand.InvitationRejected,
        ///     AppleMidiCommand.EndSession.
        /// </param>
        /// <param name="ssrc"></param>
        /// <param name="party"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static byte[] CreateInvitationCommand(AppleMidiCommand command, uint ssrc, string party, int token)
        {
            var response = new byte[16 + party.Length + ((party.Length > 0) ? 1 : 0)];
            {
                var offset = 0;
                response.SetInt16(offset, AppleMidiConstant.Signature);
                offset += 2;
                response.SetInt16(offset, (short) command);
                offset += 2;
                response.SetInt32(offset, AppleMidiConstant.Version);
                offset += 4;
                response.SetInt32(offset, token);
                offset += 4;
                response.SetUInt32(offset, ssrc);
                offset += 4;

                Encoding.UTF8.GetBytes(party).CopyTo(response, offset);
            }

            return response;
        }

        /// <summary>
        ///     Create a byte array suitable for and AppleMidi synchronize command.
        /// </summary>
        /// <param name="command">Ignored. Hardcoded to AppleMidiCommand.Synchronize.</param>
        /// <param name="ssrc"></param>
        /// <param name="count"></param>
        /// <param name="timestamps"></param>
        /// <returns></returns>
        public static byte[] CreateSynchronizeCommand(AppleMidiCommand command, uint ssrc, int count, long[] timestamps)
        {
            var response = new byte[36];
            {
                var offset = 0;
                response.SetInt16(offset, AppleMidiConstant.Signature);
                offset += 2;
                response.SetInt16(offset, (short) AppleMidiCommand.Synchronization);
                offset += 2;

                response.SetUInt32(offset, ssrc);
                offset += 4;

                response[offset] = (byte) count;
                offset += 1;
                offset += 3; // padding

                if (count == 0)
                {
                    response.SetInt64(offset, timestamps[0]);
                    offset += 8;
                    response.SetInt64(offset, 0);
                    offset += 8;
                    response.SetInt64(offset, 0);
                    // offset += 8;
                }

                else if (count == 1)
                {
                    response.SetInt64(offset, timestamps[0]);
                    offset += 8;
                    response.SetInt64(offset, timestamps[1]);
                    offset += 8;
                    response.SetInt64(offset, 0);
                    // offset += 8;
                }

                else if (count == 2)
                {
                    response.SetInt64(offset, timestamps[0]);
                    offset += 8;
                    response.SetInt64(offset, timestamps[1]);
                    offset += 8;
                    response.SetInt64(offset, timestamps[2]);
                    // offset += 8;
                }
            }

            return response;
        }
    }
}