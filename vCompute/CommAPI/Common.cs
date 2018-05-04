using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Text;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using CodeLoader;
using System.Security;
using System.Security.Policy;
using System.Diagnostics;

namespace CommAPI
{
	public enum CommandType { REQUEST = 101,APPEND_REQUEST, DOWNLOAD, EXECUTE,EXECUTE_APPEND, DELETE, PING, REGISTER_CLIENT, REGISTER_ASSEMBLY, UPLOAD_ASSEMBLY,RESULT, APPEND_ASSEMBLY,APPEND_RESULT, STATUS, DISCOVERY }
	public class Common
	{
		
		private const int payloadSize = 1024;
		private const double assemblySize = 1000.0;
		private Dictionary<int, Payload> TaskList;
		public Loader codeLoader;
		private int timeOut=30;

		public Common(string codeBinaryFilePath)
		{
			codeLoader = new Loader(codeBinaryFilePath);
			TaskList = new Dictionary<int, Payload>();
		}

		public string executeAssembly(string assemblyName, string param)
		{
			return executeAssembly(assemblyName, new JavaScriptSerializer().Deserialize<object>(param));
		}

		public string executeAssembly(string assemblyName,object param)
		{
			Evidence e = new Evidence();
			e.AddHostEvidence(new Zone(SecurityZone.Intranet));
			PermissionSet pset = SecurityManager.GetStandardSandbox(e);

			AppDomainSetup ads = new AppDomainSetup();
			ads.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
			AppDomain tempDomain = AppDomain.CreateDomain("assemblyName" + new Random().NextDouble().ToString(), e, ads, pset, null);

			if (assemblyName.ToLower() == "discovery")
				return string.Join(",", codeLoader.codeDictionary.getAssemblyList());

			byte[] assemblyBinary = codeLoader.codeDictionary.readAssembly(assemblyName);
			Assembly asm = tempDomain.Load(assemblyBinary);
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

		public void storeAssembly(string assemblyName, byte[] assemblyBinary,bool append=false,int payloadsRemaining=0)
		{
			codeLoader.codeDictionary.writeAssembly(assemblyName, assemblyBinary, payloadsRemaining);
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

		public string[] splitSerializedData(string data)
		{
			string[] splitData= new string[(int)(Math.Ceiling((data.Length / assemblySize)))];
			char[] tempData = data.ToCharArray();

			for (double i = 0, j = 0; i < splitData.Length && j <= tempData.Length; i++, j += assemblySize)
			{
				char[] tempCharData= new char[Math.Min((int)assemblySize, data.Length - (int)j)];
				Array.Copy(tempData, (int)j, tempCharData, 0, Math.Min((int)assemblySize, tempData.Length - (int)j));
				splitData[(int)i] = new string(tempCharData);
			}
			return splitData;
		}

		public object blockUntilResult(int runId)
		{
			Payload resultPayload = TaskList[runId];
			Stopwatch timer = new Stopwatch();
			timer.Start();
			while(true)
			{
				if (resultPayload == null || resultPayload.isAppend == true)
					continue;

				if (resultPayload.isAppend == false && resultPayload.remainingPayloads == 0 && timer.Elapsed==new TimeSpan(0,0,timeOut))
					break;
			}
			timer.Stop();
			if (resultPayload != null && resultPayload.isAppend == false && resultPayload.remainingPayloads == 0)
				return new JavaScriptSerializer().Deserialize<object>(resultPayload.jsonOutput);
			else
				return null;
		}

		public void storeResult(string assemblyName, int runId, string jsonOutput, bool isAppend, int remainingPayloads)
		{
			lock (TaskList)
			{
				Payload partialResult = TaskList[runId];
				if (partialResult == null)
				{
					partialResult = new Payload();
					partialResult.assemblyName = assemblyName;
					partialResult.runId = runId;
					partialResult.jsonOutput = jsonOutput;
					partialResult.isAppend = isAppend;
					partialResult.remainingPayloads = remainingPayloads;
				}
				else
				{
					partialResult.jsonOutput += jsonOutput;
					partialResult.isAppend = isAppend;
					partialResult.remainingPayloads = remainingPayloads;
				}

				TaskList[runId] = partialResult;
			}
		}

		public void addToTaskList(int runId)
		{
			lock(TaskList)
			TaskList.Add(runId, null);
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
		public int remainingPayloads;
		public int runId;
		public bool isAppend;
		public string assemblyName;
		public byte[] assemblyBytes;
		public string jsonParameters;
		public double cpuUsage;
		public double memUsage;
		public double responseTime;
		public string jsonOutput;
		public DateTime? serverTime;
		public DateTime? clientTime;
	}

	public class sortPayload:IComparer<Payload>
	{
		public int Compare(Payload x, Payload y)
		{
			return x.remainingPayloads.CompareTo(y.remainingPayloads);
		}
	}
}
