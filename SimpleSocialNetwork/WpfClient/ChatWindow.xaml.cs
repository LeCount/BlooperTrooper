using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        private App wpf_app = null;
        private string username_chatter;

        public ChatWindow(string name)
        {
            InitializeComponent();
            wpf_app = (App)Application.Current;
            username_chatter = name;
            listbox_chat_log.ItemsSource = (ObservableCollection<ChatMessage>)wpf_app.chat_conversations[name];
        }


        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (txtbox_chat.Text.Length != 0)
                wpf_app.SendChatMessage(txtbox_chat.Text, username_chatter);
            txtbox_chat.Clear();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            wpf_app.RemoveConversation(username_chatter);
            base.OnClosing(e);
        }

        private void txtbox_chat_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter) return;

            // your event handler here
            e.Handled = true;
            if(txtbox_chat.Text.Length != 0)
                wpf_app.SendChatMessage(txtbox_chat.Text, username_chatter);
            txtbox_chat.Clear();
        }
    }
}
