using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;

namespace stub
{
    struct PacketData
	{
		public byte Type;
		public byte Command;
		public Guid Guid;
		public object[] Buffer;
	}

	class Packet
	{
		static object obj = new object();
		static BinaryReader br;
		static BinaryWriter bw;
		static MemoryStream msr;
		static MemoryStream msw;
		static List<object> data;
		static Dictionary<Type, byte> type;
		static ICryptoTransform encryptor;
		static ICryptoTransform decryptor;

		static Packet()
		{
			msr = new MemoryStream();
			br = new BinaryReader(msr);

			msw = new MemoryStream();
			bw = new BinaryWriter(msw);

			data = new List<object>();
			type = new Dictionary<Type, byte>();
			type.Add(typeof(bool), 0);
			type.Add(typeof(byte), 1);
			type.Add(typeof(byte[]), 2);
			type.Add(typeof(char), 3);
			type.Add(typeof(char[]), 4);
			type.Add(typeof(decimal), 5);
			type.Add(typeof(double), 6);
			type.Add(typeof(int), 7);
			type.Add(typeof(long), 8);
			type.Add(typeof(sbyte), 9);
			type.Add(typeof(short), 10);
			type.Add(typeof(float), 11);
			type.Add(typeof(string), 12);
			type.Add(typeof(uint), 13);
			type.Add(typeof(ulong), 14);
			type.Add(typeof(ushort), 15);
			type.Add(typeof(DateTime), 16);
			type.Add(typeof(string[]), 17);
			type.Add(typeof(Guid), 18);
			type.Add(typeof(Size), 19);
			type.Add(typeof(Rectangle), 20);
			type.Add(typeof(Version), 21);
		}

		public static void Init(byte[] bytes)
		{
			DESCryptoServiceProvider descryptoServiceProvider = new DESCryptoServiceProvider();
			descryptoServiceProvider.BlockSize = 64;
			descryptoServiceProvider.Key = bytes;
			descryptoServiceProvider.IV = bytes;
			encryptor = descryptoServiceProvider.CreateEncryptor();
			decryptor = descryptoServiceProvider.CreateDecryptor();
		}

		public static byte[] WriteData(bool bool_0, byte byte_0, byte byte_1, Guid guid_0, object[] object_1)
		{
			MemoryStream obj = msw;
			byte[] result;
			lock (obj)
			{
				bw.Write(bool_0);
				bw.Write(byte_0);
				bw.Write(byte_1);
				if (guid_0 == null)
				{
					bw.Write(false);
				}
				else
				{
					bw.Write(true);
					bw.Write(guid_0.ToByteArray());
				}
				if (object_1 != null)
				{
					int num = 0;
					int num2 = object_1.Length - 1;
					for (int i = num; i <= num2; i++)
					{
						Type type = object_1[i].GetType();
						if (type.IsEnum)
						{
							type = Enum.GetUnderlyingType(type);
						}
						byte value = Packet.type[type];
						bw.Write(value);
						switch (value)
						{
							case 0:
								bw.Write((bool)object_1[i]);
								break;
							case 1:
								bw.Write((byte)object_1[i]);
								break;
							case 2:
								bw.Write(((byte[])object_1[i]).Length);
								bw.Write((byte[])object_1[i]);
								break;
							case 3:
								bw.Write((char)object_1[i]);
								break;
							case 4:
								bw.Write(((char[])object_1[i]).ToString());
								break;
							case 5:
								bw.Write((decimal)object_1[i]);
								break;
							case 6:
								bw.Write((double)object_1[i]);
								break;
							case 7:
								bw.Write((int)object_1[i]);
								break;
							case 8:
								bw.Write((long)object_1[i]);
								break;
							case 9:
								bw.Write((sbyte)object_1[i]);
								break;
							case 10:
								bw.Write((short)object_1[i]);
								break;
							case 11:
								bw.Write((float)object_1[i]);
								break;
							case 12:
								bw.Write((string)object_1[i]);
								break;
							case 13:
								bw.Write((uint)object_1[i]);
								break;
							case 14:
								bw.Write((ulong)object_1[i]);
								break;
							case 15:
								bw.Write((ushort)object_1[i]);
								break;
							case 16:
								{
									DateTime dateTime = (DateTime)object_1[i];
									bw.Write(dateTime.ToBinary());
									break;
								}
							case 17:
								bw.Write(((string[])object_1[i]).Length);
								foreach (string value2 in (string[])object_1[i])
								{
									bw.Write(value2);
								}
								break;
							case 18:
								{
									BinaryWriter binaryWriter2 = bw;
									Guid guid = (Guid)object_1[i];
									binaryWriter2.Write(guid.ToByteArray());
									break;
								}
							case 19:
								{
									Size size = (Size)object_1[i];
									bw.Write(size.Width);
									bw.Write(size.Height);
									break;
								}
							case 20:
								{
									Rectangle rectangle = (Rectangle)object_1[i];
									bw.Write(rectangle.X);
									bw.Write(rectangle.Y);
									bw.Write(rectangle.Width);
									bw.Write(rectangle.Height);
									break;
								}
							case 21:
								bw.Write(((Version)object_1[i]).ToString());
								break;
						}
					}
				}
				byte[] array2 = msw.ToArray();
				msw.SetLength(0L);
				if (bool_0 && array2.Length >= 860)
				{
					bw.Write(bool_0);
					bw.Write(array2.Length - 1);
					DeflateStream deflateStream = new DeflateStream(msw, CompressionMode.Compress, true);
					deflateStream.Write(array2, 1, array2.Length - 1);
					deflateStream.Close();
					array2 = msw.ToArray();
					msw.SetLength(0L);
				}
				else
				{
					array2[0] = 0;
				}
				byte[] buffer = encryptor.TransformFinalBlock(array2, 0, array2.Length);
				bw.Write(buffer);
				array2 = msw.ToArray();
				msw.SetLength(0L);
				result = array2;
			}
			return result;
		}

