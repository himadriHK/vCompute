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
		public Server(int port)
		{
			this.port = port;
			commUtil = new Common(AppDomain.CurrentDomain.BaseDirectory+@"server.bin");
			listener = new TcpListener(IPAddress.Any, port);
			listener.Start();
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

		private void doRegisterAssembly(Payload payload)
		{
			
		}

		private void doRegisterClient(Payload payload)
		{
			throw new NotImplementedException();
		}

		private void sendToResultQueue(Payload payload)
		{
			throw new NotImplementedException();
		}

		private void sendToExecuteQueue(Payload payload)
		{
			throw new NotImplementedException();
		}
	}

	public class payloadDispatch
	{

	}
}
