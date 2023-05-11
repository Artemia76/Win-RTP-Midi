using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;

using Windows.Storage.Streams;

namespace Spring.Net.Udp
{
    public class UdpListener : IDisposable
    {
        private DatagramSocket client_;

        private readonly HostName hostName_;
        private readonly String serviceName_;

        public UdpListener(string host, int port)
            : this (new HostName(host), Convert.ToString(port))
        {
        }

        public UdpListener(HostName hostName, string serviceName)
        {
            hostName_ = hostName;
            serviceName_ = serviceName;
        }

        #region Operations

        public virtual async Task StartAsync()
        {
            client_ = new DatagramSocket();
            client_.MessageReceived += OnReceive;
            await client_.BindEndpointAsync(hostName_, serviceName_);
        }

        public virtual void Stop()
        {
            if (client_ != null)
                client_.Dispose();
            client_ = null;
        }

        public async Task SendAsync(byte[] data, HostName hostName, string serviceName)
        {
            ThrowIfDisposed();
 
            using (var stream = await client_.GetOutputStreamAsync(hostName, serviceName))
            using (var writer = new DataWriter(stream))
            {
                writer.WriteBytes(data);
                await writer.FlushAsync();
                await writer.StoreAsync();
            }
        }

        #endregion

        #region Implementation

        public delegate void DataReceivedHandler(byte[] buffer, HostName remoteAddress, string remotePort);

        public DataReceivedHandler ReceiveHandler;

        private void OnReceive(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            byte[] bytes;

            using (var reader = args.GetDataReader())
            {
                var count = reader.UnconsumedBufferLength;
                var buffer = reader.DetachBuffer();
                bytes = buffer.ToArray();
            }

            if (ReceiveHandler != null && bytes.Length > 0)
                ReceiveHandler(bytes, args.RemoteAddress, args.RemotePort);
        }

        #endregion

        #region IDisposable Implementation

        private void ThrowIfDisposed()
        {
            if (client_ == null)
                throw new ObjectDisposedException(GetType().FullName);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing && client_ != null)
                client_.Dispose();
        }

        #endregion
    }
}