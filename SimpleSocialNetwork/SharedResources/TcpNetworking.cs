using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

    /// <summary>A class that users Json serializer to convert client and server msg data back and forth.</summary>
    public static class DataParser
    {
        public static object Serializer(User u)
        {
            Object data = (Object)JsonConvert.SerializeObject(u);
            return data;
        }

        public static User Deserialize(Object data)
        {

            User user = JsonConvert.DeserializeObject<User>((String)data);
            return user;
        }
    }

    /// <summary>A class for having parameterized user information available in memory, on the server and the client.</summary>
    public class User
    {
        public DateTime last_requested { get; set; }
        public bool access { get; set; }
        public bool status { get; set; }
        public String username { get; set; }
        public String password { get; set; }
        public String name { get; set; }
        public String surname { get; set; }
        public String mail { get; set; }
        public String about_user { get; set; }
        public String interests { get; set; }
        public List<String> friends = new List<String>();
        public List<UserEvent> wall = new List<UserEvent>();

    }

    /// <summary>A class to contain information regarding an event/post/log, on a users "wall".</summary>
    public class UserEvent
    {
        public String time { get; set; }
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
        public const int GET_FRIEND_STATUS = 6;
        public const int UPDATE_USER_DATA = 7;
        public const int GET_CLIENT_DATA = 8;
        public const int SEND_MESSAGE = 9;
        public const int GET_WALL = 10;
        public const int PING = 11;
        public const int VERIFICATION_CODE = 12;
        //

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
                case 7: return "UPDATE USER DATA";
                case 8: return "GET CLIENT DATA";
                case 9: return "SEND MESSAGE";
                case 10: return "GET_WALL";
                case 11: return "PING";
                default: return "INVALID";
            }
        }

    }
}
