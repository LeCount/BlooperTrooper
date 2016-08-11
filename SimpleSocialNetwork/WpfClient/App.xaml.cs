﻿using System;
using System.Windows;
using System.IO;
using System.Threading;
using SharedResources;
using System.Net.Sockets;
using ClientNetworking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace WpfClient
{
    /// <summary>Interaction logic for App.xaml </summary>
    public partial class App : Application
    {
        private AppLogger log = new AppLogger();

        /// <summary>A stream providing read and write operations over TCP.</summary>
        private static Stream client_stream = null;

        /// <summary>A medium for providing client connections for TCP network services.</summary>
        private TcpClient tcp_client = null;

        public Hashtable chat_conversations = new Hashtable();

        /// <summary>Session class to keep track of current session</summary>
        public Session session = new Session();

        /// <summary>Thread responsible for reading incoming messages on this client's socket, and adding them to a global list.</summary>
        private Thread add_messages = null;

        /// <summary>Thread responsible for pinging the server.</summary>
        private Thread ping_server = null;

        /// <summary>WPF window, providing login- and register new user services</summary>
        private LoginWindow login_window = null;

        /// <summary>WPF window, providing tools and services for a user that is online.</summary>
        public MainWindow main_window = null; 

        /// <summary>A reference to an instance, that provides the API for TCP communication.</summary>
        ClientTCP tcp_networking = new ClientTCP(); 

        /// <summary>Thread that handles received messages from server.</summary>
        Thread handle_messages = null;

        public App()
        {
            if (tcp_client == null)
                tcp_client = new TcpClient();

            //TODO: Server ip is assumed to be local host at the moment.
            if (client_stream == null)
                client_stream = tcp_networking.ConnectToServer(tcp_client, TcpMethods.GetIP(), TcpConst.SERVER_PORT);

           /* handle_messages = new Thread(GetNextMessage);
            handle_messages.Start();

            add_messages = new Thread(() => tcp_networking.ClientRead(client_stream));
            add_messages.Start();*/

            main_window = new MainWindow();
            login_window = new LoginWindow();
        }

        ///<summary>Login to server</summary>
        public bool LoginToServer(string username, string password)
        {
            if(tcp_client==null)
                tcp_client = new TcpClient();

            //TODO: Server ip is assumed to be local host at the moment.
            if(client_stream == null)
                client_stream = tcp_networking.ConnectToServer(tcp_client, TcpMethods.GetIP(), TcpConst.SERVER_PORT);

            log.Add("Starting handle message thread");
            handle_messages = new Thread(GetNextMessage);
            handle_messages.Start();

            log.Add("Starting add message thread");
            add_messages = new Thread(() => tcp_networking.ClientRead(client_stream));
            add_messages.Start();

            session.SetLoggedInStatus(0);
            session.SetCurrentUsername(username);
            LoginRequest_data loginData = new LoginRequest_data();

            loginData.password = password;
            loginData.username = username;

            tcp_networking.Client_send(loginData, TcpConst.LOGIN, client_stream);

            Thread.Sleep(100);

            int timeout_counter = 0;
            while (timeout_counter < 100)
            {
                if (session.GetLoggedInStatus() == 1)
                {
                    main_window.Title = "Simple Social Network - " + username;

                    ping_server = new Thread(() => tcp_networking.ServerStatusPing(client_stream));
                    ping_server.Start();

                    return true;
                }
                else if (session.GetLoggedInStatus() == -1)
                    return false;
                timeout_counter++;
                Thread.Sleep(10);
            }
            return false;
        }

        public void LogoutServer()
        {
            tcp_networking.Client_send(null, TcpConst.LOGOUT, client_stream);

            log.Add("User loged out from server.");

            if (add_messages.IsAlive)
                add_messages.Abort();

            if (ping_server.IsAlive)
                ping_server.Abort();

            if (handle_messages.IsAlive)
                handle_messages.Abort();

            try
            {
                tcp_client.Close();
                client_stream.Close();
            }
            catch(Exception){}

            tcp_client = null;
            client_stream = null;

            session.SetLoggedOut();
            login_window.Show();
        }

        public bool RequestToJoinSocialNetwork(string username, string password, string email, string firstName, string lastName, string about, string interests)
        {
            if (tcp_client == null)
                tcp_client = new TcpClient();

            if (client_stream == null)
                client_stream = tcp_networking.ConnectToServer(tcp_client, TcpMethods.GetIP(), TcpConst.SERVER_PORT);

            session.SetRegistrationNotSet();

            JoinRequest_data data_to_send = new JoinRequest_data();
            data_to_send.password = password;
            data_to_send.username = username;
            data_to_send.mail = email;
            data_to_send.name = firstName;
            data_to_send.surname = lastName;
            data_to_send.about_user = about;
            data_to_send.interests = interests;

            tcp_networking.Client_send(data_to_send, TcpConst.JOIN, client_stream);

            //TODO This method for waiting for server reply is quite unreliable. Mby a more vell structured syncronization mechanism should be used, or the client should ask the server if the join request succeeded.
            Thread.Sleep(1000);

            if (session.GetRegistrationStatus() == Session.REGISTRATION_SUCCESS)
            {
                MessageBox.Show("Registration succeeded.");
                return true;
            }
            else
            {
                MessageBox.Show("Registration failed.");
                return false;
            }
        }

        public bool RequestAllAvailableUsers()
        {
            session.users_list.Clear();

            GetUsersRequest_data request_data = new GetUsersRequest_data();
            request_data.from = session.GetCurrentUsername();

            tcp_networking.Client_send(request_data, TcpConst.GET_USERS, client_stream); 

            return true;
        }

        public bool AddFriendRequest(string username)
        {
            AddFriendRequest_data req_data = new AddFriendRequest_data();

            req_data.responder = username;
            req_data.requester = session.GetCurrentUsername();

            tcp_networking.Client_send(req_data, TcpConst.ADD_FRIEND, client_stream);

            return true;
        }

        public bool AddStatusMessage(string status, string to)
        {
            AddStatus_data asd = new AddStatus_data();
            asd.from = session.GetCurrentUsername();
            asd.to = to;
            asd.statusText = status;
            tcp_networking.Client_send(asd, TcpConst.ADD_WALL_EVENT, client_stream);
            return true;
        }

        public bool GetWallFromUser(string username)
        {
            session.wall.Clear();

            GetWallRequest_data gwrd = new GetWallRequest_data();
            gwrd.from = session.GetCurrentUsername();
            gwrd.user = username;

            tcp_networking.Client_send(gwrd, TcpConst.GET_WALL, client_stream);

            return true;
        }

        public void StartChat(string username)
        {
            if (!chat_conversations.ContainsKey(username))
            {
                ObservableCollection<ChatMessage> new_conversation = new ObservableCollection<ChatMessage>();
                chat_conversations.Add(username, new_conversation);
            }

            Dispatcher.Invoke(new Action(delegate ()
            {
                ChatWindow chat_window = new ChatWindow(username);
                chat_window.Title = string.Format("[{0}] \tConversation with {1}", session.GetCurrentUsername(), username);
                chat_window.Show();
            }
            ));
            
                
            return;
        }

        public void SendChatMessage(string text, string username)
        {
            Chat_data textToSend = new Chat_data();
            textToSend.text = text;
            textToSend.from = session.GetCurrentUsername();
            textToSend.to = username;
            tcp_networking.Client_send(textToSend, TcpConst.CHAT, client_stream);

            AddChatMessage(new ChatMessage(username, text), true);
        }

        private void AddChatMessage(ChatMessage msg, bool self)
        {
            string from_user = "";

            if (self)
                from_user = session.GetCurrentUsername();
            else
                from_user = msg.Username;

            Dispatcher.Invoke(new Action(delegate ()
            {
                if (((ObservableCollection<ChatMessage>)chat_conversations[msg.Username]) == null)
                    chat_conversations.Add(msg.Username, new ObservableCollection<ChatMessage>());

                
                ((ObservableCollection<ChatMessage>)chat_conversations[msg.Username]).Add(new ChatMessage(from_user, msg.MessageText));
            }
            ));
        }

        public ChatMessage GetNextChatMsg(string username)
        {
            if(((ObservableCollection<ChatMessage>)chat_conversations[username]) == null)
                return null;

            ChatMessage next_chat_msg = null;

            if (((ObservableCollection<ChatMessage>)chat_conversations[username]).Count > 0)
            {
                try
                {
                    next_chat_msg = ((ObservableCollection<ChatMessage>)chat_conversations[username]).ElementAt(0);
                    ((ObservableCollection<ChatMessage>)chat_conversations[username]).RemoveAt(0);
                    return next_chat_msg;
                }
                catch(Exception){return null;}
            }
            else
                return null;
        }

        internal void RemoveConversation(string uname)
        {
            if (chat_conversations.ContainsKey(uname))
                chat_conversations.Remove(uname);
        }

        /// <summary>Depending on the reply that was received, handle it accordingly. </summary>
        /// <param name="msg">Received message.</param>
        private void HandleServerReplies(ServerMsg msg)
        {
            switch (msg.type)
            {
                case TcpConst.PING:
                    Ping_data b = new Ping_data();
                    b = (Ping_data)msg.data;
                   
                    if(b.message_code == TcpMessageCode.REPLY)
                        tcp_networking.SetServerStatus(true);
                    else
                    {
                        log.Add("Invalid Ping");
                        tcp_networking.SetServerStatus(false);
                    }

                    break;

                case TcpConst.JOIN:
                    JoinReply_data join_reply = (JoinReply_data)msg.data;

                    if (join_reply.message_code == TcpMessageCode.ACCEPTED)
                    {
                        session.SetRegistrationSuccessful();
                        log.Add("Successfully Registeded user");
                    }
                    else
                    {
                        session.SetRegistrationFailed();
                        log.Add("Registration of user failed");
                    }
                    break;

                case TcpConst.LOGIN:
                    LoginReply_data lrd = new LoginReply_data();
                    lrd = (LoginReply_data)msg.data;
                    if (lrd.message_code == TcpMessageCode.ACCEPTED)
                    {
                        session.SetLoggedIn();
                        log.Add("User Logged in");
                    }
                    else
                        session.SetLoggedOut();
                    break;

                case TcpConst.LOGOUT:

                    break;

                case TcpConst.GET_USERS:
                    GetUsersReply_data udr = (GetUsersReply_data)msg.data;
                    log.Add("Added user" + udr.username);
                    session.AddUserToList(udr.username, udr.friend_status );
                    break;

                case TcpConst.ADD_FRIEND:
                    AddFriendRequest_data afreq = (AddFriendRequest_data)msg.data;

                    MessageBoxResult result = MessageBox.Show("User " + afreq.requester + " wants to be your friend!\nDo you accept?", "Friend Request", MessageBoxButton.YesNo);

                    AddFriendResponse_data afres = new AddFriendResponse_data();
                    afres.responder = afreq.responder;
                    afres.requester = afreq.requester;

                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            afres.message_code = TcpMessageCode.ACCEPTED;
                            tcp_networking.Client_send(afres, TcpConst.RESPOND_ADD_FRIEND, client_stream);
                            break;
                        case MessageBoxResult.No:
                            afres.message_code = TcpMessageCode.DECLINED;
                            tcp_networking.Client_send(afres, TcpConst.RESPOND_ADD_FRIEND, client_stream);
                            break;
                    }

                    break;
                case TcpConst.RESPOND_ADD_FRIEND:

                    break;
                case TcpConst.GET_WALL:
                    GetWallReply_data gwr = new GetWallReply_data();
                    gwr = (GetWallReply_data)msg.data;

                    session.AddStatusToWall(gwr.user, gwr.wall_event);

                    break;
                case TcpConst.GET_FRIEND_STATUS:  
                    
                    break;
                case TcpConst.GET_CLIENT_DATA:

                    break;
                case TcpConst.CHAT:
                    Chat_data received_data = (Chat_data)msg.data;

                    if (chat_conversations.ContainsKey(received_data.from))
                    {
                        AddChatMessage(new ChatMessage(received_data.from, received_data.text), false);
                    }
                    else
                    {
                        StartChat(received_data.from);
                        AddChatMessage(new ChatMessage(received_data.from, received_data.text), false);
                    }
                        
                    break;
                default: break;
            }
        }

        private void AppStart(object sender, StartupEventArgs e)
        {
            log.Add("App is starting...");

            // Show login window
            login_window.Show();
        }

        private void GetNextMessage()
        {
            ServerMsg msg = null;

            while (true)
            {
                msg = tcp_networking.GetNextMessage();

                if (msg != null)
                    HandleServerReplies(msg);
                else
                    Thread.Sleep(1000);
            }
        }

        public void AppShutdown()
        {
            log.Add("App is shutting down.");

            if (add_messages.IsAlive)
                add_messages.Abort();

            if (ping_server.IsAlive)
                ping_server.Abort();

            if (handle_messages.IsAlive)
                handle_messages.Abort();

            try {tcp_client.Close();}
            catch (Exception) { }
        }
    }
}
