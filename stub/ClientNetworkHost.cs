using NanoCore.ClientPluginHost;

namespace stub
{
	class ClientNetworkHost : IClientNetworkHost
	{
		public Plugin Plugin;

		public ClientNetworkHost(Plugin plugin)
		{
			this.Plugin = plugin;
		}

		public bool Connected
		{
			get
			{
				return Core.Client.connected;
			}
		}

		public void ClosePipe(string pipeName)
		{
			if (Core.Clients.ContainsKey(pipeName))
			{
				Core.Clients[pipeName].Close();
			}
		}

		public bool PipeExists(string pipeName)
		{
			return Core.Clients.ContainsKey(pipeName);
		}

		public void Disconnect()
		{
			Core.Client.Close();
		}

		public void SendToServer(string pipeName, bool compress, params object[] @params)
		{
			if (@params == null)
				return;

			ClientSocket client = Core.Client;
			if (!string.IsNullOrEmpty(pipeName))
			{
				if (!Core.Clients.ContainsKey(pipeName))
					return;
				client = Core.Clients[pipeName];
			}

			Core.SendMessage(client, compress, CommandType.BaseCommand, 4, this.Plugin.Guid, @params);
		}

		public void AddHostEntry(string host)
		{

		}

		public void RebuildHostCache()
		{

		}
	}
}