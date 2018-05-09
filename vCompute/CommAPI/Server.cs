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

		private void doRegisterAssembly(Payload payload, NetworkStream networkstream)
		{
            commUtil.storeAssembly(payload.assemblyName, new byte[0], false,0);
        }

        private void doRegisterClient(Payload payload, NetworkStream networkStream)
        {
            const string selections = "0123456789";
            Random random = new Random();
            Payload outputClientIdPayLoad = new Payload();
            outputClientIdPayLoad.clientId = new string(Enumerable.Repeat(selections, 2).Select(s => s[random.Next(s.Length)]).ToArray());

            commUtil.sendPacket(networkStream, outputClientIdPayLoad);
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
