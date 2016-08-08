using System;
using System.Windows;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private App wpf_app = null;

        public MainWindow()
        {
            InitializeComponent();
            wpf_app = (App)Application.Current;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Close all threads in app
            wpf_app.AppShutdown();
            Environment.Exit(0);
            base.OnClosing(e);
        }

        private void btnLogOut_Click(object sender, RoutedEventArgs e)
        {
            wpf_app.LogoutServer();
            Hide();
        }

        public void initUserList(App application)
        {
            lbUserList.ItemsSource = application.session.users_list;
            application.RequestAllAvailableUsers();
            lbUserList.Items.Refresh();
        }

        public void RefreshUserList()
        {
            Dispatcher.Invoke(new Action(delegate ()
            {
                lbUserList.Items.Refresh();
            }
            ));
        }

    }
}
