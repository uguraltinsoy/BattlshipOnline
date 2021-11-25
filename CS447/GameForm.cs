using SimpleTcp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Media;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace CS447
{
    public partial class GameForm : Form
    {
        static List<Buttons> myButtons = new List<Buttons>();
        static List<Buttons> enemyButtons = new List<Buttons>();
        static List<Ship> ships = new List<Ship>();
        static List<int> myShips = new List<int>();
        static List<int> enemyShip = new List<int>();
        static string myShipLoc = "loc";
        static string name, enemyName, type, ip;

        SimpleTcpServer server;
        SimpleTcpClient client;
        static string CLIENT_ID = "";
        static bool clientReady = false;
        static int scoreServer = 0;
        static int scoreClient = 0;
        static SoundPlayer waterPlayer = new SoundPlayer(Properties.Resources.water);
        static SoundPlayer shipPlayer = new SoundPlayer(Properties.Resources.ship);

        public GameForm(string n, string t, string i)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
            name = n;
            type = t;
            ip = i;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string IP = GetLocalIPv4(NetworkInterfaceType.Wireless80211);
            labelIP.Text = "GAME IP: " + IP;

            myNameLabel.Text = name;
            enemyNameLabel.Text = "";

            if (type.Equals("srvr"))
            {                
                server = new SimpleTcpServer(IP, 9000);
                server.Events.ClientConnected += Events_ClientConnected;
                server.Events.ClientDisconnected += Event_ClientDisconnected;
                server.Events.DataReceived += Events_DataServer;
                server.Start();
            }
            else if (type.Equals("clnt"))
            {
                labelIP.Visible = false;
                client = new SimpleTcpClient(ip, 9000);
                client.Events.DataReceived += Events_DataClient;
                client.Events.Connected += Events_Connected;
                client.Connect();

                buttonReady.Location = new Point(1283, 12);
                buttonStartGame.Visible = false;
            }


            MY();
            DESTROY();

            ships.Add(new Ship("Carrier", 'C', 5));
            ships.Add(new Ship("Battleship", 'B', 4));
            ships.Add(new Ship("Submarine", 'S', 3));
            ships.Add(new Ship("Destroyer ", 'D', 2));
            setLocShips();
        }

        public string GetLocalIPv4(NetworkInterfaceType _type)
        {
            string output = "";
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            output = ip.Address.ToString();
                        }
                    }
                }
            }
            return output;
        }

        private static void setLocShips()
        {
            Random rand = new Random();
            foreach (Ship sp in ships)
            {
                int length = sp.length;
                int xLoc = rand.Next(10);
                int yLoc = rand.Next(10);
                int booltype = rand.Next(2);
                bool isVertical = Convert.ToBoolean(booltype);

                while (!isVertical && (10 - yLoc) < length)
                {
                    yLoc = rand.Next(10);
                }
                while (isVertical && (10 - xLoc) < length)
                {
                    xLoc = rand.Next(10);
                }

                Buttons bt = myButtons[(xLoc * 10) + yLoc];

                bool checkFull = false;
                while (!checkFull)
                {
                    if (!isVertical)
                    {
                        xLoc = rand.Next(10);
                        while ((10 - yLoc) < length)
                        {
                            yLoc = rand.Next(10);
                        }
                    }
                    if (isVertical)
                    {
                        xLoc = rand.Next(10);
                        while ((10 - xLoc) < length)
                        {
                            xLoc = rand.Next(10);
                        }
                    }
                    //bt = myButtons[xLoc + yLoc].button;

                    checkFull = CheckFull(((xLoc * 10) + yLoc), isVertical, length);
                }

                if (isVertical)
                {
                    int id = (xLoc * 10) + yLoc;
                    for (int i = 0; i < length; i++)
                    {
                        bt = myButtons[id];
                        myShipLoc += (id + " ");
                        bt.button.Text = sp.letter.ToString();
                        bt.isFull = true;
                        bt.button.Font = new Font("Microsoft Sans Serif", 10, FontStyle.Bold);                        
                        bt.button.BackColor = Color.Cyan;
                        id += 10;
                    }
                }
                if (!isVertical)
                {
                    int id = (xLoc * 10) + yLoc;
                    for (int i = 0; i < length; i++)
                    {
                        bt = myButtons[id];
                        myShipLoc += (id + " ");
                        bt.button.Text = sp.letter.ToString();
                        bt.isFull = true;
                        bt.button.Font = new Font("Microsoft Sans Serif", 10, FontStyle.Bold);
                        bt.button.BackColor = Color.Cyan;
                        id += 1;
                    }
                }
            }
            myShipLoc = myShipLoc.Substring(0, myShipLoc.Length - 1);
        }

        private static bool CheckFull(int id, bool isVertical, int length)
        {
            bool fullOfLength = true;
            for (int i = 0; i < length; i++)
            {
                if (isVertical)
                {
                    int a = 10;
                    Buttons bt = myButtons[(id + (a * i))];
                    if (bt.isFull)
                    {
                        fullOfLength = false;
                        break;
                    }
                }
                if (!isVertical)
                {
                    Buttons bt = myButtons[(id + i)];
                    if (bt.isFull)
                    {
                        fullOfLength = false;
                        break;
                    }
                }
            }
            return fullOfLength;
        }

        private void Events_Connected(object sender, ClientConnectedEventArgs e)
        {
            client.Send("NAME " + name);
        }

        // SERVER READ
        private void Events_DataServer(object sender, DataReceivedEventArgs e)
        {
            string result = Encoding.UTF8.GetString(e.Data);
            if (result.Contains("NAME "))
            {
                buttonReady.Enabled = true;
                enemyNameLabel.Text = result.Substring(5);
                enemyName = result.Substring(5);
            }
            else if (result.Contains("attack"))
            {
                string[] res = result.Split(' ');

                Console.WriteLine(res[1] + " " + res[2]);
                int index = Convert.ToInt32(res[1]);
                bool isShot = myShips.Contains(index);
                if (isShot)
                {
                    myButtons[index].button.BackColor = Color.Red;
                    shipPlayer.Play();
                }
                else
                {
                    myButtons[index].button.BackColor = Color.Green;
                    waterPlayer.Play();
                }

                if (res[2].Equals("srvr")) enemyTable.Enabled = true;
                string[] score = res[3].Split(':');
                scoreServer = Convert.ToInt32(score[0]);
                scoreClient = Convert.ToInt32(score[1]);
                scoreLabel.Text = String.Format("{0}:{1}", scoreServer, scoreClient);
                if (scoreClient == 14)
                {
                    MessageBox.Show(enemyName);
                }
            } 
            else if (result.Contains("loc"))
            {
                string loc = result.Substring(3);
                string[] parseLoc = loc.Split(' ');
                foreach(string pos in parseLoc)
                {
                    enemyShip.Add(Convert.ToInt32(pos));                    
                }
                clientReady = true;
                if (clientReady && !buttonReady.Enabled) buttonStartGame.Enabled = true;
            }
        }

        // CLIENT READ
        private void Events_DataClient(object sender, DataReceivedEventArgs e)
        {
            string result = Encoding.UTF8.GetString(e.Data);            
            if (result.Contains("NAME "))
            {
                buttonReady.Enabled = true;
                enemyNameLabel.Text = result.Substring(5);
                enemyName = result.Substring(5);
            }
            else if (result.Contains("attack"))
            {
                string[] res = result.Split(' ');
                int index = Convert.ToInt32(res[1]);
                bool isShot = myShips.Contains(index);
                if (isShot)
                {
                    myButtons[index].button.BackColor = Color.Red;
                    shipPlayer.Play();
                }
                else
                {
                    myButtons[index].button.BackColor = Color.Green;
                    waterPlayer.Play();
                }
                if (res[2].Equals("clnt")) enemyTable.Enabled = true;
                string[] score = res[3].Split(':');
                scoreServer = Convert.ToInt32(score[0]);
                scoreClient = Convert.ToInt32(score[1]);
                scoreLabel.Text = String.Format("{0}:{1}", scoreServer, scoreClient);
                if (scoreServer == 14)
                {
                    MessageBox.Show(enemyName);
                }
            }
            else if (result.Contains("loc"))
            {
                string loc = result.Substring(3);
                string[] parseLoc = loc.Split(' ');
                foreach (string pos in parseLoc)
                {
                    enemyShip.Add(Convert.ToInt32(pos));
                }
            }
        }

        private void Events_ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            CLIENT_ID = e.IpPort.ToString();
            if (server.IsListening)
            {
                if (CLIENT_ID != "")
                    server.Send(CLIENT_ID, "NAME " + name);
            }
        }

        private void Event_ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            CLIENT_ID = "";
        }

        private void MY()
        {
            var rowCount = 10;
            var columnCount = 10;

            this.myTable.ColumnCount = columnCount;
            this.myTable.RowCount = rowCount;

            this.myTable.ColumnStyles.Clear();
            this.myTable.RowStyles.Clear();

            for (int i = 0; i < columnCount; i++)
            {
                this.myTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 / columnCount));
            }

            for (int i = 0; i < rowCount; i++)
            {
                this.myTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100 / rowCount));
            }

            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {

                    var button = new Button();
                    //button.Text = string.Format("{0}{1}", i, j);
                    button.Name = string.Format("my_button_{0}{1}", i, j);
                    button.BackColor = Color.White;
                    button.Dock = DockStyle.Fill;
                    button.Enabled = false;
                    myButtons.Add(new Buttons(button, false));
                    this.myTable.Controls.Add(button, j, i);
                }
            }
        }

        private void DESTROY()
        {
            var rowCount = 10;
            var columnCount = 10;

            this.enemyTable.ColumnCount = columnCount;
            this.enemyTable.RowCount = rowCount;

            this.enemyTable.ColumnStyles.Clear();
            this.enemyTable.RowStyles.Clear();

            for (int i = 0; i < columnCount; i++)
            {
                this.enemyTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 / columnCount));
            }
            for (int i = 0; i < rowCount; i++)
            {
                this.enemyTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100 / rowCount));
            }

            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    var button = new Button();
                    button.Text = "Fire";//string.Format("{0}{1}", i, j);
                    button.Name = string.Format("en_button_{0}{1}", i, j);
                    button.BackColor = Color.White;
                    button.Dock = DockStyle.Fill;
                    button.Click += enemyButton;
                    enemyButtons.Add(new Buttons(button, false));
                    this.enemyTable.Controls.Add(button, j, i);
                }
            }
        }

        private void GameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (type.Equals("srvr"))
            {
                server.Stop();
            }
            else if (type.Equals("clnt"))
            {
                client.Disconnect();
            }
        }

        // READY
        private void button1_Click(object sender, EventArgs e)
        {
            if (type.Equals("srvr"))
            {
                if (server.IsListening)
                {
                    if (CLIENT_ID != "")
                    {
                        string loc = myShipLoc.Substring(3);
                        string[] parseLoc = loc.Split(' ');
                        foreach (string pos in parseLoc)
                        {
                            myShips.Add(Convert.ToInt32(pos));
                        }                        
                        server.Send(CLIENT_ID, myShipLoc);
                        buttonReady.Enabled = false;
                        if (clientReady) buttonStartGame.Enabled = true;
                    }
                        
                }
            }
            else if (type.Equals("clnt"))
            {
                string loc = myShipLoc.Substring(3);
                string[] parseLoc = loc.Split(' ');
                foreach (string pos in parseLoc)
                {
                    myShips.Add(Convert.ToInt32(pos));
                }
                client.Send(myShipLoc);
                buttonReady.Enabled = false;
            }
        }

        // START GAME
        private void button2_Click(object sender, EventArgs e)
        {
            enemyTable.Enabled = true;
            buttonStartGame.Enabled = false;
        }

        private void enemyButton(object sender, EventArgs e)
        {
            var btn = sender as Button;
            btn.Enabled = false;
            string attack = (btn.Name).Substring(10);
            int index = Convert.ToInt32(attack);

            bool isShot = enemyShip.Contains(index);
            if (isShot)
            {
                btn.BackColor = Color.Red;
                shipPlayer.Play();
            }
            else { 
                btn.BackColor = Color.Green; 
                waterPlayer.Play(); 
            }

            string score = String.Format("{0}:{1}", scoreServer, scoreClient);

            if (type.Equals("srvr"))
            {
                if (server.IsListening)
                {
                    if (CLIENT_ID != "")
                    {
                        if (isShot)
                        {
                            score = (scoreServer + 1) + ":" + scoreClient;
                            scoreLabel.Text = String.Format("{0}:{1}", scoreServer + 1, scoreClient);                            
                        }
                        server.Send(CLIENT_ID, String.Format("attack {0} clnt {1}", index, score));
                        if (isShot && (scoreServer + 1) == 14) {
                            MessageBox.Show(name);
                        }
                    }      
                }
            }
            else if (type.Equals("clnt"))
            {
                if (isShot)
                {
                    score = scoreServer + ":" + (scoreClient + 1);
                    scoreLabel.Text = String.Format("{0}:{1}", scoreServer, scoreClient + 1);                    
                }
                client.Send(String.Format("attack {0} srvr {1}", index, score));
                if (isShot && (scoreClient + 1) == 14) {
                    MessageBox.Show(name);
                }
            }

            enemyTable.Enabled = false;
        }
    }
}