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
	public class Server
	{
		TcpListener listener;
		int port;
		TcpClient client;
		Common commUtil;
		public  Dictionary<string, Queue<Payload>> taskPayLoad = new Dictionary<string, Queue<Payload>>();
		public Dictionary<string, payloadDispatch> taskPayloadDispatch = new Dictionary<string, payloadDispatch>();
		private List<clientStatistics> clientList;
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
							doRegisterClient(payload);
						break;

						case CommandType.REGISTER_ASSEMBLY:
							doRegisterAssembly(payload);
						break;

						case CommandType.UPLOAD_ASSEMBLY:
						case CommandType.APPEND_ASSEMBLY:
							doSaveAssembly(payload);
						break;

						case CommandType.DOWNLOAD:
							doSendAssembly(payload);
						break;

					}
					Console.WriteLine(payload.clientId + " " + payload.command + " " + payload.cpuUsage + " " + payload.memUsage);
				}
			}
		}

		private void doSendAssembly(Payload payload)
		{
			throw new NotImplementedException();
		}

		private void doSaveAssembly(Payload payload)
		{
			throw new NotImplementedException();
		}

		private void doRegisterAssembly(Payload payload)
		{
			throw new NotImplementedException();
		}

		private void doRegisterClient(Payload payload)
		{
			throw new NotImplementedException();
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
