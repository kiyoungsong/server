using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DriverBase;
using DriverInterface;
using IronUtilites;

namespace Omron_FINS
{
    public class Communication : UDP
    {
        public enum FINSCMD
        {
            Bit_COI = 0x30,
            Bit_WR = 0x31,
            Bit_HR = 0x32,
            Bit_DM = 0x02,
            Bit_EM = 0x20,
            Word_COI = 0xB0,
            Word_WR = 0xB1,
            Word_HR = 0xB2,
            Word_DM = 0x82,
            Word_EM = 0x98,
        }

        const string CIO = "cio";
        const string WR = "wr";
        const string HR = "hr";
        const string DM = "dm";
        const string EM = "em";

        const byte FINSHEADER_LENGTH = 10;
        const byte FINSCMD_LENGTH = 8;

        const int BitReadMaxSize = 960;
        const int ByteReadMaxSize = 960;
        const int BitWriteMaxSize = 960;
        const int ByteWriteMaxSize = 960;

        const int RegisterCommandSize = 18;
        const int BitCommandSize = 18;
        const int RecvCommandSize = 14;
        
        public byte da1;         //server
        public byte sa1;         //client

        public Communication(bool log = false, string name = "", string path = ".", bool server = false, string addr = "127.0.0.1", ushort port = 9600, string clientAddr = "127.0.0.1", ushort clientPort = 9600, int timeout = 1, int connectTime = 1) :
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

