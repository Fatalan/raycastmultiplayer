using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestOnlineRayCasterClient
{
    public partial class MainMenu : Form
    {
        public MainMenu()
        {
            InitializeComponent();
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            GameForm gameForm = new GameForm(serverIpTextBox.Text.Split(':')[0], int.Parse(serverIpTextBox.Text.Split(':')[1]));
            gameForm.Show();
            //this.Close();
        }
    }
}
