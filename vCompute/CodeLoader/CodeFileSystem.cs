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
		private Dictionary<string, int> codeStoreStatus;

		public CodeFileSystem()
		{
			codeDictionary = new Dictionary<string, byte[]>();
			codeStoreStatus = new Dictionary<string, int>();
		}

		public int writeAssembly(string assemblyName, byte[] codeBytes,int payloadsRemaining)
		{
			if (!codeDictionary.ContainsKey(assemblyName))
			{
				codeDictionary.Add(assemblyName, codeBytes);
				codeStoreStatus.Add(assemblyName, payloadsRemaining-1);
				return codeBytes.Length;
			}
			else if(codeStoreStatus[assemblyName] >= 0 && codeStoreStatus[assemblyName]==payloadsRemaining)
			{
				byte[] temp=codeDictionary[assemblyName];
				Array.Resize<byte>(ref temp, codeDictionary[assemblyName].Length + codeBytes.Length);
				Array.Copy(codeBytes, 0, temp, codeDictionary[assemblyName].Length, codeBytes.Length);
				codeDictionary[assemblyName] = temp;
				codeStoreStatus[assemblyName]--;
				return codeBytes.Length;
			}
			return 0;
		}

		public byte[] readAssembly(string assemblyName)
		{
			if (codeDictionary.ContainsKey(assemblyName)&& codeStoreStatus.ContainsKey(assemblyName)&& codeStoreStatus[assemblyName]==-1)
				return codeDictionary[assemblyName];
			else
				return new byte[] { };
		}

        public bool containsAssembly(string assemblyName)
        {
            if (codeDictionary.ContainsKey(assemblyName) && codeStoreStatus.ContainsKey(assemblyName) && codeStoreStatus[assemblyName] == -1)
                return true; else { return false; }
        }
        public string[] getAssemblyList()
		{
			return codeDictionary.Keys.ToArray<string>();
		}
	}
}
