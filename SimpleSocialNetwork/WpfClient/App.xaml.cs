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
        private static Serializer s = new Serializer();

        /// <summary>Thread responsible for connecting to the server.</summary>
        private Thread server_connect = null;

        /// <summary>Thread responsible for reading incoming messages on this client's socket.</summary>
        private Thread message_read = null;

        /// <summary>A stream providing read and write operations, on a given medium.</summary>
        private static Stream client_stream = null;

        /// <summary>A medium for providing client connections for TCP network services.</summary>
        private TcpClient tcp_client = new TcpClient();

        /// <summary>A byte-array based buffer, where incoming messages are stored.</summary>
        private byte[] receive_buffer = new byte[TcpConst.BUFFER_SIZE];

        private bool connected = false;

        private string serverIPAddress = TcpMethods.GetIP();

        ///<summary>
        /// Login to server
        ///</summary>
        public static bool LoginToServer(string username, string password)
        {            
            User loginData = new User();

            loginData.password = password;
            loginData.username = username;

            ClientMsg msg = new ClientMsg();
            msg.type = TcpConst.LOGIN;
            msg.data = DataTransform.Serialize(loginData);

            Client_send(msg);
                   
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

        public void ClientStop()
        {
            server_connect.Abort();
            message_read.Abort();
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

                    HandleServerReplies(msg);

                    numOfBytesRead = 0;
                }
            }
        }

        /// <summary>Send message from client to server over TCP.</summary>
        /// <param name="msg">Message to be sent over TCP.</param>
        public static void Client_send(ClientMsg msg)
        {
            MessageBox.Show("Sending Message");
            byte[] byteBuffer = s.SerializeClientMsg(msg);
            try { client_stream.Write(byteBuffer, 0, byteBuffer.Length); }
            catch (Exception) { }
        }

        /// <summary>Depending on the reply that was received, handle it accordingly. </summary>
        /// <param name="msg">Received message.</param>
        private void HandleServerReplies(ServerMsg msg)
        {
            switch (msg.type)
            {
                case TcpConst.JOIN:
                    MessageBox.Show("Join response recieved");
                    break;
                case TcpConst.LOGIN:
                    MessageBox.Show("Login response Recieved");
                    if (TcpMessageCode.ACCEPTED == (int)msg.data)
                    {
                        MessageBox.Show("Logged in!");
                    }
                    break;
                case TcpConst.LOGOUT:

                    break;
                case TcpConst.GET_USERS:

                    break;
                case TcpConst.ADD_FRIEND:

                    break;
                case TcpConst.GET_FRIEND_STATUS:

                    break;
                case TcpConst.GET_CLIENT_DATA:

                    break;
                case TcpConst.CHAT:

                    break;
                default: break;
            }
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            server_connect = new Thread(ConnectToServer);
            server_connect.Start();

            message_read = new Thread(ClientRead);
            message_read.Start();

            // Show login window
            LoginWindow login = new LoginWindow();
            login.Show();
        }
    }

}
