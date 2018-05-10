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
            client.downloadAssembly(textBox2.Text);
            string paramValue = Microsoft.VisualBasic.Interaction.InputBox("Enter Parameter Value", "Input");
            MessageBox.Show(client.requestTask(textBox2.Text, paramValue).ToString());
        }

        private void TxtHostName_TextChanged(object sender, EventArgs e)
        {
            if (TxtHostName.Text.Trim().Length > 0)
                btnStartClient.Enabled = true;
        }
        public void Subscribe(Server s)
        {
            s.registerClientEvent += new Server.RegisterClientHandler(OnClientRegistration);
        }
        private void OnClientRegistration(RegisterClientEventArgs e)
        {
            lblclientID.Text = e.ClientId;
            TxtHostName.Enabled = false;
            btnStartClient.Enabled = false;
        }
    }
}
