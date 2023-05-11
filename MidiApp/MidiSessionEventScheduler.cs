using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Windows.Foundation;
using Windows.System.Threading;

using Spring.Net.Rtp;

namespace MidiApp
{
    public sealed class MidiSessionEventScheduler : IDisposable
    {
        private ConcurrentQueue<byte[]> events_ = new ConcurrentQueue<byte[]>();
        private ThreadPoolTimer timer_ = null;

        private RtpMidiSession session_ = null;

        public int Count { get { return events_.Count; } }

        public void Start(RtpMidiSession session)
        {
           session_ = session;
           StartTimer();
        }

        public void Stop()
        {
            StopTimer();
            session_ = null;
        }

        public void AddEvent(byte[] buffer)
        {
            events_.Enqueue(buffer);
        }

        #region Implementation

        private void StartTimer()
        {
            if (timer_ != null) StopTimer();

            timer_ = ThreadPoolTimer.CreatePeriodicTimer(EventScheduler, TimeSpan.FromMilliseconds(10));
        }

        private void StopTimer()
        {
            if (timer_ == null)
                return;

            timer_.Cancel();
            timer_ = null;
        }

        private void EventScheduler(ThreadPoolTimer timer)
        {
            var count = events_.Count;
            if (count == 0)
                return;

            StopTimer();

            Spring.WinRT.Utils.Console.WriteLine("Processing {0} MIDI events.", count);

            try
            {
                var commands = new List<byte[]>();

                while (count-- > 0)
                {
                    byte[] bytes;
                    if (!events_.TryDequeue(out bytes))
                        throw new Exception("Unable to process MIDI events.");
                    commands.Add(bytes);
                }

                session_.TransmitCommands(commands);
            }
            finally
            {
                StartTimer();
            }
        }

        #endregion 

        #region IDisposable Implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposed_ = false;

        private void Dispose(bool disposing)
        {
            if (disposed_) return;
            disposed_ = true;

            if (disposing)
            {
            }
        }

        #endregion
    }
}
