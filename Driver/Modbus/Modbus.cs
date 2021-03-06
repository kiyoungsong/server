using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using DriverInterface;
using IronUtilites;

namespace Modbus
{   
	public class Modbus : IDriver
    {
		const string Coil = "coil";
		const string Input = "input";
		const string InputRegister = "inputregister";
		const string HoldingRegister = "holdingregister";

		Communication comm = null;
		Driver xmlInfo = null;
		Dictionary<string, Tag> tagInfos = new Dictionary<string, Tag>();
		int requestCount = 3;
		int connectTime = 1;
		int requestTime = 1;

		public void Connect()
        {
			comm = new Communication();

			comm.IpAddress = xmlInfo.ipaddr;
			comm.ConnectTime = connectTime;

			try
			{
				comm.Port = int.Parse(xmlInfo.port);
			}
			catch (FormatException ex)
			{
				throw new FormatException("Unable to convert port number.", ex);
			}

			comm.Connect();
		}

        public void Disconnect()
        {
			comm.Disconnect();
        }

        public void SetDriverInfo(bool bHexAddress, int nScanMode, string param)
		{
            try
            {
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
			catch(Exception ex)
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
                            tag.type = tag.type.ToLower();
                            tag.memory = tag.memory.ToLower();

                            int tagType = (int)Enum.Parse(typeof(Definition.DataType), tag.type.ToUpper());

                            tag.addr = (Convert.ToInt32(tag.addr)).ToString();
							int addr = (Convert.ToInt32(tag.addr));

                            uint.TryParse(tag.scanrate, out uint scantime);
                            bool.TryParse(tag.redis, out bool redis);

							if(addr + (tagType * size) - 1 < 65536)
                            {
								if(tagInfos.ContainsKey(tag.id))
                                {
									IronUtilites.LogManager.Manager.WriteLog("Warning", $"This Tag Name- {tag.id} already used");
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
			catch(Exception ex)
            {
				IronUtilites.LogManager.Manager.WriteLog("Exception", "Get Driver Info Failed messaged : " + ex.Message);
            }
		}

		bool IDriver.CommunicationStatus()
		{
			return comm.IsCommunication();
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

			string memoryType = tagInfos[tagName].memory;
			byte[] readbuff = new byte[singleSize * 2];
			
			List<int> addr = new List<int>();
			List<int> length = new List<int>();

			addr.Add(singleAddr);
			length.Add(singleSize);

			switch (memoryType)
            {
				case Coil:
					if(!comm.GetCoils(addr.ToArray(), length.ToArray(), ref readbuff, requestCount))
                    {
						return false;
                    }
					break;
				case Input:
					if (!comm.GetInputs(addr.ToArray(), length.ToArray(), ref readbuff, requestCount))
					{
						return false;
					}
					break;
				case InputRegister:
					if(!comm.GetInputRegisters(addr.ToArray(), length.ToArray(), ref readbuff, requestCount))
                    {
						return false;
                    }
					break;
				case HoldingRegister:
					if(!comm.GetHoldingRegisters(addr.ToArray(), length.ToArray(), ref readbuff, requestCount))
                    {
						return false;
                    }
					break;
			}

			byte[] value = new byte[length[0] * 2];

			for (int j = 0; j < length[0] * 2; j++)
			{
				value[j] = readbuff[j];
			}

			data = value;
			return true;
        }

		public bool ReadTags(string[] tagNames, ref object[] datas)
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
						case Coil:
							coilList.Add(addr);

							if (coilDic.ContainsKey(addr))
							{
								coilDic[addr].Add(tagName);
							}
							else
							{
								coilDic.Add(addr, new List<string> { tagName });
							}
							break;
						case Input:
							inputList.Add(addr);

							if (inputDic.ContainsKey(addr))
							{
								inputDic[addr].Add(tagName);
							}
							else
							{
								inputDic.Add(addr, new List<string> { tagName });
							}
							break;
						case InputRegister:
							irList.Add(addr);

							if (irDic.ContainsKey(addr))
							{
								irDic[addr].Add(tagName);
							}
							else
							{
								irDic.Add(addr, new List<string> { tagName });
							}
							break;
						case HoldingRegister:
							hrList.Add(addr);

							if(hrDic.ContainsKey(addr))
                            {
								hrDic[addr].Add(tagName);
							}
                            else
                            {
								hrDic.Add(addr, new List<string> { tagName });
							}
							break;
					}
				}
			}

