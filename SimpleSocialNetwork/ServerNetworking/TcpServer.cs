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
    /// <summary>A class responsible for handling TCP related tasks on the server.</summary>
    public class TcpServer
    {
        /// <summary>The port used by the server.</summary>
        private String port { get; set; }

        /// <summary>The ip address used by the server</summary>
        private String ipAddr { get; set; }

        /// <summary>A hash table associating usernames(key) with sockets(value). </summary>
        private Hashtable usersOnSockets = new Hashtable();

        /// <summary>A thread for listening for clients that wants to connect.</summary>
        private Thread connect_listener = null;

        /// <summary>A client TCP listener</summary>
        private TcpListener client_listener = null;

        /// <summary>List containing all threads, that each listens on a specific socket.</summary>
        private List<Thread> all_active_client_threads = new List<Thread>();

        /// <summary>List for keeping client's data in memory for easier access.</summary>
        private List<User> cachedUsers = null;

        /// <summary>List containing all client sockets, assumed to be connected.</summary>
        private List<Socket> all_active_client_sockets = new List<Socket>();

        /// <summary>A list/inbox of client messages. These can be read externally.</summary>
        public ServerInbox inbox = new ServerInbox();

        /// <summary>A serializer for reading byte arrays into messages, and for writing messages into byte arrays.</summary>
        Serializer server_serializer = new Serializer();

        private object socket_list_lock = new object();


        public TcpServer() { }

        public TcpServer(string ip_to_use, string port_to_use)
        {
            ipAddr = ip_to_use;
            port = port_to_use;
        }

        public void StartServer()
        { 
            if (ipAddr == null || port == null)
                SetDefaultServerSettings();

            Console.WriteLine(String.Format("Server is now running. Port: {0}, Ip: {1}", port, ipAddr));

            client_listener = new TcpListener(IPAddress.Parse(ipAddr), Int32.Parse(port));
            client_listener.Start();

            connect_listener = new Thread(ListenForConnections);
            connect_listener.Start();

            Console.WriteLine("Server is now listening for connections. \n\n");
        }

        public void StopServer()
        {
            try {client_listener.Stop();}
            catch (Exception) {}
            

            if (connect_listener.IsAlive)
                connect_listener.Abort();


            foreach (Socket s in all_active_client_sockets)
            { 
                try { s.Close();}
                catch (Exception) {}
                all_active_client_sockets.Remove(s);
            }

            foreach (Thread t in all_active_client_threads)
            {
                if (t.IsAlive)
                    t.Abort();
                all_active_client_threads.Remove(t);
            }
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

                Console.WriteLine(String.Format("[Server]Client connection established."));

                s = client_listener.AcceptSocket();
                all_active_client_sockets.Add(s);
                AddSocketListener(s);
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
            string user_on_this_socket = null;
            int num_of_bytes_read = 0;

            List<ClientMsg> request_list = new List<ClientMsg>();
            Socket s = (Socket)client_socket;
            byte[] receive_buffer = new byte[TcpConst.BUFFER_SIZE];

            while (true)
            {
                // Detect if client disconnected
                if (!s.Poll(0, SelectMode.SelectRead))
                {
                    byte[] buff = new byte[1];
                    if (s.Receive(buff, SocketFlags.Peek) == 0)
                    {
                        // Client disconnected
                        Console.WriteLine("User disconnected.");

                        if (usersOnSockets.ContainsValue(s))
                            RemoveSocket(s);
                            

                        if(all_active_client_sockets.Contains(s))
                            all_active_client_sockets.Remove(s);

                        s.Close();
                        return;//close;
                    }
                }
                else
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

                        if (msg.type == TcpConst.JOIN)
                        {
                            JoinRequest_data received_data = (JoinRequest_data)msg.data;
                            BindUserToSocket(s, received_data.username);
                            user_on_this_socket = GetUserFromSocket(s);
                        }

                        if (msg.type == TcpConst.LOGIN)
                        {
                            LoginRequest_data received_data = (LoginRequest_data)msg.data;
                            BindUserToSocket(s, received_data.username);
                            user_on_this_socket = GetUserFromSocket(s);
                        }

                        if (msg.type == TcpConst.LOGOUT)
                        {
                            if (all_active_client_sockets.Contains(s))
                                all_active_client_sockets.Remove(s);

                            if (user_on_this_socket == null)
                            {
                                Console.WriteLine(String.Format("[ERROR]:No name assosiated with this socket. Closing corresponding socket and stoping listener."));
                                s.Close();
                                return;
                            }

                            if (usersOnSockets.ContainsValue(s))
                                usersOnSockets.Remove(user_on_this_socket);

                            Console.WriteLine(String.Format("[{0}]:Logged out.", user_on_this_socket));
                            return;
                        }

                        if(user_on_this_socket == null || user_on_this_socket == "")
                            Console.WriteLine(String.Format("[Server]:Received {0}-message from unknown user", TcpConst.IntToText(msg.type)));
                        else
                            Console.WriteLine(String.Format("[Server]:Received {0}-message from {1}", TcpConst.IntToText(msg.type), user_on_this_socket));

                    }

                    num_of_bytes_read = 0;
                }
            }
        }

        private void RemoveSocket(Socket s)
        {
            lock (socket_list_lock)
            {
                try{usersOnSockets.Remove(s);}
                catch (Exception) {/**List empty or socket does not exist**/ }
            }
        }

        public bool userIsBoundToSocket(string user)
        {
            if(usersOnSockets.ContainsKey(user))
            {
                //Console.WriteLine(String.Format("User IS bound to socket."));
                return true;
            }

            else
            {
                //Console.WriteLine(String.Format("User NOT bound to socket."));
                return false;
            }

        }

        public string GetUserFromSocket(Socket s)
        {
            lock (socket_list_lock)
            {
                foreach (string user in usersOnSockets.Keys)
                {
                    if (usersOnSockets[user] == s)
                        return user;
                }
            }

            return null;
        }

        public void BindUserToSocket(Socket s, String username)
        {
            if (!usersOnSockets.ContainsKey(username))
            {
                lock (socket_list_lock)
                    usersOnSockets.Add(username, s);
            }
        }

        public void UnbindUserFromSocket(String username)
        {
            
            if (usersOnSockets.ContainsKey(username))
            {
                lock (socket_list_lock)
                    usersOnSockets.Remove(username);
            }
        }

        public bool SendMessage(object msg_data, int msg_type, string destination_user)
        {
            ServerMsg msg = new ServerMsg();
            msg.type = msg_type;
            msg.data = msg_data;

            Socket s = GetSocketFromUser(destination_user);
            return SendMessageToSocket(msg, s);
        }

        /// <summary>Send message to specific socket (client).</summary>
        /// <param name="msg">Message to send.</param>
        /// <param name="s">Socket to send msg to.</param>
        private bool SendMessageToSocket(ServerMsg msg, Socket s)
        {
            byte[] byte_buffer = new byte[TcpConst.BUFFER_SIZE];
            byte_buffer = server_serializer.SerializeServerMsg(msg);

            if (byte_buffer == null)
            {
                Console.WriteLine(String.Format("[Error]:Server '{0}'-message could not be serialized.", TcpConst.IntToText(msg.type)));
                return false;
            }

            try
            {
                s.Send(byte_buffer);
                Console.WriteLine(String.Format("[Server]:Sent {0}-message.", TcpConst.IntToText(msg.type)));
                return true;
            }
            catch(Exception)
            {
               Console.WriteLine(String.Format("[Error]:Server '{0}'-message could not be sent. Target socket is 'dispoesed'.", TcpConst.IntToText(msg.type)));
                return false;
            }
        }

        public ClientMsg GetNextRequest()
        {
            return inbox.Pop();
        }

        public Socket GetSocketFromUser(string user)
        {
            object value = usersOnSockets[user];

            if (value == null)
            {
                Console.WriteLine(String.Format("[Error]:Target user: {0}, is not bound to a socket, or user is not online. Socket = null.", user));
            }

            return (Socket)value;
        }

        /// <summary>Add user to server user list, if user does not exist.</summary>
        public void CacheUser(User u)
        {
            if (!UserIsCached(u.username))
                cachedUsers.Add(u);
        }

        /// <summary>Remove user from server user list, if user exists, and list isn't empty</summary>
        public void RemoveCachedUser(String username)
        {
            if (cachedUsers.Count > 0)
            {
                foreach (User u in cachedUsers)
                {
                    if (u.name.Equals(username))
                        cachedUsers.Remove(u);
                }
            }
        }

        /// <summary>Check if server users list contains a specific user (by username)</summary>
        public bool UserIsCached(String username)
        {
            if (cachedUsers.Count > 0)
            {
                foreach (User u in cachedUsers)
                {
                    if (u.name.Equals(username))
                        return true;
                }
            }

            return false;
        }
    }

    public class ServerInbox
    {
        private object inbox_lock = new object();
        private List<ClientMsg> list = new List<ClientMsg>();

        public bool Empty()
        {
            lock (((IList)list).SyncRoot)
            {
                if (list.Count < 1)
                    return true;
                else
                    return false;
            }

        }

        public ClientMsg Pop()
        {
            lock (inbox_lock)
            {
                ClientMsg next = null;
                next = list.First();
                list.RemoveAt(list.IndexOf(list.First()));
                return next;
            }
        }

        public void Push(ClientMsg msg)
        {
            lock (inbox_lock) {list.Add(msg);}
        }
    }
}
