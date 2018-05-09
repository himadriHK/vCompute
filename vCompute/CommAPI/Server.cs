﻿using System;
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
	public class Server
	{
		TcpListener listener;
		int port;
		TcpClient client;
		Common commUtil;
		public  Dictionary<string, Queue<Payload>> taskPayLoad = new Dictionary<string, Queue<Payload>>();
		public Dictionary<string, payloadDispatch> taskPayloadDispatch = new Dictionary<string, payloadDispatch>();
		private List<clientStatistics> clientList;
		public static int ClientNo = 1;
		 public Server(int port)
		{
			this.port = port;
			commUtil = new Common(AppDomain.CurrentDomain.BaseDirectory+@"server.bin");
			listener = new TcpListener(IPAddress.Any, port);
			listener.Start();
			clientList=new List<clientStatistics>();
			Thread acceptConnections = new Thread(processConnections);
			acceptConnections.Start();
		}

		private void processConnections()
		{
			while (true)
			{
				client = listener.AcceptTcpClient();
				if (client.Connected)
				{
					Console.WriteLine("Server Conected");
					ParameterizedThreadStart processMethod = new ParameterizedThreadStart(processClient);
					Thread clientThread = new Thread(processMethod);
					clientThread.Start(client);
				}
			}
		}

		private void processClient(object clientInfo)
		{
			TcpClient client = (TcpClient)clientInfo;
			NetworkStream networkStream = client.GetStream();

			while (true)
			{
				if (networkStream.DataAvailable)
				{
					Payload payload = commUtil.getPacket(networkStream);
					switch (payload.command)
					{
						case CommandType.REQUEST:
						case CommandType.APPEND_REQUEST:
							sendToExecuteQueue(payload);
						break;

						case CommandType.RESULT:
						case CommandType.APPEND_RESULT:
							sendToResultQueue(payload);
						break;

						case CommandType.REGISTER_CLIENT:
							doRegisterClient(payload,networkStream);
						break;

						case CommandType.REGISTER_ASSEMBLY:
							doRegisterAssembly(payload, networkStream);
						break;

						case CommandType.UPLOAD_ASSEMBLY:
						case CommandType.APPEND_ASSEMBLY:
							doSaveAssembly(payload);
						break;

						case CommandType.DOWNLOAD:
							doSendAssembly(payload, networkStream);
						break;

					}
					Console.WriteLine(payload.clientId + " " + payload.command + " " + payload.cpuUsage + " " + payload.memUsage);
				}
			}
		}

		private void doSendAssembly(Payload payload, NetworkStream networkDataStream)
		{
            byte[] outputAssembly = commUtil.readAssembly(payload.assemblyName);

            byte[][] serializedBytes = commUtil.splitBytes(outputAssembly);

            Payload assemblyPayload = new Payload();
            assemblyPayload.command = CommandType.DOWNLOAD;
            assemblyPayload.clientId = payload.clientId;
            assemblyPayload.assemblyName = payload.assemblyName;
            assemblyPayload.clientTime = DateTime.Now;
            assemblyPayload.assemblyBytes = serializedBytes[0];
            assemblyPayload.isAppend = serializedBytes.GetLength(0) > 1;
            assemblyPayload.remainingPayloads = serializedBytes.GetLength(0) - 1;
            commUtil.sendPacket(networkDataStream, assemblyPayload);

            for (int i = 1; i < serializedBytes.GetLength(0); i++)
            {
                Payload assemblyExtraPayload = new Payload();
                assemblyExtraPayload.command = CommandType.APPEND_ASSEMBLY;
                assemblyExtraPayload.clientId = payload.clientId;
                assemblyExtraPayload.assemblyName = payload.assemblyName;
                assemblyExtraPayload.clientTime = DateTime.Now;
                assemblyExtraPayload.assemblyBytes = serializedBytes[i];
                assemblyExtraPayload.isAppend = (i != (serializedBytes.GetLength(0) - 1));
                assemblyExtraPayload.remainingPayloads = (serializedBytes.GetLength(0) - 1) - i;
                commUtil.sendPacket(networkDataStream, assemblyExtraPayload);
            }
        }

		private void doSaveAssembly(Payload payload)
		{
            commUtil.storeAssembly(payload.assemblyName, payload.assemblyBytes, payload.isAppend, payload.remainingPayloads);
        }

		private void doRegisterAssembly(Payload payload, NetworkStream networkstream)
		{
            commUtil.storeAssembly(payload.assemblyName, new byte[0], false,0);
        }

        private void doRegisterClient(Payload payload, NetworkStream networkStream)
        {
            string ClientId = "Client" + ClientNo;
            Payload outputClientIdPayLoad = new Payload();
            outputClientIdPayLoad.clientId = ClientId;
	        if (!taskPayLoad.ContainsKey(outputClientIdPayLoad.clientId))
	        {
				taskPayLoad.Add(outputClientIdPayLoad.clientId, new Queue<Payload>());
				Thread newClientThreadStart = new Thread(()=>sendPacketsToClient(ClientId,networkStream));
				newClientThreadStart.Start();
				ClientNo++;
			}
			commUtil.sendPacket(networkStream, outputClientIdPayLoad);
        }

		private void sendPacketsToClient(string clientId, NetworkStream networkStream)
		{
			var Q = taskPayLoad[clientId];
			while(true)
			lock (Q)
			{
				foreach(Payload p in Q)
					commUtil.sendPacket(networkStream,p);
			}
		}

		private void sendToResultQueue(Payload payload)
		{
			string toId = null;
			string fromId = null;
			string runId;
			payloadDispatch dispatch = null;
			if (taskPayloadDispatch.ContainsKey(payload.runId))
			{
				dispatch = taskPayloadDispatch[payload.runId];
				toId = dispatch.fromClient;
				fromId = dispatch.toClient;
				payload.runId = payload.runId.Replace(dispatch.toClient,"");
			}

			if (toId != null)
			{
				var requestQueue = taskPayLoad[toId];
				requestQueue.Enqueue(payload);
			}
		}

		private void sendToExecuteQueue(Payload payload)
		{
			string runId = payload.clientId + payload.runId;
			payloadDispatch dispatch = null;
			if (taskPayLoad.ContainsKey(runId))
			{
				dispatch = taskPayloadDispatch[runId];
			}
			else
			{
				dispatch.fromClient = payload.clientId;
				dispatch.runId = payload.clientId + payload.runId;
				dispatch.toClient = clientList.First().Name;
				taskPayloadDispatch.Add(runId, dispatch);
			}

			var toId = dispatch.toClient;
			
			payload.runId = dispatch.runId;
			if (payload.command == CommandType.REQUEST)
				payload.command = CommandType.EXECUTE;
			else if(payload.command == CommandType.APPEND_REQUEST)
				payload.command = CommandType.APPEND_EXECUTE;

			var Q = taskPayLoad[toId];
			Q.Enqueue(payload);
		}
	}

	public class payloadDispatch
	{
		public string fromClient;
		public string toClient;
		public string runId;
	}

	public class clientStatistics : IComparable<clientStatistics>
	{
		List<clientStatistics> containerList;
		double load;
		public clientStatistics(List<clientStatistics> list)
		{
			containerList = list;
		}
		public string Name { get; set; }
		public double Load
		{
			get
			{
				return load;
			}

			set
			{
				load = value;
				lock (containerList)
				{
					containerList.Sort();
				}
			}
		}


		public int CompareTo(clientStatistics other)
		{
			return Load.CompareTo(other.Load);
		}
	}
}
