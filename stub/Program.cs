using System;
using System.Threading;

namespace stub
{
    class Program
	{
		static void Main(string[] args)
		{
			try
            {
				string host = args[0];
				ushort port = ushort.Parse(args[1]);

				while (true)
				{
					Core.Run(host, port);
					Core.Wait();

					Thread.Sleep(3000);
				}
			}
			catch(Exception ex)
            {
				Console.WriteLine(ex.Message);
            }
		}
	}
}
