using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;

//To bind a collor to an bool:
//http://www.wpf-tutorial.com/data-binding/value-conversion-with-ivalueconverter/

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
        public ObservableCollection<UserSimple> users_list = new ObservableCollection<UserSimple>();
        public bool users_list_collected;

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
            u.Username = username;

            Application.Current.Dispatcher.BeginInvoke(new Action(() => this.users_list.Add(u)));
        }
    }

    public class UserSimple : INotifyPropertyChanged
    {
        private string username = "";
        public string Username
        {
            get
            {
                return username;
            }

            set
            {
                username = value;
                NotifyPropertyChanged();
            }
        }

        private string friend = "red";

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Friend
        {
            get
            {
                return friend;
            }

            set
            {
                friend = value;
                NotifyPropertyChanged();
            }
        }
    }
}
