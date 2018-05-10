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

namespace vComputeServer
{
    public enum MessageType {Success = 1, Error, Information };
    public partial class Server_form : Form
    {
        delegate void StringArgReturningVoidDelegate(string text, MessageType type);
        public Server_form()
        {
            InitializeComponent();
            this.logtxt.BackColor = Color.Black;
        }

        private void btnServerStart_Click(object sender, EventArgs e)
        {
            Server server = new Server(8080);
            Subscribe(server);
        }

        public void Subscribe(Server s)
        {
            s.registerClientEvent += new Server.RegisterClientHandler(OnClientRegistration);
           // s.updateStatusEvent += new Server.UpdateStatusHandler(OnUpdateStatus);
            s.executeEvent += new Server.ExecuteHandler(OnExecuteStatus);
            s.resultEvent += new Server.ResultHandler(OnResultStatus);
        }
        private void OnClientRegistration(RegisterClientEventArgs e)
        {
            SetText(string.Format("New Client {0} has been registered", e.ClientId), MessageType.Success);
        }
        private void OnUpdateStatus(UpdateStatusEventArgs e)
        {
            SetText(string.Format("Recieved Status Update from {0}. Load is {1}", e.ClientId, e.load), MessageType.Information);
        }

        private void OnExecuteStatus(ExectueEventArgs e)
        {
            SetText(string.Format("Sent execute request from {0} to {1}", e.FromClientId, e.ToClientId), MessageType.Success);
        }

        private void OnResultStatus(ResultEventArgs e)
        {
            SetText(string.Format("Recieved Result  from {0} for {1}", e.FromClientId, e.ToClientId), MessageType.Success);
        }

        private void Server_form_Load(object sender, EventArgs e)
        {

        }
        private void SetText(string text, MessageType type)
        {
            // InvokeRequired required compares the thread ID of the  
            // calling thread to the thread ID of the creating thread.  
            // If these threads are different, it returns true.  
            if (this.logtxt.InvokeRequired)
            {
                StringArgReturningVoidDelegate d = new StringArgReturningVoidDelegate(SetText);
                this.Invoke(d, new object[] { text, type });
            }
            else
            {
                this.logtxt.SelectionStart = this.logtxt.TextLength > 0 ? this.logtxt.TextLength : 0;
                if (type == MessageType.Success)
                    this.logtxt.SelectionColor = Color.Green;
                else if (type == MessageType.Information)
                    this.logtxt.SelectionColor = Color.White;
                else
                    this.logtxt.SelectionColor = Color.Red;
                text = text + System.Environment.NewLine;
                this.logtxt.AppendText(text);
            }
        }

    }
}
