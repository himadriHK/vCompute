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
		public static int ClientNo = 1;
        public delegate void RegisterClientHandler(RegisterClientEventArgs e);
        public event RegisterClientHandler registerClientEvent;
        public delegate void UpdateStatusHandler(UpdateStatusEventArgs e);
        public event UpdateStatusHandler updateStatusEvent;
        public delegate void ExecuteHandler(ExectueEventArgs e);
        public event ExecuteHandler executeEvent;
        public delegate void ResultHandler(ResultEventArgs e);
        public event ResultHandler resultEvent;
        public delegate void UploadHandler(UploadEventArgs e);
        public event UploadHandler uploadEvent;

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
            clientStatistics stats = new clientStatistics(clientList);
            Queue<Payload> payloadQueue = new Queue<Payload>();
            lock (clientList)
            {
                clientList.Add(stats);
                clientList.Sort();
            }

            while (true)
			{
                while (networkStream.DataAvailable)
                {
                    byte[] buffer = new byte[commUtil.payloadSize];
                    int readBytes = 0;
                    readBytes = networkStream.Read(buffer, 0, commUtil.payloadSize);
                    Payload output = null;
                    if (readBytes == commUtil.payloadSize)
                    {
                        string serializedData = Encoding.UTF8.GetString(buffer);
                        output = commUtil.preparePayload(serializedData);
                        payloadQueue.Enqueue(output);
                    }
                    if (output != null && output.command != CommandType.STATUS)
                        Debug.Print("Receiving " + Enum.GetName(typeof(CommandType), output.command));
                }
                while(payloadQueue.Count!=0)
                {
                    Payload payload = null;
                    if (payloadQueue != null && payloadQueue.Count > 0)
                        payload = payloadQueue.Dequeue();

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
                            doRegisterClient(payload, networkStream);
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

                        case CommandType.STATUS:
                            updateStatus(payload, stats);
                            break;
                    }
                }
					//Console.WriteLine(payload.clientId + " " + payload.command + " " + payload.cpuUsage + " " + payload.memUsage);
                    Thread.Sleep(150);
				}
			}

        private void updateStatus(Payload payload, clientStatistics stats)
        {
            if (!taskPayLoad.ContainsKey(payload.clientId))
                return;

            double load = (payload.cpuUsage + payload.memUsage) / 2;
            stats.Load = (stats.Load+ load)/2;
            stats.Name = payload.clientId;

            UpdateStatusEventArgs args = new UpdateStatusEventArgs();
            args.ClientId = stats.Name;
            args.load = stats.Load;
            var e = updateStatusEvent;
            if (e != null)
                e.Invoke(args);
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
            commUtil.StoreAssembly(payload.assemblyName, payload.assemblyBytes, payload.isAppend, payload.remainingPayloads);

            if(payload.remainingPayloads == 0 && !payload.isAppend)
            {
                UploadEventArgs args = new UploadEventArgs()
                {
                    FromClientId = payload.clientId,
                    AssemblyName = payload.assemblyName
                };
                uploadEvent(args);
            }
        }

		private void doRegisterAssembly(Payload payload, NetworkStream networkstream)
		{
            commUtil.StoreAssembly(payload.assemblyName, new byte[0], false,0);
        }

        private void doRegisterClient(Payload payload, NetworkStream networkStream)
        {
            string ClientId = "Client" + ClientNo;
            Payload outputClientIdPayLoad = new Payload();
            outputClientIdPayLoad.command = CommandType.CLIENT_REGISTRTION;
            outputClientIdPayLoad.clientId = ClientId;
            RegisterClientEventArgs args = new RegisterClientEventArgs();
            args.ClientId = ClientId;
            var e = registerClientEvent;
            if (e != null)
                e.Invoke(args);

            commUtil.sendPacket(networkStream, outputClientIdPayLoad);
            if (!taskPayLoad.ContainsKey(outputClientIdPayLoad.clientId))
	        {
				taskPayLoad.Add(outputClientIdPayLoad.clientId, new Queue<Payload>());
				Thread newClientThreadStart = new Thread(()=>sendPacketsToClient(ClientId,networkStream));
				newClientThreadStart.Start();
				ClientNo++;
			}

            args.ClientId = ClientId+" second";
            if (e!=null)
            e.Invoke(args);

        }

		private void sendPacketsToClient(string clientId, NetworkStream networkStream)
		{
			var Q = taskPayLoad[clientId];
			while(true)
//			lock (Q)
			{
                while(Q.Count>0)
					commUtil.sendPacket(networkStream,Q.Dequeue());
                Thread.Sleep(300);
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
                if (payload.remainingPayloads == 0 && payload.isAppend == false)
                {
                   ResultEventArgs args = new ResultEventArgs()
                   {
                       FromClientId = dispatch.fromClient,
                       ToClientId = dispatch.toClient,
                       RunId = dispatch.runId
                   };
                    var e = resultEvent;
                    if (e != null)
                        e.Invoke(args);
                }
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
                dispatch = new payloadDispatch();
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
            if(payload.remainingPayloads == 0 && payload.isAppend == false)
            {
                ExectueEventArgs args = new ExectueEventArgs()
                {
                    FromClientId = payload.clientId,
                    ToClientId = toId,
                    RunId = runId
                };
                var e = executeEvent;
                if (e != null)
                    e.Invoke(args);
            }  
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
    public class RegisterClientEventArgs : EventArgs
    {
        public string ClientId { get; set; }
    }

    public class UpdateStatusEventArgs : EventArgs
    {
        public string ClientId { get; set; }
        public double load { get; set; }
    }

    public class ExectueEventArgs : EventArgs
    {
        public string FromClientId { get; set; }
        public string ToClientId { get; set; }
        public string RunId { get; set; }

    }

    public class ResultEventArgs : EventArgs
    {
        public string FromClientId { get; set; }
        public string ToClientId { get; set; }
        public string RunId { get; set; }
    }

    public class UploadEventArgs : EventArgs
    {
        public string FromClientId { get; set; }
        public string AssemblyName { get; set; }
    }

}
