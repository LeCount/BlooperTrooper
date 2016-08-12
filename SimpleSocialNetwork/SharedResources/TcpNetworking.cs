using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace SharedResources
{
    /// <summary>A class meant to distribute TCP related methods, used by both client and server.</summary>
    public static class TcpMethods
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
    }

    /// <summary>A class for representing the field "data", in the ClientMsg class, when a user want to Join the social network.</summary>
    [Serializable]
    public class JoinRequest_data
    {
        public String username { get; set; }
        public String password { get; set; }
        public String mail { get; set; }
        public String name { get; set; }
        public String surname { get; set; }
        public String about_user { get; set; }
        public String interests { get; set; }
    }

    /// <summary>A class for representing the field "data", in the ClientMsg- or ServerMsg class, when sender want to send a ping message.</summary>
    [Serializable]
    public class JoinReply_data
    {
        /// <summary>Code indicating the success or failure of the request.</summary>
        public int message_code { get; set; }
    }

    /// <summary>A class for representing the field "data", in the ClientMsg class, when a user want to send a login request.</summary>
    [Serializable]
    public class LoginRequest_data
    {
        /// <summary>Suggested username of the user.</summary>
        public String username { get; set; }

        /// <summary>Suggested password for the user.</summary>
        public String password { get; set; }

        /// <summary>Suggested confirmation code for the user.</summary>
        public String confirmation_code { get; set; }
    }

    /// <summary>A class for representing the field "data", in the ServerMsg class, when the server wants to reply to a login request.</summary>
    [Serializable]
    public class LoginReply_data
    {
        /// <summary>Code indicating the success or failure of the request.</summary>
        public int message_code { get; set; }
    }

    /// <summary>A ping request.</summary>
    [Serializable]
    public class Ping_data
    {
        /// <summary>Code indicating if message is a request or a reply</summary>
        public int message_code = TcpMessageCode.REQUEST;
    }

    /// <summary>A class for representing the field "data", in the ClientMsg class, when a user want to send a get_users request.</summary>
    [Serializable]
    public class GetUsersRequest_data
    {
        /// <summary>Username of the one who sent the request.</summary>
        public String from { get; set; }
    }

    /// <summary>A class for representing the field "data", in the ServerMsg class, when the server wants to reply to a get_users request.</summary>
    [Serializable]
    public class GetUsersReply_data
    {
        /// <summary>One of the available users in the social network.</summary>
        public String username { get; set; }

        /// <summary>Indication on whether you and this user are friends or not.</summary>
        public bool friend_status { get; set; }

        /// <summary>Indication on whether there are any more users to send.</summary>
        public bool no_more_users { get; set; }
    }

    /// <summary>A class for representing the field "data", in the ClientMsg- or ServerMsg class, when a user sends an "add friend" request.</summary>
    [Serializable]
    public class AddFriendRequest_data
    {
        /// <summary>Username of the one who sent the request.</summary>
        public String requester { get; set; }

        /// <summary>Username of the one who the request concerns.</summary>
        public String responder { get; set; }
    }

    /// <summary>A class for representing the field "data", in the ClientMsg- or ServerMsg class, when a user is responding to an "add friend" request.</summary>
    [Serializable]
    public class AddFriendResponse_data
    {
        /// <summary>Username of the one who initially sent the request.</summary>
        public String requester { get; set; }

        /// <summary>Username of the responder.</summary>
        public String responder { get; set; }

        /// <summary>Code indicating the acceptance or the denial, of the friend request.</summary>
        public int message_code { get; set; }
    }

    /// <summary>A class for representing the field "data", in the ClientMsg class, when a user want to send a get_friends_status request.</summary>
    [Serializable]
    public class GetFriendStatusRequest_data
    {
        /// <summary>Username of the one who sent the request.</summary>
        public String from { get; set; }

        /// <summary>The user that the client wants to know the online status of.</summary>
        public String user { get; set; }
    }

    /// <summary>A class for representing the field "data", in the ServerMsg class, when the server wants to reply to a get_friends_status request.</summary>
    [Serializable]
    public class GetFriendStatusReply_data
    {
        /// <summary>The status of the requested user</summary>
        public bool online { get; set; }
    }

    /// <summary>A class for representing the field "data", in the ClientMsg class, when a user want to send a GET_WALL request.</summary>
    [Serializable]
    public class GetWallRequest_data
    {
        /// <summary>Username of the one who sent the request.</summary>
        public String requesting_user { get; set; }

        /// <summary>The user who's wall is being requested.</summary>
        public String owner_of_wall { get; set; }
    }

    /// <summary>A class for representing the field "data", in the ServerMsg class, when the server wants to reply to a GET_WALL request.</summary>
    [Serializable]
    public class GetWallReply_data
    {
        /// <summary>The user who's wall is being requested.</summary>
        public String owner_of_wall { get; set; }

        public UserEvent wall_event { get; set; }
    }

    /// <summary>A class for representing the field "data", in the ClientMsg class, when a user want to send a update request.</summary>
    [Serializable]
    public class UpdateRequest_data
    {
        public String new_username { get; set; }
        public String new_password { get; set; }
        public String new_mail { get; set; }
        public String new_name { get; set; }
        public String new_surname { get; set; }
        public String new_about_user { get; set; }
        public String new_interests { get; set; }
    }

    /// <summary>A class for representing the field "data", in the ServerMsg class, when the server wants to reply to a update request.</summary>
    [Serializable]
    public class UpdateReply_data
    {
        /// <summary>Code indicating the success or failure of the request.</summary>
        public int message_code { get; set; }
    }

    /// <summary>A class for representing the field "data", in the ClientMsg class, when a user want to send a get_user_data request.</summary>
    [Serializable]
    public class GetUserDataRequest_data
    {
        /// <summary>Username of the one who sent the request.</summary>
        public String from { get; set; }

        /// <summary>The user who's data is being requested.</summary>
        public String user { get; set; }
    }

    /// <summary>A class for representing the field "data", in the ServerMsg class, when the server wants to reply to a get_user_data request.</summary>
    [Serializable]
    public class GetUserDataReply_data
    {
        /// <summary>Code indicating the success or failure of the request.</summary>
        public int message_code { get; set; }

        public String new_username { get; set; }
        public String new_mail { get; set; }
        public String new_name { get; set; }
        public String new_surname { get; set; }
        public String new_about_user { get; set; }
        public String new_interests { get; set; }
    }

    /// <summary>A class for representing the field "data", in the ClientMsg class, when the user wants to send a chat msg to another user.</summary>
    [Serializable]
    public class Chat_data
    {
        /// <summary>Username of the one who sent the request.</summary>
        public String from { get; set; }

        /// <summary>The user to send message to</summary>
        public String to { get; set; }

        /// <summary>The message</summary>
        public String text { get; set; }
    }

    /// <summary>A class for representing the field "data", in the ClientMsg class, when the user wants to add a status message.</summary>
    [Serializable]
    public class AddStatus_data
    {
        /// <summary> Status text.</summary>
        public String wall_post { get; set; }
        public String poster { get; set; }
        public String owner_of_wall { get; set; }
    }

    /// <summary>A class for having parameterized user information available in memory, on the server and the client.</summary>
    public class User
    {
        public DateTime last_requested  { get; set; }
        public String username          { get; set; }
        public String password          { get; set; }
        public String name              { get; set; }
        public String surname           { get; set; }
        public String mail              { get; set; }
        public String about_user        { get; set; }
        public String interests         { get; set; }
        public List<String> friends = new List<String>();
        public List<UserEvent> wall = new List<UserEvent>();
    }

    /// <summary>A class to contain information regarding an event/post/log, on a users "wall".</summary>
    [Serializable]
    public class UserEvent
    {
        public DateTime time{ get; set; }
        public String text { get; set; }
    }


    /// <summary>A class with methods for validating expressions and formats.</summary>
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

            User a = new User();
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
    }

    /// <summary>A class meant to distribute related error codes to tcp messages.</summary>
    public static class TcpMessageCode
    {
        public const int INVALID = -1;
        public const int USER_EXISTS = -2;
        public const int USER_DONT_EXISTS = -3;
        public const int INCORRECT_PASSWORD = -4;
        public const int INCORRECT_CODE = -5;

        public const int DECLINED = 0;
        public const int ACCEPTED = 1;
        public const int CONFIRMED = 2;

        public const int REQUEST = 3;
        public const int REPLY = 4;

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
        public const int RESPOND_ADD_FRIEND = 6;
        public const int GET_FRIEND_STATUS = 7;
        public const int UPDATE = 8;
        public const int GET_CLIENT_DATA = 9;
        public const int CHAT = 10;
        public const int ADD_WALL_EVENT = 11;
        public const int GET_WALL = 12;
        public const int PING = 13;

        public const int INVALID = -1;

        public const int SERVER_PORT = 8001;
        private static DirectoryInfo directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        public static string DATABASE_FILE = "ServerDB.db";
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
                case 6:return " RESPOND_ADD_FRIEND";
                case 7: return "GET FRIEND STATUS";
                case 8: return "UPDATE";
                case 9: return "GET CLIENT DATA";
                case 10: return "CHAT";
                case 11: return "ADD STATUS";
                case 12: return "GET_WALL";
                case 13: return "PING";
                default: return "INVALID";
            }
        }

    }
}
