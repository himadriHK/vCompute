using CommAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace vComputeClient
{
    public partial class Server_form : Form
    {
        public Server_form()
        {
            InitializeComponent();
        }

        private void btnServerStart_Click(object sender, EventArgs e)
        {
            Server server = new Server(8080);
        }
    }
}
