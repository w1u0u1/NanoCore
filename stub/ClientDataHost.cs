using NanoCore;
using NanoCore.ClientPluginHost;

namespace stub
{
    class ClientDataHost : IClientDataHost
	{
		public Plugin Plugin;

		public ClientDataHost(Plugin plugin)
		{
			Plugin = plugin;
		}

		public IClientNameObjectCollection Variables
		{
			get
			{
				return Core.Variables;
			}
		}

		public IClientNameObjectCollection ClientSettings
		{
			get
			{
				return Core.ClientSettings;
			}
		}

		public IClientReadOnlyNameObjectCollection BuilderSettings
		{
			get
			{
				return Core.BuilderSettings;
			}
		}
	}
}