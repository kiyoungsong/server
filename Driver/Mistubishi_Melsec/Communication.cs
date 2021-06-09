using DriverBase;
using IronUtilites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System.Collections;

namespace Mistubishi_Melsec
{
    class Communication : TCP
    {
        bool connectStatus = false;
        ScriptEngine engine = null;
        ScriptSource source = null;
        ScriptScope scope = null;

        //Func<int, int, int, int, int, List<byte>, string> makePacketResult;
        //Func<List<int>, int, string> exportDataResult;
        //Func<Dictionary<string, int>> initSize;
        //Func<Dictionary<string, string>> initValue;
        //Func<string, bool> bitWordDistinguish;

        dynamic scriptClass;
        dynamic func;

        public enum DataCommand
        {
            Read = 1,
            Write = 2
        };

        public Communication(bool log = false, string name = "", string path = ".", bool server = false, string addr = "192.168.0.1", ushort port = 2004, int timeout = 1, int connectTime = 1) :
           base(addr, port, log, name, path, server, timeout, connectTime)
        {
            engine = Python.CreateEngine();
            source = engine.CreateScriptSourceFromFile(path);
            scope = engine.CreateScope();
            source.Execute(scope);
            string[] pyName = path.Split('\\');
            scriptClass = scope.GetVariable(pyName[pyName.Length-1].Substring(0, pyName[pyName.Length - 1].Length - 3));
            func = engine.Operations.CreateInstance(scriptClass);

            //initSize = scope.GetVariable<Func<Dictionary<string, int>>>("InitSize");
            //initValue = scope.GetVariable<Func<Dictionary<string, string>>>("InitValue");
            //makePacketResult = scope.GetVariable<Func<int, int, int, int, int, List<byte>, string>>("MakePacket");
            //exportDataResult = scope.GetVariable<Func<List<int>, int, string>>("ExportData");
            //bitWordDistinguish = scope.GetVariable<Func<string, bool>>("BitWordDistinguish");
        }

        public bool CommunicationStatus()
        {
            return connectStatus;
        }

        public void Connect()
        {
            Open();
            connectStatus = true;
        }

        public void DIsConnect()
        {
            Close();
            connectStatus = false;
        }

        public void Init(ref Dictionary<string,byte> initDic, ref Dictionary<string,int> initSizeArray)
        {
            initDic = new Dictionary<string, byte>();
            initSizeArray = new Dictionary<string, int>();

            Dictionary<string, string> tempDic = func.InitValue();
            initSizeArray = func.InitSize();

            foreach (string item in tempDic.Keys)
            {
                initDic[item] = tempDic[item].ToString().Select(x => Convert.ToByte(x)).ToArray()[0];
            }
        }

        public bool GetBits(int[] addrs, int[] lengths, int device, ref byte[] buff, int requestCount, int read_maxSize, int readSize, int isBit)
        {
            lock (this)
            {
                byte[] commandBuff;
                byte[] writeBuff = new byte[22];

                //readMaxSize 를 여기서 받아오고 
                MemoryManager.MemoryMap map = MemoryManager.CreateMemoryMap(addrs, lengths, read_maxSize);

                int totalLength = 0;

                foreach (int l in map.lengths)
                {
                    totalLength += l;
                }

                byte[] readData = new byte[totalLength];

                int index = 0;

                for (int i = 0; i < map.addrs.Count; i++)
                {
                    int length = 0;

                    // Address Length
                    if (isBit == 1) 
                    {
                        length = map.lengths[i];
                    }
                    else
                    {
                        length = ((map.lengths[i] + map.addrs[i] % 16) / 16) + ((map.lengths[i] + map.addrs[i] % 16) % 16 > 0 ? 1 : 0);
                    }

                    int addr = map.addrs[i] / 16;
                    int offset = map.addrs[i] % 16;

                    commandBuff = MakePacket((int)DataCommand.Read, device, (ushort)length, (ushort)(addr), (ushort)offset, writeBuff.ToList());

                    byte[] readBuff = null;

                    for (int reqCnt = 0; reqCnt < requestCount; reqCnt++)
                    {
                        bool writeResult = Write(ref commandBuff);

                        readBuff = new byte[readSize + 2 * length];

                        bool readResult = Read(ref readBuff, readSize + 2 * length);

                        if (!readResult)
                        {
                            IronUtilites.LogManager.Manager.WriteLog("Error", "Read Failed, Please Check Connected Sockets");

                            if (reqCnt == requestCount - 1)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            break;
                        }

                        break;
                    }

                    List<int> bytesAsInts = readBuff.Select(x => (int)x).ToArray().ToList();
                    int dwRegNum = ((map.lengths[i] + map.addrs[i] % 16) / 16) + ((map.lengths[i] + map.addrs[i] % 16) % 16 > 0 ? 1 : 0);
                    byte[] tempData = ExportData(bytesAsInts, dwRegNum);
                    
                    BitArray bitArr = new BitArray(tempData);
                    byte[] bytes = bitArr.Cast<bool>().Select(bit => bit ? (byte)1 : (byte)0).ToArray<byte>();
                    
                    Buffer.BlockCopy(bytes, offset, readData, index, map.lengths[i]);
                    index += map.lengths[i];
                }

                index = 0;

                for (int k = 0; k < addrs.Length; k++)
                {
                    int buffIndex = 0;

                    for (int j = 0; j < map.addrs.Count; j++)
                    {
                        if (addrs[k] >= map.addrs[j] && addrs[k] <= map.addrs[j] + map.lengths[j])
                        {
                            Buffer.BlockCopy(readData, (buffIndex + addrs[k] - map.addrs[j]), buff, index, lengths[k]);
                            index += lengths[k];
                            break;
                        }

                        buffIndex += map.lengths[j];
                    }
                }

                return true;
            }
        }

