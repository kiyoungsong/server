using DriverBase;
using DriverInterface;
using IronUtilites;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace Mistubishi_Melsec
{
    public class Melsec : TCP, IDriver
    {
        Communication comm;
        Driver xmlInfo;
        Dictionary<string, Tag> tagInfos = new Dictionary<string, Tag>();

        bool hexAddress = false;
        int requestCount = 3;
        int connectTime = 1;
        int requestTime = 1;
        public bool isConnected = false;

        Dictionary<string, int> initSizeArray; // Packet Size
        Dictionary<string, byte> initDic; // memoryType

        public bool CommunicationStatus()
        {
            isConnected = comm.CommunicationStatus();
            return isConnected;
        }

        public void Connect()
        {
            comm = new Communication(false, "", Definition.DRIVER +"\\" + xmlInfo.engine, false, xmlInfo.ipaddr, ushort.Parse(xmlInfo.port), requestTime, connectTime);
            comm.Connect();
            isConnected = comm.CommunicationStatus();
            comm.SetTimeout(requestTime);

            comm.Init(ref initDic,ref initSizeArray);
        }

        public void Disconnect()
        {
            comm.DIsConnect();
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
                            uint.TryParse(tag.scanrate, out uint scantime);
                            bool.TryParse(tag.redis, out bool redis);

                            int addr = 0;
                            if (hexAddress)
                            {
                                addr = Definition.HexToDec(tag.addr);
                            }
                            else
                            {
                                addr = int.Parse(tag.addr);
                            }

                            {
                                tag.addr = addr.ToString();
                            }

                            tagInfos.Add(tag.id, tag);

                            nameList.Add(tag.id);
                            typeList.Add(tag.type);
                            redisList.Add(redis);
                            sizeList.Add(size);
                            memoryList.Add(tag.memory);
                        }
                        else
                        {
                            IronUtilites.LogManager.Manager.WriteLog("Warning", "Tag Size must be over 0");
                        }
                    }
                    catch (OverflowException ex)
                    {
                        IronUtilites.LogManager.Manager.WriteLog("Exception", "integer type size is overflow. : " + ex.Message);
                    }
                    catch (FormatException ex)
                    {
                        IronUtilites.LogManager.Manager.WriteLog("Exception", "Only integer type can be used for size. : " + ex.Message);
                    }
                    catch (ArgumentException ex)
                    {
                        IronUtilites.LogManager.Manager.WriteLog("Exception", "TagName Duplicate. : " + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        IronUtilites.LogManager.Manager.WriteLog("Exception", "Get Driver Tag Failed messaged : " + ex.Message);
                        Debug.WriteLine("Get Driver Tag Failed messaged : " + ex.Message);
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

        public void SetDriverInfo(bool bHexAddress, int nScanMode, string param)
        {
            try
            {
                hexAddress = bHexAddress;
                string strFilename = param;

                string currentDir = Directory.GetCurrentDirectory();

                XmlDocument doc = new XmlDocument();
                doc.Load(strFilename);
                //doc.Load(currentDir + "\\Config\\" + strFilename);

                
                xmlInfo = new Driver();
                xmlInfo = (Driver)XMLParser.DeserializeXML(strFilename, typeof(Driver));
                //xmlInfo = (Driver)XMLParser.DeserializeXML(currentDir + "\\Config\\" + strFilename, typeof(Driver));

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
                IronUtilites.LogManager.Manager.WriteLog("Exception", "Read Xml File Failed Error Message : " + ex.Message);
                Debug.WriteLine("Read Xml File Failed Error Message : " + ex.Message);
            }
        }

        public bool ReadTag(string tagName, ref object data)
        {
            List<int> memoryList = new List<int>();
            Dictionary<int, List<string>> memoryDic = new Dictionary<int, List<string>>();
            Dictionary<string, object> valueLists = new Dictionary<string, object>();
            bool bErr = false;
            int devType;
            try
            {
                devType = initDic[tagInfos[tagName].memory];
                int addr = int.Parse(tagInfos[tagName].addr);

                memoryList.Add(addr);
                memoryDic[addr] = new List<string>() { tagName };
            }
            catch (Exception ex)
            {
                IronUtilites.LogManager.Manager.WriteLog("Error", "MELSECNET DataType Not Definition : " + ex.Message);
                return false;
            }

            #region Type String
            if (comm.IsBit(tagInfos[tagName].memory))
            {
                bErr = GetBits(memoryList, memoryDic, valueLists, (int)devType);
            }
            else
            {
                bErr = GetRegisters(memoryList, memoryDic, valueLists, (int)devType);
            }
            #endregion

            if (!bErr)
            {
                throw new Exception("Melsecnet Read Fail");
            }
            else
            {
                data = valueLists[tagName];
            }

            return bErr;
        }

        public bool ReadTags(string[] tagNames, ref object[] datas)
        {
            Dictionary<string, VarClass> varClassDIc = new Dictionary<string, VarClass>();
            Dictionary<string, object> valueLists = new Dictionary<string, object>();

            foreach (var tagName in tagNames)
            {
                if (!int.TryParse(tagInfos[tagName].addr, out int addr))
                {
                    continue;
                }

                CreateLists(varClassDIc, tagInfos[tagName].memory, tagName, addr);
            }

            #region Modifing
            foreach (var devType in varClassDIc.Keys)
            {
                if (comm.IsBit(devType))
                {
                    if (!GetBits(varClassDIc[devType].memoryList, varClassDIc[devType].memoryDic, valueLists, (int)initDic[devType]))
                    {
                        return false;
                    }
                }
                else
                {
                    if(!GetRegisters(varClassDIc[devType].memoryList, varClassDIc[devType].memoryDic, valueLists, (int)initDic[devType]))
                    {
                        return false;
                    }
                }
            }
            #endregion

            for (int i = 0; i < tagNames.Length; i++)
            {
                if (valueLists.ContainsKey(tagNames[i]))
                {
                    datas[i] = valueLists[tagNames[i]];
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
                string dataType = "";

                try
                {
                    singleAddr = int.Parse(tagInfos[tagName].addr);
                    singleSize = int.Parse(tagInfos[tagName].size);

                    dataType = tagInfos[tagName].type;

                    if (dataType == "")
                    {
                        throw new ArgumentException("Type cannot be empty");
                    }

                    if (data is byte[] dataList)
                    {
                        int length = singleSize * (int)Enum.Parse(typeof(Definition.DataType), tagInfos[tagName].type.ToUpper());
                        
                        if (comm.IsBit(tagInfos[tagName].memory))
                        {
                            if(!comm.SetBit(singleAddr, length, ref dataList, initDic[tagInfos[tagName].memory], requestCount, initSizeArray["writeMaxSize"], initSizeArray["readCommandLength"]))
                            {
                                return false;
                            }
                        }
                        else
                        {
                            if(!comm.SetRegister(singleAddr, length, ref dataList, initDic[tagInfos[tagName].memory], requestCount, initSizeArray["writeMaxSize"], initSizeArray["readCommandLength"]))
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
            }

            return false;
        }

        public bool WriteTags(string[] tagNames, object[] datas)
        {
            for (int i = 0; i < tagNames.Length; i++)
            {
                if(!WriteTag(tagNames[i], datas[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private void CreateLists(Dictionary<string, VarClass> varClassDIc, string devType, string tagName, int addr)
        {
            if (!varClassDIc.ContainsKey(devType))
            {
                varClassDIc[devType] = new VarClass();
                varClassDIc[devType].memoryList = new List<int>() { addr };
                varClassDIc[devType].memoryDic = new Dictionary<int, List<string>>() { [addr] = new List<string>() { tagName } };
            }
            else
            {
                varClassDIc[devType].memoryList.Add(addr);

                if (!varClassDIc[devType].memoryDic.ContainsKey(addr))
                {
                    varClassDIc[devType].memoryDic[addr] = new List<string>() { tagName };
                }
                else
                {
                    varClassDIc[devType].memoryDic[addr].Add(tagName);
                }

            }
        }

        private bool GetBits(List<int> bitList, Dictionary<int, List<string>> bitDic, Dictionary<string, object> valueLists, int devType)
        {
            int bitSize = 0;
            byte[] bitBuff;

            List<int> lengths = new List<int>();

            bitList.Sort();

            for (int i = 0; i < bitList.Count;)
            {
                foreach (string tagName in bitDic[bitList[i]])
                {
                    int addrSize = Convert.ToInt32(tagInfos[tagName].size) * (int)Enum.Parse(typeof(Definition.DataType), tagInfos[tagName].type.ToUpper());
                    lengths.Add(addrSize);

                    bitSize += addrSize;

                    i++;
                }
            }

            if (lengths.Count > 0)
            {
                //bitBuff = new byte[bitSize * 2];
                bitBuff = new byte[bitSize];

                comm.GetBits(bitList.ToArray(), lengths.ToArray(), devType, ref bitBuff, requestCount, 
                    initSizeArray["readMaxSize"], initSizeArray["readCommandLength"], initSizeArray["isBit"] );

                int index = 0;

                for (int i = 0; i < bitList.Count;)
                {
                    foreach (string tagName in bitDic[bitList[i]])
                    {
                        int length = Convert.ToInt32(tagInfos[tagName].size) * (int)Enum.Parse(typeof(Definition.DataType), tagInfos[tagName].type.ToUpper());
                        byte[] value = new byte[length * 2];
                        int j = 0;

                        for (; j < length; j++)
                        {
                            value[j * 2] = bitBuff[index + j];
                        }

                        valueLists.Add(tagName, value);

                        index += j;
                        i++;
                    }
                }

            }

            return true;
        }

        private bool GetRegisters(List<int> wordList, Dictionary<int, List<string>> wordDic, Dictionary<string, object> valueLists, int devType)
        {
            int wordSize = 0;
            byte[] wordBuff;

            List<int> lengths = new List<int>();

            wordList.Sort();

            for (int i = 0; i < wordList.Count;)
            {
                foreach (string tagName in wordDic[wordList[i]])
                {
                    int addrSize = Convert.ToInt32(tagInfos[tagName].size) * (int)Enum.Parse(typeof(Definition.DataType), tagInfos[tagName].type.ToUpper());
                    lengths.Add(addrSize);

                    wordSize += addrSize;

                    i++;
                }
            }

            if (lengths.Count > 0)
            {
                wordBuff = new byte[wordSize * 2];

                comm.GetRegisters(wordList.ToArray(), lengths.ToArray(), devType, ref wordBuff, requestCount, initSizeArray["readMaxSize"], initSizeArray["readCommandLength"]);

                int index = 0;

                for (int i = 0; i < wordList.Count;)
                {
                    foreach (string tagName in wordDic[wordList[i]])
                    {
                        int length = Convert.ToInt32(tagInfos[tagName].size) * (int)Enum.Parse(typeof(Definition.DataType), tagInfos[tagName].type.ToUpper());
                        byte[] value = new byte[length * 2];
                        int j = 0;

                        for (; j < length * 2; j++)
                        {
                            value[j] = wordBuff[index + j];
                        }

                        valueLists.Add(tagName, value);

                        index += j;
                        i++;
                    }
                }

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
