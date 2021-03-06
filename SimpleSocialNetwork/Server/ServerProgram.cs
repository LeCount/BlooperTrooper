﻿namespace Program
{
    using System;
    using System.Linq;
    using System.Threading;
    using ServerNetworking;
    using SharedResources;
    using System.Collections.Generic;
    using System.Net.Mail;
    using System.Net;
    using Server;
    public static class ServerEntryPoint
    {
        static void Main(string[] args)
        {
            if (args.Count() < 2)
                new ServerApp();
            else
            {
                string server_ipAddr = args.ElementAt(0);
                string server_port = args.ElementAt(1);
                new ServerApp(server_ipAddr, server_port);
            }
        }
    }

    public class ServerApp
    {
        private SQLiteDB sqlite_database = new SQLiteDB(TcpConst.DATABASE_FILE);
        private TcpServer tcp_server = null;
        private Thread executeRequests = null;
        private SMTP_window smtp_form = null;
        private SmtpClient smtp_client = null;

        public ServerApp(){InitServerApp(null, null);}

        public ServerApp(string ipaddr, string port){InitServerApp(ipaddr, port);}

        private void InitServerApp(string ip_addr_to_use, string port_to_use)
        {
            //smtp_form = new SMTP_window(this);

            tcp_server = new TcpServer(ip_addr_to_use, port_to_use);
            tcp_server.StartServer();

            executeRequests = new Thread(GetNextRequest);
            executeRequests.Start();
        }

        private void GetNextRequest()
        {
            ClientMsg next_request = null;

            while (true)
            {
                if (!tcp_server.inbox.Empty())
                {
                    next_request = tcp_server.GetNextRequest();
                    HandleClientRequest(next_request);
                }
                else
                    Thread.Sleep(1000);
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
                    break;
                case TcpConst.GET_USERS:
                    HandleGetUsersRequest(msg.data);
                    break;
                case TcpConst.ADD_FRIEND:
                    ForwardFriendRequest(msg.data);
                    break;
                case TcpConst.RESPOND_ADD_FRIEND:
                    ForwardAddFriendResponse(msg.data);
                    break;
                case TcpConst.GET_FRIEND_STATUS:
                    break;
                case TcpConst.ADD_WALL_EVENT:
                    AddWallPostToDB(msg.data);
                    break;
                case TcpConst.GET_WALL:
                    HandleGetWallRequest(msg.data);
                    break;
                case TcpConst.CHAT:
                    HandleChatRequest(msg.data);
                    break;
                case TcpConst.UPDATE:
                    break;
                case TcpConst.GET_CLIENT_DATA:
                    break;

                case TcpConst.INVALID:
                default:
                    break;
            }
        }

        private void AddWallPostToDB(object data)
        {
            AddStatus_data received_data = (AddStatus_data)data;
            sqlite_database.AddWallPost(received_data.poster, received_data.owner, received_data.wall_post);
        }

        private void HandleChatRequest(object data)
        {
            
            Chat_data data_to_send = (Chat_data)data;
            Chat_data error_data = new Chat_data();

            if (data_to_send.from == data_to_send.to)
                return;

            if (!AreFriends(data_to_send.from, data_to_send.to))
            {
                error_data.text = string.Format("We are not friends yet. Send me a fried request.", data_to_send.to);
                error_data.from = data_to_send.to;
                error_data.to = data_to_send.from;
                tcp_server.SendMessage(error_data, TcpConst.CHAT, error_data.to);
                return;
            }

            if(!tcp_server.SendMessage(data_to_send, TcpConst.CHAT, data_to_send.to))
            {
                error_data.text = string.Format("I am offline. Please try again later...", data_to_send.to);
                error_data.from = data_to_send.to;
                error_data.to = data_to_send.from;
                tcp_server.SendMessage(error_data, TcpConst.CHAT, error_data.to);
            }
        }

        private void HandleGetWallRequest(object data)
        {
            GetWallRequest_data received_data = (GetWallRequest_data)data;
            SendWall(received_data.owner_of_wall, received_data.requesting_user);
        }

        private void SendWall(string user_owning_wall, string user_requesting_wall)
        {
            GetWallReply_data data_to_send = new GetWallReply_data();

            if (AreFriends(user_owning_wall, user_requesting_wall) || user_owning_wall == user_requesting_wall)
            {
                List<WallPost> wall = null;

                wall = sqlite_database.GetAllEventsFromUser(user_owning_wall);

                if (wall == null)
                    return;

                foreach (WallPost e1 in wall)
                {
                    
                    data_to_send.owner_of_wall = user_owning_wall;
                    data_to_send.wall_event = e1;

                    tcp_server.SendMessage(data_to_send, TcpConst.GET_WALL, user_requesting_wall);
                }
            }
            else
            {
                WallPost e2 = new WallPost();
                e2.text = string.Format("Looks like you and user {0} are not friends yet. Send {0} a fried request to see his wall.", user_owning_wall);
                e2.time = DateTime.Now;
                data_to_send.owner_of_wall = user_owning_wall;
                data_to_send.wall_event = e2;

                tcp_server.SendMessage(data_to_send, TcpConst.GET_WALL, user_requesting_wall);
            }
        }

        /// <summary>Gets the data on a specific user from local db, and puts it parameterized into a user-class object.</summary>
        /// <param name="username">The name of the user.</param>
        private User GetUserFromDB(String username)
        {
            User user = new User();

            user.last_requested = DateTime.Today;
            user.name = username;
            user.wall = sqlite_database.GetAllEventsFromUser(username);
            
            return user;
        }

        private void HandleJoinRequest(Object obj)
        {
            JoinRequest_data received_data = (JoinRequest_data)obj;
        
            JoinReply_data data_to_send = new JoinReply_data();
            data_to_send.message_code = ValidateJoinRequest(received_data);

            tcp_server.SendMessage(data_to_send, TcpConst.JOIN, received_data.username);
        }

        private int ValidateJoinRequest(JoinRequest_data data)
        {
            if ( sqlite_database.EntryExistsInTable(data.username, "User", "username") )
                return TcpMessageCode.USER_EXISTS;
            else
            {
                sqlite_database.AddNewUser(data.username, data.password, null);
                Console.WriteLine(String.Format("[{0}]:Joined the network.", data.username));
                return TcpMessageCode.ACCEPTED;
            }
        }

        private void HandleLoginRequest(Object obj)
        {
            LoginRequest_data received_data = (LoginRequest_data)obj;
            
            while(!tcp_server.userIsBoundToSocket(received_data.username))
            {
                Thread.Sleep(100);
            }

            LoginReply_data data_to_send = new LoginReply_data();
            data_to_send.message_code = ValidateLoginRequest(received_data);

            if(data_to_send.message_code == TcpMessageCode.ACCEPTED)
                Console.WriteLine(String.Format("[Server]:{0} was granted access to network.", received_data.username));
            else
                Console.WriteLine(String.Format("[Server]:{0} was denied access to network.", received_data.username));

            tcp_server.SendMessage(data_to_send, TcpConst.LOGIN, received_data.username);
        }

        private int ValidateLoginRequest(LoginRequest_data data)
        {
            if (!sqlite_database.EntryExistsInTable(data.username, "User", "username"))
            {
                return TcpMessageCode.USER_DONT_EXISTS;
            }
            else if (!sqlite_database.EntryExistsInTable(data.password, "User", "password"))
            {
                return TcpMessageCode.INCORRECT_PASSWORD;
            }
            else
                return TcpMessageCode.ACCEPTED;
        }

        private void HandleGetUsersRequest(object obj)
        {
            GetUsersRequest_data received_data = (GetUsersRequest_data)obj;

            List<string> all_usernames = GetAllUsersFromDB();

            for(int i=0; i<all_usernames.Count; i++)
            {
                GetUsersReply_data data_to_send = new GetUsersReply_data();
                data_to_send.username = all_usernames.ElementAt(i);

                if(AreFriends(all_usernames.ElementAt(i), received_data.from))
                    data_to_send.friend_status = true;
                else
                    data_to_send.friend_status = false;

                if (i == all_usernames.Count - 1)
                    data_to_send.no_more_users = true;
                else
                    data_to_send.no_more_users = false;

                tcp_server.SendMessage(data_to_send, TcpConst.GET_USERS, received_data.from);
            }
        }

        private void ForwardFriendRequest(object obj)
        {
            AddFriendRequest_data received_data = (AddFriendRequest_data)obj;

            if (AreFriends(received_data.requester, received_data.responder))
            {
                AddFriendResponse_data data_to_send = new AddFriendResponse_data();
                data_to_send.requester = received_data.requester;
                data_to_send.responder = received_data.responder;
                data_to_send.message_code = TcpMessageCode.DECLINED;

                tcp_server.SendMessage(data_to_send, TcpConst.RESPOND_ADD_FRIEND, received_data.requester);
            }
            else
                tcp_server.SendMessage(received_data, TcpConst.ADD_FRIEND, received_data.responder);
        }

        private void ForwardAddFriendResponse(object obj)
        {
            AddFriendResponse_data received_data = (AddFriendResponse_data)obj;

            if (received_data.message_code == TcpMessageCode.ACCEPTED)
                sqlite_database.AddFriendRelation(received_data.requester, received_data.responder);

            tcp_server.SendMessage(received_data, TcpConst.RESPOND_ADD_FRIEND, received_data.requester);
        }

        private bool AreFriends(string username1, string username2)
        {
            return sqlite_database.FriendRelationExists(sqlite_database.GetUserId(username1), sqlite_database.GetUserId(username2));
        }

        private List<String> GetAllUsersFromDB()
        {
            return sqlite_database.GetAllUsernames();
        }

        private void SendEmailTo(string suggested_email, string text, string subject)
        {
            if (smtp_client != null)
            { 
                MailMessage msg = new MailMessage();

                msg.From = new MailAddress("server.test@gmail.com", "Server Verification");
                msg.To.Add(suggested_email);
                msg.Subject = subject;
                msg.Body = text;
                smtp_client.Send(msg);
            }
        }

        public void SetSMTP_client(SmtpClient smtpc)
        {
            if (smtpc == null)
                Console.WriteLine("No SMTP client were specified.");
            else
                Console.WriteLine("Using SMTP client.");
            smtp_client = smtpc;
        }
    }
}
