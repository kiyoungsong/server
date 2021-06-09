using IronUtilites;
using System;
using System.Collections.Generic;
using System.Text;
using DriverBase;
using System.Collections;
using System.Linq;
using DriverInterface;

namespace ModbusRTU
{
    public class Communication
    {
        const int BYTE_READ_MAXSIZE = 125;
        const int BYTE_WRITE_MAXSIZE = 120;
        const int BIT_WRITE_MAXSIZE = 1960;
        const int BIT_READ_MAXSIZE = 2000;

        const int HEADER_LENGTH = 8;
        const int READ_HEADER_LENGTH = 3;
        const int CRC_LENGTH = 2;

        public enum MODBUSRTUCMD
        {
            RD_COIL = 0x01,
            RD_INPUT = 0x02,
            RD_IR = 0x04,
            RD_HR = 0x03,
            WR_COIL = 0x0F,
            WR_HR = 0x10
        };

        EasyModbus.ModbusClient client;

        public Communication()
        {
        }

        public string Port { get; set; }
        public ushort BaudRate { get; set; }
        public int ConnectTime { get; set; }

        public void Connect()
        {
            client = new EasyModbus.ModbusClient(Port);
            client.Baudrate = BaudRate;
            client.ConnectionTimeout = ConnectTime;
            client.Connect();

            if (!client.Connected)
            {
                IronUtilites.LogManager.Manager.WriteLog("Console", "Successed to Connect");
            }
            else
            {
                IronUtilites.LogManager.Manager.WriteLog("Error", "Failed to Connect ");
            }
        }

        public void Disconnect()
        {
            client.Disconnect();

            if (!client.Connected)
            {
                IronUtilites.LogManager.Manager.WriteLog("Console", "Disconnected");
            }
        }

