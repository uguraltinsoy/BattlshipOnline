using System;
using System.Windows.Forms;

namespace CS447
{
    public partial class StartForm : Form
    {
        public StartForm()
        {
            InitializeComponent();
        }

        // CREATE
        private void button1_Click(object sender, EventArgs e)
        {
            string name = textBox1.Text;
            if (!name.Equals(""))
            {
                this.Hide();
                GameForm gameForm = new GameForm(name, "srvr", "");           
                gameForm.ShowDialog();
                this.Close();
            }
            else
            {
                MessageBox.Show("Please enter name");
            }
        }

        // CONNECTION
        private void button2_Click(object sender, EventArgs e)
        {
            string name = textBox1.Text;
            string ip = textBox2.Text.Trim();
            if (!name.Equals("") && !ip.Equals(""))
            {
                this.Hide();
                GameForm gameForm = new GameForm(name, "clnt", ip);                
                gameForm.ShowDialog();
                this.Close();
            }
            else
            {
                MessageBox.Show("Please enter name");
            }
        }
    }
}
