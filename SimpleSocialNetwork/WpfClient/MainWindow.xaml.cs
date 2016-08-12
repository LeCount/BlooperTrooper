using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace WpfClient
{
    /// <summary>Interaction logic for MainWindow.xaml</summary>
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

        public void InitUserList()
        {
            lbUserList.ItemsSource = wpf_app.session.users_list;
            wpf_app.RequestAllAvailableUsers();
        }

        public void RefreshUserList()
        {
            Dispatcher.Invoke(new Action(delegate ()
            {
                lbUserList.Items.Refresh();
            }
            ));
        }

        /// <summary>Initialize and populate the wall</summary>
        public void InitWall()
        {
            lbWall.ItemsSource = wpf_app.session.wall;

            RefreshWall();
        }

        /// <summary>Refresh wall</summary>
        public void RefreshWall()
        {
            Dispatcher.Invoke(new Action(delegate ()
            {
                lbWall.Items.Refresh();
            }
            ));
        }

        private void lbUserList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lbx = sender as ListBox;

            if(lbUserList.SelectedItem != null)
            {
                wpf_app.GetWallFromUser(((UserSimple)lbUserList.SelectedItem).Username);
                txtWall.Text = "Wall of " + ((UserSimple)lbUserList.SelectedItem).Username;
            }
        }

        private void btnAddFriend_Click(object sender, RoutedEventArgs e)
        {
            if(lbUserList.SelectedItem != null)
            {
                string selected_user = ((UserSimple)lbUserList.SelectedItem).Username;
                wpf_app.AddFriendRequest(selected_user);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            wpf_app.RequestAllAvailableUsers();
            wpf_app.GetWallFromUser(((UserSimple)lbUserList.SelectedItem).Username);
        }

        private void btnStatusSubmit_Click(object sender, RoutedEventArgs e)
        {
            wpf_app.AddStatusMessage(txtStatus.Text, ((UserSimple)lbUserList.SelectedItem).Username);
        }

        private void btnStartChat_Click(object sender, RoutedEventArgs e)
        {
            string selected_user = ((UserSimple)lbUserList.SelectedItem).Username;
            wpf_app.StartChat(selected_user);
        }
    }
}