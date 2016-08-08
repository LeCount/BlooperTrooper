using System.Windows;

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
            if (((App)Application.Current).RequestToJoinSocialNetwork(txtUsername.Text, txtPassword.Password, txtEmail.Text, txtFirstName.Text, txtLastName.Text, txtAbout.Text, txtInterests.Text))
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
