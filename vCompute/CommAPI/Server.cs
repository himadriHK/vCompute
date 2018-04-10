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
			commUtil = new Common(@"C:\Users\Himadri-HK\Documents\server.bin");
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
					Debug.Print(payload.clientId + " " + payload.command + " " + payload.cpuUsage + " " + payload.memUsage);
				}
			}
		}
	}
}
