using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace CodeLoader
{
	public class Loader
	{
		public CodeFileSystem codeDictionary { get; set; }
		IFormatter binaryFormatter;
		string codeFilePath;
		FileStream fs;
		public Loader(string path)
		{
			codeFilePath = path;
			reloadAssemblies();
		}

		public void reloadAssemblies()
		{
			binaryFormatter = new BinaryFormatter();
			try
			{
				fs = new FileStream(codeFilePath, FileMode.OpenOrCreate);
				codeDictionary = (CodeFileSystem)binaryFormatter.Deserialize(fs);
				//fs.Dispose();

			}
			catch (Exception)
			{
				codeDictionary = new CodeFileSystem();
			}
		}

		public void saveCodeDictionary()
		{
			//FileStream fs = new FileStream(codeFilePath, FileMode.Create);
			binaryFormatter.Serialize(fs, codeDictionary);

			//fs.Dispose();
		}
	}
}