        public bool GetRegisters(int[] addrs, int[] lengths, int device, ref byte[] buff, int requestCount, int read_maxSize, int readSize)
        {
            lock (this)
            {
                byte[] commandBuff;
                byte[] writeBuff = new byte[22];

                MemoryManager.MemoryMap map = MemoryManager.CreateMemoryMap(addrs, lengths, read_maxSize);

                int totalLength = 0;

                foreach (int l in map.lengths)
                {
                    totalLength += l;
                }

                byte[] readData = new byte[totalLength * 2];
                byte[] readBuff = null;
                int index = 0;

                for (int i = 0; i < map.addrs.Count; i++)
                {
                    commandBuff = MakePacket((int)DataCommand.Read, device, (ushort)map.lengths[i], (ushort)(map.addrs[i]), 0, writeBuff.ToList());

                    for (int reqCnt = 0; reqCnt < requestCount; reqCnt++)
                    {
                        bool writeResult = Write(ref commandBuff);

                        #region Modify readSize
                        // address Size
                        // default readBuff Size + addressSize ( dataLength)
                        // Ironpython Execute
                        #endregion
                        readBuff = new byte[readSize + 2 * map.lengths[i]];

                        bool readResult = Read(ref readBuff, readSize + 2 * map.lengths[i]);

                        if (!readResult)
                        {
                            IronUtilites.LogManager.Manager.WriteLog("Error", "Read Failed, Please Check Connected Sockets");

                            if (reqCnt == requestCount - 1)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            break;
                        }

                        break;
                    }
                    List<int> bytesAsInts = readBuff.Select(x => (int)x).ToArray().ToList();
                    byte[] tempData = ExportData(bytesAsInts, map.lengths[i]);
                    // 확인필요
                    //byte[] tempData = ExportData(readBuff, false, map.lengths[i]);
                    Buffer.BlockCopy(tempData, 0, readData, index, map.lengths[i]*2);
                    index += map.lengths[i];
                }

                index = 0;

                for (int k = 0; k < addrs.Length; k++)
                {
                    int buffIndex = 0;

                    for (int j = 0; j < map.addrs.Count; j++)
                    {
                        if (addrs[k] >= map.addrs[j] && addrs[k] <= map.addrs[j] + map.lengths[j])
                        {
                            Buffer.BlockCopy(readData, (buffIndex + addrs[k] - map.addrs[j])*2, buff, index, lengths[k] * 2);
                            index += lengths[k] * 2;
                            break;
                        }

                        buffIndex += map.lengths[j];
                    }
                }

                return true;
            }
        }

