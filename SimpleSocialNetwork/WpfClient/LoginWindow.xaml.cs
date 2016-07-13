using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.LoginToServer(this.txtLogin.Text,this.txtPassword.Password)) {
                MainWindow main = new MainWindow();
                main.Show();
                this.Close();
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ((App)Application.Current).App_Shutdown();
        }
    }
}
