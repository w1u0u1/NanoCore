using NanoCore.ClientPluginHost;

namespace stub
{
    class ClientAppHost : IClientAppHost
	{
		public Plugin Plugin;

		public ClientAppHost(Plugin plugin)
		{
			Plugin = plugin;
		}

		public void Restart()
		{

		}

		public void Shutdown()
		{

		}

		public void DisableProtection()
		{

		}

		public void RestoreProtection()
		{

		}

		public void Uninstall()
		{

		}
	}
}