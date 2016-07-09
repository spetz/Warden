using System.Net;
using System.Net.NetworkInformation;

namespace Warden.Watchers.Server
{
    /// <summary>
    /// Details of the resolved connection to the specified hostname and port.
    /// </summary>
    public class ConnectionInfo
    {
        /// <summary>
        /// Resolved hostname.
        /// </summary>
        public string Hostname { get; }

        /// <summary>
        /// IP address of the resolved hostname.
        /// </summary>
        public IPAddress IpAddress { get; }

        /// <summary>
        /// Optional port number that was checked.
        /// </summary>
        public int Port { get; }

        /// <summary>
        ///  Flag determining whether the port number (if specified) is being opened.
        /// </summary>
        public bool PortOpened { get; }

        /// <summary>
        /// Status of the ping request.
        /// </summary>
        public IPStatus PingStatus { get; }

        /// <summary>
        /// Status message of the ping request.
        /// </summary>
        public string PingStatusMessage { get; }

        protected ConnectionInfo(string hostname, IPAddress ipAddress, 
            int port, bool portOpened, 
            IPStatus pingStatus, string pingStatusMessage)
        {
            Hostname = hostname;
            Port = port;
            PortOpened = portOpened;
            PingStatus = pingStatus;
            PingStatusMessage = pingStatusMessage;
            IpAddress = ipAddress;
        }

        /// <summary>
        /// Factory method for creating connection info details.
        /// </summary>
        /// <param name="hostname">Resolved hostname.</param>
        /// <param name="port">IP address of the resolved hostname.</param>
        /// <param name="ipAddress">Optional port number that was checked.</param>
        /// <param name="portOpened">Flag determining whether the port number (if specified) is being opened.</param>
        /// <param name="pingStatus">Status of the ping request.</param>
        /// <param name="pingStatusMessage">Status message of the ping request.</param>
        /// <returns>Instance of ConnectionInfo.</returns>
        public static ConnectionInfo Create(string hostname, IPAddress ipAddress, 
            int port, bool portOpened, IPStatus pingStatus, string pingStatusMessage)
            => new ConnectionInfo(hostname, ipAddress, port, portOpened, pingStatus, pingStatusMessage);
    }
}