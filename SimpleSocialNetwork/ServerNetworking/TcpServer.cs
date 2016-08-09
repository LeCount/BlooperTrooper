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
        private List<User> cachedUsers = new List<User>();

        /// <summary>List containing all client sockets, assumed to be connected.</summary>
        private List<Socket> all_active_client_sockets = new List<Socket>();

        /// <summary>A list/inbox of client messages. These can be read externally.</summary>
        public ServerInbox inbox = new ServerInbox();

        /// <summary>A serializer for reading byte arrays into messages, and for writing messages into byte arrays.</summary>
        Serializer server_serializer = new Serializer();

        public TcpServer() { }

        public void StartServer()
        { 
            if (ipAddr == null || port == null)
                SetDefaultServerSettings();

            Console.WriteLine(String.Format("Server is now running. Port: {0}, Ip: {1}", port, ipAddr));

            client_listener = new TcpListener(IPAddress.Parse(ipAddr), Int32.Parse(port));
            client_listener.Start();

            connect_listener = new Thread(ListenForConnections);
            connect_listener.Start();

            Console.WriteLine("Server is now listening for connections. \n");
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

                Console.WriteLine(String.Format("Client connection occurred."));

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
            List<ClientMsg> request_list = new List<ClientMsg>();

            Socket s = (Socket)client_socket;
            int num_of_bytes_read = 0;
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
                        Console.WriteLine("A client disconnected.");

                        if(usersOnSockets.ContainsValue(s))
                            usersOnSockets.Remove(s);

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

                        Console.WriteLine(String.Format("Message received: {0}.", TcpConst.IntToText(msg.type)));

                        inbox.Push(msg);

                        if (msg.type == TcpConst.PING)
                        {
                            Ping_data received_data = new Ping_data();
                            received_data = (Ping_data)msg.data;

                            ServerMsg msg_to_send = new ServerMsg();
                            msg_to_send.type = TcpConst.PING;

                            Ping_data data_to_send = new Ping_data();
                            data_to_send.message_code = TcpMessageCode.REPLY;
                            msg_to_send.data = (Object)data_to_send;
                            SendMessageToSocket(msg_to_send, s);
                        }

                        if (msg.type == TcpConst.JOIN)
                        {
                            JoinRequest_data received_data = (JoinRequest_data)msg.data;
                            BindUserToSocket(s, received_data.username);
                        }

                        if (msg.type == TcpConst.LOGIN)
                        {
                            LoginRequest_data received_data = (LoginRequest_data)msg.data;
                            BindUserToSocket(s, received_data.username);
                        }

                        if (msg.type == TcpConst.LOGOUT)
                        {
                            if (all_active_client_sockets.Contains(s))
                                all_active_client_sockets.Remove(s);

                            string username = GetUserFromSocket(s);
                            if (username == null)
                            { 
                                s.Close();
                                return;
                            }

                            if (usersOnSockets.ContainsValue(s))
                                usersOnSockets.Remove(username);

                            s.Close();
                            return;
                        }
                    }

                    num_of_bytes_read = 0;
                }
            }
        }

        public string GetUserFromSocket(Socket s)
        {
            foreach (string user in usersOnSockets.Keys)
            {
                if(usersOnSockets[user] == s)
                    return user;
            }

            return null;
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
            //user is not online, or something went very wrong 0o????
            if(s == null)
            {
                //this should not be done HERE, but something that should happen, is that the msg should be saved in db as pending friend request
                return;
            }

            byte[] byte_buffer = new byte[TcpConst.BUFFER_SIZE];
            byte_buffer = server_serializer.SerializeServerMsg(msg);

            try
            {
                s.Send(byte_buffer);
                Console.WriteLine(String.Format("Message sent:     {0}.", TcpConst.IntToText(msg.type)));
            }
            catch(ObjectDisposedException)
            {
                Console.WriteLine(String.Format("Message of type '{0}' could not be sent. Target socket is 'null'.", TcpConst.IntToText(msg.type)));
            }
        }

        public ClientMsg GetNextRequest()
        {
            return inbox.Pop();
        }

        private Socket GetSocketFromUser(string user)
        {
            object value = usersOnSockets[user];

            if (value == null)
            {
                Console.WriteLine(String.Format("Message not delivered: User {0} is not online.", user));
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
        private List<ClientMsg> list = new List<ClientMsg>();

        public bool Empty()
        {
            if (list.Count < 1)
                return true;
            else
                return false;
        }

        public ClientMsg Pop()
        {
            ClientMsg next = null;
            next = list.First();
            list.RemoveAt(list.IndexOf(list.First()));
            return next;
        }

        public void Push(ClientMsg msg)
        {
            list.Add(msg);
        }
    }
}
