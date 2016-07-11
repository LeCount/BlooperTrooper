using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace ClientNetworking
{
    public class ClientTCP
    {
        /// <summary>Try to connect to server.</summary>
        /// <param name="c">TCP client</param>
        /// <param name="addr">IP address to server</param>
        /// <param name="port">Server port</param>
        /// <returns>A network stream to reveive and send data on.</returns>
        public Stream ConnectToServer(TcpClient c, string server_addr, int port)
        {
            bool connected = false;
            Stream client_stream = null;

            while (!connected)
            {
                try
                {
                    c.Connect(IPAddress.Parse(server_addr), port);
                    client_stream = c.GetStream();
                    connected = true;
                }

                catch (Exception) { }
            }

            return client_stream;
        }
    }
}
