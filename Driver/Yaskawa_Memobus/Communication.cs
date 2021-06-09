using DriverBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DriverInterface;
using IronUtilites;
using System.Data;
using System.Collections;

namespace Yaskawa_Memobus
{
    public class Communication : UDP
    {
        public enum MEMOBUSCMD
        {
            RD_COIL = 0x01,
            RD_INPUT = 0x02,
            RD_IREG = 0x0A,
            RD_MREG = 0x09,
            WR_COIL = 0x05,
            WR_COILS = 0x0F,
            WR_MREG = 0x0B,
            WR_MREGS = 0x0E
        };
        
        const byte HEADER218_LENGTH = 12;
        const byte MEMOBUS_LENGTH = 10;
        const int BitReadMaxSize = 2000;
        const int ByteReadMaxSize = 1020;
        const int BitWriteMaxSize = 800;
        const int ByteWriteMaxSize = 1019;
        const byte MFC = 0x20;
        const int RegisterCommandSize = 22;
        const int BitCommandSize = 21;
        byte byIDNumber = 0;


        public byte ID
        {
            get { return byIDNumber; }
            set { byIDNumber = value; }
        }

        public Communication(bool log = false, string name = "", string path = ".", bool server = false, string addr = "192.168.1.1", ushort port = 10001, string clientAddr = "192.168.1.95", ushort clientPort = 10001, int timeout = 1, int connectTime = 1) :
            base(log, name, path, server, addr, port, clientAddr, clientPort, timeout, connectTime)
        {
        }

        ~Communication()
        {
        }

        public string IpAddress { get; set; }
        public int Port { get; set; }

        public void Connect()
        {
            Open();
        }

        public void Disconnect()
        {
            Close();
        }

