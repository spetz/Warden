namespace Warden.Watchers.Port
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    public interface ITcpClient : IDisposable
    {
        /// <summary>
        /// Connects to specified IP address via TCP protocol.
        /// </summary>
        /// <param name="ip">A ip address of server.</param>
        /// <param name="port">A port name of server to connect.</param>
        /// <param name="timeout">Time for connection trial.</param>
        /// <returns>Task that perfroms a connection.</returns>
        Task ConnectAsync(IPAddress ip, int port, TimeSpan? timeout);

        /// <summary>
        /// Gets a flag that indicates wheter current instance is connected to any server.
        /// </summary>
        bool IsConnected { get; }
    }

    public class TcpClient : ITcpClient
    {
        /// <summary>
        /// Sockets instance that is used to connect to remote server.
        /// </summary>
        private readonly Socket _socket;

        /// <summary>
        /// Gets a flag that indicates wheter current instance is connected to any server.
        /// </summary>
        public bool IsConnected { get; private set; }


        /// <summary>
        /// Creates a default TCP client that allows to connect to a remote server via TCP protocol.
        /// </summary>
        public TcpClient()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        /// <summary>
        /// Connects to specified IP address via TCP protocol.
        /// </summary>
        /// <param name="ip">A IP address of a server.</param>
        /// <param name="port">A port name of the server to connect.</param>
        /// <param name="timeout">Time for connection trial.</param>
        /// <returns>Task that perfroms a connection.</returns>
        public async Task ConnectAsync(IPAddress ip, int port, TimeSpan? timeout)
        {
            try
            {
                var asyncConnectionResult = _socket.BeginConnect(ip, port, null, null);

                await
                    Task.Factory.StartNew(
                        () => timeout != null ? asyncConnectionResult.AsyncWaitHandle.WaitOne(timeout.Value) : asyncConnectionResult.AsyncWaitHandle.WaitOne());

                IsConnected = _socket.Connected;

                _socket.EndConnect(asyncConnectionResult);
            }
            catch (SocketException)
            {
                IsConnected = false;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _socket.Dispose();
        }
    }
}
