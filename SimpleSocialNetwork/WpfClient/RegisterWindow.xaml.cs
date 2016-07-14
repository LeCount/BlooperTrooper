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
    /// Interaction logic for RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            
            InitializeComponent();
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (App.JoinRequest(txtUsername.Text, txtPassword.Password, txtEmail.Text, txtFirstName.Text, txtLastName.Text, txtAbout.Text, txtInterests.Text))
            {
                Close();
            }
            
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {

            Close();
        }
    }
}
