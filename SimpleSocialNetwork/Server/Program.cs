namespace ServerProgram
{
    using System.Data.SQLite;
    using SharedResources;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public class Program
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

    public class Server
    {
        private Thread message_handler = null;
        Serializer server_serializer = new Serializer();
        ServerTCP networking = new ServerTCP();

        public Server()
        {
            Init(null, null);
        }

        public Server(string ipaddr, string port)
        {
            Init(ipaddr, port);
        }

        private void Init(string ipAddr, string port)
        {
            networking.server_ipAddr = ipAddr;
            networking.server_port = port;
            networking.StartServer();
            StartHandlingRequests();
        }

        private void StartHandlingRequests()
        {
            message_handler = new Thread(executeRequests);
            message_handler.Start();
        }

        private void executeRequests()
        {
            ClientMsg next_request = null;

            while (true)
            {
                next_request = networking.GetNextRequest();

                if (next_request == null)
                    Thread.Sleep(10);
                else
                    HandleClientRequest(next_request);
            }
        }

        private void HandleClientRequest(ClientMsg msg)
        {
            switch(msg.type)
            {
                case TcpConst.JOIN:
                    HandleJoinRequest(msg);
                    break;
                case TcpConst.LOGIN:
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
                case TcpConst.SEND_MESSAGE:
                    break;
                case TcpConst.GET_WALL:
                    break;
                case TcpConst.PING:
                    break;
                case TcpConst.INVALID:
                    break;
            }
        }

        private void HandleJoinRequest(ClientMsg msg)
        {
            if (ValidateJoinRequest())
                return;
            else
                return;

        }

        private bool ValidateJoinRequest()
        {
            throw new NotImplementedException();
        }
    }
}
