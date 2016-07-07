namespace ServerProgram
{
    using SharedResources;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Count() < 2)
                new Server();
            else
            {
                String server_ipAddr = args.ElementAt(0);
                String server_port = args.ElementAt(1);

                new Server(server_ipAddr, server_port);
            }
        }
    }

    internal class Server
    {
        private int server_port;
        private String server_ipAddr;

        private Thread connect_listener = null;
        private TcpListener client_listener = null;
        private List<Thread> all_active_client_threads = new List<Thread>();
        private List<Socket> all_active_client_sockets = new List<Socket>();
        Serializer server_serializer = new Serializer();

        public Server(){ init();}

        public Server(String ip_addr, String port)
        {
            try
            {
                server_ipAddr = ip_addr;
                int.TryParse(port, out server_port);
            }
            catch(FormatException)
            {
                Console.WriteLine("Invalid server configuration.");
                init();
            }

            Console.WriteLine(String.Format("Using custom server  settings: IP: {0} PORT: {1}", server_ipAddr, TcpConst.SERVER_PORT));
        }

        private void init()
        {
            Console.WriteLine(String.Format("Using default server settings: IP: {0} PORT: {1}", TcpNetworking.GetIP(), TcpConst.SERVER_PORT));
            StartServer();
        }


        private void StartServer()
        {
            client_listener = new TcpListener(IPAddress.Parse(TcpNetworking.GetIP()), TcpConst.SERVER_PORT);
            client_listener.Start();

            connect_listener = new Thread(ListenForConnections);
            connect_listener.Start();
        }

        private void StopServer()
        {
            throw new NotImplementedException();
        }

        private void ListenForConnections()
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            while (true)
            {
                while (!client_listener.Pending()) { }
                s = client_listener.AcceptSocket();
                all_active_client_sockets.Add(s);
                AddSocketListener(s);

                ServerMsg reply = new ServerMsg();
                reply.data = "Hi from mr Boaty Mc Boatface!";
                reply.type = TcpConst.CONNECT;

                SendMessageToSocket(reply, s);
            }
        }

        private void AddSocketListener(Socket s)
        {
            Thread socket_listener = new Thread(ListenOnSocket);
            socket_listener.Start(s);

            all_active_client_threads.Add(socket_listener);
        }

        private void ListenOnSocket(object client_socket)
        {
            List<ClientMsg> request_list = new List<ClientMsg>();

            Socket s = (Socket)client_socket;
            int num_of_bytes_read = 0;
            byte[] receive_buffer = new byte[TcpConst.BUFFER_SIZE];

            while (true)
            {
                if (s.Connected)
                {
                    try
                    {
                        num_of_bytes_read = s.Receive(receive_buffer);
                    }
                    catch (Exception) { }

                    if (num_of_bytes_read > 0)
                    {
                        ClientMsg msg = server_serializer.DeserializeClientMsg(receive_buffer);
                       
                        request_list.Add(msg);
                    }

                    if (request_list != null && request_list.Count >= 1)
                    {
                        HandleMessage(request_list.ElementAt(0), s);
                        request_list.RemoveAt(0);
                    }

                    num_of_bytes_read = 0;
                }
                else
                    s.Close();
            }
        }

        /// <summary>Send message to specific socket (client).</summary>
        /// <param name="msg">Message to send.</param>
        /// <param name="s">Socket to send msg to.</param>
        public void SendMessageToSocket(ServerMsg msg, Socket s)
        {
            byte[] byte_buffer = server_serializer.SerializeServerMsg(msg);
            s.Send(byte_buffer);
        }

        private void HandleMessage(ClientMsg msg, Socket s)
        {
            switch(msg.type)
            {
                case TcpConst.JOIN:
                    break;
                case TcpConst.LOGIN:
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
                case TcpConst.INVALID:
                    break;
            }
        }
    }
}
