using System;
using System.ComponentModel;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private App wpf_app = null;
        private ServerAvailability server_status = new ServerAvailability();
        public ServerAvailability Server_status
        {
            get{return server_status;}
            set{server_status = value;}
        }

        private NetworkAvailability network_status = new NetworkAvailability();
        public NetworkAvailability Network_status
        {
            get{return network_status;}
            set{network_status = value;}
        }

        public LoginWindow()
        {
            InitializeComponent();
            wpf_app = (App)Application.Current;
            CheckNetworkStatus();
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            if (wpf_app.LoginToServer(this.txtLogin.Text, this.txtPassword.Password))
            {
                Thread.Sleep(100);
                wpf_app.main_window.InitUserList();
                wpf_app.main_window.InitWall();
                wpf_app.main_window.Show();
                this.Hide();
            }
            else
                MessageBox.Show("Login was denied.");
        }

        public void SetNetworkAvailability(bool val)
        {
            Dispatcher.Invoke(new Action(delegate ()
            {
                network_status.Network_availability = val;
            }
            ));
        }

        public void SetServerAvailability(bool val)
        {
            Dispatcher.Invoke(new Action(delegate ()
            {
                server_status.Server_availability = val;
            }
            ));
        }

        public void CheckNetworkStatus()
        {
            if (NetworkInterface.GetIsNetworkAvailable())
                SetNetworkAvailability(true);
            else
                SetNetworkAvailability(true);
        }

        private void lblRegister_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Hide();
            RegisterWindow register = new RegisterWindow();
            register.Owner = this;
            bool? dialogResult = register.ShowDialog();
            Show();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Close all threads in app
            wpf_app.AppShutdown();
            Environment.Exit(0);
            base.OnClosing(e);
        }


    }

    public class NetworkAvailability : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool network_availability;

        public bool Network_availability
        {
            get { return network_availability; }
            set
            {
                network_availability = value;
                NotifyPropertyChanged();
            }
        }
    }

    public class ServerAvailability : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool server_availability;

        public bool Server_availability
        {
            get { return server_availability; }
            set
            {
                server_availability = value;
                NotifyPropertyChanged();
            }
        }
    }

    public class BoolToStringConverter : IValueConverter
    {
        public string TrueString { get; set; }
        public string FalseString { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return TrueString;
            else
                return FalseString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

 
}