		public static PacketData ReadData(byte[] buf)
		{
			PacketData result = new PacketData();
			lock (obj)
			{
				buf = decryptor.TransformFinalBlock(buf, 0, buf.Length);
				msr = new MemoryStream(buf);
				br = new BinaryReader(msr);
				if (br.ReadBoolean())
				{
					int num = br.ReadInt32();
					DeflateStream stream = new DeflateStream(msr, CompressionMode.Decompress, false);
					byte[] array = new byte[num];
					stream.Read(array, 0, array.Length);
					stream.Close();
					msr = new MemoryStream(array);
					br = new BinaryReader(msr);
				}
				result.Type = br.ReadByte();
				result.Command = br.ReadByte();
				if (br.ReadBoolean())
				{
					result.Guid = new Guid(br.ReadBytes(16));
				}
				while (msr.Position != msr.Length)
				{
					switch (br.ReadByte())
					{
						case 0:
							data.Add(br.ReadBoolean());
							break;
						case 1:
							data.Add(br.ReadByte());
							break;
						case 2:
							data.Add(br.ReadBytes(br.ReadInt32()));
							break;
						case 3:
							data.Add(br.ReadChar());
							break;
						case 4:
							data.Add(br.ReadString().ToCharArray());
							break;
						case 5:
							data.Add(br.ReadDecimal());
							break;
						case 6:
							data.Add(br.ReadDouble());
							break;
						case 7:
							data.Add(br.ReadInt32());
							break;
						case 8:
							data.Add(br.ReadInt64());
							break;
						case 9:
							data.Add(br.ReadSByte());
							break;
						case 10:
							data.Add(br.ReadInt16());
							break;
						case 11:
							data.Add(br.ReadSingle());
							break;
						case 12:
							data.Add(br.ReadString());
							break;
						case 13:
							data.Add(br.ReadUInt32());
							break;
						case 14:
							data.Add(br.ReadUInt64());
							break;
						case 15:
							data.Add(br.ReadUInt16());
							break;
						case 16:
							data.Add(DateTime.FromBinary(br.ReadInt64()));
							break;
						case 17:
							{
								string[] array2 = new string[br.ReadInt32() - 1 + 1];
								int num2 = 0;
								int num3 = array2.Length - 1;
								for (int i = num2; i <= num3; i++)
								{
									array2[i] = br.ReadString();
								}
								data.Add(array2);
								break;
							}
						case 18:
							{
								Guid guid = new Guid(br.ReadBytes(16));
								data.Add(guid);
								break;
							}
						case 19:
							{
								Size size = new Size(br.ReadInt32(), br.ReadInt32());
								data.Add(size);
								break;
							}
						case 20:
							{
								Rectangle rectangle = new Rectangle(br.ReadInt32(), br.ReadInt32(), br.ReadInt32(), br.ReadInt32());
								data.Add(rectangle);
								break;
							}
						case 21:
							data.Add(new Version(br.ReadString()));
							break;
					}
				}
				result.Buffer = data.ToArray();
				data.Clear();
				br.Close();
			}
			return result;
		}
	}
}