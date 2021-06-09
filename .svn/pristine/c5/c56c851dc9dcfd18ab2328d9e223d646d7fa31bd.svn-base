using System;
using System.Collections.Generic;
using System.Text;
using DriverInterface;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Xml;
using Modbus;
using IronUtilites;
using System.IO;
using System.Globalization;
using System.Drawing;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Diagnostics.Contracts;

namespace Melsecneth
{
    public class Melsecneth : IDriver
    {
        #region InteropServices/MdFunc32
        [DllImport("MdFunc32.dll")]
        public static extern short mdopen(short Chan, short Mode, ref int Path);

        [DllImport("MdFunc32.dll")]
        public static extern short mdclose(int Path);

        [DllImport("MdFunc32.dll")]
        public static extern short mdsend(int Path, short Stno, short Devtyp, short Devno, short Size_Renamed, byte[] Buf);

        [DllImport("MdFunc32.dll")]
        public static extern short mdreceive(int Path, short Stno, short Devtyp, short Devno, short Size_Renamed, byte[] Buf);

        [DllImport("MdFunc32.dll")]
        public static extern short mddevset(int Path, short Stno, short Devtyp, short Devno);

        [DllImport("MdFunc32.dll")]
        public static extern short mddevrst(int Path, short Stno, short Devtyp, short Devno);

        [DllImport("MdFunc32.dll")]
        public static extern short mdrandw(int Path, short Stno, ref short dev, ref short Buf, short bufsiz);

        [DllImport("MdFunc32.dll")]
        public static extern short mdrandr(int Path, short Stno, ref short dev, ref short Buf, short bufsiz);

        [DllImport("MdFunc32.dll")]
        public static extern short mdcontrol(int Path, short Stno, short Buf);

        [DllImport("MdFunc32.dll")]
        public static extern short mdtyperead(int Path, short Stno, ref short Buf);

        [DllImport("MdFunc32.dll")]
        public static extern int mdsendex(int Path, int netno, int Stno, int Devtyp, int Devno, ref int Size_Renamed, byte[] Buf);
        [DllImport("MdFunc32.dll")]
        public static extern int mdreceiveex(int Path, int netno, int Stno, int Devtyp, int Devno, ref int Size_Renamed, byte[] Buf);
        [DllImport("MdFunc32.dll")]
        public static extern int mddevsetex(int Path, int netno, int Stno, int Devtyp, int Devno);
        [DllImport("MdFunc32.dll")]
        public static extern int mddevrstex(int Path, int netno, int Stno, int Devtyp, int Devno);
        [DllImport("MdFunc32.dll")]
        public static extern int mdrandwex(int Path, int netno, int Stno, ref int dev, ref int Buf, int bufsize);
        [DllImport("MdFunc32.dll")]
        public static extern int mdrandrex(int Path, int netno, int Stno, ref int dev, ref int Buf, int bufsize);
        [DllImport("MdFunc32.dll")]
        public static extern short mdwaitbdevent(int Path, short eventno, ref int timeout, ref short signaledno, short details);
        #endregion

        #region Const Memory Type
        public enum MemoryType
        {
            b = 23,
            x = 1,
            y = 2,
            s = b,
            sb = b,
            m = b,
            l = b,
            f = b,
            v = b,

            w = 24,
            t = w,
            st = w,
            c = w,
            d = w,
            sw = w
        }

        public const int bit_Max_Addr = 0x7fff;
        public const int word_Max_Addr = 0x1ffff;
        #endregion

        Driver xmlInfo = null;
        Dictionary<string, Tag> tagInfos = new Dictionary<string, Tag>(); // key : id
        Communication comm = null;

        int nErr = 1;
        int nPrevBitError = 0;
        int nPrevWordError = 0;
        int nPath = 0;
        int nNetwork = 0;
        int nStation = 0;
        short nChannel = 0;

        bool connectStatus = false;
        bool hexAddress = false;

        int requestCount = 3;
        int connectTime = 1;
        int requestTime = 1;

        public void Connect()
        {
            short.TryParse(xmlInfo.channel,out nChannel);
            int.TryParse(xmlInfo.network, out nNetwork);
            int.TryParse(xmlInfo.station, out nStation);
            comm = new Communication();
            
            nErr = Melsecneth.mdopen(nChannel, -1, ref nPath);

            if (nErr != 0)
            {
                Debug.WriteLine("Not Connect, nErr : " + nErr);
                connectStatus = false;
            }
            else
            {
                Debug.WriteLine("Connect, path : " + nPath);
                connectStatus = true;
            }
        }

