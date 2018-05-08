using CodeLoader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace CommAPI
{
    public class Client
    {
		private string clientId;
		private string hostAddress;
		private int port;
		private TcpClient clientInstance;
		private NetworkStream networkDataStream;
		private Common commUtil;
		private int runID;
		private Dictionary<string, Payload> execData;

		//client constructor
		public Client(string host,int port,string clientKey)
		{
			hostAddress = host;
			this.port = port;
			clientId = clientKey;
			clientInstance = new TcpClient();
			commUtil = new Common(AppDomain.CurrentDomain.BaseDirectory + @"client.bin");
			runID = 1;
			execData = new Dictionary<string, Payload>();
			try
			{
			clientInstance.Connect(hostAddress, port);
			if(clientInstance.Connected)
			
			Debug.WriteLine("Client Connected");
			networkDataStream = clientInstance.GetStream();
			Thread pingThread = new Thread(pingHandler);
			Thread dispatcher = new Thread(serverMessageHandler);
			pingThread.Start();
			dispatcher.Start();
			}
			catch(Exception ex)
			{
			
			}
		}

		private void serverMessageHandler()
		{
			while (true)
			{
				if (networkDataStream.DataAvailable)
				{
					Payload payload = commUtil.getPacket(networkDataStream);
					if(payload.command!=CommandType.STATUS)
					Debug.Print("Printing Client " + payload.command);
					switch (payload.command)
					{
						case CommandType.DOWNLOAD:
						case CommandType.APPEND_ASSEMBLY:
							commUtil.storeAssembly(payload.assemblyName, payload.assemblyBytes, payload.isAppend, payload.remainingPayloads);
							break;

						case CommandType.EXECUTE:
						case CommandType.EXECUTE_APPEND:
							executeTask(payload);
							break;

						case CommandType.RESULT:
						case CommandType.APPEND_RESULT:
							commUtil.storeResult(payload.assemblyName, payload.runId, payload.jsonOutput, payload.isAppend, payload.remainingPayloads);
							break;
					}
				}
			}
		}

		public object requestTask(string assemblyName,object param)
		{
			string[] serializedParams = commUtil.splitSerializedData(new JavaScriptSerializer().Serialize(param));
			int runId= getNewRunID();

			Payload taskPayload = new Payload();
			taskPayload.command = CommandType.REQUEST;
			taskPayload.clientId = clientId;
			taskPayload.assemblyName = assemblyName;
			taskPayload.clientTime = DateTime.Now;
			taskPayload.jsonParameters = serializedParams[0];
			taskPayload.runId = runId.ToString();
			taskPayload.isAppend = serializedParams.Length > 1;
			taskPayload.remainingPayloads = serializedParams.Length - 1;
			commUtil.sendPacket(networkDataStream, taskPayload);
			
			for(int i=1;i<serializedParams.Length;i++)
			{
				Payload taskExtraPayload = new Payload();
				taskExtraPayload.command = CommandType.APPEND_REQUEST;
				taskExtraPayload.clientId = clientId;
				taskExtraPayload.assemblyName = assemblyName;
				taskExtraPayload.clientTime = DateTime.Now;
				taskExtraPayload.jsonParameters = serializedParams[i];
				taskExtraPayload.isAppend = (i != (serializedParams.Length - 1));
				taskExtraPayload.remainingPayloads = (serializedParams.Length - 1)-i;
				taskExtraPayload.runId = runId.ToString();
				commUtil.sendPacket(networkDataStream, taskExtraPayload);
			}

			commUtil.addToTaskList(runId.ToString());

			return commUtil.blockUntilResult(runId.ToString());
		}

		private int getNewRunID()
		{
			return runID++;
		}

		public void registerClient(string clientID)
		{
			Payload tempPayload = new Payload();
			tempPayload.clientId = clientId;
			tempPayload.command = CommandType.REGISTER_CLIENT;
			commUtil.sendPacket(networkDataStream, tempPayload);
		}

		public void registerAssembly(string assemblyName,int remainingPayloads)
		{
			Payload tempPayload = new Payload();
			tempPayload.clientId = clientId;
			tempPayload.assemblyName = assemblyName;
			tempPayload.command = CommandType.REGISTER_ASSEMBLY;
			tempPayload.remainingPayloads = remainingPayloads;
			commUtil.sendPacket(networkDataStream, tempPayload);
		}

		public void downloadAssembly(string assemblyName)
		{
			Payload downloadRqst = new Payload();
			downloadRqst.command = CommandType.DOWNLOAD;
			downloadRqst.clientId = clientId;
			downloadRqst.assemblyName = assemblyName;
			commUtil.sendPacket(networkDataStream, downloadRqst);
		}

		public void uploadAssembly(string assemblyName,byte[] assemblyBytes)
		{

			byte[][] serializedBytes = commUtil.splitBytes(assemblyBytes);

			Payload assemblyPayload = new Payload();
			assemblyPayload.command = CommandType.UPLOAD_ASSEMBLY;
			assemblyPayload.clientId = clientId;
			assemblyPayload.assemblyName = assemblyName;
			assemblyPayload.clientTime = DateTime.Now;
			assemblyPayload.assemblyBytes = serializedBytes[0];
			assemblyPayload.isAppend = serializedBytes.GetLength(0) > 1;
			assemblyPayload.remainingPayloads = serializedBytes.GetLength(0) - 1;
			commUtil.sendPacket(networkDataStream, assemblyPayload);

			for (int i = 1; i < serializedBytes.GetLength(0); i++)
			{
				Payload assemblyExtraPayload = new Payload();
				assemblyExtraPayload.command = CommandType.APPEND_ASSEMBLY;
				assemblyExtraPayload.clientId = clientId;
				assemblyExtraPayload.assemblyName = assemblyName;
				assemblyExtraPayload.clientTime = DateTime.Now;
				assemblyExtraPayload.assemblyBytes = serializedBytes[i];
				assemblyExtraPayload.isAppend = (i != (serializedBytes.GetLength(0) - 1));
				assemblyExtraPayload.remainingPayloads = (serializedBytes.GetLength(0) - 1)-i;
				commUtil.sendPacket(networkDataStream, assemblyExtraPayload);
			}
		}

		private void executeTask(Payload incoming)
		{
			if (execData.ContainsKey(incoming.runId))
			{
				lock (execData)
				{
					Payload prevData = execData[incoming.runId];
					prevData.remainingPayloads = incoming.remainingPayloads;
					prevData.jsonParameters += incoming.jsonParameters;
					prevData.isAppend = incoming.isAppend;
					execData[incoming.runId] = prevData;
				}
			}
			else
			{
				Payload resultParams = new Payload();
				resultParams.runId = incoming.runId;
				resultParams.assemblyName = incoming.assemblyName;
				resultParams.command = incoming.command;
				resultParams.isAppend = incoming.isAppend;
				resultParams.jsonParameters = incoming.jsonParameters;
				resultParams.remainingPayloads = incoming.remainingPayloads;
				execData.Add(incoming.runId, resultParams);
			}

			if (execData[incoming.runId].isAppend==false && execData[incoming.runId].remainingPayloads==0)
				{
					string result = commUtil.executeAssembly(execData[incoming.runId].assemblyName, execData[incoming.runId].jsonParameters);
					string[] serializedResult = commUtil.splitSerializedData(result);

					Payload resultPayload = new Payload();
					resultPayload.command = CommandType.RESULT;
					resultPayload.clientId = clientId;
					resultPayload.assemblyName = incoming.assemblyName;
					resultPayload.clientTime = DateTime.Now;
					resultPayload.jsonOutput = serializedResult[0];
					resultPayload.runId = incoming.runId;
					resultPayload.isAppend = serializedResult.Length > 1;
					resultPayload.remainingPayloads = serializedResult.Length - 1;
					commUtil.sendPacket(networkDataStream, resultPayload);

					for (int i = 1; i < serializedResult.Length; i++)
					{
						Payload resultExtraPayload = new Payload();
						resultExtraPayload.command = CommandType.APPEND_RESULT;
						resultExtraPayload.clientId = clientId;
						resultExtraPayload.assemblyName = incoming.assemblyName;
						resultExtraPayload.clientTime = DateTime.Now;
						resultExtraPayload.jsonOutput = serializedResult[i];
						resultExtraPayload.isAppend = (i != (serializedResult.Length - 1));
						resultExtraPayload.remainingPayloads = (serializedResult.Length - 1)-i;
						resultExtraPayload.runId = incoming.runId;
						commUtil.sendPacket(networkDataStream, resultExtraPayload);
					}
					execData.Remove(incoming.runId);
				}
			}

		private void pingHandler()
		{
			while(true)
			{
				Payload tempLoad = new Payload();
				float[] perfData = getPerfCounter();
				tempLoad.clientId = clientId;
				tempLoad.command = CommandType.STATUS;
				tempLoad.cpuUsage = perfData[0];
				tempLoad.memUsage = perfData[1];
				tempLoad.clientTime = DateTime.Now;
				commUtil.sendPacket(networkDataStream, tempLoad);
				Thread.Sleep(1000);
			}
		}

		public float[] getPerfCounter()
		{

			PerformanceCounter cpuCounter = new PerformanceCounter();
			cpuCounter.CategoryName = "Processor Information";
			cpuCounter.CounterName = "% Processor Time";
			cpuCounter.InstanceName = "_Total";

			PerformanceCounter ramCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");


			dynamic ramfirstValue = ramCounter.NextValue();


			dynamic cpufirstValue = cpuCounter.NextValue();
			System.Threading.Thread.Sleep(1000);

			dynamic cpusecondValue = cpuCounter.NextValue();
			dynamic ramsecondValue = ramCounter.NextValue();

			return new float[] { cpusecondValue, ramsecondValue };

		}

	}
}
