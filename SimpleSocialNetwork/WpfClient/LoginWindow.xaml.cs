using System;
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
                wpf_app.main_window.initUserList(wpf_app);
                wpf_app.main_window.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Login Failed.");
            }
            
        }

        private void lblRegister_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Hide();
            RegisterWindow register = new RegisterWindow();
            register.Owner = this;
            Nullable<bool> dialogResult = register.ShowDialog();
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
