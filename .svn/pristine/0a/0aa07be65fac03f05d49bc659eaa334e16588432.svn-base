using DriverInterface;
using IronUtilites;
using System;
using System.Collections.Generic;
using System.Xml;

namespace ModbusRTU
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

        public bool CommunicationStatus()
        {
            //if (comm == null || !comm.IsOpen())
            //{
            //    return false;
            //}

            //return comm.IsCommunication();
            return false;
        }

        public void Connect()
        {
            comm = new Communication();
            comm.Port = xmlInfo.port;

            try
            {
                comm.BaudRate = ushort.Parse(xmlInfo.baudrate);
                comm.Connect();
            }
            catch(FormatException ex)
            {
                throw new FormatException("Unable to convert BaudRate.", ex);
            }
            catch(Exception ex)
            {
                IronUtilites.LogManager.Manager.WriteLog("Exception", "Connection Failed : " + ex.Message);
            }
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

                requestCount = Convert.ToInt32(xmlInfo.requestCount);
                connectTime = Convert.ToInt32(xmlInfo.connectTime);
                requestTime = Convert.ToInt32(xmlInfo.requestTime);
            }
            catch (Exception ex)
            {
                IronUtilites.LogManager.Manager.WriteLog("Exception", "Read Xml File Failed Error Message : " + ex.Message);
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
                            tag.type = tag.type.ToLower();
                            tag.memory = tag.memory.ToLower();

                            int tagType = (int)Enum.Parse(typeof(Definition.DataType), tag.type.ToUpper());

                            tag.addr = (Convert.ToInt32(tag.addr)).ToString();
                            int addr = (Convert.ToInt32(tag.addr));

                            uint.TryParse(tag.scanrate, out uint scantime);
                            bool.TryParse(tag.redis, out bool redis);

                            if (addr + (tagType * size) - 1 < 65536)
                            {
                                if (tagInfos.ContainsKey(tag.id))
                                {
                                    IronUtilites.LogManager.Manager.WriteLog("Warning", $"This Tag Name- {tag.id} already used");
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
            }
        }

        #region Read
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
            bool isWrite = false;

            List<int> addr = new List<int>();
            List<int> length = new List<int>();

            addr.Add(singleAddr);
            length.Add(singleSize);

            if (memoryType == Coil || memoryType == HoldingRegister)
            {
                isWrite = true;
            }

            switch (memoryType)
            {
                case Coil:
                case Input:
                    if (!comm.GetBits(addr.ToArray(), length.ToArray(), ref readbuff, isWrite, requestCount))
                    {
                        return false;
                    }
                    break;
                case InputRegister:
                case HoldingRegister:
                    if (!comm.GetRegisters(addr.ToArray(), length.ToArray(), ref readbuff, isWrite, requestCount))
                    {
                        return false;
                    }
                    break;
            }

            data = readbuff;
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
							CreateLists(coilList, coilDic, tagName, addr);
							break;
						case Input:
							CreateLists(inputList, inputDic, tagName, addr);
							break;
						case InputRegister:
							CreateLists(irList, irDic, tagName, addr);
							break;
						case HoldingRegister:
							CreateLists(hrList, hrDic, tagName, addr);
							break;
					}
				}
			}

			// Coil
			if (coilList.Count > 0)
			{
				bool result = GetBits(coilList, coilDic, valueLists, true);
			}

			// Input
			if (inputList.Count > 0)
			{
                bool result = GetBits(inputList, inputDic, valueLists, false);
            }

			// Holding Register
			if (hrList.Count > 0)
			{
                bool result = GetRegisters(hrList, hrDic, valueLists, true);
            }

			// Input Register
			if (irList.Count > 0)
			{
                bool result = GetRegisters(irList, irDic, valueLists, false);
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
        #endregion

        #region Write
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

                        switch (dataType)
                        {
                            case Coil:
                                if (!comm.SetBit(singleAddr, length, ref dataList, requestCount))
                                {
                                    return false;
                                }
                                break;
                            case HoldingRegister:
                                if (!comm.SetRegister(singleAddr, length, ref dataList, requestCount))
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

        public bool WriteTags(string[] tagNames, object[] datas)
        {
            throw new NotImplementedException();
        }
        #endregion

    }
}
