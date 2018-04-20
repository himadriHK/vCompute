﻿using CodeLoader;
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

		public Client(string host,int port,string clientKey,string swapFilePath)
		{
			hostAddress = host;
			this.port = port;
			clientId = clientKey;
			clientInstance = new TcpClient();
			commUtil = new Common(swapFilePath);
			runID = 1;
			try
			{
			clientInstance.Connect(hostAddress, port);
			if(clientInstance.Connected)
			
			Console.WriteLine("Client Connected");
			networkDataStream = clientInstance.GetStream();
			Thread pingThread = new Thread(pingHandler);
				//Thread dispatcher = new Thread(serverMessageHandler);
				pingThread.Start();
			//dispatcher.Start();
			
			}
			catch(Exception ex)
			{
			
			}
		}

		private void serverMessageHandler()
		{
			Payload payload = commUtil.getPacket(networkDataStream);
			switch(payload.command)
			{
				case CommandType.DOWNLOAD:
				case CommandType.APPEND_ASSEMBLY:
					commUtil.storeAssembly(payload.assemblyName, payload.assemblyBytes,payload.isAppend,payload.remainingPayloads);
				break;

				case CommandType.EXECUTE:
					executeTask(payload.assemblyName, payload.jsonParameters);
				break;

				case CommandType.RESULT:
				case CommandType.APPEND_RESULT:
					storeResult(payload.assemblyName, payload.runId, payload.jsonOutput,payload.isAppend, payload.remainingPayloads);
				break;
			}
		}

		private void storeResult(string assemblyName, int runId, string jsonOutput, bool isAppend, int remainingPayloads)
		{
			throw new NotImplementedException();
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
			taskPayload.runId = getNewRunID();

			commUtil.sendPacket(networkDataStream, taskPayload);
			
			for(int i=1;i<serializedParams.Length;i++)
			{
				Payload taskExtraPayload = new Payload();
				taskExtraPayload.command = CommandType.APPEND_REQUEST;
				taskExtraPayload.clientId = clientId;
				taskExtraPayload.assemblyName = assemblyName;
				taskExtraPayload.clientTime = DateTime.Now;
				taskExtraPayload.jsonParameters = serializedParams[i];
				taskExtraPayload.runId = runId;
				commUtil.sendPacket(networkDataStream, taskExtraPayload);
			}

			commUtil.addToTaskList(runId);

			return commUtil.blockUntilResult(runId);
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

		public void registerAssembly(string assemblyName)
		{
			Payload tempPayload = new Payload();
			tempPayload.clientId = clientId;
			tempPayload.assemblyName = assemblyName;
			tempPayload.command = CommandType.REGISTER_ASSEMBLY;
			commUtil.sendPacket(networkDataStream, tempPayload);
		}

		public void uploadAssembly(string assemblyName,byte[] assemblyBytes)
		{
			Payload tempPayload = new Payload();
			tempPayload.clientId = clientId;
			tempPayload.assemblyName = assemblyName;
			tempPayload.assemblyBytes = assemblyBytes;
			tempPayload.command = CommandType.UPLOAD_ASSEMBLY;
			commUtil.sendPacket(networkDataStream, tempPayload);
		}

		private void executeTask(string assemblyName, object parameters)
		{
			string result=commUtil.executeAssembly(assemblyName, parameters);
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

			PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");


			dynamic ramfirstValue = ramCounter.NextValue();


			dynamic cpufirstValue = cpuCounter.NextValue();
			System.Threading.Thread.Sleep(1000);

			dynamic cpusecondValue = cpuCounter.NextValue();
			dynamic ramsecondValue = ramCounter.NextValue();

			return new float[] { cpusecondValue, ramsecondValue };

		}

	}
}
