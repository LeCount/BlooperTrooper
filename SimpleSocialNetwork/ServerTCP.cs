using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SharedResources;

namespace ServerNetworking
{
    /// <summary>A class for handling server and TCP related tasks.</summary>
    public class ServerTCP
    {
        /// <summary>The port used by the server.</summary>
        public String server_port { get; set; }

        /// <summary>The ip address used by the server</summary>
        public String server_ipAddr { get; set; }

        /// <summary>A hash table associating usernames(key) with sockets(value). </summary>
        private Hashtable usersOnSockets = new Hashtable();

        /// <summary>A thread for listening for clients that wants to connect.</summary>
        private Thread connect_listener = null;

        /// <summary>A client TCP listener</summary>
        private TcpListener client_listener = null;

        /// <summary>List containing all threads, that each listens on a specific socket.</summary>
        private List<Thread> all_active_client_threads = new List<Thread>();

        /// <summary>List containing all client sockets, assumed to be connected.</summary>
        private List<Socket> all_active_client_sockets = new List<Socket>();

        /// <summary>A list/inbox of client messages. These can be read externally.</summary>
        private ServerInbox inbox = new ServerInbox();

        /// <summary>A serializer for reading byte arrays into messages, and for writing messages into byte arrays.</summary>
        Serializer server_serializer = new Serializer();

        public ServerTCP() { }

        public void StartServer()
        {
            if (server_ipAddr == null || server_port == null)
                SetDefaultServerSettings();

            client_listener = new TcpListener(IPAddress.Parse(server_ipAddr), Int32.Parse(server_port));
            client_listener.Start();

            connect_listener = new Thread(ListenForConnections);
            connect_listener.Start();
        }

        public void StopServer()
        {
            if (connect_listener.IsAlive)
                connect_listener.Abort();

            foreach (Thread t in all_active_client_threads)
                t.Abort();

            client_listener.Stop();
        }

        public void SetDefaultServerSettings()
        {
            server_ipAddr = TcpMethod.GetIP();
            server_port = TcpConst.SERVER_PORT.ToString();
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
                        inbox.Push(msg);

                        if (msg.type == TcpConst.JOIN || msg.type == TcpConst.LOGIN)
                            BindUserToSocket(s, msg.user);
                    }

                    num_of_bytes_read = 0;
                }
                else
                    s.Close();
            }
        }

        public void BindUserToSocket(Socket s, String username)
        {
            if (!usersOnSockets.ContainsKey(username))
                usersOnSockets.Add(username, s);
        }

        public void UnbindUserToSocket(String username)
        {
            if (usersOnSockets.ContainsKey(username))
                usersOnSockets.Remove(username);
        }

        public void SendMessage(ServerMsg msg)
        {
            Socket s = GetSocketFromUser(msg.user);
            SendMessageToSocket(msg, s);
        }

        /// <summary>Send message to specific socket (client).</summary>
        /// <param name="msg">Message to send.</param>
        /// <param name="s">Socket to send msg to.</param>
        private void SendMessageToSocket(ServerMsg msg, Socket s)
        {
            byte[] byte_buffer = server_serializer.SerializeServerMsg(msg);
            s.Send(byte_buffer);
        }

        public ClientMsg GetNextRequest()
        {
            return inbox.Pop();
        }

        private Socket GetSocketFromUser(string user)
        {
            object value = usersOnSockets[user];
            return (Socket)value;
        }
    }

    internal class ServerInbox
    {
        private List<ClientMsg> list = new List<ClientMsg>();

        internal ClientMsg Pop()
        {
            ClientMsg next = null;

            if (list.Count > 0 && list != null)
                next = list.ElementAt(0);

            return next;
        }

        internal void Push(ClientMsg msg) { list.Add(msg); }
    }
}
