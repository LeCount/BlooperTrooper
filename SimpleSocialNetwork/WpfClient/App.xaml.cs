using System;
using System.Windows;
using System.IO;
using System.Threading;
using SharedResources;
using System.Net.Sockets;
using ClientNetworking;

namespace WpfClient
{
    /// <summary>Interaction logic for App.xaml </summary>
    public partial class App : Application
    {
        private AppLogger log = new AppLogger();

        /// <summary>A stream providing read and write operations over TCP.</summary>
        private static Stream client_stream = null;

        /// <summary>A medium for providing client connections for TCP network services.</summary>
        private TcpClient tcp_client = new TcpClient();

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
            main_window = new MainWindow();
            login_window = new LoginWindow();
        }

        ///<summary>Login to server</summary>
        public bool LoginToServer(string username, string password)
        {
            int timeout_counter = 0;
            session.SetLoggedInStatus(0);
            session.SetCurrentUsername(username);
            LoginRequest_data loginData = new LoginRequest_data();

            loginData.password = password;
            loginData.username = username;

            ClientMsg msg = new ClientMsg();
            msg.type = TcpConst.LOGIN;
            msg.data = (object)loginData;

            tcp_networking.Client_send(msg, client_stream);

            while(timeout_counter < 1000)
            {
                if (session.GetLoggedInStatus() == 1)
                    return true;
                else if (session.GetLoggedInStatus() == -1)
                    return false;
                timeout_counter++;
                Thread.Sleep(100);
            }
            return false;
        }

        public bool LogoutServer()
        {
            session.SetLoggedOut();
            login_window.Show();
            return true;
        }

        public bool RequestToJoinSocialNetwork(string username, string password, string email, string firstName, string lastName, string about, string interests)
        {
            session.SetRegistrationNotSet();

            JoinRequest_data j = new JoinRequest_data();
            j.password = password;
            j.username = username;
            j.mail = email;
            j.name = firstName;
            j.surname = lastName;
            j.about_user = about;
            j.interests = interests;

            ClientMsg msg = new ClientMsg();
            msg.type = TcpConst.JOIN;
            msg.data = (Object)j;

            tcp_networking.Client_send(msg,client_stream);

            // Wait for registration confirmation
            for (int i=0;  session.GetRegistrationStatus() == Session.REGISTRATION_NOTSET && i < 10; i++)
            {
                if (session.GetRegistrationStatus() == Session.REGISTRATION_SUCCESS)
                    return true;

                else if (session.GetRegistrationStatus() == Session.REGISTRATION_FAILED)
                {
                    MessageBox.Show("Registration failed");
                    return false;
                }
                    
                Thread.Sleep(500);
            }

            MessageBox.Show("Registration Timeout");
            return false;
        }

        public bool RequestAllAvailableUsers()
        {
            session.users_list.Clear();

            GetUsersRequest_data request_data = new GetUsersRequest_data();
            request_data.from = session.GetCurrentUsername();

            ClientMsg msg = new ClientMsg();
            msg.type = TcpConst.GET_USERS;
            msg.data = (object)request_data;
            tcp_networking.Client_send(msg, client_stream); 

            return true;
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
                    JoinReply_data joinreply = new JoinReply_data();
                    joinreply = (JoinReply_data)msg.data;

                    if (joinreply.message_code == TcpMessageCode.ACCEPTED)
                    {
                        session.SetRegistrationSuccessful();
                        MessageBox.Show("Successfully Registered");
                        log.Add("Successfully Registeded user");

                    }
                    else
                    {
                        session.SetRegistrationFailed();
                        MessageBox.Show("Registration Failed");
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
                    GetUsersReply_data udr = new GetUsersReply_data();
                    udr = (GetUsersReply_data)msg.data;

                    session.AddUserToList(udr.username, udr.friend_status );

                    main_window.RefreshUserList();
                    log.Add("Added user" + udr.username);
                    break;
                case TcpConst.ADD_FRIEND:

                    break;
                case TcpConst.GET_FRIEND_STATUS:  

                    break;
                case TcpConst.GET_CLIENT_DATA:

                    break;
                case TcpConst.CHAT:

                    break;
                default: break;
            }
        }

        private void AppStart(object sender, StartupEventArgs e)
        {
            log.Add("App is starting...");
            //TODO: Server ip is assumed to be local host at the moment.
            client_stream = tcp_networking.ConnectToServer(tcp_client, TcpMethods.GetIP(), TcpConst.SERVER_PORT);

            add_messages = new Thread(() => tcp_networking.ClientRead(client_stream)); 
            add_messages.Start();

            handle_messages = new Thread(GetNextMessage);
            handle_messages.Start();

            ping_server = new Thread(() => tcp_networking.ServerStatusPing(client_stream));
            ping_server.Start();

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

            tcp_client.Close();  
        }
    }

}
