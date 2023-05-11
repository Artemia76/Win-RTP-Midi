namespace Spring.Net.Rtp.AppleMidi
{
    public enum AppleMidiCommand : short
    {
        EndSession = 0x4259, /*   "BY"   */
        Synchronization = 0x434b, /*   "CK"   */
        Invitation = 0x494e, /*   "IN"   */
        InvitationRejected = 0x4e4f, /*   "NO"   */
        InvitationAccepted = 0x4f4b, /*   "OK"   */
        BitRateReceiveLimit = 0x524c, /*   "RL"   */
        ReceiverFeedback = 0x5253, /*   "RS"   */
    }

    public static class AppleMidiConstant
    {
        public const short Signature = -1;
        public const int Version = 2;
    }
}