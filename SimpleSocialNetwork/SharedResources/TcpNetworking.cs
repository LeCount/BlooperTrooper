using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SharedResources
{
    /// <summary>A class meant to distribute TCP related methods used by both client and server.</summary>
    public static class TcpNetworking
    {
        static public string GetIP()
        {
            foreach (IPAddress IPA in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (IPA.AddressFamily == AddressFamily.InterNetwork)
                    return IPA.ToString();
            }

            return "No ip address found";
        }

        /// <summary>Try to connect to server.</summary>
        /// <param name="c">TCP client</param>
        /// <param name="addr">IP address to server</param>
        /// <param name="port">Server port</param>
        /// <returns>A network stream to reveive and send data on.</returns>
       
    }

    /// <summary>A class for handling server and TCP related tasks.</summary>
    public class ServerTCP
    {
        /// <summary>The port used by the server.</summary>
        public String server_port { get; set; }

        /// <summary>The ip address used by the server</summary>
        public String server_ipAddr { get; set; }

        //private SQLiteDB db = new SQLiteDB(TcpConst.DATABASE_FILE);

        /// <summary>A hash table associating usernames(key) with sockets(value). </summary>
        private Hashtable userOnSocket = new Hashtable();

        /// <summary>A thread for listening for clients that wants to connect.</summary>
        private Thread connect_listener = null;

        /// <summary>A client TCP listener</summary>
        private TcpListener client_listener = null;

        /// <summary>List containing all threads, that each listens on a specific socket.</summary>
        private List<Thread> all_active_client_threads = new List<Thread>();


        private List<Socket> all_active_client_sockets = new List<Socket>();
        private ServerInbox inbox = new ServerInbox();
        Serializer server_serializer = new Serializer();

        public ServerTCP() {}

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
            server_ipAddr = TcpNetworking.GetIP();
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
            if(!userOnSocket.ContainsKey(username))
                userOnSocket.Add(username, s);
        }

        public void UnbindUserToSocket(String username)
        {
            if (userOnSocket.ContainsKey(username))
                userOnSocket.Remove(username);
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
            object value = userOnSocket[user];
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

        internal void Push(ClientMsg msg){list.Add(msg);}
    }

    public class ClientTCP
    {
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

    public static class Validation
    {
        /// <summary>Checks if mail address format is valid. 
        /// Conditions{
        /// minlength: 5 
        /// maxlength: 20 
        /// uppercase letter: > 0 
        /// lowercase letter: > 0
        /// digits: > 2}</summary>
        /// <param name="suggested_password"></param>
        /// <returns></returns>
        static public bool PasswordFormatIsValid(string suggested_password)
        {
            int MIN_LENGTH = 5;
            int MAX_LENGTH = 20;

            if (suggested_password == null)
                return false;

            bool meetsLengthRequirements = suggested_password.Length >= MIN_LENGTH && suggested_password.Length <= MAX_LENGTH;

            if (!meetsLengthRequirements)
                return false;

            bool hasUpperCaseLetter = false;
            bool hasLowerCaseLetter = false;
            int digitCounter = 0;

            foreach (char c in suggested_password)
            {
                if (char.IsUpper(c)) hasUpperCaseLetter = true;
                else if (char.IsLower(c)) hasLowerCaseLetter = true;
                else if (char.IsDigit(c)) digitCounter++;
            }

            if (hasUpperCaseLetter && hasLowerCaseLetter && digitCounter == 3)
                return true;
            else
                return false;
        }

        /// <summary>Checks if mail address format is valid.</summary>
        /// <param name="suggested_password"></param>
        /// <returns></returns>
        static public bool EmailFormatIsValid(string suggested_email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(suggested_email);
                return addr.Address == suggested_email;
            }
            catch { return false; }
        }

        static public object ContstructMessageData()
        {
            throw new NotImplementedException();
        }

        static public List<Object> ParseMessageData()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>A class meant to distribute TCP related constants used by both client and server.</summary>
    public static class TcpConst
    {
        //Message identifiers
        public const int CONNECT = 0;
        public const int JOIN = 1;
        public const int LOGIN = 2;
        public const int LOGOUT = 3;
        public const int GET_USERS = 4;
        public const int ADD_FRIEND = 5;
        public const int GET_FRIEND_STATUS = 6;
        public const int GET_CLIENT_DATA = 7;
        public const int SEND_MESSAGE = 8;
        public const int GET_WALL = 9;
        public const int PING = 10;


        //message types:
        public const int REQUEST = 11;
        public const int REPLY = 12;

        public const int INVALID = -1;

        public const int SERVER_PORT = 8001;
        public const string DATABASE_FILE = "serverDB.db";
        public const int BUFFER_SIZE = 1024 * 4;

        /// <summary>Convert an integer constant to its corresponding text.</summary>
        public static string IntToText(int i)
        {
            switch (i)
            {
                case 0: return "CONNECT";
                case 1: return "JOIN";
                case 2: return "LOGIN";
                case 3: return "LOGOUT";
                case 4: return "GET USERS";
                case 5: return "ADD FRIEND";
                case 6: return "GET FRIEND STATUS";
                case 7: return "GET CLIENT DATA";
                case 8: return "SEND MESSAGE";
                case 9: return "GET_WALL";
                case 10: return "PING";
                case 11: return "REQUEST";
                case 12: return "REPLY";
                default: return "INVALID";
            }
        }

    }
}
