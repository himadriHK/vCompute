using CodeLoader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

		public Client(string host,int port,string clientKey,string swapFilePath)
		{
			hostAddress = host;
			this.port = port;
			clientId = clientKey;
			clientInstance = new TcpClient();
			commUtil = new Common(swapFilePath);
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
					commUtil.storeAssembly(payload.assemblyName, payload.assemblyBytes);
				break;

				case CommandType.EXECUTE:
					executeTask(payload.assemblyName, payload.parameters);
				break;

				case CommandType.APPEND_ASSEMBLY:
					commUtil.storeAssembly(payload.assemblyName, payload.assemblyBytes,true);
				break;

			}
		}

		public void registerClient(string clientID)
		{

		}

		public void registerAssembly(string assemblyName)
		{

		}

		public void uploadAssembly(string assemblyName)
		{

		}

		private void executeTask(string assemblyName, object parameters)
		{
			throw new NotImplementedException();
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
