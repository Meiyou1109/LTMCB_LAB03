using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Bai04
{
    public partial class frm_Client : Form
    {
        public frm_Client()
        {
            InitializeComponent();
            ConnectToServer();
            StartListening();
        }


        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);
        TcpClient tcpClient = new TcpClient();
        NetworkStream ns;
        bool isConnected = false;


        private void ConnectToServer()
        {
            try
            {
                if (!isConnected)
                {
                    tcpClient.Connect(ipEndPoint);
                    ns = tcpClient.GetStream();
                    isConnected = true;
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void StartListening()
        {
            Thread listenThread = new Thread(ListenForMessages);
            listenThread.IsBackground = true;
            listenThread.Start();
        }

        private void ListenForMessages()
        {
            try
            {
                byte[] buffer = new byte[1024];
                int bytesRead;

                while (isConnected)
                {
                    bytesRead = ns.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        UpdateChatList(receivedMessage.Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btn_Send_Click(object sender, EventArgs e)
        {
            try
            {
                if (!tcpClient.Connected)
                {
                    ConnectToServer();
                }

                string clientName = txt_Name.Text.Trim();
                string message = txt_Message.Text.Trim();
                if (string.IsNullOrEmpty(clientName) || string.IsNullOrEmpty(message)) return;

                string formattedMessage = $"{clientName}: {message}";
                byte[] data = Encoding.ASCII.GetBytes(formattedMessage + "\n");
                if (ns != null && tcpClient.Connected)
                {
                    ns.Write(data, 0, data.Length);
                    lst_Chat.Items.Add(new ListViewItem(formattedMessage));
                    txt_Message.Clear();
                }
                else
                {
                    MessageBox.Show("Connection is not available. Please reconnect.");
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void frm_Client_FormClosing(object sender, FormClosingEventArgs e)
        {
            tcpClient.Close();
            if (ns != null) ns.Close();
        }

        public void UpdateChatList(string message)
        {
            if (lst_Chat.InvokeRequired)
            {
                lst_Chat.Invoke(new Action(() => lst_Chat.Items.Add(new ListViewItem(message))));
            }
            else
            {
                lst_Chat.Items.Add(new ListViewItem(message));
            }
        }
    }
}
