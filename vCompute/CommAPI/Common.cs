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
using System.Runtime.Remoting;

namespace CommAPI
{
	public enum CommandType { REQUEST = 101,APPEND_REQUEST, DOWNLOAD, EXECUTE, APPEND_EXECUTE, DELETE, PING, REGISTER_CLIENT, REGISTER_ASSEMBLY, UPLOAD_ASSEMBLY,RESULT, APPEND_ASSEMBLY,APPEND_RESULT, STATUS, DISCOVERY, CLIENT_REGISTRTION,NOTHING }
	public class Common
	{
		
		private const int payloadSize = 5120;
		private const double assemblySize = 200.0;
		private Dictionary<string, Payload> TaskList;
		public Loader codeLoader;
		private int timeOut=60;

		public Common(string codeBinaryFilePath)
		{
			codeLoader = new Loader(codeBinaryFilePath);
			TaskList = new Dictionary<string, Payload>();
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
			AppDomain tempDomain = AppDomain.CreateDomain(assemblyName + new Random().NextDouble().ToString(), e, ads, pset, null);

			tempDomain.Load("CodeLoader");

			if (assemblyName.ToLower() == "discovery")
				return string.Join(",", codeLoader.codeDictionary.getAssemblyList());

			ObjectHandle handle = Activator.CreateInstanceFrom(tempDomain, typeof(Sandboxer).Assembly.ManifestModule.FullyQualifiedName,typeof(Sandboxer).FullName);

			Sandboxer newDomainInstance = (Sandboxer)handle.Unwrap();

			byte[] assemblyBinary = codeLoader.codeDictionary.readAssembly(assemblyName);

			object output = newDomainInstance.ExecuteNewAssembly(assemblyBinary, param);

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
			return new JavaScriptSerializer().Serialize(payload).PadRight(payloadSize,'a');
		}

		public Payload preparePayload(string payload)
		{
            string temp = payload.Trim('\0').Trim('a');
            try
            {
                return new JavaScriptSerializer().Deserialize<Payload>(temp);
            }
            catch(Exception ex)
            {
                Payload output = new Payload();
                output.command = CommandType.NOTHING;
                return output;
            }
		}

		public void sendPacket(NetworkStream stream,Payload payload)
		{
			string serializedData = preparePayload(payload);
            if (string.IsNullOrEmpty(serializedData.Trim('a')))
                return;

			byte[] data = Encoding.UTF8.GetBytes(serializedData);
			stream.Write(data, 0, payloadSize);
			if(payload.command!=CommandType.STATUS)
			Debug.Print("Sending " + Enum.GetName(typeof(CommandType), payload.command));
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

		public object blockUntilResult(string runId)
		{
	
			Stopwatch timer = new Stopwatch();
			timer.Start();
			TimeSpan span = new TimeSpan(0, 0, timeOut);
			while (true)
			{
				Payload resultPayload = TaskList[runId];
				if (resultPayload == null || resultPayload.isAppend == true)
					continue;

				if (timer.Elapsed > span)
					return "";

				if (resultPayload.isAppend == false && resultPayload.remainingPayloads == 0 && timer.Elapsed <= span)
				{
					timer.Stop();
					return new JavaScriptSerializer().Deserialize<object>(resultPayload.jsonOutput);
				}
			}
		}

		public void storeResult(string assemblyName, string runId, string jsonOutput, bool isAppend, int remainingPayloads)
		{
			lock (TaskList)
			{
				Payload partialResult = null;
				if (TaskList.ContainsKey(runId))
					partialResult = TaskList[runId];

				if (partialResult == null)
				{
					partialResult = new Payload();
					partialResult.assemblyName = assemblyName;
					partialResult.runId = runId.ToString();
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

        public byte[] readAssembly(string assemblyName)
        {
            return codeLoader.codeDictionary.readAssembly(assemblyName);
        }

		public void addToTaskList(string runId)
		{
			lock(TaskList)
			TaskList.Add(runId, null);
		}

		public Payload getPacket(NetworkStream stream)
		{
				byte[] buffer = new byte[payloadSize];
                int readBytes = 0;
                readBytes=stream.Read(buffer, 0, payloadSize);
                Payload output;
                if (readBytes == payloadSize)
                {
                    string serializedData = Encoding.UTF8.GetString(buffer);
                    output = preparePayload(serializedData);
                }
                else
                {
                    Debug.Print("*******************NULLLL******");
                    output = new Payload();
                    output.command = CommandType.NOTHING;
                }
				if (output!=null && output.command != CommandType.STATUS)
					Debug.Print("Receiving " + Enum.GetName(typeof(CommandType), output.command));
				return output;
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
		public string runId;
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

	class Sandboxer : MarshalByRefObject
	{
		public object ExecuteNewAssembly(byte[] rawAssm, object param)
		{
			Assembly asm = Assembly.Load(rawAssm);
			CodeInterface assemblyObj = null;

			foreach (Type t in asm.GetTypes())
				if (t.GetInterface("CodeInterface") != null)
					assemblyObj = (CodeInterface)Activator.CreateInstance(t);

			object output = string.Empty;

			try
			{
				output = assemblyObj.DoWork(param);
			}
			catch (Exception ex)
			{
				output = ex.Message;
			}
			return output;
		}
	}
}
