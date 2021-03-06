using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using DriverInterface;
using IronUtilites;

namespace Omron_FINS
{
    public class FINS : IDriver
    {
		const string CIO = "cio";
		const string WR = "wr";
		const string HR = "hr";
		const string DM = "dm";
		const string EM = "em";

		const int cioBitMaxSize = 614315;
		const int cioWordMaxSize = 6143;
		const int wrhrBitMaxSize = 51115;
		const int wrhrWordMaxSize = 511;
		const int dmemBitMaxSize = 3276715;
		const int dmemWordMaxSize = 32767;

		Communication comm = null;
        bool hexAddress = false;
        Driver xmlInfo = null;
        public Dictionary<string, Tag> tagInfos = new Dictionary<string, Tag>();
		int requestCount = 3;
		int connectTime = 1;
		int requestTime = 1;

		public bool CommunicationStatus()
        {
			if (comm == null || !comm.IsOpen())
			{
				return false;
			}

			return comm.IsCommunication();
		}

        public void Connect()
        {
			
			comm = new Communication(false, "", "", false, xmlInfo.ipServerAddr, ushort.Parse(xmlInfo.serverPort), xmlInfo.ipClientaddr, ushort.Parse(xmlInfo.clientPort), requestTime, connectTime);
			string[] tempArray = xmlInfo.ipServerAddr.Split('.');
			comm.da1 = (byte)Convert.ToInt32(tempArray[tempArray.Length - 1]);

			tempArray = xmlInfo.ipClientaddr.Split('.');
			comm.sa1 = (byte)Convert.ToInt32(tempArray[tempArray.Length - 1]);
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

		public void GetTagInfo(ref string[] strName, ref string[] strType, ref bool[] bRedis, ref int[] iSize, ref string[] strMemory)
        {
			try
			{
				List<string> nameList = new List<string>();
				List<string> typeList = new List<string>();
				//List<uint> scanTimeList = new List<uint>();
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

							int memoryMaxSize = 0;

                            switch (tag.memory)
                            {
								case CIO:
									if(tag.type == Definition.BOOL)
										memoryMaxSize = cioBitMaxSize;
                                    else
										memoryMaxSize = cioWordMaxSize;
									break;
								case WR:
								case HR:
									if (tag.type == Definition.BOOL)
										memoryMaxSize = wrhrBitMaxSize;
									else
										memoryMaxSize = wrhrWordMaxSize;
									break;
								case DM:
								case EM:
									if (tag.type == Definition.BOOL)
										memoryMaxSize = dmemBitMaxSize;
									else
										memoryMaxSize = dmemWordMaxSize;
									break;
							}

							if (addr + (tagType * size) - 1 < memoryMaxSize)
							{
								if (tagInfos.ContainsKey(tag.id))
								{
									IronUtilites.LogManager.Manager.WriteLog("Warning", $"This Tag Name- {tag.id} alread used");
								}
								else
								{
									nameList.Add(tag.id);
									typeList.Add(tag.type);
									//scanTimeList.Add(scantime);
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
				//uScantime = scanTimeList.ToArray();
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

        public bool ReadTag(string tagName, ref object data)
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

			string memoryType = tagInfos[tagName].memory.ToLower();
			string dataType = tagInfos[tagName].type;
			byte[] readbuff = new byte[singleSize * 2];

			List<int> addr = new List<int>();
			List<int> length = new List<int>();

			addr.Add(singleAddr);
			length.Add(singleSize);

			byte[] value = new byte[length[0] * 2];

			if (dataType == Definition.BOOL)
            {
				if (!comm.GetBits(addr.ToArray(), length.ToArray(), ref readbuff, memoryType, requestCount))
				{
					return false;
                }
                else
                {
					for (int j = 0; j < length[0]; j++)
					{
						value[j * 2] = readbuff[j];
					}
				}
			}
            else
            {
				if (!comm.GetRegisters(addr.ToArray(), length.ToArray(), ref readbuff, memoryType, requestCount))
				{
					return false;
				}
			}

			data = value;
			return true;
		}

        public bool ReadTags(string[] tagNames, ref object[] datas)
        {
			#region variables
			List<int> cioBitList = new List<int>();
			List<int> cioWordList = new List<int>();
			List<int> wrBitList = new List<int>();
			List<int> wrWordList = new List<int>();
			List<int> hrBitList = new List<int>();
			List<int> hrWordList = new List<int>();
			List<int> dmBitList = new List<int>();
			List<int> dmWordList = new List<int>();
			List<int> emBitList = new List<int>();
			List<int> emWordList = new List<int>();

			Dictionary<int, List<string>> cioBitDic = new Dictionary<int, List<string>>();
			Dictionary<int, List<string>> cioWordDic = new Dictionary<int, List<string>>();
			Dictionary<int, List<string>> wrBitDic = new Dictionary<int, List<string>>();
			Dictionary<int, List<string>> wrWordDic = new Dictionary<int, List<string>>();
			Dictionary<int, List<string>> hrBitDic = new Dictionary<int, List<string>>();
			Dictionary<int, List<string>> hrWordDic = new Dictionary<int, List<string>>();
			Dictionary<int, List<string>> dmBitDic = new Dictionary<int, List<string>>();
			Dictionary<int, List<string>> dmWordDic = new Dictionary<int, List<string>>();
			Dictionary<int, List<string>> emBitDic = new Dictionary<int, List<string>>();
			Dictionary<int, List<string>> emWordDic = new Dictionary<int, List<string>>();

			Dictionary<string, object> valueLists = new Dictionary<string, object>();

			string dataType = "";
            #endregion

            foreach (var tagName in tagNames)
			{
				if (tagInfos.ContainsKey(tagName))
				{
					int addr = 0;
					dataType = tagInfos[tagName].type.ToLower();

					try
					{
						addr = int.Parse(tagInfos[tagName].addr);
					}
					catch (Exception ex)
					{
						throw new FormatException("", ex);
					}

					switch (tagInfos[tagName].memory)
					{
						case CIO:
							if (dataType == Definition.BOOL)
								CreateLists(cioBitList, cioBitDic, tagName, addr);
							else
								CreateLists(cioWordList, cioWordDic, tagName, addr);
							break;
						case WR:
							if (dataType == Definition.BOOL)
								CreateLists(wrBitList, wrBitDic, tagName, addr);
							else
								CreateLists(wrWordList, wrWordDic, tagName, addr);
							break;
						case HR:
							if (dataType == Definition.BOOL)
								CreateLists(hrBitList, hrBitDic, tagName, addr);
							else
								CreateLists(hrWordList, hrWordDic, tagName, addr);
							break;
						case DM:
							if (dataType == Definition.BOOL)
								CreateLists(dmBitList, dmBitDic, tagName, addr);
							else
								CreateLists(dmWordList, dmWordDic, tagName, addr);
							break;
						case EM:
							if (dataType == Definition.BOOL)
								CreateLists(emBitList, emBitDic, tagName, addr);
							else
								CreateLists(emWordList, emWordDic, tagName, addr);
							break;
					}
				}
			}

			// CIO
			if (cioBitList.Count > 0)
			{
				if(!GetBits(cioBitList, cioBitDic, valueLists, FINS.CIO))
                {
					return false;
                }
			}

			if (cioWordList.Count > 0)
            {
				if(!GetRegisters(cioWordList, cioWordDic, valueLists, FINS.CIO))
				{
					return false;
				}
			}

			// WR
			if (wrBitList.Count > 0)
			{
				if(!GetBits(wrBitList, wrBitDic, valueLists, FINS.WR))
				{
					return false;
				}
			}

			if (wrWordList.Count > 0)
			{
				if(!GetRegisters(wrWordList, wrWordDic, valueLists, FINS.WR))
				{
					return false;
				}
			}

			// HR
			if (hrBitList.Count > 0)
			{
				if(!GetBits(hrBitList, hrBitDic, valueLists, FINS.HR))
				{
					return false;
				}
			}

			if (hrWordList.Count > 0)
			{
				if(!GetRegisters(hrWordList, hrWordDic, valueLists, FINS.HR))
				{
					return false;
				}
			}

			// DM
			if (dmBitList.Count > 0)
			{
				if(!GetBits(dmBitList, dmBitDic, valueLists, FINS.DM))
				{
					return false;
				}
			}

			if (dmWordList.Count > 0)
			{
				if(!GetRegisters(dmWordList, dmWordDic, valueLists, FINS.DM))
				{
					return false;
				}
			}

			// EM
			if (emBitList.Count > 0)
			{
				if(!GetBits(emBitList, emBitDic, valueLists, FINS.EM))
				{
					return false;
				}
			}

			if (emWordList.Count > 0)
			{
				if(!GetRegisters(emWordList, emWordDic, valueLists, FINS.EM))
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

		private bool GetBits(List<int> bitList, Dictionary<int, List<string>> bitDic, Dictionary<string, object> valueLists, string memoryType)
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

			if (!comm.GetBits(bitList.ToArray(), lengths.ToArray(), ref readBuff, memoryType, requestCount))
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

			if (!comm.GetRegisters(wordList.ToArray(), lengths.ToArray(), ref readBuff, memoryType, requestCount))
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

		public bool WriteTag(string tagName, object data)
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

					if (data is byte[] dataList)
					{
						int length = singleSize * (int)Enum.Parse(typeof(Definition.DataType), tagInfos[tagName].type.ToUpper());


						if(dataType == Definition.BOOL)
                        {
							if(!comm.SetBit(singleAddr, length, ref dataList, memoryType, requestCount))
                            {
								return false;
                            }
                        }
                        else
                        {
							if(!comm.SetRegister(singleAddr, length, ref dataList, memoryType, requestCount))
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

        public bool WriteTags(string[] tagNames, object[] datas)
        {
			return false;
        }
    }
}
