using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private App wpf_app = null;

        public LoginWindow()
        {
            InitializeComponent();
            wpf_app = (App)Application.Current;
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            if (wpf_app.LoginToServer(this.txtLogin.Text,this.txtPassword.Password))
            {
                Thread.Sleep(100);
                wpf_app.main_window.InitUserList();
                wpf_app.main_window.InitWall();
                wpf_app.main_window.Show();
                this.Hide();
            }            
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
}
