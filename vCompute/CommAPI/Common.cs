using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace CommAPI
{
	public enum CommandType { UPLOAD = 101, DOWNLOAD, EXECUTE, DELETE, PING, REGISTER_CLIENT, REGISTER_ASSEMBLY, RESULT, APPEND_ASSEMBLY, STATUS }
	public class Common
	{
		
		private const int payloadSize = 1024;

		public string preparePayload(Payload payload)
		{
			return new JavaScriptSerializer().Serialize(payload).PadRight(payloadSize);
		}

		public Payload preparePayload(string payload)
		{
			return new JavaScriptSerializer().Deserialize<Payload>(payload.TrimEnd());
		}

		public void sendPacket(NetworkStream stream,Payload payload)
		{
			string serializedData = preparePayload(payload);
			byte[] data = Encoding.ASCII.GetBytes(serializedData);

			lock (stream)
			{
				stream.Write(data, 0, payloadSize);
			}
		}

		public Payload getPacket(NetworkStream stream)
		{
			lock(stream)
			{
				byte[] buffer = new byte[payloadSize];
				stream.Read(buffer, 0, payloadSize);
				string serializedData = Encoding.ASCII.GetString(buffer);
				Payload output = preparePayload(serializedData);
				return output;
			}
		}
	}

	[Serializable]
	public class Payload
	{
		public CommandType command;
		public string serverId;
		public string clientId;
		public string payloadId;
		public string assemblyName;
		public byte[] assemblyBytes;
		public double cpuUsage;
		public double memUsage;
		public double responseTime;
		public string jsonOutput;
		public DateTime? serverTime;
		public DateTime? clientTime;
	}
}
