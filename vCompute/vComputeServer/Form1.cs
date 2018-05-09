using CommAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace vComputeServer
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

        private void btnStartClient_Click(object sender, EventArgs e)
        {
            Client client = new Client("localhost", 8080);
            client.registerClient();
            Thread.Sleep(1000);
            lblclientID.Text = client.clientId;

        }
    }
}