        public bool SetBit(int addr, int length, ref byte[] writebuff, int device, int requestCount, int write_maxSize, int readSize)
        {
            lock (this)
            {
                int[] addrs = { addr };
                int[] lengths = { length };

                MemoryManager.MemoryMap map = MemoryManager.CreateMemoryMap(addrs, lengths, write_maxSize);

                int byteLength = length / 8 + (length % 8 > 0 ? 1 : 0);
                byte[] commandBuff;
                //byte[] writeBuff = new byte[byteLength];

                for (int i = 0; i < map.lengths.Count; i++)
                {
                    int offset = map.addrs[i] % 16;
                    int packetLength = map.lengths[i] / 16 + (map.lengths[i] % 16 > 0 ? 1 : 0);
                    byte[] writeData = new byte[map.lengths[i]];
                    for (int j = 0; j < map.lengths[i]; j++)
                    {
                        writeData[j] = writebuff[j * 2];
                    }

                    // Read
                    commandBuff = MakePacket((int)DataCommand.Read, device, (ushort)length, (ushort)(addr/16), (ushort)offset, writebuff.ToList());

                    byte[] readBuff = null;

                    for (int reqCnt = 0; reqCnt < requestCount; reqCnt++)
                    {
                        bool writeResult = Write(ref commandBuff);

                        readBuff = new byte[readSize + 2 * length];

                        bool readResult = Read(ref readBuff, readSize + 2 * length);

                        if (!readResult)
                        {
                            IronUtilites.LogManager.Manager.WriteLog("Error", "Read Failed, Please Check Connected Sockets");

                            if (reqCnt == requestCount - 1)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            break;
                        }

                        break;
                    }

                    List<int> bytesAsInts = readBuff.Select(x => (int)x).ToArray().ToList();
                    int dwRegNum = ((map.lengths[i] + map.addrs[i] % 16) / 16) + ((map.lengths[i] + map.addrs[i] % 16) % 16 > 0 ? 1 : 0);
                    byte[] tempData = ExportData(bytesAsInts, dwRegNum);

                    BitArray bitArr = new BitArray(tempData);
                    bool[] bytes = bitArr.Cast<bool>().Select(bit => bit ? true : false).ToArray<bool>();

                    //write
                    int index = 0;
                    for (int k = offset; k < bytes.Length; k++)
                    {
                        if(writeData.Length > index)
                        {
                            bytes[k] = writeData[index++] == 1 ? true : false;
                        }
                        else
                        {
                            break;
                        }
                    }

                    byte[] setBytes = new byte[bytes.Length / 8];
                    bitArr = new BitArray(bytes);
                    bitArr.CopyTo(setBytes, 0);

                    commandBuff = MakePacket((int)DataCommand.Write, device, (ushort)packetLength, (ushort)(map.addrs[i] / 16), (ushort)offset, setBytes.ToList());

                    for (int reqCnt = 0; reqCnt < requestCount; reqCnt++)
                    {
                        bool writeResult = Write(ref commandBuff);

                        byte[] temp = new byte[readSize];// + map.lengths[i]];
                        bool readResult = Read(ref temp, temp.Length);

                        if (!readResult)
                        {
                            IronUtilites.LogManager.Manager.WriteLog("Error", "Read Failed, Please Check Connected Sockets");

                            if (reqCnt == requestCount - 1)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            break;
                        }

                        break;
                    }
                }

                return true;
            }
        }

        public bool SetRegister(int addr, int length, ref byte[] writebuff, int device, int requestCount, int write_maxSize, int readSize)
        {
            lock (this)
            {
                int[] addrs = { addr };
                int[] lengths = { length };

                MemoryManager.MemoryMap map = MemoryManager.CreateMemoryMap(addrs, lengths, write_maxSize);

                int index = 0;

                for (int i = 0; i < map.addrs.Count; i++)
                {
                    byte[] writeTempBuff = new byte[lengths[index] * 2];
                    byte[] commandBuff;

                    Buffer.BlockCopy(writebuff, index, writeTempBuff, 0, lengths[index] * 2);

                    commandBuff = MakePacket((int)DataCommand.Write, device, (ushort)lengths[index], (ushort)(addrs[index]), 0, writeTempBuff.ToList());

                    for (int reqCnt = 0; reqCnt < requestCount; reqCnt++)
                    {
                        bool writeResult = Write(ref commandBuff);

                        byte[] temp = new byte[readSize];// + lengths[index]];
                        bool readResult = Read(ref temp, temp.Length);

                        if (!readResult)
                        {
                            IronUtilites.LogManager.Manager.WriteLog("Error", "Read Failed, Please Check Connected Sockets");

                            if (reqCnt == requestCount - 1)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            break;
                        }

                        break;
                    }

                    index += map.lengths[i] * 2;
                }

                return true;
            }
        }

        public byte[] MakePacket(int byCMD, int device, ushort dwRegNum, ushort dwStartAddr, ushort offset, List<byte> writeBuff)
        {
            string packet = func.MakePacket(byCMD, device, (int)dwRegNum, (int)dwStartAddr, (int)offset, writeBuff);

            return packet.Select(x => Convert.ToByte(x)).ToArray();
        }

        public byte[] ExportData(List<int> packetByte, int length)
        {
            string result = "";

            result = func.ExportData(packetByte, length);

            byte[] unico = Encoding.Unicode.GetBytes(result);
            //return result.Select(x => Convert.ToByte(x)).ToArray();
            return unico;
        }

        public bool IsBit(string deviceType)
        {
            return func.BitWordDistinguish(deviceType);
        }
    }
}
