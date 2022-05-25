using System.Collections.Generic;
using NanoCore;

namespace stub
{
    public sealed class ClientReadOnlyNameObjectCollection : IClientReadOnlyNameObjectCollection
	{
		private Dictionary<string, object> values;

		public ClientReadOnlyNameObjectCollection(Dictionary<string, object> values)
		{
			this.values = values;
		}

		public object GetValue(string name, object value)
		{
			object result;
			lock (values)
			{
				if (values.ContainsKey(name))
					result = this.values[name];
				else
					result = value;
			}
			return result;
		}

		public void SetValue(string name, object value)
		{
			lock (values)
			{
				if (values.ContainsKey(name))
				{
					if (values[name].Equals(value))
						return;

					values[name] = value;
				}
				else
				{
					values.Add(name, value);
				}
			}
		}

		public bool EntryExists(string name)
		{
			bool result;
			lock (values)
			{
				result = values.ContainsKey(name);
			}
			return result;
		}

		public KeyValuePair<string, object>[] GetEntries()
		{
			KeyValuePair<string, object>[] result;
			lock (values)
			{
				List<KeyValuePair<string, object>> list = new List<KeyValuePair<string, object>>();
				foreach (var item in values)
				{
					list.Add(item);
				}
				result = list.ToArray();
			}
			return result;
		}
	}
}