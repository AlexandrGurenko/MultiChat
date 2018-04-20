using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MultiChat
{
    public partial class Form1 : Form
    {
        int port;
        IPAddress ip;
        IPEndPoint ep;
        Socket senderSocket;
        Thread listenThread;

        public Form1()
        {
            InitializeComponent();
            port = 9005;
            ip = IPAddress.Parse("224.7.7.7");
            ep = new IPEndPoint(ip, port);
            try
            {
                senderSocket = new Socket(AddressFamily.InterNetwork,
                                    SocketType.Dgram,
                                    ProtocolType.Udp);

                senderSocket.SetSocketOption(SocketOptionLevel.IP,
                                       SocketOptionName.MulticastTimeToLive, 1);

                senderSocket.SetSocketOption(SocketOptionLevel.IP,
                                       SocketOptionName.AddMembership,
                                       new MulticastOption(ip));

                listenThread = new Thread(new ThreadStart(Listen));
                listenThread.IsBackground = true;
            }catch(Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void Listen()
        {
            try
            {
                while (true)
                {
                    Socket receiverSocket = new Socket(AddressFamily.InterNetwork,
                                                       SocketType.Dgram,
                                                       ProtocolType.Udp);

                    IPEndPoint epx = new IPEndPoint(IPAddress.Any, 9005);
                    receiverSocket.Bind(epx);

                    receiverSocket.SetSocketOption(SocketOptionLevel.IP,
                                       SocketOptionName.AddMembership,
                                       new MulticastOption(ip, IPAddress.Any));

                    byte[] buff = new byte[1024];
                    receiverSocket.Receive(buff);

                    string mess = Encoding.UTF8.GetString(buff);
                    chatView.Text += mess + "\r\n";
                    receiverSocket.Close();
                }
            }
            catch (Exception err)
            {
                MessageBox.Show("Ошибочка: " + err.Message);
            }
        }

        private void SendMessage(string mess)
        {
            senderSocket.Send(Encoding.UTF8.GetBytes(mess));
        }

        private void connectBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (!senderSocket.Connected)
                {
                    senderSocket = new Socket(AddressFamily.InterNetwork,
                                    SocketType.Dgram,
                                    ProtocolType.Udp);

                    senderSocket.SetSocketOption(SocketOptionLevel.IP,
                                           SocketOptionName.MulticastTimeToLive, 1);

                    senderSocket.SetSocketOption(SocketOptionLevel.IP,
                                           SocketOptionName.AddMembership,
                                           new MulticastOption(ip));
                    listenThread = new Thread(new ThreadStart(Listen));
                    listenThread.IsBackground = true;

                    senderSocket.Connect(ep);
                    connectedState.ForeColor = Color.Green;
                    connectedState.Text = "Connected";
                    listenThread.Start();
                    SendMessage($"{DateTime.Now.ToShortTimeString()}: {userNameBox.Text} присоединился к уютному чатику:)\r\n");
                }

            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void disconnectBtn_Click(object sender, EventArgs e)
        {
            SendMessage($"{DateTime.Now.ToShortTimeString()}: {userNameBox.Text} уходит от нас:(\r\n");
            senderSocket.Close();
            senderSocket.Dispose();
            connectedState.ForeColor = Color.Red;
            connectedState.Text = "Disconnected";
        }

        private void sendBtn_Click(object sender, EventArgs e)
        {
            if (senderSocket.Connected)
            {
                if (messBox.Text != "")
                    SendMessage($"{DateTime.Now.ToShortTimeString()}: {userNameBox.Text}: {messBox.Text}\r\n");
                messBox.Clear();
                messBox.Focus();
            }
        }
    }
}
