using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace vComputeClient
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{

		}


		public byte[] ComplieAssembly(string sourceFile)
		{

			var provider = CodeDomProvider.CreateProvider("CSharp");
			CompilerParameters cp = new CompilerParameters();
			cp.GenerateExecutable = false;
			cp.GenerateInMemory = true;
			cp.ReferencedAssemblies.Add("CodeLoader.dll");
			
			CompilerResults result=provider.CompileAssemblyFromSource(cp, sourceFile);
			if (result.Errors.HasErrors)
			{
				MessageBox.Show("Errors: "+result.Errors[0]);
				return null;
			}
			else
			{
				using (MemoryStream stream = new MemoryStream())

				{

					BinaryFormatter formatter = new BinaryFormatter();

					formatter.Serialize(stream, result.CompiledAssembly);

					return stream.ToArray();

				}
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			var sourceFile = textBox1.Text;
			var compliedCodeInByte = ComplieAssembly(sourceFile);

		}
	}
}
