using Spring.Net.Rtp.Protocols;

using Windows.Networking;

namespace Spring.Net.Rtp.Interop
{
    public interface IProvideSequenceNumber
    {
        ulong GetNextSequence();

        void ConfirmLastSequence(ulong sequence);
    }
}