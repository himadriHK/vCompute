using CommAPI;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace vComputeClient
{
	public partial class Form1 : Form
	{
        Client client;
        delegate void StringArgReturningVoidDelegate(string text);
        public Form1()
		{
			InitializeComponent();
		}

        public byte[] complieAssembly(string sourceFile)
        {

            var provider = CodeDomProvider.CreateProvider("CSharp");
            CompilerParameters cp = new CompilerParameters();
            cp.GenerateExecutable = false;
            cp.GenerateInMemory = false;
            cp.ReferencedAssemblies.Add("CodeLoader.dll");

            CompilerResults result = provider.CompileAssemblyFromSource(cp, sourceFile);
            if (result.Errors.HasErrors)
            {
                MessageBox.Show("Errors: " + result.Errors[0]);
                return null;
            }
            else
                return File.ReadAllBytes(result.PathToAssembly);
        }

        private void btnStartClient_Click(object sender, EventArgs e)
        {
            client = new Client(TxtHostName.Text, 8080);
            client.registerClient();
            Subscribe(client);
            TxtHostName.Enabled = false;
            btnStartClient.Enabled = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            btnStartClient.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            byte[] rawAssemblyBytes = complieAssembly(textBox1.Text);
            string assemblyName = textBox2.Text;

            if(rawAssemblyBytes!=null && rawAssemblyBytes.Length>0)
            client.uploadAssembly(assemblyName, rawAssemblyBytes);

            Debug.Print("Assembly Size : " + rawAssemblyBytes.Length);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //client.downloadAssembly(textBox2.Text);
            string paramValue = Microsoft.VisualBasic.Interaction.InputBox("Enter Parameter Value", "Input");
            MessageBox.Show(client.requestTask(textBox2.Text, paramValue).ToString());
        }

        private void TxtHostName_TextChanged(object sender, EventArgs e)
        {
            if (TxtHostName.Text.Trim().Length > 0)
                btnStartClient.Enabled = true;
        }
        public void Subscribe(Client c)
        {
            c.registerClientEvent += new Client.RegisterClientHandlerFromClient(OnClientRegistration);
        }
        private void OnClientRegistration(RegisterClientEventArgs e)
        {
            SetText(e.ClientId);
            
        }

        private void SetText(string text)
        {
            // InvokeRequired required compares the thread ID of the  
            // calling thread to the thread ID of the creating thread.  
            // If these threads are different, it returns true.  
            if (this.lblclientID.InvokeRequired)
            {
                StringArgReturningVoidDelegate d = new StringArgReturningVoidDelegate(SetText);
                this.Invoke(d, new object[] { text});
            }
            else
            {
                this.lblclientID.Text = text;
            }
        }
    }
}
