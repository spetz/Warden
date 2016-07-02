namespace Warden.Watchers.ServerStatus
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    public interface ITcpClient : IDisposable
    {
        /// <summary>
        /// Connects to specified ip address via TCP protocol
        /// </summary>
        /// <param name="ip">A ip address of server/</param>
        /// <param name="port">A port name of server to connect.</param>
        /// <returns>Task that perfroms a connection.</returns>
        Task ConnectAsync(IPAddress ip, int port, TimeSpan? timeout);

        /// <summary>
        /// Gets a flag that indicates wheter current instance is connected to any server.
        /// </summary>
        bool IsConnected { get; }
    }

    internal class DefaultTcpClient : ITcpClient
    {
        private readonly Socket wrapped;

        public DefaultTcpClient()
        {
            this.wrapped = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public async Task ConnectAsync(IPAddress ip, int port, TimeSpan? timeout)
        {
            try
            {
                var st = this.wrapped.BeginConnect(ip, port, null, null);

                await
                    Task.Factory.StartNew(
                        () => timeout != null ? st.AsyncWaitHandle.WaitOne(timeout.Value) : st.AsyncWaitHandle.WaitOne());

                this.IsConnected = this.wrapped.Connected;

                this.wrapped.EndConnect(st);
            }
            catch (SocketException)
            {
                this.IsConnected = false;
            }
        }

        public bool IsConnected { get; private set; }

        public void Dispose()
        {
            ((IDisposable)this.wrapped).Dispose();
        }
    }
}