        public void Disconnect()
        {
            nErr = Melsecneth.mdclose(nPath);

            if (nErr != 0)
            {
                Debug.WriteLine("Connect, nErr : " + nErr);
                connectStatus = true;
            }
            else
            {
                Debug.WriteLine("Disconnect, path : " + nPath);
                connectStatus = false;
            }
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
                IronUtilites.LogManager.Manager.WriteLog("Console","Read Xml File Failed Error Message : " + ex.Message);
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
                            if (tag.dword != "0" && tag.dword != "1")
                            {
                                IronUtilites.LogManager.Manager.WriteLog("Exception", tag.id + " dword Value : " + tag.dword);
                                throw new Exception("MELSECH XML Attribute 'dword' Value must be 0 or 1");
                            }

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
                                int memory = (int)Enum.Parse(typeof(MemoryType), tag.memory);
                                
                                if ((memory <= 23 && (addr + size) > bit_Max_Addr) || (memory >= 24 && (addr + size) > word_Max_Addr))
                                {
                                    throw new Exception("Address range exceeded the limit");
                                }
                                
                                if(memory <= 23)
                                {
                                    if (!(tag.type == "bool" || tag.type == "short"))
                                    {
                                        throw new Exception("Memory Type 'B' must be DataType 'bool' or 'short'");
                                    }
                                }

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
                    catch(Exception ex)
                    {
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
                Debug.WriteLine("Get Driver Info Failed messaged : " + ex.Message);
            }
        }

        public bool ReadTag(string tagName, ref object data)
        {
            List<Tag> taginfoList = new List<Tag>();
            int.TryParse(tagInfos[tagName].size, out int size);
            byte[] ReadBuff = new byte[size * 2];
            taginfoList.Add(tagInfos[tagName]);
            bool bErr = false;
            int devType;

            try
            {
                devType = (int)Enum.Parse(typeof(MemoryType), tagInfos[tagName].memory);
            }
            catch (Exception ex)
            {
                IronUtilites.LogManager.Manager.WriteLog("Error", "MELSECNET DataType Not Definition : " + ex.Message);
                return false;
            }

            if (devType >= 24)
            {
                bErr = comm.GetWord(taginfoList, nPath, nNetwork, nStation, devType, ref nPrevWordError, ref ReadBuff, requestCount);
            }
            else
            {
                bErr = comm.GetBit(taginfoList, nPath, nNetwork, nStation, devType, ref nPrevBitError, ref ReadBuff, requestCount);
            }

            if (!bErr)
            {
                throw new Exception("Melsecnet Read Fail");
            }
            else
            {
                data = ReadBuff;
            }

            return bErr;
        }

