using Program;
using System;
using System.Net;
using System.Net.Mail;
using System.Windows.Forms;

namespace Server
{
    public partial class SMTP_window : Form
    {
        SmtpClient smtp_client = null;
        ServerApp app_reference = null;

        public SMTP_window(ServerApp app)
        {
            InitializeComponent();
            app_reference = app;
            ShowDialog();
        }

        private void btn_confirm_Click(object sender, EventArgs e)
        {
            if (txtbox_gmail.Text != null && txtbox_password.Text != null)
            {
                SetupSmtpClientOverGoogle(txtbox_gmail.Text, txtbox_password.Text);

                if (!VerifySmtpClient(txtbox_gmail.Text))
                    MessageBox.Show("The selected gmail could not be assigned to SMTP client.");
                else
                    app_reference.SetSMTP_client(smtp_client);
            }
            else
                MessageBox.Show("Invalid input. Gmail and/or password can't be empty.");
            this.Close();
        }

        public void SetupSmtpClientOverGoogle(string mail, string password)
        {
            smtp_client = new SmtpClient();

            smtp_client.EnableSsl = true;
            smtp_client.UseDefaultCredentials = false;
            smtp_client.Credentials = new NetworkCredential(mail, password);
            smtp_client.Host = "smtp.gmail.com";
            smtp_client.Port = 587;
            smtp_client.DeliveryMethod = SmtpDeliveryMethod.Network;

        }

        public bool VerifySmtpClient(string suggested_email)
        {
            try
            {
                MailMessage msg = new MailMessage();

                msg.From = new MailAddress("server.test@gmail.com", "Verification of SMTP client");
                msg.To.Add(suggested_email);
                msg.Subject = "";
                msg.Body = "";

                smtp_client.Send(msg);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
