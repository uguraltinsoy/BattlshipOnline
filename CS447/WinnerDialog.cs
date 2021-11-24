using System;
using System.Windows.Forms;

namespace CS447
{
    public partial class WinnerDialog : Form
    {
        static string winName;
        public WinnerDialog(string name)
        {
            winName = name;
            InitializeComponent();
        }

        private void WinnerDialog_Load(object sender, EventArgs e)
        {
            label1.Text = "Winner " + winName;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            StartForm startForm = new StartForm();
            startForm.ShowDialog();
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
