using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfClient
{
    public class Session
    {
        public const int REGISTRATION_FAILED = 0;
        public const int REGISTRATION_SUCCESS = 1;
        public const int REGISTRATION_NOTSET = -1;

        private int grubb { set; get; }
        /// <summary>
        /// List to keep track of users in SSN
        /// </summary>
        public List<UserSimple> users_list = new List<UserSimple>();
        public static bool users_list_collected;

        private int registration = 0;
        /// <summary>User information in current session</summary>
        private string current_username = "";
        private int logged_in = 0; // 0 not set, -1 false, 1 true

        public int GetRegistrationStatus()
        {
            return registration;
        } 
        public void SetRegistrationFailed()
        {
            registration = REGISTRATION_FAILED;
        }
        public void SetRegistrationSuccessful()
        {
            registration = REGISTRATION_SUCCESS;
        }
        public void SetRegistrationNotSet()
        {
            registration = REGISTRATION_NOTSET;
        }

        public void SetCurrentUsername(string un)
        {
            current_username = un;
        }
        public string GetCurrentUsername()
        {
            return current_username;
        }

        public void SetLoggedIn()
        {
            logged_in = 1;
        }
        public void SetLoggedOut()
        {
            logged_in = -1;
        }
        public int GetLoggedInStatus()
        {
            return logged_in;
        }
        public void SetLoggedInStatus(int status)
        {
            logged_in = status;
        }

        public void AddUserToList(string username, bool friend)
        {
            UserSimple u = new UserSimple();
            u.username = username;
            if (friend)
                u.friend = "green";
            users_list.Add(u);

        }
    }

    public class UserSimple
    {
        public string username = "";
        public string friend = "red";
    }
}
