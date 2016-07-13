using System;
using System.Collections.Generic;
using System.Linq;
using SharedResources;
using System.Collections;
using System.Threading;
using System.Net.Sockets;
using System.Net;


namespace ServerNetworking
{
    /// <summary>A class for handling server and TCP related tasks.</summary>
    public class TcpServer
    {
        /// <summary>The port used by the server.</summary>
        public String port { get; set; }

        /// <summary>The ip address used by the server</summary>
        public String ipAddr { get; set; }

        /// <summary>A hash table associating usernames(key) with sockets(value). </summary>
        private Hashtable usersOnSockets = new Hashtable();

        /// <summary>A thread for listening for clients that wants to connect.</summary>
        private Thread connect_listener = null;

        /// <summary>A client TCP listener</summary>
        private TcpListener client_listener = null;

        /// <summary>List containing all threads, that each listens on a specific socket.</summary>
        private List<Thread> all_active_client_threads = new List<Thread>();

        /// <summary>List for keeping client's data in memory for easier access.</summary>
        private List<User> users_data_list = new List<User>();

        /// <summary>List containing all client sockets, assumed to be connected.</summary>
        private List<Socket> all_active_client_sockets = new List<Socket>();

        /// <summary>A list/inbox of client messages. These can be read externally.</summary>
        private ServerInbox inbox = new ServerInbox();

        /// <summary>A serializer for reading byte arrays into messages, and for writing messages into byte arrays.</summary>
        Serializer server_serializer = new Serializer();

        public TcpServer() { }

        public void StartServer()
        { 
            if (ipAddr == null || port == null)
                SetDefaultServerSettings();

            Console.WriteLine(String.Format("Server is now running! Port: {0}, Ip: {1}", port, ipAddr));

            client_listener = new TcpListener(IPAddress.Parse(ipAddr), Int32.Parse(port));
            client_listener.Start();

            connect_listener = new Thread(ListenForConnections);
            connect_listener.Start();

            Console.WriteLine(String.Format("Server is now listening for connections!"));
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
            Console.WriteLine("No settings specified! Using default settings and local host.");

            ipAddr = TcpMethods.GetIP();
            port = TcpConst.SERVER_PORT.ToString();
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

                Console.WriteLine(String.Format("Client connection occurred."));

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

            Console.WriteLine("Socketlistener added for client!");
        }

        private void ListenOnSocket(object client_socket)
        {
            Console.WriteLine("Awaiting messages on new socket...");

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

                        Console.WriteLine(String.Format("new message received of type: {0}", TcpConst.IntToText(msg.type)));

                        inbox.Push(msg);

                        
                        if (msg.type == TcpConst.JOIN)
                        {
                            JoinRequest_data d1 = (JoinRequest_data)DataTransform.Deserialize(msg.data);
                            BindUserToSocket(s, d1.username);
                        }

                        if (msg.type == TcpConst.LOGIN)
                        {
                            LoginRequest_data d2 = (LoginRequest_data)DataTransform.Deserialize(msg.data);
                            BindUserToSocket(s, d2.username);
                        }
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

        public void SendMessage(String username,ServerMsg msg)
        {
            Socket s = GetSocketFromUser(username);
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

        /// <summary>Add user to server user list, if user does not exist.</summary>
        public void AddToUserList(User u)
        {
            if (!UserExistsInList(u.username))
                users_data_list.Add(u);
        }

        /// <summary>Remove user from server user list, if user exists, and list isn't empty</summary>
        public void RemoveUserFromList(String username)
        {
            if (users_data_list.Count > 0)
            {
                foreach (User u in users_data_list)
                {
                    if (u.name.Equals(username))
                        users_data_list.Remove(u);
                }
            }
        }

        /// <summary>Check if server users list contains a specific user (by username)</summary>
        public bool UserExistsInList(String username)
        {
            if (users_data_list.Count > 0)
            {
                foreach (User u in users_data_list)
                {
                    if (u.name.Equals(username))
                        return true;
                }
            }

            return false;
        }
    }

    internal class ServerInbox
    {
        private List<ClientMsg> list = new List<ClientMsg>();

        internal ClientMsg Pop()
        {
            ClientMsg next = null;

            if (list.Count > 0 && list != null)
            {
                next = list.ElementAt(0);
                list.RemoveAt(0);
            }

            return next;
        }

        internal void Push(ClientMsg msg) { list.Add(msg); }
    }
}
