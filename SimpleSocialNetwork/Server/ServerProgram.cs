namespace Program
{
    using System;
    using System.Linq;
    using System.Threading;
    using ServerNetworking;
    using SharedResources;


    public static class Entrypoint
    {
        static void Main(string[] args)
        {
            if (args.Count() < 2)
                new ServerApp();
            else
            {
                String server_ipAddr = args.ElementAt(0);
                String server_port = args.ElementAt(1);

                new ServerApp(server_ipAddr, server_port);
            }
        }
    }

    public interface I_TcpServer
    {
        void Start();

        void Stop();

        /// <summary>A method for collecting the next client request to execute, from the server's request inbox.
        /// Perferably, this method will be executed inside it's own thread.</summary>
        /// <returns>The next client request to execute.</returns>
        ClientMsg GetNextClientRequest();

        /// <summary>A method where actions are taken accordingl,y to the specific type of the client requests.</summary>
        /// <param name="msg">A client request.</param>
        void HandleNextClientRequest(ClientMsg msg);

    }

    public class ServerApp
    {
        private SQLiteDB sqlite_database = new SQLiteDB(TcpConst.DATABASE_FILE);
        TcpServer tcp_server = null;
        private Thread get_next_request = null;

        
        public ServerApp()
        {
            Console.WriteLine("HERP!");
            Init(null, null);
        }

        public ServerApp(string ipaddr, string port)
        {
            Init(ipaddr, port);
        }

        private void Init(string ip, string port)
        {
            tcp_server = new TcpServer();
            tcp_server.ipAddr = ip;
            tcp_server.port = port;
            tcp_server.StartServer();

            get_next_request = new Thread(executeRequests);
            get_next_request.Start();
        }

        private void executeRequests()
        {
            ClientMsg next_request = null;

            while (true)
            {
                next_request = tcp_server.GetNextRequest();

                if (next_request == null)
                    Thread.Sleep(100);
                else
                    HandleClientRequest(next_request);
            }
        }

        private void HandleClientRequest(ClientMsg msg)
        {
            switch (msg.type)
            {
                case TcpConst.JOIN:

                    HandleJoinRequest(msg.data);

                    break;
                case TcpConst.LOGIN:

                    HandleLoginRequest(msg.data);

                    //Add the user to the userlist on server
                    // networking.AddToUserList(GetUserFromDB(user.username));

                    break;
                case TcpConst.LOGOUT:

                    //Remove the user from the userlist on server
                    //networking.RemoveUserFromList(user.username);

                    break;
                case TcpConst.GET_USERS:
                    break;
                case TcpConst.ADD_FRIEND:
                    break;
                case TcpConst.GET_FRIEND_STATUS:
                    break;
                case TcpConst.UPDATE:

                    //Update the user, that was updated, in the userlist on server
                    //networking.RemoveUserFromList(user.username);
                    //networking.AddToUserList(GetUserFromDB(user.username));

                    break;
                case TcpConst.GET_CLIENT_DATA:

                    //Add requested user to userlist on server
                    //networking.AddToUserList(GetUserFromDB(user.username));

                    break;
                case TcpConst.CHAT:
                    break;
                case TcpConst.GET_WALL:
                    break;
                case TcpConst.INVALID:
                    break;
            }
        }

        /// <summary>Gets the data on a specific user from local db, and puts it parameterized into a user-class object.</summary>
        /// <param name="username">The name of the user.</param>
        private User GetUserFromDB(String username)
        {
            User user = new User();

            user.name = username;
            user.mail = sqlite_database.GetMail(username);
            user.name = sqlite_database.GetName(username);
            user.surname = sqlite_database.GetSurname(username);
            user.about_user = sqlite_database.GetAbout(username);
            user.interests = sqlite_database.GetInterest(username);
            user.friends = sqlite_database.GetFriends(username);
            user.wall = sqlite_database.GetEvents(username);

            return user;
        }

        private void HandleJoinRequest(Object obj)
        {
            JoinRequest_data received_data = (JoinRequest_data)obj;
        
            ServerMsg msg_to_send = new ServerMsg();
            msg_to_send.type = TcpConst.JOIN;

            JoinReply_data data_to_send = new JoinReply_data();
            data_to_send.message_code = ValidateJoinRequest(received_data);

            //if(reply_data.message_code == TcpMessageCode.ACCEPTED)
            //    sqlite_database.AddNewUser(request_data.username, request_data.password)

            msg_to_send.data = (Object)data_to_send;

            tcp_server.SendMessage(received_data.username, msg_to_send);
        }

        private int ValidateJoinRequest(JoinRequest_data data)
        {
            //if (sqlite_database.EntryExistsInTable(data.username, "User", "user_id"))
            //{
            //    sqlite_database.AddNewUser(data.username, data.password, null);
            //    return TcpMessageCode.ACCEPTED;
            //}
            //else
            //    return TcpMessageCode.USER_EXISTS;

            return TcpMessageCode.ACCEPTED;
        }

        private void HandleLoginRequest(Object obj)
        {
            LoginRequest_data received_data = (LoginRequest_data)obj;

            ServerMsg msg_to_send = new ServerMsg();
            msg_to_send.type = TcpConst.LOGIN;

            LoginReply_data data_to_send = new LoginReply_data();

            data_to_send.message_code = ValidateLoginRequest(received_data);

            msg_to_send.data = (Object)data_to_send;

            tcp_server.SendMessage(received_data.username, msg_to_send);

        }

        private int ValidateLoginRequest(LoginRequest_data data)
        {
            //if (!sqlite_database.EntryExistsInTable(data.username, "User", "username"))
            //{
            //    return TcpMessageCode.USER_DONT_EXISTS;
            //}
            //else if(!sqlite_database.EntryExistsInTable(data.password, "User", "password"))
            //{
            //    return TcpMessageCode.INCORRECT_PASSWORD;
            //}
            //else 
            return TcpMessageCode.ACCEPTED;
        }
    }
}
