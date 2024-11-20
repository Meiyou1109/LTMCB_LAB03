using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Bai04
{
    public partial class frm_Server : Form
    {
        public frm_Server()
        {
            InitializeComponent();
        }

        bool isListening = false;
        IPEndPoint ipepServer = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);

        Thread serverThread;
        Socket listenerSocket;

        List<Socket> clientSockets = new List<Socket>();

        private void StartUnsafeThread()
        {
            listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenerSocket.Bind(ipepServer);
            listenerSocket.Listen(-1);

            Invoke(new Action(() =>
            {
                lsv_Messages.Items.Add("Server is running on " + ipepServer.ToString());
            }));

            while (true)
            {
                Socket clientSocket = listenerSocket.Accept();
                clientSockets.Add(clientSocket);
                Invoke(new Action(() =>
                {
                    lsv_Messages.Items.Add("New client connected from: " + clientSocket.RemoteEndPoint.ToString());
                }));

                Thread clientThread = new Thread(() => HandleClientCommunication(clientSocket));
                clientThread.IsBackground = true;
                clientThread.Start();
            }
        }

        private void HandleClientCommunication(Socket clientSocket)
        {
            int bytesReceived;
            byte[] recvBuffer = new byte[1024];
            string message = "";

            while (clientSocket.Connected)
            {
                bytesReceived = clientSocket.Receive(recvBuffer);
                message += Encoding.ASCII.GetString(recvBuffer, 0, bytesReceived);

                if (message.Contains("\n"))
                {
                    string fullMessage = message.Trim();
                    message = "";
                    string[] splitMessage = fullMessage.Split(new[] { ':' }, 2);
                    if (splitMessage.Length == 2)
                    {
                        string clientName = splitMessage[0].Trim();
                        string clientMessage = splitMessage[1].Trim();
                        string messageToSend = $"{clientName}: {clientMessage}";
                        BroadcastMessage(clientSocket, messageToSend);
                        Invoke(new Action(() =>
                        {
                            string clientEndPoint = clientSocket.RemoteEndPoint.ToString();
                            lsv_Messages.Items.Add($"{clientEndPoint}: {messageToSend}");
                        }));
                    }
                }
            }

            clientSocket.Close();
        }

        private void BroadcastMessage(Socket senderSocket, string message)
        {
            foreach (var client in clientSockets)
            {
                try
                {
                    if (client != senderSocket)
                    {
                        byte[] data = Encoding.ASCII.GetBytes(message + "\n");
                        client.Send(data);
                    }
                }
                catch
                {
                    continue;
                }
            }
        }

        private void btn_Listen_Click(object sender, EventArgs e)
        {
            if (isListening) return;
            isListening = true;

            serverThread = new Thread(StartUnsafeThread);
            serverThread.IsBackground = true;
            serverThread.Start();
        }

        private void frm_Server_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (listenerSocket != null) listenerSocket.Close();
            foreach (var client in clientSockets)
            {
                client.Close();
            }
            if (serverThread != null) serverThread.Abort();
        }
    }
}