        public bool GetBits(int[] addrs, int[] lengths, ref byte[] buff, bool isWrite, int requestCount)
        {
            lock (this)
            {
                byte[] commandBuff = new byte[HEADER_LENGTH];
                byte[] writeBuff = new byte[22];

                MemoryManager.MemoryMap map = MemoryManager.CreateMemoryMap(addrs, lengths, BIT_READ_MAXSIZE);

                int totalLength = 0;

                foreach (int l in map.lengths)
                {
                    totalLength += l;
                }

                byte[] readData = new byte[totalLength];

                int index = 0;

                for (int i = 0; i < map.addrs.Count; i++)
                {
                    int length = map.lengths[i];
                    int addr = map.addrs[i];
                    int offset = map.addrs[i] % 16;

                    if (isWrite)
                    {
                        GetBitCommand((int)MODBUSRTUCMD.RD_COIL, (ushort)length, (ushort)addr, ref writeBuff, ref commandBuff);
                    }
                    else
                    {
                        GetBitCommand((int)MODBUSRTUCMD.RD_INPUT, (ushort)length, (ushort)addr, ref writeBuff, ref commandBuff);
                    }

                    byte[] readBuff = null;

                    for (int reqCnt = 0; reqCnt < requestCount; reqCnt++)
                    {
                       // bool writeResult = client.read(ref commandBuff);

                        int readSize = READ_HEADER_LENGTH + (length / 8 + (length % 8 > 0 ? 1 : 0)) + CRC_LENGTH;
                        readBuff = new byte[readSize];
                       // bool readResult = Read(ref readBuff, readSize);

                        int count = readBuff.Count(cnt => cnt == 0);

                        if (count == readBuff.Length)
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
                    }

                    byte[] temp = readBuff.ToList().GetRange(READ_HEADER_LENGTH, readBuff.Length - (READ_HEADER_LENGTH + CRC_LENGTH)).ToArray();
                    readBuff = temp;

                    BitArray bitArr = new BitArray(readBuff);
                    byte[] bytes = bitArr.Cast<bool>().Select(bit => bit ? (byte)1 : (byte)0).ToArray<byte>();

                    Buffer.BlockCopy(bytes, 0, readData, index, map.lengths[i]);
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

        public bool GetRegisters(int[] addrs, int[] lengths, ref byte[] buff, bool isWrite, int requestCount)
        {
            lock (this)
            {
                byte[] commandBuff = new byte[HEADER_LENGTH];
                byte[] writeBuff = new byte[22];

                MemoryManager.MemoryMap map = MemoryManager.CreateMemoryMap(addrs, lengths, BYTE_READ_MAXSIZE);

                int totalLength = 0;

                foreach (int l in map.lengths)
                {
                    totalLength += l;
                }

                byte[] readData = new byte[totalLength * 2];

                int index = 0;

                for (int i = 0; i < map.addrs.Count; i++)
                {
                    if (isWrite)
                    {
                        GetByteCommand((int)MODBUSRTUCMD.RD_HR, (ushort)map.lengths[i], (ushort)map.addrs[i], ref writeBuff, ref commandBuff);
                    }
                    else
                    {
                        GetByteCommand((int)MODBUSRTUCMD.RD_IR, (ushort)map.lengths[i], (ushort)map.addrs[i], ref writeBuff, ref commandBuff);
                    }

                    byte[] readBuff = null;

                    for (int reqCnt = 0; reqCnt < requestCount; reqCnt++)
                    {
                        //bool writeResult = Write(ref commandBuff);

                        int readSize = READ_HEADER_LENGTH + CRC_LENGTH + map.lengths[i] * 2;
                        readBuff = new byte[readSize];

                        //bool readResult = Read(ref readBuff, readSize);

                        int count = readBuff.Count(cnt => cnt == 0);

                        if (count == readBuff.Length)
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
                    }

                    Buffer.BlockCopy(readBuff, READ_HEADER_LENGTH, readData, index, map.lengths[i] * 2);
                    index += map.lengths[i] * 2;
                }

                index = 0;

                for (int k = 0; k < addrs.Length; k++)
                {
                    int buffIndex = 0;

                    for (int j = 0; j < map.addrs.Count; j++)
                    {
                        if (addrs[k] >= map.addrs[j] && addrs[k] <= map.addrs[j] + map.lengths[j])
                        {
                            Buffer.BlockCopy(readData, (buffIndex + addrs[k] - map.addrs[j]) * 2, buff, index, lengths[k] * 2);
                            index += lengths[k] * 2;
                            break;
                        }

                        buffIndex += map.lengths[j];
                    }
                }

                return true;
            }
        }

        public bool SetBit(int addr, int length, ref byte[] writebuff, int requestCount)
        {
            lock (this)
            {
                int byteLength = length / 8 + (length % 8 > 0 ? 1 : 0);
                byte[] commandBuff = new byte[HEADER_LENGTH + byteLength + 1];
                byte[] writeBuff = new byte[byteLength];

                for (int i = 0; i < length; i++)
                {
                    int len = i / 8 + 1;
                    int shift = i % 8;

                    if (writebuff[i * 2] > 0)
                    {
                        writeBuff[len - 1] |= (byte)(1 << shift);
                    }
                    else
                    {
                        writeBuff[len - 1] &= (byte)~(1 << shift);
                    }
                }
                
                GetBitCommand((int)MODBUSRTUCMD.WR_COIL, (ushort)length, (ushort)addr, ref writeBuff, ref commandBuff);

                for (int reqCnt = 0; reqCnt < requestCount; reqCnt++)
                {
                    //bool writeResult = Write(ref commandBuff);

                    byte[] temp = new byte[HEADER_LENGTH];
                    //bool readResult = Read(ref temp, HEADER_LENGTH);

                    int count = temp.Count(cnt => cnt == 0);

                    if (count == temp.Length)
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
                }

                return true;
            }
        }

        public bool SetRegister(int addr, int length, ref byte[] writebuff, int requestCount)
        {
            lock (this)
            {
                int[] addrs = { addr };
                int[] lengths = { length };

                MemoryManager.MemoryMap map = MemoryManager.CreateMemoryMap(addrs, lengths, BYTE_WRITE_MAXSIZE);

                int index = 0;

                for (int i = 0; i < map.addrs.Count; i++)
                {
                    byte[] writeTempBuff = new byte[map.lengths[i] * 2];
                    byte[] commandBuff = new byte[HEADER_LENGTH + writeTempBuff.Length + 1];

                    Buffer.BlockCopy(writebuff, index, writeTempBuff, 0, map.lengths[i] * 2);

                    GetByteCommand((int)MODBUSRTUCMD.WR_HR, (ushort)map.lengths[i], (ushort)map.addrs[i], ref writeTempBuff, ref commandBuff);

                    for (int reqCnt = 0; reqCnt < requestCount; reqCnt++)
                    {
                        //bool writeResult = Write(ref commandBuff);

                        byte[] temp = new byte[HEADER_LENGTH];
                        //bool readResult = Read(ref temp, HEADER_LENGTH);

                        int count = temp.Count(cnt => cnt == 0);

                        if (count == temp.Length)
                        {
                            IronUtilites.LogManager.Manager.WriteLog("Error", "Read Failed, Please Check Connected Sockets");

                            if (reqCnt == requestCount - 1)
                            {
                                return false;
                            }
                        }
                    }

                    index += map.lengths[i] * 2;
                }

                return true;
            }
        }

        void GetBitCommand(int byCMD, ushort dwRegNum, ushort dwStartAddr, ref byte[] bypWBuf, ref byte[] bypCmdBuf)
        {
            byte[] byHeader = new byte[HEADER_LENGTH];

            if (byCMD == (int)MODBUSRTUCMD.WR_HR)
            {
                byHeader = new byte[HEADER_LENGTH + bypWBuf.Length + 1];
            }

            //ID
            byHeader[0] = 0x01;

            //Function Code
            byHeader[1] = (byte)byCMD;

            //StartAddress
            byHeader[2] = (byte)((dwStartAddr & 0xFF00) >> 8);  // (H)
            byHeader[3] = (byte)(dwStartAddr & 0x00FF);         // (L)

            //Data Length
            byHeader[4] = (byte)((dwRegNum & 0xFF00) >> 8);  // (H)
            byHeader[5] = (byte)(dwRegNum & 0x00FF);         // (L)

            byte[] temp = new byte[2];

            if (byCMD == (int)MODBUSRTUCMD.WR_HR)
            {
                byHeader[6] = (byte)(bypWBuf.Length);

                Buffer.BlockCopy(bypWBuf, 0, byHeader, 7, bypWBuf.Length);

                temp = GetCRC16(byHeader, byHeader.Length - 2);
            }
            else
            {
                //CRC
                temp = GetCRC16(byHeader, byHeader.Length - 2);
            }

            Buffer.BlockCopy(byHeader, byHeader.Length - 2, temp, 0, temp.Length);
            Buffer.BlockCopy(byHeader, 0, bypCmdBuf, 0, byHeader.Length);
        }

        void GetByteCommand(int byCMD, ushort dwRegNum, ushort dwStartAddr, ref byte[] bypWBuf, ref byte[] bypCmdBuf)
        {
            byte[] byHeader = new byte[HEADER_LENGTH];

            if (byCMD == (int)MODBUSRTUCMD.WR_HR)
            {
                byHeader = new byte[HEADER_LENGTH + bypWBuf.Length + 1];
            }

            //ID
            byHeader[0] = 0x01;

            //Function Code
            byHeader[1] = (byte)byCMD;

            //StartAddress
            byHeader[2] = (byte)((dwStartAddr & 0xFF00) >> 8);  // (H)
            byHeader[3] = (byte)(dwStartAddr & 0x00FF);         // (L)

            //Data Length
            byHeader[4] = (byte)((dwRegNum & 0xFF00) >> 8);  // (H)
            byHeader[5] = (byte)(dwRegNum & 0x00FF);         // (L)

            byte[] temp = new byte[2];

            if (byCMD == (int)MODBUSRTUCMD.WR_HR)
            {
                byHeader[6] = (byte)(bypWBuf.Length);

                Buffer.BlockCopy(bypWBuf, 0, byHeader, 7, bypWBuf.Length);

                temp = GetCRC16(byHeader, byHeader.Length - 2);
            }
            else
            {
                //CRC
                temp = GetCRC16(byHeader, byHeader.Length - 2);
            }

            Buffer.BlockCopy(byHeader, byHeader.Length - 2, temp, 0, temp.Length);
            Buffer.BlockCopy(byHeader, 0, bypCmdBuf, 0, byHeader.Length);
        }

        byte[] GetCRC16(byte[] bytes, int len)
        {
            int icrc = 0xFFFF;
            for (int i = 0; i < len; i++)
            {
                icrc = (icrc >> 8) ^ Definition.CRCTable[(icrc ^ bytes[i]) & 0xff];
            }
            byte[] ret = BitConverter.GetBytes(icrc);

            return ret;
        }
    }
}