        public bool GetBits(int[] addrs, int[] lengths, ref byte[] buff, bool isWrite, int requestCount)
        {
            lock (this)
            {
                byte cupNo = 1;
                byte[] commandBuff = new byte[21];
                byte[] writeBuff = new byte[22];

                MemoryManager.MemoryMap map = MemoryManager.CreateMemoryMap(addrs, lengths, BitReadMaxSize);

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

                    if(isWrite)
                    {
                        GetBitCommand((int)MEMOBUSCMD.RD_COIL, cupNo, (ushort)length, (ushort)addr, ref writeBuff, ref commandBuff);
                    }
                    else
                    {
                        GetBitCommand((int)MEMOBUSCMD.RD_INPUT, cupNo, (ushort)length, (ushort)addr, ref writeBuff, ref commandBuff);
                    }

                    byte[] readBuff = null;

                    for (int reqCnt = 0; reqCnt < requestCount; reqCnt++)
                    {
                        bool writeResult = Write(ref commandBuff);

                        int readSize = 12 + 5 + (length / 8 + (length % 8 > 0 ? 1 : 0));
                        readBuff = new byte[readSize];
                        bool readResult = Read(ref readBuff, readSize);

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

                    byte[] temp = readBuff.ToList().GetRange(17, readBuff.Length - 17).ToArray();
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
                byte cupNo = 1;
                byte[] commandBuff = new byte[22];
                byte[] writeBuff = new byte[22];

                MemoryManager.MemoryMap map = MemoryManager.CreateMemoryMap(addrs, lengths, ByteReadMaxSize);

                int totalLength = 0;

                foreach (int l in map.lengths)
                {
                    totalLength += l;
                }

                byte[] readData = new byte[totalLength * 2];

                int index = 0;

                for (int i = 0; i < map.addrs.Count; i++)
                {
                    if(isWrite)
                    {
                        GetByteCommand((int)MEMOBUSCMD.RD_MREG, cupNo, (ushort)map.lengths[i], (ushort)map.addrs[i], ref writeBuff, ref commandBuff);
                    }
                    else
                    {
                        GetByteCommand((int)MEMOBUSCMD.RD_IREG, cupNo, (ushort)map.lengths[i], (ushort)map.addrs[i], ref writeBuff, ref commandBuff);
                    }

                    byte[] readBuff = null;

                    for (int reqCnt = 0; reqCnt < requestCount; reqCnt++)
                    {
                        bool writeResult = Write(ref commandBuff);

                        int readSize = 20 + map.lengths[i] * 2;
                        readBuff = new byte[readSize];

                        bool readResult = Read(ref readBuff, readSize);

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

                    Buffer.BlockCopy(readBuff, 20, readData, index, map.lengths[i] * 2);
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
                byte cupNo = 1;
                int byteLength = length / 8 + (length % 8 > 0 ? 1 : 0);
                byte[] commandBuff = new byte[BitCommandSize + byteLength];
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

                GetBitCommand((int)MEMOBUSCMD.WR_COILS, cupNo, (ushort)length, (ushort)addr, ref writeBuff, ref commandBuff);

                for (int reqCnt = 0; reqCnt < requestCount; reqCnt++)
                {
                    bool writeResult = Write(ref commandBuff);

                    byte[] temp = new byte[BitCommandSize];
                    bool readResult = Read(ref temp, BitCommandSize);

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
                byte cupNo = 1;
                int[] addrs = { addr };
                int[] lengths = { length };

                MemoryManager.MemoryMap map = MemoryManager.CreateMemoryMap(addrs, lengths, ByteWriteMaxSize);

                int index = 0;

                for (int i = 0; i < map.addrs.Count; i++)
                {
                    byte[] writeTempBuff = new byte[map.lengths[i] * 2];
                    byte[] commandBuff = new byte[RegisterCommandSize + writeTempBuff.Length];
                    
                    Buffer.BlockCopy(writebuff, index, writeTempBuff, 0, map.lengths[i] * 2);

                    GetByteCommand((int)MEMOBUSCMD.WR_MREG, cupNo, (ushort)map.lengths[i], (ushort)map.addrs[i], ref writeTempBuff, ref commandBuff);


                    for (int reqCnt = 0; reqCnt < requestCount; reqCnt++)
                    {
                        bool writeResult = Write(ref commandBuff);

                        byte[] temp = new byte[RegisterCommandSize];
                        bool readResult = Read(ref temp, RegisterCommandSize);

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

        void GetBitCommand(int byCMD, byte byCpuNo, ushort dwRegNum, ushort dwStartAddr, ref byte[] bypWBuf, ref byte[] bypCmdBuf)
        {
            byte[] by218Header = new byte[HEADER218_LENGTH];
            byte[] byMemobusMsg = new byte[MEMOBUS_LENGTH - 1];

            ushort byDataLength = (ushort)(by218Header.Length + byMemobusMsg.Length);
            ushort byMemobusLength = MEMOBUS_LENGTH - 3;

            if (byCMD == (int)MEMOBUSCMD.WR_COILS)
            {
                byDataLength = (ushort)(byDataLength + bypWBuf.Length);
                byMemobusMsg = new byte[MEMOBUS_LENGTH - 1 + bypWBuf.Length];
            }

            // MEMOBUS command
            by218Header[0] = 0x11;

            // Identification number
            by218Header[1] = byIDNumber;

            // Destination channel number
            by218Header[2] = 0x00;

            // Source channel number
            by218Header[3] = 0x00;

            // Not used
            by218Header[4] = 0x00;
            by218Header[5] = 0x00;

            // Data length
            by218Header[6] = (byte)(byDataLength & 0x00FF);         // (L)
            by218Header[7] = (byte)((byDataLength & 0xFF00) >> 8);  // (H)

            // Not used
            by218Header[8] = 0x00;
            by218Header[9] = 0x00;
            by218Header[10] = 0x00;
            by218Header[11] = 0x00;

            if (byCMD == (int)MEMOBUSCMD.WR_COILS)
            {
                byMemobusLength = (ushort)(0x07 + dwRegNum / 8 + (dwRegNum % 8 > 0 ? 1 : 0));
            }

            // Command length
            byMemobusMsg[0] = (byte)(byMemobusLength & 0x00FF);
            byMemobusMsg[1] = (byte)((byMemobusLength & 0xFF00) >> 8);

            // Always MFC : 0x20
            byMemobusMsg[2] = MFC;

            // MEMOBUS command
            byMemobusMsg[3] = (byte)byCMD;

            // CPU number
            byMemobusMsg[4] = (byte)(byCpuNo << 4);

            byMemobusMsg[5] = (byte)((dwStartAddr & 0xFF00) >> 8);  // Addr(H)
            byMemobusMsg[6] = (byte)(dwStartAddr & 0x00FF);         // Addr(L)

            byMemobusMsg[7] = (byte)((dwRegNum & 0xFF00) >> 8);     // DataNum(H)
            byMemobusMsg[8] = (byte)(dwRegNum & 0x00FF);            // DataNum(L)
            

            Buffer.BlockCopy(by218Header, 0, bypCmdBuf, 0, by218Header.Length);
            Buffer.BlockCopy(byMemobusMsg, 0, bypCmdBuf, HEADER218_LENGTH, byMemobusMsg.Length);
            
            if (bypWBuf != null)
            {
                switch (byCMD)
                {
                    case (int)MEMOBUSCMD.WR_COILS:
                        Buffer.BlockCopy(bypWBuf, 0, bypCmdBuf, HEADER218_LENGTH + MEMOBUS_LENGTH - 1, bypWBuf.Length);
                        break;
                }
            }
        }
        
        void GetByteCommand(int byCMD, byte byCpuNo, ushort dwRegNum, ushort dwStartAddr, ref byte[] bypWBuf, ref byte[] bypCmdBuf)
        {
            byte[] by218Header = new byte[HEADER218_LENGTH];
            byte[] byMemobusMsg = new byte[MEMOBUS_LENGTH];

            ushort byDataLength = 0;
            ushort byMemobusLength = 0;

            switch (byCMD)
            {
                case (int)MEMOBUSCMD.RD_IREG:
                case (int)MEMOBUSCMD.RD_MREG:
                    byDataLength = (ushort)(HEADER218_LENGTH + MEMOBUS_LENGTH);
                    byMemobusLength = MEMOBUS_LENGTH - 2;
                    break;

                case (int)MEMOBUSCMD.WR_MREG:
                    byDataLength = (ushort)(HEADER218_LENGTH + MEMOBUS_LENGTH + bypWBuf.Length);
                    byMemobusLength = MEMOBUS_LENGTH - 2;
                    break;
            }

            // MEMOBUS command
            by218Header[0] = 0x11;

            // Identification number
            by218Header[1] = byIDNumber;

            // Destination channel number
            by218Header[2] = 0x00;

            // Source channel number
            by218Header[3] = 0x00;

            // Not used
            by218Header[4] = 0x00;
            by218Header[5] = 0x00;

            // Data length
            by218Header[6] = (byte)(byDataLength & 0x00FF);         // (L)
            by218Header[7] = (byte)((byDataLength & 0xFF00) >> 8);  // (H)

            // Not used
            by218Header[8] = 0x00;
            by218Header[9] = 0x00;
            by218Header[10] = 0x00;
            by218Header[11] = 0x00;

            if (byCMD == (int)MEMOBUSCMD.WR_MREG)
            {
                byMemobusLength = (ushort)(0x06 + (dwRegNum * 4));// MEMOBUS_LENGTH;
            }

            // Command length
            byMemobusMsg[0] = (byte)(byMemobusLength & 0x00FF);
            byMemobusMsg[1] = (byte)((byMemobusLength & 0xFF00) >> 8);

            // Always MFC : 0x20
            byMemobusMsg[2] = MFC;

            // MEMOBUS command
            byMemobusMsg[3] = (byte)byCMD;

            // CPU number
            byMemobusMsg[4] = (byte)(byCpuNo << 4);

            // Spare
            byMemobusMsg[5] = 0x00;

            byMemobusMsg[6] = (byte)(dwStartAddr & 0x00FF);         // Addr(L)
            byMemobusMsg[7] = (byte)((dwStartAddr & 0xFF00) >> 8);  // Addr(H)

            byMemobusMsg[8] = (byte)(dwRegNum & 0x00FF);            // DataNum(L)
            byMemobusMsg[9] = (byte)((dwRegNum & 0xFF00) >> 8);     // DataNum(H)

            Buffer.BlockCopy(by218Header, 0, bypCmdBuf, 0, by218Header.Length);
            Buffer.BlockCopy(byMemobusMsg, 0, bypCmdBuf, HEADER218_LENGTH, byMemobusMsg.Length);

            if (byCMD == (int)MEMOBUSCMD.WR_MREG)
            {
                if (bypWBuf != null)
                {
                    Buffer.BlockCopy(bypWBuf, 0, bypCmdBuf, HEADER218_LENGTH + MEMOBUS_LENGTH, bypWBuf.Length);
                }
            }
        }
    }
}
