using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Threading;

using SharedResources;
using System.Net;
using System.Net.Sockets;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Serializer s = new Serializer();

        /// <summary>Thread responsible for connecting to the server.</summary>
        private Thread server_connect = null;

        /// <summary>Thread responsible for reading incoming messages on this client's socket.</summary>
        private Thread message_read = null;

        /// <summary>A stream providing read and write operations, on a given medium.</summary>
        private Stream client_stream = null;

        /// <summary>A medium for providing client connections for TCP network services.</summary>
        private TcpClient tcp_client = new TcpClient();

        /// <summary>A byte-array based buffer, where incoming messages are stored.</summary>
        private byte[] receive_buffer = new byte[TcpConst.BUFFER_SIZE];

        private bool connected = false;

        private string serverIPAddress = "192.168.12.170";

        ///<summary>
        /// Login to server
        ///</summary>
        public static bool LoginToServer(string username, string password)
        {
            MessageBox.Show(username + password);
            
            return true;
        }

        /// <summary>Try until success to connect to the server.</summary>
        private void ConnectToServer()
        {
            while (!connected)
            {
                try
                {
                    tcp_client.Connect(IPAddress.Parse(serverIPAddress), TcpConst.SERVER_PORT);
                    client_stream = tcp_client.GetStream();
                    connected = true;
                }

                catch (Exception)
                {
                    MessageBox.Show("Server not available.");
                }
            }
        }
        /// <summary>Read messages from the server</summary>
        public void ClientRead()
        {
            int numOfBytesRead = 0;

            while (true)
            {
                try
                {
                    numOfBytesRead = client_stream.Read(receive_buffer, 0, TcpConst.BUFFER_SIZE);
                }
                catch (Exception) { }

                if (numOfBytesRead > 0)
                {
                    ServerMsg msg = s.DeserializeServerMsg(receive_buffer);

                    if (msg.type == TcpConst.REPLY)
                        HandleServerReplies(msg);

                    numOfBytesRead = 0;
                }
            }
        }

        /// <summary>Depending on the reply that was received, handle it accordingly. </summary>
        /// <param name="msg">Received message.</param>
        private void HandleServerReplies(ServerMsg msg)
        {
            switch (msg.id)
            {
                case TcpConst.JOIN:
                    MessageBox.Show("Join response recieved");
                    break;
                case TcpConst.LOGIN:
                    MessageBox.Show("Login response Recieved");
                    break;
                case TcpConst.LOGOUT:

                    break;
                case TcpConst.GET_USERS:

                    break;
                case TcpConst.ADD_FRIEND:

                    break;
                case TcpConst.GET_FRIENDS_STATUS:

                    break;
                case TcpConst.GET_CLIENT_DATA:

                    break;
                case TcpConst.SEND_MESSAGE:

                    break;
                default: break;
            }
        }


    }





}