        public bool GetBits(int[] addrs, int[] lengths, ref byte[] buff, string memoryType, int requestCount)
        {
            lock (this)
            {
                byte[] commandBuff = new byte[BitCommandSize];
                byte[] writeBuff = new byte[0];

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
                    int length = ((map.lengths[i] + map.addrs[i] % 16) / 16) + ((map.lengths[i] + map.addrs[i] % 16) % 16 > 0 ? 1 : 0); // Address Length
                    int addr = map.addrs[i] / 16;
                    int offset = map.addrs[i] % 16;

                    switch (memoryType)
                    {
                        case CIO:
                            GetSendCommand((int)FINSCMD.Word_COI, da1, sa1, (ushort)length, (ushort)addr, (ushort)offset, ref writeBuff, ref commandBuff, false);
                            break;
                        case WR:
                            GetSendCommand((int)FINSCMD.Word_WR, da1, sa1, (ushort)length, (ushort)addr, (ushort)offset, ref writeBuff, ref commandBuff, false);
                            break;
                        case HR:
                            GetSendCommand((int)FINSCMD.Word_HR, da1, sa1, (ushort)length, (ushort)addr, (ushort)offset, ref writeBuff, ref commandBuff, false);
                            break;
                        case DM:
                            GetSendCommand((int)FINSCMD.Word_DM, da1, sa1, (ushort)length, (ushort)addr, (ushort)offset, ref writeBuff, ref commandBuff, false);
                            break;
                        case EM:
                            GetSendCommand((int)FINSCMD.Word_EM, da1, sa1, (ushort)length, (ushort)addr, (ushort)offset, ref writeBuff, ref commandBuff, false);
                            break;
                    }

                    byte[] readBuff = null;

                    for (int reqCnt = 0; reqCnt < requestCount; reqCnt++)
                    {
                        bool writeResult = Write(ref commandBuff);

                        int readSize = RecvCommandSize + length * 2;
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

                    byte[] temp = readBuff.ToList().GetRange(RecvCommandSize, readBuff.Length - RecvCommandSize).ToArray();
                    readBuff = temp;

                    //swap 
                    for (int j = 0; j < readBuff.Length; j += 2)
                    {
                        byte tempByte = readBuff[j];
                        readBuff[j] = readBuff[j + 1];
                        readBuff[j + 1] = tempByte;
                    }

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

        public bool GetRegisters(int[] addrs, int[] lengths, ref byte[] buff, string memoryType, int requestCount)
        {
            lock (this)
            {
                byte[] commandBuff = new byte[18];
                byte[] writeBuff = new byte[0];
                ushort bitIndex = 0;

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
                    switch (memoryType)
                    {
                        case CIO:
                            GetSendCommand((int)FINSCMD.Word_COI, da1, sa1, (ushort)map.lengths[i], (ushort)map.addrs[i], bitIndex, ref writeBuff, ref commandBuff, false);
                            break;
                        case WR:
                            GetSendCommand((int)FINSCMD.Word_WR, da1, sa1, (ushort)map.lengths[i], (ushort)map.addrs[i], bitIndex, ref writeBuff, ref commandBuff, false);
                            break;
                        case HR:
                            GetSendCommand((int)FINSCMD.Word_HR, da1, sa1, (ushort)map.lengths[i], (ushort)map.addrs[i], bitIndex, ref writeBuff, ref commandBuff, false);
                            break;
                        case DM:
                            GetSendCommand((int)FINSCMD.Word_DM, da1, sa1, (ushort)map.lengths[i], (ushort)map.addrs[i], bitIndex, ref writeBuff, ref commandBuff, false);
                            break;
                        case EM:
                            GetSendCommand((int)FINSCMD.Word_EM, da1, sa1, (ushort)map.lengths[i], (ushort)map.addrs[i], bitIndex, ref writeBuff, ref commandBuff, false);
                            break;
                    }

                    byte[] readBuff = null;

                    for (int reqCnt = 0; reqCnt < requestCount; reqCnt++)
                    {
                        bool writeResult = Write(ref commandBuff);

                        int readSize = RecvCommandSize + map.lengths[i] * 2;
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

                    //swap 
                    for (int j = 0; j < readBuff.Length; j += 2)
                    {
                        byte temp = readBuff[j];
                        readBuff[j] = readBuff[j + 1];
                        readBuff[j + 1] = temp;
                    }

                    Buffer.BlockCopy(readBuff, RecvCommandSize, readData, index, map.lengths[i] * 2);
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

        public bool SetBit(int addr, int length, ref byte[] writebuff, string memoryType, int requestCount)
        {
            lock (this)
            {
                if (memoryType.Contains(EM))
                {
                    return false;
                }

                byte[] commandBuff = new byte[BitCommandSize + length];
                byte[] writeBuff = new byte[writebuff.Length/2];
                int offset = addr % 16;
                int realAddr = addr / 16;

                for (int i = 0; i < length; i++)
                {
                    writeBuff[i] = writebuff[i * 2];
                }

                switch (memoryType)
                {
                    case CIO:
                        GetSendCommand((int)FINSCMD.Bit_COI, da1, sa1, (ushort)length, (ushort)realAddr, (ushort)offset, ref writeBuff, ref commandBuff, true);
                        break;
                    case WR:
                        GetSendCommand((int)FINSCMD.Bit_WR, da1, sa1, (ushort)length, (ushort)realAddr, (ushort)offset, ref writeBuff, ref commandBuff, true);
                        break;
                    case HR:
                        GetSendCommand((int)FINSCMD.Bit_HR, da1, sa1, (ushort)length, (ushort)realAddr, (ushort)offset, ref writeBuff, ref commandBuff, true);
                        break;
                    case DM:
                        GetSendCommand((int)FINSCMD.Bit_DM, da1, sa1, (ushort)length, (ushort)realAddr, (ushort)offset, ref writeBuff, ref commandBuff, true);
                        break;
                    case EM:
                        GetSendCommand((int)FINSCMD.Bit_EM, da1, sa1, (ushort)length, (ushort)realAddr, (ushort)offset, ref writeBuff, ref commandBuff, true);
                        break;
                }

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

        public bool SetRegister(int addr, int length, ref byte[] writebuff, string memoryType, int requestCount)
        {
            lock (this)
            {
                int[] addrs = { addr };
                int[] lengths = { length };
                ushort bitIndex = 0;

                MemoryManager.MemoryMap map = MemoryManager.CreateMemoryMap(addrs, lengths, ByteWriteMaxSize);

                int index = 0;

                for (int i = 0; i < map.addrs.Count; i++)
                {
                    byte[] writeTempBuff = new byte[map.lengths[i] * 2];
                    byte[] commandBuff = new byte[RegisterCommandSize + writeTempBuff.Length];

                    Buffer.BlockCopy(writebuff, index, writeTempBuff, 0, map.lengths[i] * 2);

                    switch (memoryType)
                    {
                        case CIO:
                            GetSendCommand((int)FINSCMD.Word_COI, da1, sa1, (ushort)length, (ushort)addr, bitIndex, ref writeTempBuff, ref commandBuff, true);
                            break;
                        case WR:
                            GetSendCommand((int)FINSCMD.Word_WR, da1, sa1, (ushort)length, (ushort)addr, bitIndex, ref writeTempBuff, ref commandBuff, true);
                            break;
                        case HR:
                            GetSendCommand((int)FINSCMD.Word_HR, da1, sa1, (ushort)length, (ushort)addr, bitIndex, ref writeTempBuff, ref commandBuff, true);
                            break;
                        case DM:
                            GetSendCommand((int)FINSCMD.Word_DM, da1, sa1, (ushort)length, (ushort)addr, bitIndex, ref writeTempBuff, ref commandBuff, true);
                            break;
                        case EM:
                            GetSendCommand((int)FINSCMD.Word_EM, da1, sa1, (ushort)length, (ushort)addr, bitIndex, ref writeTempBuff, ref commandBuff, true);
                            break;
                    }

                    for (int reqCnt = 0; reqCnt < requestCount; reqCnt++)
                    {
                        bool writeResult = Write(ref commandBuff);

                        byte[] temp = new byte[RecvCommandSize];
                        bool readResult = Read(ref temp, RecvCommandSize);

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

                    index += map.lengths[i] * 2;
                }

                return true;
            }
        }

        void GetSendCommand(int byCMD, byte da1, byte sa1, ushort dwRegNum, ushort dwStartAddr, ushort dwBitIdx,ref byte[] bypWBuf, ref byte[] bypCmdBuf, bool isWrite)
        {
            byte[] finsHeader = new byte[FINSHEADER_LENGTH];
            byte[] byCmdMsg = new byte[FINSCMD_LENGTH];

            finsHeader[0] = 0x80;             // 00 ICF Information control field 
            finsHeader[1] = 0x00;             // 01 RSC Reserved 
            finsHeader[2] = 0x02;             // 02 GTC Gateway count
            finsHeader[3] = 0x00;             // 03 DNA Destination network address (0=local network)
            finsHeader[4] = da1;              // 04 DA1 Destination node number
            finsHeader[5] = 0x00;             // 05 DA2 Destination unit address
            finsHeader[6] = 0x00;             // 06 SNA Source network address (0=local network)
            finsHeader[7] = sa1;   		      // 07 SA1 Source node number
            finsHeader[8] = 0x00;             // 08 SA2 Source unit address
            finsHeader[9] = 0x00;             // 09 SID Service ID

            byCmdMsg[0] = 0x01;                // 0 MC Main command
            byCmdMsg[1] = 0x01;                // 1 SC Subcommand(read:1, write:2)
            
            if(isWrite)
            {
                byCmdMsg[1] = 0x02;
            }
            
            byCmdMsg[2] = (byte)byCMD;                                      // 2 reserved area for additional params 
            byCmdMsg[3] = (byte)((dwStartAddr >> 8) & 0xFF);                // 3,4 start addr 
            byCmdMsg[4] = (byte)(dwStartAddr & 0xFF);                       
            byCmdMsg[5] = (byte)dwBitIdx;                                    // 5 BitIndex
            byCmdMsg[6] = (byte)((dwRegNum >> 8) & 0xFF);                   // 6, 7 Length
            byCmdMsg[7] = (byte)(dwRegNum & 0xFF);

            Buffer.BlockCopy(finsHeader, 0, bypCmdBuf, 0, finsHeader.Length);
            Buffer.BlockCopy(byCmdMsg, 0, bypCmdBuf, finsHeader.Length, byCmdMsg.Length);

            if(isWrite && bypWBuf != null)
            {
                if (bypWBuf.Length > 1)
                {
                    //swap 
                    switch (byCMD)
                    {
                        case (byte)FINSCMD.Word_COI:
                        case (byte)FINSCMD.Word_DM:
                        case (byte)FINSCMD.Word_EM:
                        case (byte)FINSCMD.Word_HR:
                        case (byte)FINSCMD.Word_WR:
                            for (int j = 0; j < bypWBuf.Length; j += 2)
                            {
                                byte temp = bypWBuf[j];
                                bypWBuf[j] = bypWBuf[j + 1];
                                bypWBuf[j + 1] = temp;
                            }
                            break;
                    }
                }

                Buffer.BlockCopy(bypWBuf, 0, bypCmdBuf, finsHeader.Length + byCmdMsg.Length, bypWBuf.Length);
            }
        }
    }
}
