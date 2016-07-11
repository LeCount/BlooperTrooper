namespace ServerProgram
{
    using SharedResources;
    using System;
    using System.Linq;
    using System.Threading;
    using ServerNetworking;

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
        private SQLiteDB db = new SQLiteDB(TcpConst.DATABASE_FILE);
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
            User user = DataParser.Deserialize(msg.data);

            switch(msg.type)
            {
                case TcpConst.JOIN:

                    HandleJoinRequest(user);

                    break;
                case TcpConst.LOGIN:

                    HandleLoginRequest(user);

                    //Add the user to the userlist on server
                    networking.AddToUserList(GetUserFromDB(user.username));

                    break;
                case TcpConst.LOGOUT:

                    //Remove the user from the userlist on server
                    networking.RemoveUserFromList(user.username);

                    break;
                case TcpConst.GET_USERS:
                    break;
                case TcpConst.ADD_FRIEND:
                    break;
                case TcpConst.GET_FRIEND_STATUS:
                    break;
                case TcpConst.UPDATE_USER_DATA:

                    //Update the user, that was updated, in the userlist on server
                    networking.RemoveUserFromList(user.username);
                    networking.AddToUserList(GetUserFromDB(user.username));

                    break;
                case TcpConst.GET_CLIENT_DATA:

                    //Add requested user to userlist on server
                    networking.AddToUserList(GetUserFromDB(user.username));

                    break;
                case TcpConst.SEND_MESSAGE:
                    break;
                case TcpConst.GET_WALL:
                    break;
                case TcpConst.PING:
                    break;
                case TcpConst.VERIFICATION_CODE:
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
            user.mail = db.GetMail(username);
            user.name = db.GetName(username);
            user.surname = db.GetSurname(username);
            user.about_user = db.GetAbout(username);
            user.interests = db.GetInterest(username);
            user.friends = db.GetFriends(username);
            user.wall = db.GetEvents(username);

            return user;
        }

        private void HandleJoinRequest(User u)
        {
            ServerMsg reply = new ServerMsg();
            reply.type = TcpConst.JOIN;

            reply.data = ValidateJoinRequest(u);
            networking.SendMessage(u.username, reply);
        }

        private int ValidateJoinRequest(User u)
        {
            if (db.EntryExistsInTable(u.username, "User", "user_id"))
            {
                db.AddNewUser(u.username, u.password, null);
                return TcpMessageCode.ACCEPTED;
            }
            else
                return TcpMessageCode.USER_EXISTS;
        }

        private void HandleLoginRequest(User u)
        {

        }
    }
}
