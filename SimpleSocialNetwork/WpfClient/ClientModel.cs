using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfClient
{
    /// <summary>
    /// Stores wall posts in client
    /// </summary>
    public class WallPost : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string username = "";
        public string Username
        {
            get { return username; }
            set
            {
                username = value;
                NotifyPropertyChanged();
            }
        }

        private string writer = "";
        public string Writer
        {
            get { return writer; }
            set
            {
                writer = value;
                NotifyPropertyChanged();
            }
        }


        private string status = "";

        public string Status
        {
            get { return status; }
            set
            {
                status = value;
                NotifyPropertyChanged();
            }
        }

        private DateTime time;

        public DateTime Time
        {
            get { return time; }
            set
            {
                time = value;
                NotifyPropertyChanged();
            }
        }
    }
    /// <summary>
    /// Stores chat messages in client
    /// </summary>
    public class ChatMessage : INotifyPropertyChanged
    {

        public ChatMessage (string u, string mt)
        {
            username = u;
            message_text = mt;
            time = DateTime.Now;
        }

        private string username = "";
        public string Username
        {
            get { return username; }
            set
            {
                username = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private string message_text = "";

        public string MessageText
        {
            get { return message_text; }
            set
            {
                message_text = value;
                NotifyPropertyChanged();
            }
        }

        private DateTime time;

        public DateTime Time
        {
            get { return time; }
            set
            {
                time = value;
                NotifyPropertyChanged();
            }
        }

    }

}