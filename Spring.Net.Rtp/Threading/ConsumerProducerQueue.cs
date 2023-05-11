using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Spring.Net.Threading
{
    public sealed class ConsumerProducerQueue<T> : IDisposable where T : class
    {
        private readonly ConcurrentQueue<T> items_ = new ConcurrentQueue<T>();
        private readonly AutoResetEvent published_ = new AutoResetEvent(false);

        public ConsumerProducerQueue()
        {
        }

        #region Attributes

        public WaitHandle Published
        {
            get { return published_; }
        }

        #endregion

        #region Operations

        public void Enqueue(T item)
        {
            ThrowIfDisposed();
            items_.Enqueue(item);
            published_.Set();
        }

        public bool TryDequeue(out T item)
        {
            ThrowIfDisposed();
            return items_.TryDequeue(out item);
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
                published_.Dispose();
        }

        private void ThrowIfDisposed()
        {
            if (disposed_)
                throw new ObjectDisposedException(GetType().FullName);
        }

        #endregion
    }
}