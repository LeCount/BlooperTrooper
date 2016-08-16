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
        ObservableCollection<ChatMessage> conversation = new ObservableCollection<ChatMessage>();
        private string username_chatter;

        public ChatWindow(string name)
        {
            InitializeComponent();
            wpf_app = (App)Application.Current;
            username_chatter = name;
            listbox_chat_log.ItemsSource = (ObservableCollection<ChatMessage>)conversation;
        }


        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (txtbox_chat.Text.Length != 0)
                wpf_app.SendChatMessage(txtbox_chat.Text, username_chatter);
            txtbox_chat.Clear();
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

        public void AddNewChatMessage(ChatMessage chatMessage)
        {
            Dispatcher.Invoke(new Action(delegate ()
            {
                conversation.Add(chatMessage);
            }
            ));
        }
    }
}
