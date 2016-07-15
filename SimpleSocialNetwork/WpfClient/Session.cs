using System;
using System.Collections.Generic;
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

        /// <summary>
        /// List to keep track of users in SSN
        /// </summary>
        public List<UserSimple> users_list = new List<UserSimple>();
        public bool users_list_collected;

        private int registration = 0;
        /// <summary>User information in current session</summary>
        private string current_username = "";
        private bool logged_in = false;

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
            logged_in = true;
        }
        public void SetLoggedOut()
        {
            logged_in = false;
        }
        public bool GetLoggedInStatus()
        {
            return logged_in;
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
        public string friend = "gray";
    }
}
