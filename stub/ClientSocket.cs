using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace stub
{
    class ClientSocket
	{
		public delegate void HANDLE_PACKET(ClientSocket client, byte[] bytes);
		public delegate void HANDLE_EXCEPTION(ClientSocket client, Exception ex);
		public delegate void HANDLE_CONNECT(ClientSocket client, bool bool_0);

		private int totalRecv;
		private int totalSend;
		private int totalLength;

		private bool getLengthed;
		private bool sendStarted;

		private byte[] buffer;
		private byte[] bufferLength;
		private byte[] recvBuffer;
		private byte[] sendBuffer;

		private int bufferSize;
		private int limitPacketSize;

		private Queue<byte[]> sendQueue;

		public string Host;
		public ushort Port;
		public bool connected = false;
		public PluginConnect PluginConnect;

		private object obj = new object();
		private Socket socket;

		public HANDLE_CONNECT HandleConnect = null;
		public HANDLE_EXCEPTION HandleException = null;
		public HANDLE_PACKET HandlePacket = null;

		private SocketAsyncEventArgs SocketReceiveEvent;
		private SocketAsyncEventArgs SocketSendEvent;
		private SocketAsyncEventArgs SocketConnectEvent;

		public ClientSocket()
		{
			bufferSize = 65535;
			limitPacketSize = 10485760;
		}

		private void Init()
		{
			getLengthed = false;
			sendStarted = false;

			buffer = new byte[bufferSize * 2];
			bufferLength = new byte[4];
			recvBuffer = new byte[0];
			sendBuffer = new byte[0];
			sendQueue = new Queue<byte[]>();

			SocketReceiveEvent = new SocketAsyncEventArgs();
			SocketSendEvent = new SocketAsyncEventArgs();
			SocketConnectEvent = new SocketAsyncEventArgs();
			SocketReceiveEvent.SetBuffer(buffer, 0, bufferSize);
			SocketSendEvent.SetBuffer(buffer, bufferSize, bufferSize);

			SocketReceiveEvent.Completed += HandleEvent;
			SocketSendEvent.Completed += HandleEvent;
			SocketConnectEvent.Completed += HandleEvent;
		}

		private void Connect(IPAddress ipa, ushort port)
		{
			try
			{
				socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				socket.LingerState = new LingerOption(true, 0);
				SocketConnectEvent.RemoteEndPoint = new IPEndPoint(ipa, (int)port);

				if (!socket.ConnectAsync(SocketConnectEvent))
				{
					HandleEvent(socket, SocketConnectEvent);
				}
			}
			catch (Exception ex)
			{
				HandleException?.Invoke(this, ex);
				Close();
			}
		}

		public void Connect(string host, ushort port)
		{
			try
			{
				Host = host;
				Port = port;
				Init();

				IPAddress ipa = IPAddress.None;
				if (!IPAddress.TryParse(Host, out ipa))
					ipa = Dns.GetHostEntry(Host).AddressList[0];

				Connect(ipa, Port);
			}
			catch (Exception ex)
			{
				HandleException?.Invoke(this, ex);
				Close();
			}
		}

		private void ParsePacket(byte[] byte_4, int int_8, int int_9)
		{
			if (getLengthed)
			{
				int num = Math.Min(recvBuffer.Length - totalRecv, int_9 - int_8);
				Buffer.BlockCopy(byte_4, int_8, recvBuffer, totalRecv, num);
				totalRecv += num;
				if (totalRecv == recvBuffer.Length)
				{
					HandlePacket?.Invoke(this, recvBuffer);

					getLengthed = false;
					Array.Resize<byte>(ref recvBuffer, 0);
				}
				if (num < int_9 - int_8)
				{
					ParsePacket(byte_4, int_8 + num, int_9);
				}
			}
			else
			{
				int num2 = Math.Min(int_9 - int_8, 4 - totalLength);
				Buffer.BlockCopy(byte_4, int_8, bufferLength, totalLength, num2);
				int_8 += num2;
				totalLength += num2;
				if (totalLength == 4)
				{
					int num3 = BitConverter.ToInt32(bufferLength, 0);
					if (num3 > limitPacketSize)
					{
						HandleException?.Invoke(this, new Exception("Packet size exceeds MaxPacketSize."));
						Close();
						return;
					}
					if (num3 <= 0)
					{
						HandleException?.Invoke(this, new Exception("Packet size must be greater than 0."));
						Close();
						return;
					}
					totalRecv = 0;
					totalLength = 0;
					getLengthed = true;
					Array.Resize<byte>(ref recvBuffer, num3);
					if (int_8 < int_9)
					{
						ParsePacket(byte_4, int_8, int_9);
					}
				}
			}
		}

		private void HandleEvent(object sender, SocketAsyncEventArgs e)
		{
			try
			{
				if (socket != null)
				{
					if (socket == sender)
					{
						if (e.SocketError == SocketError.Success)
						{
							switch (e.LastOperation)
							{
								case SocketAsyncOperation.Connect:
                                    {
										SocketConnectEvent.Dispose();
										SocketConnectEvent = null;
										connected = true;

										HandleConnect?.Invoke(this, true);

										Receive();
									}
									break;
								case SocketAsyncOperation.Receive:
                                    {
										if (e.BytesTransferred == 0)
										{
											Close();
										}
										else
										{
											ParsePacket(e.Buffer, 0, e.BytesTransferred);
											if (!socket.ReceiveAsync(e))
											{
												HandleEvent(socket, e);
											}
										}
									}
									break;
								case SocketAsyncOperation.Send:
									{
										if (totalSend == 0)
										{
											totalSend = -4;
										}
										totalSend += e.BytesTransferred;
										bool flag = false;
										if (totalSend == sendBuffer.Length)
										{
											flag = true;
										}
										lock (obj)
										{
											if (sendQueue.Count == 0 && flag)
											{
												sendStarted = false;
												Array.Resize<byte>(ref sendBuffer, 0);
												totalSend = 0;
											}
											else
											{
												Send();
											}
										}
										break;
									}
							}
						}
						else
						{
							HandleException?.Invoke(this, new SocketException((int)e.SocketError));
							Close();
						}
					}
				}
			}
			catch (Exception ex)
			{
				HandleException?.Invoke(this, ex);
				Close();
			}
		}

		private void Receive()
		{
			if (!socket.ReceiveAsync(SocketReceiveEvent))
			{
				HandleEvent(socket, SocketReceiveEvent);
			}
		}

		private void Send()
		{
			if (totalSend == sendBuffer.Length)
			{
				totalSend = 0;
				lock (obj)
				{
					sendBuffer = sendQueue.Dequeue();
				}
			}
			int num = 0;
			if (totalSend == 0)
			{
				num = 4;
				Buffer.BlockCopy(BitConverter.GetBytes(sendBuffer.Length), 0, buffer, SocketSendEvent.Offset, 4);
			}
			int num2 = Math.Min(sendBuffer.Length - totalSend, bufferSize - num);
			Buffer.BlockCopy(sendBuffer, totalSend, buffer, SocketSendEvent.Offset + num, num2);
			SocketSendEvent.SetBuffer(SocketSendEvent.Offset, num2 + num);
			if (!socket.SendAsync(SocketSendEvent))
			{
				HandleEvent(socket, SocketSendEvent);
			}
		}

		public void SendBytes(byte[] bytes)
		{
			try
			{
				lock (obj)
				{
					sendQueue.Enqueue(bytes);
					if (!sendStarted)
					{
						sendStarted = true;
						Send();
					}
				}
			}
			catch (Exception ex)
			{
				HandleException?.Invoke(this, ex);
				Close();
			}
		}

		public void Close()
		{
			connected = false;

			if (socket != null)
			{
				socket.Close();
				socket = null;
			}
			if (SocketReceiveEvent != null)
			{
				SocketReceiveEvent.Dispose();
				SocketReceiveEvent = null;
			}
			if (SocketSendEvent != null)
			{
				SocketSendEvent.Dispose();
				SocketSendEvent = null;
			}
			if (SocketConnectEvent != null)
			{
				SocketConnectEvent.Dispose();
				SocketConnectEvent = null;
			}
			if (sendQueue != null)
			{
				sendQueue.Clear();
				sendQueue = null;
			}

			totalSend = 0;
			totalRecv = 0;
			recvBuffer = null;
			sendBuffer = null;
			buffer = null;
			bufferLength = null;
			getLengthed = false;
			sendStarted = false;
		}
	}
}