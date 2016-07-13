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
        ClientMsg GetNextRequest();
        void HandleRequest(ClientMsg msg);

    }

    public class ServerApp
    {
        private SQLiteDB sqlite_database = new SQLiteDB(TcpConst.DATABASE_FILE);
        TcpServer tcp_server = null;
        private Thread get_next_request = null;

        
        public ServerApp(){Init(null, null);}

        public ServerApp(string ipaddr, string port){Init(ipaddr, port);}

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
                case TcpConst.PING:

                    PingRequest_data request_data = (PingRequest_data)msg.data;

                    ServerMsg msg_reply = new ServerMsg();
                    msg_reply.type = TcpConst.PING;

                    PingReply_data reply_data = new PingReply_data();

                    reply_data.message_code = TcpMessageCode.CONFIRMED;
                    msg_reply.data = (Object)reply_data;

                    tcp_server.SendMessage(request_data.from, msg_reply);

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
            ServerMsg msg_reply = new ServerMsg();
            msg_reply.type = TcpConst.JOIN;

            JoinRequest_data request_data = (JoinRequest_data)obj;

            JoinReply_data reply_data = new JoinReply_data();
            reply_data.message_code = ValidateJoinRequest(request_data);

            //if(reply_data.message_code == TcpMessageCode.ACCEPTED)
            //    sqlite_database.AddNewUser(request_data.username, request_data.password)

            msg_reply.data = (Object)reply_data;

            tcp_server.SendMessage(request_data.username, msg_reply);
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
            ServerMsg msg_reply = new ServerMsg();
            msg_reply.type = TcpConst.LOGIN;

            LoginRequest_data request_data = (LoginRequest_data)obj;


            Console.WriteLine(request_data.username);
            tcp_server.SendMessage(request_data.username, msg_reply);

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
