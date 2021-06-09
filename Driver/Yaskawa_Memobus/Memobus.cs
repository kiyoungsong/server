using DriverInterface;
using IronUtilites;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

namespace Yaskawa_Memobus
{
	public class Memobus : IDriver
    {
		const string M = "m";
		const string I = "i";

		Communication comm = null;
		bool hexAddress = false;
		Driver xmlInfo = null;
		public Dictionary<string, Tag> tagInfos = new Dictionary<string, Tag>();
		int requestCount = 3;
		int connectTime = 1;
		int requestTime = 1;

		public void Connect()
		{
			comm = new Communication(false, "", "", false, xmlInfo.ipServerAddr, ushort.Parse(xmlInfo.serverPort), xmlInfo.ipClientaddr, ushort.Parse(xmlInfo.clientPort), requestTime, connectTime);
			comm.Connect();
			comm.SetTimeout(requestCount);
		}

		public void Disconnect()
		{
			comm.Disconnect();
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

		public void GetTagInfo(ref string[] strName, ref string[] strTyp, ref bool[] bRedis, ref int[] iSize, ref string[] strMemory)
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
									addr = Definition.HexToDec(tag.addr.Substring(0, tag.addr.Length - 1)) * 16 + Definition.HexToDec(tag.addr.Substring(tag.addr.Length -1));
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
				strTyp = typeList.ToArray();
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

			string memoryType = tagInfos[tagName].memory;
			string dataType = tagInfos[tagName].type;
			byte[] readbuff = new byte[singleSize * 2];
			bool isWrite = false;

			if (memoryType == M)
			{
				isWrite = true;
			}

			List<int> addr = new List<int>();
			List<int> length = new List<int>();

			addr.Add(singleAddr);
			length.Add(singleSize);

			if (dataType == Definition.BOOL)
			{
				if (!comm.GetBits(addr.ToArray(), length.ToArray(), ref readbuff, isWrite, requestCount))
				{
					return false;
				}
            }
            else
            {
				if (!comm.GetRegisters(addr.ToArray(), length.ToArray(), ref readbuff, isWrite, requestCount))
				{
					return false;
				}
			}

			byte[] value = new byte[length[0] * 2];

			for (int j = 0; j < length[0] * 2; j++)
			{
				value[j] = readbuff[j];
			}

			data = value;
			return true;
		}

		bool IDriver.CommunicationStatus()
		{
			if (comm == null || !comm.IsOpen())
			{
				return false;
			}

			return comm.IsCommunication();
		}

		bool IDriver.ReadTags(string[] tagNames, ref object[] datas)
		{
			List<int> coilList = new List<int>();
			List<int> inputList = new List<int>();
			List<int> hrList = new List<int>();
			List<int> irList = new List<int>();

			Dictionary<int, List<string>> coilDic = new Dictionary<int, List<string>>();
			Dictionary<int, List<string>> inputDic = new Dictionary<int, List<string>>();
			Dictionary<int, List<string>> hrDic = new Dictionary<int, List<string>>();
			Dictionary<int, List<string>> irDic = new Dictionary<int, List<string>>();

			Dictionary<string, object> valueLists = new Dictionary<string, object>();

			foreach (var tagName in tagNames)
			{
				if (tagInfos.ContainsKey(tagName))
				{
					
					int addr = 0;
					string dataType = "";
					try
					{
						addr = int.Parse(tagInfos[tagName].addr);
						dataType = tagInfos[tagName].type;
					}
					catch (Exception ex)
					{
						throw new FormatException("", ex);
					}

					switch (tagInfos[tagName].memory)
					{
						case M:
							if(dataType == Definition.BOOL)
								CreateLists(coilList, coilDic, tagName, addr);
                            else
								CreateLists(hrList, hrDic, tagName, addr);
							break;
						case I:
							if (dataType == Definition.BOOL)
								CreateLists(inputList, inputDic, tagName, addr);
                            else
								CreateLists(irList, irDic, tagName, addr);
							break;
					}
				}
			}

			// Coil
			if (coilList.Count > 0)
			{
				if(!GetBits(coilList, coilDic, valueLists, true))
				{
					return false;
				}
			}

			// Input
			if (inputList.Count > 0)
			{
				if(!GetBits(inputList, inputDic, valueLists, false))
				{
					return false;
				}
			}

			// Input Register
			if (irList.Count > 0)
			{
				if(!GetRegisters(irList, irDic, valueLists, false))
				{
					return false;
				}
			}

			// Holding Register
			if (hrList.Count > 0)
			{
				if(!GetRegisters(hrList, hrDic, valueLists, true))
				{
					return false;
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

		private static void CreateLists(List<int> memoryList, Dictionary<int, List<string>> memoryDic, string tagName, int addr)
		{
			memoryList.Add(addr);

			if (memoryDic.ContainsKey(addr))
			{
				memoryDic[addr].Add(tagName);
			}
			else
			{
				memoryDic.Add(addr, new List<string> { tagName });
			}
		}

		private bool GetBits(List<int> bitList, Dictionary<int, List<string>> bitDic, Dictionary<string, object> valueLists, bool isWrite)
		{
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

			if (!comm.GetBits(bitList.ToArray(), lengths.ToArray(), ref readBuff, isWrite, requestCount))
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

		private bool GetRegisters(List<int> wordList, Dictionary<int, List<string>> wordDic, Dictionary<string, object> valueLists, bool isWrite)
		{
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

			if (!comm.GetRegisters(wordList.ToArray(), lengths.ToArray(), ref readBuff, isWrite, requestCount))
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

					if (memoryType == I)
					{
						return false;
					}

					if (data is byte[] dataList)
					{
						int length = singleSize * (int)Enum.Parse(typeof(Definition.DataType), tagInfos[tagName].type.ToUpper());

						switch (memoryType)
						{
							case M:
								if(dataType == Definition.BOOL)
                                {
									if (!comm.SetBit(singleAddr, length, ref dataList, requestCount))
									{
										return false;
									}
                                }
                                else
                                {
									if (!comm.SetRegister(singleAddr, length, ref dataList, requestCount))
									{
										return false;
									}
								}
								break;
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
    }
}
