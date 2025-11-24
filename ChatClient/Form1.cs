using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;
using System.Drawing;

namespace ChatClient
{
    public partial class Form1 : Form
    {
        private TcpClient client;
        private NetworkStream stream;
        private Task receiveTask;

        public Form1()
        {
            InitializeComponent();
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtServerIp.Text) || string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Vui long nhap IP Server va ten ban.");
                return;
            }

            try
            {
                client = new TcpClient();
                await client.ConnectAsync(txtServerIp.Text, 9001);
                stream = client.GetStream();

                string username = txtUsername.Text;
                byte[] userMessage = AesEncryption.Encrypt($"USER:{username}");
                await stream.WriteAsync(userMessage, 0, userMessage.Length);

                receiveTask = Task.Run(ReceiveMessages);

                btnConnect.Enabled = false;
                txtServerIp.Enabled = false;
                txtUsername.Enabled = false;
                rtbChatBox.AppendText("Da ket noi den server.\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Khong the ket noi: {ex.Message}");
                if (client != null) client.Close();
            }
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            if (client == null || !client.Connected || string.IsNullOrWhiteSpace(txtMessage.Text))
            {
                MessageBox.Show("Vui long ket noi va nhap tin nhan.");
                return;
            }

            try
            {
                string message = txtMessage.Text;
                byte[] chatMessage = AesEncryption.Encrypt($"MSG:{message}");
                await stream.WriteAsync(chatMessage, 0, chatMessage.Length);

                AppendMessage($"Toi: {message}");
                txtMessage.Clear();
            }
            catch (Exception ex)
            {
                AppendMessage($"Loi khi gui: {ex.Message}");
            }
        }

        private async Task ReceiveMessages()
        {
            byte[] buffer = new byte[4096];
            try
            {
                while (client.Connected)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string decryptedMessage = AesEncryption.Decrypt(buffer.Take(bytesRead).ToArray());

                    if (decryptedMessage.StartsWith("MSG:"))
                    {
                        AppendMessage(decryptedMessage.Substring(4));
                    }
                    else if (decryptedMessage.StartsWith("USERLIST:"))
                    {
                        string[] users = decryptedMessage.Substring(9).Split(',');
                        UpdateUserList(users);
                    }
                }
            }
            catch (Exception ex)
            {
                AppendMessage($"Mat ket noi: {ex.Message}");
            }
            finally
            {
                if (client != null) client.Close();
            }
        }

        private void AppendMessage(string message)
        {
            if (rtbChatBox.InvokeRequired)
            {
                rtbChatBox.Invoke(new Action(() => AppendMessage(message)));
            }
            else
            {
                rtbChatBox.AppendText(message + "\n");
            }
        }

        private void UpdateUserList(string[] users)
        {
            if (lstUsers.InvokeRequired)
            {
                lstUsers.Invoke(new Action(() => UpdateUserList(users)));
            }
            else
            {
                lstUsers.Items.Clear();
                lstUsers.Items.AddRange(users);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (client != null)
            {
                client.Close();
            }
        }

        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                btnSend_Click(sender, e);
            }
        }
    }
}
