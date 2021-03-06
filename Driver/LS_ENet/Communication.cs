using DriverBase;
using DriverInterface;
using IronUtilites;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace LS_ENet
{
    public class Communication : TCP
    {
        const int READ_MAXSIZE = 512; //[Unit : Byte] spec : 1400;
        const int WRITE_MAXSIZE = 1;

        bool connectStatus = false;

        const int HEADER20_LENGTH = 20;
        const int ENETBODY_LENGTH = 21;
        const int READCOMMANDBUFF_LENGTH = 41;
        const int WRITECOMMANDBITBUFF_LENGTH = 43;
        const int WRITECOMMANDBYTEBUFF_LENGTH = 41;
        int writeID = 0;

        public enum DataCommand
        {
            Read = 0x54,
            Write = 0x58
        };

        public enum DeviceType
        {
            M = 0x4d,
            W = 0x57
        };


        public Communication(bool log = false, string name = "", string path = ".", bool server = false, string addr = "192.168.0.1", ushort port = 2004, int timeout = 1, int connectTime = 1) :
            base(addr, port, log, name, path, server, timeout, connectTime)
        {
        }

        ~Communication()
        {
        }

        public void Connect()
        {
            Open();
            connectStatus = true;
            base.isError = false;
        }

        public bool IsSocketError() 
        {
            bool socketError = base.isError;

            return socketError;
        }

        public void DisConnect()
        {
            Close();
            connectStatus = false;
        }

        public bool GetBits(int[] addrs, int[] lengths, ref byte[] buff, int device, int requestCount)
        {
            lock (this)
            {
                byte[] commandBuff = new byte[READCOMMANDBUFF_LENGTH];
                byte[] writeBuff = new byte[22];

                MemoryManager.MemoryMap map = MemoryManager.CreateMemoryMap(addrs, lengths, READ_MAXSIZE);

                int totalLength = 0;

                foreach (int l in map.lengths)
                {
                    totalLength += l;
                }

                byte[] readData = new byte[totalLength];

                int index = 0;

                for (int i = 0; i < map.addrs.Count; i++)
                {
                    int length = ((map.lengths[i] + map.addrs[i] % 16) / 16) + ((map.lengths[i] + map.addrs[i] % 16) % 16 > 0 ? 1: 0); // Address Length
                    int addr = map.addrs[i] / 16;
                    int offset = map.addrs[i] % 16;

                    GetBitCommand((int)DataCommand.Read, device, (ushort)length, (ushort)(addr * 2), (ushort)offset, ref writeBuff, ref commandBuff);

                    byte[] readBuff = null;

                    for (int reqCnt = 0; reqCnt < requestCount; reqCnt++)
                    {
                        bool writeResult = Write(ref commandBuff);

                        int readSize = 32 + length * 2;
                        readBuff = new byte[readSize];
                        bool readResult = Read(ref readBuff, readSize);

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
                    }

                    byte[] temp = readBuff.ToList().GetRange(32, readBuff.Length - 32).ToArray();
                    readBuff = temp;

                    BitArray bitArr = new BitArray(readBuff);
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

        public bool GetRegisters(int[] addrs, int[] lengths, ref byte[] buff, int device, int requestCount)
        {
            lock (this)
            {
                byte[] commandBuff = new byte[READCOMMANDBUFF_LENGTH];
                byte[] writeBuff = new byte[22];

                MemoryManager.MemoryMap map = MemoryManager.CreateMemoryMap(addrs, lengths, READ_MAXSIZE);

                int totalLength = 0;

                foreach (int l in map.lengths)
                {
                    totalLength += l;
                }

                byte[] readData = new byte[totalLength * 2];

                int index = 0;

                byte[] readBuff = null;

                for (int i = 0; i < map.addrs.Count; i++)
                {

                    GetByteCommand((int)DataCommand.Read, device, (ushort)map.lengths[i], (ushort)(map.addrs[i] * 2), ref writeBuff, ref commandBuff);

                    for (int reqCnt = 0; reqCnt < requestCount; reqCnt++)
                    {
                        bool writeResult = Write(ref commandBuff);

                        int readSize = 32 + map.lengths[i] * 2;
                        readBuff = new byte[readSize];

                        bool readResult = Read(ref readBuff, readSize);

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
                    }

                    Buffer.BlockCopy(readBuff, 32, readData, index, map.lengths[i] * 2);
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

        public bool SetBit(int addr, int length, ref byte[] writebuff, int device, int requestCount)
        {
            lock (this)
            {
                int byteLength = length / 8 + (length % 8 > 0 ? 1 : 0);
                byte[] commandBuff = new byte[WRITECOMMANDBITBUFF_LENGTH];
                byte[] writeBuff = new byte[byteLength];

                for (int i = 0; i < length; i++)
                {
                    int offset = addr % 16;
                    byte[] writeData = writebuff.ToList().GetRange(i * 2, 1).ToArray();
                    
                    GetBitCommand((int)DataCommand.Write, device, (ushort)length, (ushort)(addr / 16), (ushort)offset, ref writeData, ref commandBuff);

                    for (int reqCnt = 0; reqCnt < requestCount; reqCnt++)
                    {
                        bool writeResult = Write(ref commandBuff);

                        byte[] temp = new byte[30];
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
                    }

                    addr += 1;
                }

                return true;
            }
        }

        public bool SetRegister(int addr, int length, ref byte[] writebuff, int device, int requestCount)
        {
            lock (this)
            {
                int[] addrs = { addr };
                int[] lengths = { length };

                int index = 0;

                byte[] writeTempBuff = new byte[lengths[index] * 2];
                byte[] commandBuff = new byte[WRITECOMMANDBYTEBUFF_LENGTH + writebuff.Length];

                Buffer.BlockCopy(writebuff, index, writeTempBuff, 0, lengths[index] * 2);

                GetByteCommand((int)DataCommand.Write, device, (ushort)lengths[index], (ushort)(addrs[index] * 2), ref writeTempBuff, ref commandBuff);

                for (int reqCnt = 0; reqCnt < requestCount; reqCnt++)
                {
                    bool writeResult = Write(ref commandBuff);

                    byte[] temp = new byte[30];
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
                }

                return true;
            }
        }

        void GetBitCommand(int byCMD, int device, ushort dwRegNum, ushort dwStartAddr, ushort offset, ref byte[] bypWBuf, ref byte[] bypCmdBuf)
        {
            byte[] by20Header = new byte[HEADER20_LENGTH];
            byte[] byEnetMsg = new byte[ENETBODY_LENGTH];

            if(byCMD == (int)DataCommand.Write)
            {
                byEnetMsg = new byte[ENETBODY_LENGTH + 2];
            }

            // Header
            by20Header[0] = 0x4c;                   // L
            by20Header[1] = 0x47;                   // G
            by20Header[2] = 0x49;                   // I
            by20Header[3] = 0x53;                   // S
            by20Header[4] = 0x2d;                   // -
            by20Header[5] = 0x47;                   // G
            by20Header[6] = 0x4c;                   // L
            by20Header[7] = 0x4f;                   // O
            by20Header[8] = 0x46;                   // F
            by20Header[9] = 0x41;                   // A
            by20Header[10] = 0x00;                  // plc info 
            by20Header[11] = 0x00;                  // plc info
            by20Header[12] = 0x00;                  // reserved
            by20Header[13] = 0x33;                  // h33 mmi -> plc
            by20Header[14] = 0x00;                  // invoke ID
            by20Header[15] = 0x00;                  // invoke ID
            by20Header[16] = 0x15;                  // Application Instruction
            by20Header[17] = 0x00;                  // Application Instruction
            by20Header[18] = 0x00;                  // Reserved
            by20Header[19] = HeaderSum(by20Header); // Check Sum

            // Body
            byEnetMsg[0] = 0x54;                    // Command Read : 54, Write : 58
            byEnetMsg[1] = 0x00;                    // Command
            byEnetMsg[2] = 0x14;                    // Data Type 14(Continue)
            byEnetMsg[3] = 0x00;                    // Data Type
            byEnetMsg[4] = 0x00;                    // Don't Care
            byEnetMsg[5] = 0x00;                    // Don't Care
            byEnetMsg[6] = 0x01;                    // Block Number
            byEnetMsg[7] = 0x00;                    // Block Number
            byEnetMsg[8] = 0x09;                    // Address Length
            byEnetMsg[9] = 0x00;                    // Address Length
            byEnetMsg[10] = 0x25;                   // %
            byEnetMsg[11] = (byte)device;                 // Device Hex(메모리 영역)
            byEnetMsg[12] = 0x42;                   // bit : 58, word : 57
            byEnetMsg[13] = Encoding.ASCII.GetBytes(Convert.ToInt32((dwStartAddr.ToString("D6").Substring(0, 1))).ToString("X"))[0];
            byEnetMsg[14] = Encoding.ASCII.GetBytes(Convert.ToInt32((dwStartAddr.ToString("D6").Substring(1, 1))).ToString("X"))[0];
            byEnetMsg[15] = Encoding.ASCII.GetBytes(Convert.ToInt32((dwStartAddr.ToString("D6").Substring(2, 1))).ToString("X"))[0];
            byEnetMsg[16] = Encoding.ASCII.GetBytes(Convert.ToInt32((dwStartAddr.ToString("D6").Substring(3, 1))).ToString("X"))[0];
            byEnetMsg[17] = Encoding.ASCII.GetBytes(Convert.ToInt32((dwStartAddr.ToString("D6").Substring(4, 1))).ToString("X"))[0];
            byEnetMsg[18] = Encoding.ASCII.GetBytes(Convert.ToInt32((dwStartAddr.ToString("D6").Substring(5, 1))).ToString("X"))[0];
            byEnetMsg[19] = (byte)(dwRegNum * 2 % 256);                   // ReadCount
            byEnetMsg[20] = (byte)(dwRegNum * 2 / 256);                   // ReadCount

            if (byCMD == (int)DataCommand.Write)
            {
                by20Header[14] = (byte)writeID;
                by20Header[16] = 0x17;                                              // body Length
                by20Header[19] = HeaderSum(by20Header);                             // Check Sum
                byEnetMsg[0] = 0x58;       
                byEnetMsg[2] = 0x00;                                                // 22   data type
                byEnetMsg[8] = 0x0A;                                                // Address Length
                byEnetMsg[12] = 0x58;                                               // bit : 58, word : 57
                byEnetMsg[19] = Encoding.ASCII.GetBytes(offset.ToString("X"))[0];
                byEnetMsg[20] = 0x01;                                               // write count
                byEnetMsg[21] = 0x00;                                               // write count
            }

            Buffer.BlockCopy(by20Header, 0, bypCmdBuf, 0, by20Header.Length);
            Buffer.BlockCopy(byEnetMsg, 0, bypCmdBuf, HEADER20_LENGTH, byEnetMsg.Length);
            
            if (bypWBuf != null)
            {
                switch (byCMD)
                {
                    case (int)DataCommand.Write:
                        Buffer.BlockCopy(bypWBuf, 0, bypCmdBuf, HEADER20_LENGTH + ENETBODY_LENGTH + 1, bypWBuf.Length);
                        break;
                }
            }
        }

        void GetByteCommand(int byCMD, int device, ushort dwRegNum, ushort dwStartAddr, ref byte[] bypWBuf, ref byte[] bypCmdBuf)
        {
            byte[] by20Header = new byte[HEADER20_LENGTH];
            byte[] byEnetMsg = new byte[ENETBODY_LENGTH];

            if (byCMD == (int)DataCommand.Write)
            {
                byEnetMsg = new byte[ENETBODY_LENGTH + 2];
            }

            // Header
            by20Header[0] = 0x4c;                   // L
            by20Header[1] = 0x47;                   // G
            by20Header[2] = 0x49;                   // I
            by20Header[3] = 0x53;                   // S
            by20Header[4] = 0x2d;                   // -
            by20Header[5] = 0x47;                   // G
            by20Header[6] = 0x4c;                   // L
            by20Header[7] = 0x4f;                   // O
            by20Header[8] = 0x46;                   // F
            by20Header[9] = 0x41;                   // A
            by20Header[10] = 0x00;                  // plc info 
            by20Header[11] = 0x00;                  // plc info
            by20Header[12] = 0x00;                  // reserved
            by20Header[13] = 0x33;                  // h33 mmi -> plc
            by20Header[14] = 0x00;                  // invoke ID
            by20Header[15] = 0x00;                  // invoke ID
            by20Header[16] = 0x15;                  // Application Instruction
            by20Header[17] = 0x00;                  // Application Instruction
            by20Header[18] = 0x00;                  // Reserved
            by20Header[19] = HeaderSum(by20Header); // Check Sum

            // Body
            byEnetMsg[0] = 0x54;                    // Command Read : 54, Write : 58
            byEnetMsg[1] = 0x00;                    // Command
            byEnetMsg[2] = 0x14;                    // Data Type 14(Continue)
            byEnetMsg[3] = 0x00;                    // Data Type
            byEnetMsg[4] = 0x00;                    // Don't Care
            byEnetMsg[5] = 0x00;                    // Don't Care
            byEnetMsg[6] = 0x01;                    // Block Number
            byEnetMsg[7] = 0x00;                    // Block Number
            byEnetMsg[8] = 0x09;                    // Address Length
            byEnetMsg[9] = 0x00;                    // Address Length
            byEnetMsg[10] = 0x25;                   // %
            byEnetMsg[11] = (byte)device;                 // Device Hex(메모리 영역)
            byEnetMsg[12] = 0x42;                   // bit : 58, word : 77, byte 42
            byEnetMsg[13] = Encoding.ASCII.GetBytes(Convert.ToInt32((dwStartAddr.ToString("D6").Substring(0, 1))).ToString("X"))[0];
            byEnetMsg[14] = Encoding.ASCII.GetBytes(Convert.ToInt32((dwStartAddr.ToString("D6").Substring(1, 1))).ToString("X"))[0];
            byEnetMsg[15] = Encoding.ASCII.GetBytes(Convert.ToInt32((dwStartAddr.ToString("D6").Substring(2, 1))).ToString("X"))[0];
            byEnetMsg[16] = Encoding.ASCII.GetBytes(Convert.ToInt32((dwStartAddr.ToString("D6").Substring(3, 1))).ToString("X"))[0];
            byEnetMsg[17] = Encoding.ASCII.GetBytes(Convert.ToInt32((dwStartAddr.ToString("D6").Substring(4, 1))).ToString("X"))[0];
            byEnetMsg[18] = Encoding.ASCII.GetBytes(Convert.ToInt32((dwStartAddr.ToString("D6").Substring(5, 1))).ToString("X"))[0];
            byEnetMsg[19] = (byte)(dwRegNum * 2 % 256);                   // ReadCount
            byEnetMsg[20] = (byte)(dwRegNum * 2 / 256);                   // ReadCount

            if (byCMD == (int)DataCommand.Write)
            {
                by20Header[14] = (byte)writeID;
                by20Header[16] = (byte)(bypCmdBuf.Length - 20); // body Length , Single : 0x17
                by20Header[19] = HeaderSum(by20Header);         // Check Sum
                byEnetMsg[0] = 0x58;
                byEnetMsg[8] = 0x09;                            // 28 Addr Length
                byEnetMsg[19] = (byte)(dwRegNum * 2);           // write length
                byEnetMsg[20] = 0;                              // write length
            }

            Buffer.BlockCopy(by20Header, 0, bypCmdBuf, 0, by20Header.Length);
            Buffer.BlockCopy(byEnetMsg, 0, bypCmdBuf, HEADER20_LENGTH, byEnetMsg.Length);

            if (bypWBuf != null)
            {
                switch (byCMD)
                {
                    case (int)DataCommand.Write:
                        Buffer.BlockCopy(bypWBuf, 0, bypCmdBuf, HEADER20_LENGTH + ENETBODY_LENGTH, bypWBuf.Length);
                        break;
                }
            }
        }

        private byte HeaderSum(byte[] value)
        {
            int v = 0;

            for (int i = 0; i < 17; i++)
            {
                v += value[i];

                if (v > 256)
                {
                    v = v - 256;
                }
            }

            return (byte)v;
        }

        public bool CommunicationStatus()
        {
            return connectStatus;
        }
    }
}
