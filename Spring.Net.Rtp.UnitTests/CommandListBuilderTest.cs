using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spring.Net.Extensions;

namespace Spring.Net.Rtp.UnitTests
{
    [TestClass]
    public class CommandListBuilderTest
    {
        [TestMethod]
        public void GetCommandListFifteenBytes()
        {
            // [Channel: 1] Note On (C4, v=63)
            var commandBuilder = new RtpMidiCommandListBuilder();
            commandBuilder.AddCommand(0U, new byte[] {0x91, 0x3F, 0x3F,});

            var commandList = commandBuilder.GetCommandList();

            //  0                   1                   2                   3
            //  0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
            // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
            // |B|J|Z|P|LEN... |  MIDI list ...                                 |
            // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

            Assert.AreEqual(0, commandList.GetVal(0, 0x80)); // B = 0 ==> LEN is 4 bits
            Assert.IsFalse(commandList.GetFlag(0, 0x40)); // J = 0 ==> no journal
            Assert.IsFalse(commandList.GetFlag(0, 0x20)); // Z = 0 ==> no initial delta time
            Assert.IsFalse(commandList.GetFlag(0, 0x10)); // P ?

            Assert.AreEqual(3, commandList.GetVal(0, 0x0F)); // LEN = 3
            Assert.AreEqual(4, commandList.Length);
        }
    }
}