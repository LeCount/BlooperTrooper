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
            User user = DataTransform.Deserialize(msg.data);

            switch(msg.type)
            {
                case TcpConst.JOIN:

                    HandleJoinRequest(user);

                    break;
                case TcpConst.LOGIN:

                    HandleLoginRequest(user);

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

                    ServerMsg reply = new ServerMsg();
                    reply.type = TcpConst.PING;
                    reply.data = TcpMessageCode.CONFIRMED;
                    tcp_server.SendMessage(user.username, reply);

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

        private void HandleJoinRequest(User u)
        {
            ServerMsg reply = new ServerMsg();
            reply.type = TcpConst.JOIN;

            reply.data = ValidateJoinRequest(u);
            tcp_server.SendMessage(u.username, reply);
        }

        private int ValidateJoinRequest(User u)
        {
            if (sqlite_database.EntryExistsInTable(u.username, "User", "user_id"))
            {
                sqlite_database.AddNewUser(u.username, u.password, null);
                return TcpMessageCode.ACCEPTED;
            }
            else
                return TcpMessageCode.USER_EXISTS;
        }

        private void HandleLoginRequest(User u)
        {
            ServerMsg reply = new ServerMsg();
            reply.type = TcpConst.LOGIN;
            reply.data = TcpMessageCode.ACCEPTED;

            Console.WriteLine(u.username);
            tcp_server.SendMessage(u.username, reply);

        }
    }
}
