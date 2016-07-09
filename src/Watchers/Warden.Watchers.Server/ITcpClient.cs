using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Warden.Watchers.Server
{
    /// <summary>
    /// Custom TCP Client service.
    /// </summary>
    public interface ITcpClient : IDisposable
    {
        /// <summary>
        /// Flag determining whether the connection has been established.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Connects to  thespecified IP address via TCP protocol.
        /// </summary>
        /// <param name="ipAddress">IP address of the server.</param>
        /// <param name="port">Port number of the server to connect to.</param>
        /// <param name="timeout">Optional timeout of the connection.</param>
        /// <returns></returns>
        Task ConnectAsync(IPAddress ipAddress, int port, TimeSpan? timeout = null);
    }

    /// <summary>
    /// Default implementation of the ITcpClient.
    /// </summary>
    public class TcpClient : ITcpClient
    {
        /// <summary>
        /// Socket instance that is used to connect to the remote server.
        /// </summary>
        private readonly Socket _socket;

        /// <summary>
        /// Gets a flag that indicates whether the current instance is connected to any server.
        /// </summary>
        public bool IsConnected { get; protected set; }

        /// <summary>
        /// Creates a default TCP Client that allows to connect to a remote server via TCP protocol.
        /// </summary>
        public TcpClient()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        /// <summary>
        /// Connects to the specified IP address via TCP protocol.
        /// </summary>
        /// <param name="ipAddress">IP address of a server.</param>
        /// <param name="port">Port number of the hostname to connect to.</param>
        /// <param name="timeout">Optional timeout for the connection.</param>
        /// <returns>Task that perfroms a connection.</returns>
        public async Task ConnectAsync(IPAddress ipAddress, int port, TimeSpan? timeout = null)
        {
            try
            {
                var asyncConnectionResult = _socket.BeginConnect(ipAddress, port, null, null);
                await Task.Factory.StartNew(() => timeout.HasValue
                    ? asyncConnectionResult.AsyncWaitHandle.WaitOne(timeout.Value)
                    : asyncConnectionResult.AsyncWaitHandle.WaitOne());

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