			// Coil
			if(coilList.Count > 0 )
            {
				List<int> lengths = new List<int>();
				int buffSize = 0;

				coilList.Sort();

				for (int i = 0; i < coilList.Count;)
                {
					foreach (string tagName in coilDic[coilList[i]])
					{
						int addrSize = Convert.ToInt32(tagInfos[tagName].size) * (int)Enum.Parse(typeof(Definition.DataType), tagInfos[tagName].type.ToUpper());
						lengths.Add(addrSize);

						buffSize += addrSize * 2;
						i++;
					}
				}

				byte[] coilBuff = new byte[buffSize];

				if(!comm.GetCoils(coilList.ToArray(), lengths.ToArray(), ref coilBuff, requestCount))
                {
					return false;
                }

				int index = 0;

				for (int i = 0; i < coilList.Count;)
				{
					foreach(string tagName in coilDic[coilList[i]])
                    {
						byte[] value = new byte[lengths[i] * 2];
						int j = 0;

						for (; j < lengths[i] * 2; j++)
						{
							value[j] = coilBuff[index + j];
						}

						valueLists.Add(tagName, value);

						index += j;
						i++;
					}
				}
			}

			// Input
			if (inputList.Count > 0)
			{
				List<int> lengths = new List<int>();
				int buffSize = 0;

				inputList.Sort();

				for (int i = 0; i < inputList.Count;)
				{
					foreach (string tagName in inputDic[inputList[i]])
					{
						int addrSize = Convert.ToInt32(tagInfos[tagName].size) * (int)Enum.Parse(typeof(Definition.DataType), tagInfos[tagName].type.ToUpper());
						lengths.Add(addrSize);

						buffSize += addrSize * 2;
						i++;
					}
				}

				byte[] inputBuff = new byte[buffSize];

				if(!comm.GetInputs(inputList.ToArray(), lengths.ToArray(), ref inputBuff, requestCount))
                {
					return false;
                }

				int index = 0;

				for (int i = 0; i < inputList.Count;)
				{
					foreach (string tagName in inputDic[inputList[i]])
					{
						byte[] value = new byte[lengths[i] * 2];
						int j = 0;

						for (; j < lengths[i] * 2; j++)
						{
							value[j] = inputBuff[index + j];
						}

						valueLists.Add(tagName, value);

						index += j;
						i++;
					}
				}
			}
				
			// Holding Register
			if(hrList.Count > 0)
            {
				List<int> lengths = new List<int>();
				int buffSize = 0;
				
				hrList.Sort();

				for (int i = 0; i < hrList.Count;)
				{
					foreach (string tagName in hrDic[hrList[i]])
					{
						int addrSize = Convert.ToInt32(tagInfos[tagName].size) * (int)Enum.Parse(typeof(Definition.DataType), tagInfos[tagName].type.ToUpper());
						lengths.Add(addrSize);
						
						buffSize += addrSize * 2;
						i++;
					}
				}

				byte[] hrBuff = new byte[buffSize];

				if(!comm.GetHoldingRegisters(hrList.ToArray(), lengths.ToArray(), ref hrBuff, requestCount))
                {
					return false;
				}

				int index = 0;

				for (int i = 0; i < hrList.Count;)
				{
					foreach(string tagName in hrDic[hrList[i]])
                    {
						byte[] value = new byte[lengths[i] * 2];
						int j = 0;

						for (; j < lengths[i] * 2; j++)
						{
							value[j] = hrBuff[index + j];
						}
						
						valueLists.Add(tagName, value);

						index += j;
						i++;
					}					
				}
			}
			
			// Input Register
			if(irList.Count > 0)
            {
				List<int> lengths = new List<int>();
				int buffSize = 0;

				irList.Sort();

				for (int i = 0; i < irList.Count;)
				{
					foreach (string tagName in irDic[irList[i]])
					{
						int addrSize = Convert.ToInt32(tagInfos[tagName].size) * (int)Enum.Parse(typeof(Definition.DataType), tagInfos[tagName].type.ToUpper());
						lengths.Add(addrSize);

						buffSize += addrSize * 2;
						i++;
					}
				}

				byte[] irBuff = new byte[buffSize];

				if(!comm.GetInputRegisters(irList.ToArray(), lengths.ToArray(), ref irBuff, requestCount))
                {
					return false;
				}

				int index = 0;

				for (int i = 0; i < irList.Count;)
				{
					foreach (string tagName in irDic[irList[i]])
					{
						byte[] value = new byte[lengths[i] * 2];
						int j = 0;

						for (; j < lengths[i] * 2; j++)
						{
							value[j] = irBuff[index + j];
						}

						valueLists.Add(tagName, value);
						
						index += j;
						i++;
					}
				}
			}

			for (int i = 0; i < tagNames.Length;i++)
            {
				if(valueLists.ContainsKey(tagNames[i]))
                {
					datas[i]=valueLists[tagNames[i]];
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

					if (memoryType == Input || memoryType == InputRegister)
                    {
						return false;
                    }

					if (data is byte[] dataList)
					{
						int length = singleSize * (int)Enum.Parse(typeof(Definition.DataType), tagInfos[tagName].type.ToUpper());

						switch (memoryType)
						{
							case Coil:
								if(!comm.SetCoil(singleAddr, length, ref dataList, requestCount))
                                {
									return false;
								}
								break;
							case HoldingRegister:
								if (!comm.SetHoldingRegister(singleAddr, length, ref dataList, requestCount))
								{
									return false;
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

		public bool WriteTags(string[] tagNames, object[] data)
		{
			return false;
		}
	}
}
