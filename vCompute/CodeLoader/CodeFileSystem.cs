using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeLoader
{
	[Serializable]
	public class CodeFileSystem
	{
		private Dictionary<string, byte[]> codeDictionary;

		public CodeFileSystem()
		{
			codeDictionary = new Dictionary<string, byte[]>();
		}

		public int writeAssembly(string assemblyName, byte[] codeBytes)
		{
			codeDictionary.Add(assemblyName, codeBytes);
			return codeBytes.Length;
		}

		public byte[] readAssembly(string assemblyName)
		{
			if (codeDictionary.ContainsKey(assemblyName))
				return codeDictionary[assemblyName];
			else
				return new byte[] { };
		}
	}
}
