using System;
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
        private string name_of_chater;
        private Thread chat_msg_reader = null;
        private string new_message = null;

        public ChatWindow(string name)
        {
            InitializeComponent();
            wpf_app = (App)Application.Current;
            name_of_chater = name;

            chat_msg_reader = new Thread(GetNext);
            chat_msg_reader.Start();
        }

        private void GetNext()
        {
            while(true)
            {
                new_message = wpf_app.GetNextChatMsg(name_of_chater);

                if (new_message != null)
                    AddMessageToListbox(new_message);
                else
                    Thread.Sleep(1000);
            }
        }

        private void AddMessageToListbox(string msg)
        {
            Dispatcher.Invoke(new Action(delegate ()
            {
                listbox_chat_log.Items.Add(msg);
            }
            ));
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            wpf_app.SendChatMessage(txtbox_chat.Text, name_of_chater);
            wpf_app.AddChatMsgToConversation(txtbox_chat.Text, name_of_chater);
            txtbox_chat.Clear();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (chat_msg_reader.IsAlive)
                chat_msg_reader.Abort();

            base.OnClosing(e);
        }
    }
}
