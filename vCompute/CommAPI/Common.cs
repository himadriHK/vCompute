using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Text;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using CodeLoader;

namespace CommAPI
{
	public enum CommandType { REQUEST = 101, DOWNLOAD, EXECUTE, DELETE, PING, REGISTER_CLIENT, REGISTER_ASSEMBLY, UPLOAD_ASSEMBLY,RESULT, APPEND_ASSEMBLY, STATUS }
	public class Common
	{
		
		private const int payloadSize = 1024;
		private const double assemblySize = 5.0;
		public Loader codeLoader;

		public Common(string codeBinaryFilePath)
		{
			codeLoader = new Loader(codeBinaryFilePath);
		}

		public string executeAssembly(string assemblyName,object param)
		{
			byte[] assemblyBinary = codeLoader.codeDictionary.readAssembly(assemblyName);
			Assembly asm = Assembly.Load(assemblyBinary);
			CodeInterface assemblyObj = null;

			foreach (Type t in asm.GetTypes())
				if (t.GetInterface("CodeInterface") != null)
					assemblyObj = (CodeInterface)Activator.CreateInstance(t);

			object output = string.Empty;

			try
			{
				output = assemblyObj.DoWork(param);
			}
			catch(Exception ex)
			{
				output = ex.Message;
			}
			return new JavaScriptSerializer().Serialize(output);
		}

		public void storeAssembly(string assemblyName, byte[] assemblyBinary,bool append=false)
		{
			codeLoader.codeDictionary.writeAssembly(assemblyName, assemblyBinary,3);
			codeLoader.saveCodeDictionary();
			codeLoader.reloadAssemblies();
		}

		public string preparePayload(Payload payload)
		{
			return new JavaScriptSerializer().Serialize(payload).PadRight(payloadSize);
		}

		public Payload preparePayload(string payload)
		{
			return new JavaScriptSerializer().Deserialize<Payload>(payload.TrimEnd());
		}

		public void sendPacket(NetworkStream stream,Payload payload)
		{
			if(payload.assemblyBytes.Length>assemblySize)
			{
				byte[][] splittedArray = splitBytes(payload.assemblyBytes);
				//payload.assemblyBytes
					//Array.Copy()
			}
			string serializedData = preparePayload(payload);
			byte[] data = Encoding.ASCII.GetBytes(serializedData);

			lock (stream)
			{
				stream.Write(data, 0, payloadSize);
			}
		}
		public byte[][] splitBytes(byte[] assemblyBytes)
		{
			byte[][] splitBytes = new byte[(int)(Math.Ceiling((assemblyBytes.Length / assemblySize)))][];

			for (double i = 0, j = 0; i < splitBytes.GetLength(0) && j <= assemblyBytes.Length; i++, j += assemblySize)
			{
				splitBytes[(int)i] = new byte[Math.Min((int)assemblySize, assemblyBytes.Length - (int)j)];
				Array.Copy(assemblyBytes, (int)j, splitBytes[(int)i], 0, Math.Min((int)assemblySize, assemblyBytes.Length - (int)j));
			}

			return splitBytes;
		}
		public Payload getPacket(NetworkStream stream)
		{
			lock(stream)
			{
				byte[] buffer = new byte[payloadSize];
				stream.Read(buffer, 0, payloadSize);
				string serializedData = Encoding.ASCII.GetString(buffer);
				Payload output = preparePayload(serializedData);
				return output;
			}
		}
	}

	[Serializable]
	public class Payload
	{
		public CommandType command;
		public string serverId;
		public string clientId;
		public string payloadId;
		public string assemblyName;
		public byte[] assemblyBytes;
		public object parameters;
		public double cpuUsage;
		public double memUsage;
		public double responseTime;
		public string jsonOutput;
		public DateTime? serverTime;
		public DateTime? clientTime;
	}
}