        public bool ReadTags(string[] tagNames, ref object[] datas)
        {
            Dictionary<MemoryType, VarClass> varClassDIc = new Dictionary<MemoryType, VarClass>();
            Dictionary<string, object> valueLists = new Dictionary<string, object>();

            foreach (var tagName in tagNames)
            {
                if(!int.TryParse(tagInfos[tagName].addr, out int addr))
                {
                    continue;
                }

                CreateLists(varClassDIc, (MemoryType)Enum.Parse(typeof(MemoryType), tagInfos[tagName].memory), tagName, addr);
            }

            foreach (var devType in varClassDIc.Keys)
            {
                if(devType == MemoryType.b || devType == MemoryType.x || devType == MemoryType.y)
                {
                    if(!GetBits(varClassDIc[devType].memoryList, varClassDIc[devType].memoryDic, valueLists, (int)devType))
                    {
                        return false;
                    }
                }
                else
                {
                    if(!GetRegisters(varClassDIc[devType].memoryList, varClassDIc[devType].memoryDic, valueLists, (int)devType))
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

        public bool WriteTag(string tagName, object data)
        {
            int devType = (int)Enum.Parse(typeof(MemoryType), tagInfos[tagName].memory);

            if (data is byte[] WriteByte)
            {
                if (devType == (int)MemoryType.b || devType == (int)MemoryType.x || devType == (int)MemoryType.y)//bit
                {
                    comm.SetBit(tagInfos[tagName], nPath, nNetwork, nStation, devType, ref nPrevBitError, WriteByte, requestCount);
                }
                else//word
                {
                    comm.SetWord(tagInfos[tagName], nPath, nNetwork, nStation, devType, ref nPrevWordError, WriteByte, requestCount);
                } 
            }

            return true;
        }

        public bool WriteTags(string[] tagNames, object[] datas)
        {
            for (int i = 0; i < tagNames.Length; i++)
            {
                WriteTag(tagNames[i], datas[i]);
            }

            return true;
        }

        public bool CommunicationStatus()
        {
            if (connectStatus)
            {
                if (nPrevBitError == 0 && nPrevWordError == 0)
                {
                    return true;
                }

                return false;
            }
            else
            {
                return false;
            }
        }

        private void CreateLists(Dictionary<MemoryType, VarClass> varClassDIc, MemoryType devType, string tagName, int addr)
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
            bool bBitErr = true;
            int bitSize = 0;
            byte[] bitBuff;

            List<Tag> bitAddrList = new List<Tag>();

            bitList.Sort();

            for (int i = 0; i < bitList.Count;)
            {
                foreach (string tagName in bitDic[bitList[i]])
                {
                    bitAddrList.Add(tagInfos[tagName]);
                    bitSize += (Convert.ToInt32(tagInfos[tagName].size) * (int)Enum.Parse(typeof(Definition.DataType), tagInfos[tagName].type.ToUpper()));

                    i++;
                }
            }

            if (bitAddrList.Count > 0)
            {
                bitBuff = new byte[bitSize * 2];
                bBitErr = comm.GetBit(bitAddrList, nPath, nNetwork, nStation, devType, ref nPrevBitError, ref bitBuff, requestCount);

                if (!bBitErr)
                {
                    IronUtilites.LogManager.Manager.WriteLog("Error", "bit Tag Read Fail , Error Code : " + nPrevBitError);
                }
                else
                {
                    int index = 0;

                    for (int i = 0; i < bitList.Count;)
                    {
                        foreach (string tagName in bitDic[bitList[i]])
                        {
                            int lengths = Convert.ToInt32(tagInfos[tagName].size) * (int)Enum.Parse(typeof(Definition.DataType), tagInfos[tagName].type.ToUpper());
                            byte[] value = new byte[lengths * 2];
                            int j = 0;

                            for (; j < lengths * 2; j++)
                            {
                                value[j] = bitBuff[index + j];
                            }

                            valueLists.Add(tagName, value);

                            index += j;
                            i++;
                        }
                    }
                }
            }

            return true;
        }

        private bool GetRegisters(List<int> wordList, Dictionary<int, List<string>> wordDic, Dictionary<string, object> valueLists, int devType)
        {
            bool bWordErr = true;
            int wordSize = 0;
            byte[] wordBuff;

            List<Tag> wordAddrList = new List<Tag>();

            wordList.Sort();

            for (int i = 0; i < wordList.Count;)
            {
                foreach (string tagName in wordDic[wordList[i]])
                {
                    wordAddrList.Add(tagInfos[tagName]);
                    wordSize += (Convert.ToInt32(tagInfos[tagName].size) * (int)Enum.Parse(typeof(Definition.DataType), tagInfos[tagName].type.ToUpper()));

                    i++;
                }
            }

            if (wordAddrList.Count > 0)
            {
                wordBuff = new byte[wordSize * 2];
                bWordErr = comm.GetWord(wordAddrList, nPath, nNetwork, nStation, devType, ref nPrevBitError, ref wordBuff, requestCount);

                if (!bWordErr)
                {
                    IronUtilites.LogManager.Manager.WriteLog("Error", "word Tag Read Fail , Error Code : " + nPrevWordError);
                }
                else
                {
                    int index = 0;

                    for (int i = 0; i < wordList.Count;)
                    {
                        foreach (string tagName in wordDic[wordList[i]])
                        {
                            int lengths = Convert.ToInt32(tagInfos[tagName].size) * (int)Enum.Parse(typeof(Definition.DataType), tagInfos[tagName].type.ToUpper());
                            byte[] value = new byte[lengths * 2];
                            int j = 0;

                            for (; j < lengths * 2; j++)
                            {
                                value[j] = wordBuff[index + j];
                            }

                            valueLists.Add(tagName, value);

                            index += j;
                            i++;
                        }
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
