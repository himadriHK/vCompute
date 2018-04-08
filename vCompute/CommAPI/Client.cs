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
		private Loader codeLoader;
		private NetworkStream networkDataStream;
		private Common commUtil;

		public Client(string host,int port,string clientKey,string swapFilePath)
		{
			hostAddress = host;
			this.port = port;
			clientId = clientKey;
			clientInstance = new TcpClient();
			//Loader codeLoader = new Loader(swapFilePath);
			commUtil = new Common();
			//try
			//{
				clientInstance.Connect(hostAddress, port);
				if(clientInstance.Connected)
				{
					Console.WriteLine("Client Connected");
					networkDataStream = clientInstance.GetStream();
					Thread pingThread = new Thread(pingHandler);
					//Thread dispatcher = new Thread(commandHandler);
					pingThread.Start();
					//dispatcher.Start();
				}
			//}
			//catch(Exception ex)
			//{
			//
			//}
		}

		private void commandHandler()
		{
			Payload payload = commUtil.getPacket(networkDataStream);
			switch(payload.command)
			{

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
