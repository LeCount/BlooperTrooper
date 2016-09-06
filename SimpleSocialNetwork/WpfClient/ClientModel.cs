using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace WpfClient
{
    /// <summary>
    /// Stores wall posts in client
    /// </summary>
    public class WallPostItem : INotifyPropertyChanged
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

    public class UserSimple : INotifyPropertyChanged
    {
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


        private bool friend = false;

        public bool Friend
        {
            get { return friend; }
            set
            {
                friend = value;
                NotifyPropertyChanged();
            }
        }
    }

    public class FriendToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((bool)value)
            {
                case true:
                    return "green";
                case false:
                    return "none";
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch (value.ToString().ToLower())
            {
                case "green":
                    return true;
                case "red":
                    return false;
            }
            return false;
        }
    }

}