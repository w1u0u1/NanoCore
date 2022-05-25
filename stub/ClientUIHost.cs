using NanoCore;
using NanoCore.ClientPluginHost;

namespace stub
{
    class ClientUIHost : IClientUIHost
	{
		public Plugin Plugin;

		public ClientUIHost(Plugin plugin)
		{
			this.Plugin = plugin;
		}

		public void Invoke(ClientInvokeDelegate method, object state)
		{

		}
	}
}