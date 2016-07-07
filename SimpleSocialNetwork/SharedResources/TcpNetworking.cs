using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

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
        static public Stream ConnectToServer(TcpClient c, string server_addr, int port)
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
            catch{return false;}
        }
    }

    /// <summary>A class meant to distribute TCP related constants used by both client and server.</summary>
    public static class TcpConst
    {
        //Message identifiers
        public const int JOIN = 1;
        public const int LOGIN = 2;
        public const int LOGOUT = 3;
        public const int GET_USERS = 4;
        public const int ADD_FRIEND = 5;
        public const int GET_FRIENDS_STATUS = 6;
        public const int GET_CLIENT_DATA = 7;
        public const int SEND_MESSAGE = 8;

        //message types:
        public const int REQUEST = 9;
        public const int REPLY = 10;

        public const int INVALID = 0;

        public const int SERVER_PORT = 8001;
        public const string DATABASE_FILE = "serverDB.db";
        public const int BUFFER_SIZE = 1024 * 4;

        /// <summary>Convert an integer constant to its corresponding text.</summary>
        public static string IntToText(int i)
        {
            switch (i)
            {
                case 1: return "JOIN";
                case 2: return "LOGIN";
                case 3: return "LOGOUT";
                case 4: return "GET USERS";
                case 5: return "ADD FRIEND";
                case 6: return "GET FRIENDS STATUS";
                case 7: return "GET CLIENT DATA";
                case 8: return "SEND MESSAGE";
                case 9: return "REQUEST";
                case 10: return "REPLY";
                default: return "INVALID";
            }
        }

    }
}
