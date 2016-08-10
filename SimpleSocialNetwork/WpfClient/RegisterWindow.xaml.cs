using System.Windows;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        private App wpf_app = null;

        public RegisterWindow()
        {
            InitializeComponent();
            wpf_app = (App)Application.Current;
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (wpf_app.RequestToJoinSocialNetwork( txtUsername.Text, 
                                                    txtPassword.Password, 
                                                    txtEmail.Text, 
                                                    txtFirstName.Text, 
                                                    txtLastName.Text, 
                                                    txtAbout.Text, 
                                                    txtInterests.Text))
                Close();
            
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
