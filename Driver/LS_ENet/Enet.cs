using DriverInterface;
using IronUtilites;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;

namespace LS_ENet
{
	class Enet : IDriver
	{
		const string F = "f";
		const string N = "n";
		const string P = "p";
		const string M = "m";
		const string K = "k";
		const string T = "t";
		const string C = "c";
		const string L = "l";
		const string D = "d";
		const string Z = "z";
		const string S = "s";

		Communication comm = null;
		bool hexAddress = false;
		Driver xmlInfo = null;
		public Dictionary<string, Tag> tagInfos = new Dictionary<string, Tag>();
		int requestCount = 3;
		int connectTime = 1;
		int requestTime = 1;
		public void Connect()
		{
			comm = new Communication(false, "", "", false, xmlInfo.ipaddr, ushort.Parse(xmlInfo.port), requestTime, connectTime);
			comm.Connect();
			comm.SetTimeout(requestCount);
		}

		public void Disconnect()
		{
			comm.DisConnect();
		}

		public void SetDriverInfo(bool bHexAddress, int nScanMode, string param)
		{
			try
			{
				hexAddress = bHexAddress;
				string strFilename = param;

				XmlDocument doc = new XmlDocument();
				doc.Load(strFilename);

				xmlInfo = new Driver();
				xmlInfo = (Driver)XMLParser.DeserializeXML(strFilename, typeof(Driver));

				if (Convert.ToInt32(xmlInfo.requestCount) != 0)
				{
					requestCount = Convert.ToInt32(xmlInfo.requestCount);
				}
				if (Convert.ToInt32(xmlInfo.connectTime) != 0)
				{
					connectTime = Convert.ToInt32(xmlInfo.connectTime);
				}
				if (Convert.ToInt32(xmlInfo.requestTime) != 0)
				{
					requestTime = Convert.ToInt32(xmlInfo.requestTime);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Read Xml File Failed Error Message : " + ex.Message);
			}
		}

		public void GetTagInfo(ref string[] strName, ref string[] strType, ref bool[] bRedis, ref int[] iSize, ref string[] strMemory)
		{
			try
			{
				List<string> nameList = new List<string>();
				List<string> typeList = new List<string>();
				List<bool> redisList = new List<bool>();
				List<int> sizeList = new List<int>();
				List<string> memoryList = new List<string>();

				foreach (var tag in xmlInfo.Tag)
				{
					try
					{
						int size = int.Parse(tag.size);

						if (size > 0)
						{
							tag.memory = tag.memory.ToLower();
							tag.type = tag.type.ToLower();

							int tagType = (int)Enum.Parse(typeof(Definition.DataType), tag.type.ToUpper());

							int addr = 0;

							if (hexAddress)
							{
								if (tag.type == Definition.BOOL)
								{
									addr = Definition.HexToDec(tag.addr.Substring(0, tag.addr.Length - 1)) * 16 + Definition.HexToDec(tag.addr.Substring(tag.addr.Length - 1));
								}
								else
								{
									addr = Definition.HexToDec(tag.addr);
								}
							}
							else
							{
								addr = (Convert.ToInt32(tag.addr));
							}

							tag.addr = addr.ToString();

							uint.TryParse(tag.scanrate, out uint scantime);
							bool.TryParse(tag.redis, out bool redis);

							if (addr + (tagType * size) - 1 < 1048577)
							{
								if (tagInfos.ContainsKey(tag.id))
								{
									IronUtilites.LogManager.Manager.WriteLog("Warning", $"This Tag Name- {tag.id} alread used");
								}
								else
								{
									nameList.Add(tag.id);
									typeList.Add(tag.type);
									redisList.Add(redis);
									sizeList.Add(size);
									tagInfos[tag.id] = tag;
									memoryList.Add(tag.memory);
								}
							}
							else
							{
								IronUtilites.LogManager.Manager.WriteLog("Warning", $"This Tag - {tag.id} Address can use between 0 and 65535");
							}
						}
						else
						{
							IronUtilites.LogManager.Manager.WriteLog("Warning", "Tag Size must be over 0");
						}
					}
					catch (FormatException ex)
					{
						IronUtilites.LogManager.Manager.WriteLog("Exception", "Only integer type can be used for size. : " + ex.Message);
					}
				}

				strName = nameList.ToArray();
				strType = typeList.ToArray();
				bRedis = redisList.ToArray();
				iSize = sizeList.ToArray();
				strMemory = memoryList.ToArray();
			}
			catch (Exception ex)
			{
				IronUtilites.LogManager.Manager.WriteLog("Exception", "Get Driver Info Failed messaged : " + ex.Message);
				Debug.WriteLine("Get Driver Info Failed messaged : " + ex.Message);
			}
		}
		bool IDriver.ReadTag(string tagName, ref object data)
		{
			int singleAddr = 0;
			int singleSize = 0;

			try
			{
				singleAddr = int.Parse(tagInfos[tagName].addr);
				singleSize = Convert.ToInt32(tagInfos[tagName].size) * (int)Enum.Parse(typeof(Definition.DataType), tagInfos[tagName].type.ToUpper());
			}
			catch (Exception ex)
			{
				throw new FormatException("Invalid Tag Name", ex);
			}

			string memoryType = tagInfos[tagName].memory.ToUpper();
			string dataType = tagInfos[tagName].type;
			byte[] readbuff = new byte[singleSize * 2];


			List<int> addr = new List<int>();
			List<int> length = new List<int>();

			addr.Add(singleAddr);
			length.Add(singleSize);

			string deviceType = memoryType.Substring(0, 1).ToUpper();
			byte device = Encoding.ASCII.GetBytes(deviceType)[0];

			if (dataType == Definition.BOOL)
			{
				if (!comm.GetBits(addr.ToArray(), length.ToArray(), ref readbuff, device, requestCount))
				{
					return false;
				}
			}
			else
			{
				if (!comm.GetRegisters(addr.ToArray(), length.ToArray(), ref readbuff, device, requestCount))
				{
					return false;
				}
			}

			data = readbuff;
			return true;
		}

		bool IDriver.CommunicationStatus()
		{
			if (comm == null || !comm.IsOpen() || comm.isError)
			{
				return false;
			}

			return comm.IsCommunication();
		}

		bool IDriver.ReadTags(string[] tagNames, ref object[] datas)
		{
			Dictionary<string, VarClass> varClassDIc = new Dictionary<string, VarClass>();
			Dictionary<string, object> valueLists = new Dictionary<string, object>();

			foreach (var tagName in tagNames)
			{
				if (tagInfos.ContainsKey(tagName))
				{
					int addr = 0;

					try
					{
						addr = int.Parse(tagInfos[tagName].addr);
					}
					catch (Exception ex)
					{
						throw new FormatException("", ex);
					}

					string dataType = tagInfos[tagName].type;
					string memory = tagInfos[tagName].memory;

					CreateLists(varClassDIc, memory, dataType, tagName, addr);
				}
			}

			foreach (var memoryType in varClassDIc.Keys)
			{
				if (memoryType.Substring(1,1) == "b")
				{
					if(!GetBits(varClassDIc[memoryType].memoryList, varClassDIc[memoryType].memoryDic, valueLists, memoryType.Substring(0, 1)))
                    {
						return false;
                    }
				}
				else
				{
					if(!GetRegisters(varClassDIc[memoryType].memoryList, varClassDIc[memoryType].memoryDic, valueLists, memoryType.Substring(0, 1)))
					{
						return false;
					}
				}
			}

			for (int i = 0; i < tagNames.Length; i++)
			{
				if (valueLists.ContainsKey(tagNames[i]))
				{
					datas[i] = valueLists[tagNames[i]];
				}
			}

			return true;
		}

		private void CreateLists(Dictionary<string, VarClass> varClassDIc, string memory, string dataType, string tagName, int addr)
		{
			string key = "";

			if(dataType == Definition.BOOL)
            {
				key = memory + "b";
            }
            else
            {
				key = memory + "w";
            }

			if (!varClassDIc.ContainsKey(key))
			{
				varClassDIc[key] = new VarClass();
				varClassDIc[key].memoryList = new List<int>() { addr };
				varClassDIc[key].memoryDic = new Dictionary<int, List<string>>() { [addr] = new List<string>() { tagName } };
			}
			else
			{
				varClassDIc[key].memoryList.Add(addr);

				if (!varClassDIc[key].memoryDic.ContainsKey(addr))
				{
					varClassDIc[key].memoryDic[addr] = new List<string>() { tagName };
				}
				else
				{
					varClassDIc[key].memoryDic[addr].Add(tagName);
				}
			}
		}

		private bool GetBits(List<int> bitList, Dictionary<int, List<string>> bitDic, Dictionary<string, object> valueLists, string memoryType)
		{
			string deviceType = memoryType.Substring(0, 1).ToUpper();
			byte device = Encoding.ASCII.GetBytes(deviceType)[0];

			List<int> lengths = new List<int>();
			int buffSize = 0;

			bitList.Sort();

			for (int i = 0; i < bitList.Count;)
			{
				foreach (string tagName in bitDic[bitList[i]])
				{
					int addrSize = Convert.ToInt32(tagInfos[tagName].size) * (int)Enum.Parse(typeof(Definition.DataType), tagInfos[tagName].type.ToUpper());
					lengths.Add(addrSize);

					buffSize += addrSize;
					i++;
				}
			}

			byte[] readBuff = new byte[buffSize];

			if (!comm.GetBits(bitList.ToArray(), lengths.ToArray(), ref readBuff, device, requestCount))
			{
				return false;
			}

			int index = 0;

			for (int i = 0; i < bitList.Count;)
			{
				foreach (string tagName in bitDic[bitList[i]])
				{
					byte[] value = new byte[lengths[i] * 2];
					int j = 0;

					for (; j < lengths[i]; j++)
					{
						value[j * 2] = readBuff[index + j];
					}

					valueLists.Add(tagName, value);

					index += j;
					i++;
				}
			}

			return true;
		}

		private bool GetRegisters(List<int> wordList, Dictionary<int, List<string>> wordDic, Dictionary<string, object> valueLists, string memoryType)
		{
			string deviceType = memoryType.Substring(0, 1).ToUpper();
			byte device = Encoding.ASCII.GetBytes(deviceType)[0];

			List<int> lengths = new List<int>();
			int buffSize = 0;

			wordList.Sort();

			for (int i = 0; i < wordList.Count;)
			{
				foreach (string tagName in wordDic[wordList[i]])
				{
					int addrSize = Convert.ToInt32(tagInfos[tagName].size) * (int)Enum.Parse(typeof(Definition.DataType), tagInfos[tagName].type.ToUpper());
					lengths.Add(addrSize);

					buffSize += addrSize * 2;
					i++;
				}
			}

			byte[] readBuff = new byte[buffSize];

			if (!comm.GetRegisters(wordList.ToArray(), lengths.ToArray(), ref readBuff, device, requestCount))
			{
				return false;
			}

			int index = 0;

			for (int i = 0; i < wordList.Count;)
			{
				foreach (string tagName in wordDic[wordList[i]])
				{
					byte[] value = new byte[lengths[i] * 2];
					int j = 0;

					for (; j < lengths[i] * 2; j++)
					{
						value[j] = readBuff[index + j];
					}

					valueLists.Add(tagName, value);

					index += j;
					i++;
				}
			}

			return true;
		}


		bool IDriver.WriteTag(string tagName, object data)
		{
			if (tagInfos.ContainsKey(tagName))
			{
				int singleAddr = 0;
				int singleSize = 0;
				string memoryType = "";
				string dataType = "";

				try
				{
					singleAddr = int.Parse(tagInfos[tagName].addr);
					singleSize = int.Parse(tagInfos[tagName].size);

					memoryType = tagInfos[tagName].memory;
					dataType = tagInfos[tagName].type;

					if (memoryType == "" || dataType == "")
					{
						throw new ArgumentException("memory or Type cannot be empty");
					}
					else if(memoryType == F || memoryType == N)
                    {
						return false;
                    }

					if (data is byte[] dataList)
					{
						int length = singleSize * (int)Enum.Parse(typeof(Definition.DataType), tagInfos[tagName].type.ToUpper());

						string deviceType = memoryType.Substring(0, 1).ToUpper();
						byte device = Encoding.ASCII.GetBytes(deviceType)[0];

						if (memoryType == S)
						{
							return false;
						}

						if (dataType == Definition.BOOL)
						{
                            switch (memoryType)
                            {
								case T:
								case C:
									if(!SetTCMemory(singleAddr, singleSize, device, dataList))
                                    {
										return false;
                                    }
									break;
								default:
									if (!comm.SetBit(singleAddr, length, ref dataList, device, requestCount))
									{
										return false;
									}
									break;
                            }
						}
						else
						{
							if (!comm.SetRegister(singleAddr, length, ref dataList, device, requestCount))
							{
								return false;
							}
						}

						IronUtilites.LogManager.Manager.WriteLog("Console", $"Write Tag - TagName : {tagName} , WriteData : {BitConverter.ToString(dataList)}");

						return true;
					}
				}
				catch (Exception ex)
				{
					LogManager.Manager.WriteLog("Exception", ex.Message);
					LogManager.Manager.WriteLog("Exception", $"The Tag's Address, Size, memoryType is not Invalid, tag : {tagName}");
				}

				return false;
			}

			return false;
		}

		bool IDriver.WriteTags(string[] tagNames, object[] datas)
		{
			return false;
		}

		bool SetTCMemory(int singleAddr, int length, byte device, byte[] writebuff)
		{
			int[] addr = new int[1] { singleAddr / 16 };
			int[] len = new int[1];
			
			int tempLength = ((length + singleAddr % 16) / 16) + ((length + singleAddr % 16) % 16 > 0 ? 1 : 0); // Address Length
			
			len[0] = tempLength;

			byte[] readDatas = new byte[len[0] * 2];

			if(!comm.GetRegisters(addr, len, ref readDatas, device, requestCount))
            {
				return false;
            }

			byte[] tempData = new byte[writebuff.Length / 2];

			for (int k = 0; k < writebuff.Length / 2; k++)
			{
				tempData[k] = writebuff[k * 2];
			}

			BitArray bitArr = new BitArray(readDatas);
			bool[] bytes = bitArr.Cast<bool>().Select(bit => bit ? true : false).ToArray();

			int index = 0;
			int offset = singleAddr % 16;

			for (int j = offset; j < bytes.Length; j++)
			{
				if (tempData.Length > index)
				{
					bytes[j] = tempData[index++] == 1 ? true : false;
				}
				else
				{
					break;
				}
			}

			byte[] setBytes = new byte[bytes.Length / 8];
			bitArr = new BitArray(bytes);
			bitArr.CopyTo(setBytes, 0);

			if (!comm.SetRegister(addr[0], len[0], ref setBytes, device, requestCount))
			{
				return false;
			}

			return true;
		}
	}

	#region variables
	public class VarClass
	{
		public List<int> memoryList = new List<int>();
		
		public Dictionary<int, List<string>> memoryDic = new Dictionary<int, List<string>>();
	}
	#endregion
}
