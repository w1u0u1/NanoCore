using System;
using NanoCore.ClientPluginHost;

namespace stub
{
	class ClientLoggingHost : IClientLoggingHost
	{
		public Plugin Plugin;

		public ClientLoggingHost(Plugin plugin)
		{
			Plugin = plugin;
		}

		public void LogClientException(Exception ex, string site)
		{

		}

		public void LogClientMessage(string message)
		{

		}
	}
}