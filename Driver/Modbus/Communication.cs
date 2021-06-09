using System;
using EasyModbus;
using IronUtilites;

namespace Modbus
{
    public class Communication
    {
        const int BYTE_READ_MAXSIZE = 125;
        const int BYTE_WRITE_MAXSIZE = 120;
        const int BIT_WRITE_MAXSIZE = 1960;
        const int BIT_READ_MAXSIZE = 2000;

        EasyModbus.ModbusClient client;
        
        public Communication()
        {
        }

        public string IpAddress { get; set; }
        public int Port { get; set; }
        public int ConnectTime { get; set; }

        public void Connect()
        {
            client = new ModbusClient(IpAddress, Port, ConnectTime);
            client.Connect();

            if(client.Connected)
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

        public bool IsCommunication()
        {
            return client.Connected;
        }

        public bool GetCoils(int[] addrs, int[] lengths, ref byte[] buff, int requestCount)
        {
            lock (this)
            {
                MemoryManager.MemoryMap map = MemoryManager.CreateMemoryMap(addrs, lengths, BIT_READ_MAXSIZE);

                int totalLength = 0;

                foreach (int l in map.lengths)
                {
                    totalLength += l;
                }

                ushort[] readData = new ushort[totalLength];

                int index = 0;

                for (int i = 0; i < map.addrs.Count; i++)
                {
                    ushort[] tempData = null;

                    for (int reqCnt = 0; reqCnt < requestCount; reqCnt++)
                    {
                        try
                        {
                            tempData = client.ReadCoils(map.addrs[i], map.lengths[i]);
                            break;
                        }
                        catch (Exception ex)
                        {
                            IronUtilites.LogManager.Manager.WriteLog("Exception", "ReadCoils failed");
                        }
                    }

                    Buffer.BlockCopy(tempData, 0, readData, index, tempData.Length * 2);

                    index += tempData.Length * 2;
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

        public bool GetInputs(int[] addrs, int[] lengths, ref byte[] buff, int requestCount)
        {
            lock (this)
            {
                MemoryManager.MemoryMap map = MemoryManager.CreateMemoryMap(addrs, lengths, BIT_READ_MAXSIZE);

                int totalLength = 0;

                foreach (int l in map.lengths)
                {
                    totalLength += l;
                }

                ushort[] readData = new ushort[totalLength];

                int index = 0;

                for (int i = 0; i < map.addrs.Count; i++)
                {
                    ushort[] tempData = null;

                    for (int reqCnt = 0; reqCnt < requestCount; reqCnt++)
                    {
                        try
                        {
                            tempData = client.ReadDiscreteInputs(map.addrs[i], map.lengths[i]);
                            break;
                        }
                        catch (Exception ex)
                        {
                            IronUtilites.LogManager.Manager.WriteLog("Exception", "ReadInputs failed");
                        }
                    }

                    Buffer.BlockCopy(tempData, 0, readData, index, tempData.Length * 2);

                    index += tempData.Length * 2;
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

        public bool GetInputRegisters(int[] addrs, int[] lengths, ref byte[] buff, int requestCount)
        {
            lock (this)
            {
                MemoryManager.MemoryMap map = MemoryManager.CreateMemoryMap(addrs, lengths, BYTE_READ_MAXSIZE);

                int totalLength = 0;

                foreach (int l in map.lengths)
                {
                    totalLength += l;
                }

                ushort[] readData = new ushort[totalLength];

                int index = 0;

                for (int i = 0; i < map.addrs.Count; i++)
                {
                    ushort[] tempData = null;

                    for (int reqCnt = 0; reqCnt < requestCount; reqCnt++)
                    {
                        try
                        {
                            tempData = client.ReadInputRegisters(map.addrs[i], map.lengths[i]);
                            break;
                        }
                        catch (Exception ex)
                        {
                            IronUtilites.LogManager.Manager.WriteLog("Exception", "ReadInputRegisters failed");
                        }
                    }

                    Buffer.BlockCopy(tempData, 0, readData, index, tempData.Length * 2);

                    index += tempData.Length * 2;
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

        public bool GetHoldingRegisters(int[] addrs, int[] lengths, ref byte[] buff, int requestCount)
        {
            lock(this)
            {
                MemoryManager.MemoryMap map = MemoryManager.CreateMemoryMap(addrs, lengths, BYTE_READ_MAXSIZE);

                int totalLength = 0;

                foreach (int l in map.lengths)
                {
                    totalLength += l;
                }
                
                ushort[] readData = new ushort[totalLength];

                int index = 0;

                for (int i = 0; i < map.addrs.Count; i++)
                {
                    ushort[] tempData = null;

                    for (int reqCnt = 0; reqCnt < requestCount; reqCnt++)
                    {
                        try
                        {
                            tempData = client.ReadHoldingRegisters(map.addrs[i], map.lengths[i]);
                            break;
                        }
                        catch (Exception ex)
                        {
                            IronUtilites.LogManager.Manager.WriteLog("Exception", "ReadHoldingRegisters failed");
                        }
                    }

                    Buffer.BlockCopy(tempData, 0, readData, index, tempData.Length * 2);

                    index += tempData.Length * 2;
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

        public bool SetCoil(int addr, int length, ref byte[] writebuff, int requestCount)
        {
            lock(this)
            {
                int[] addrs = { addr };
                int[] lengths = { length };

                MemoryManager.MemoryMap map = MemoryManager.CreateMemoryMap(addrs, lengths, BIT_WRITE_MAXSIZE);

                int index = 0;

                for (int i = 0; i < map.addrs.Count; i++)
                {
                    bool[] writeTempBuff = new bool[map.lengths[i]];

                    for (int j = 0; j < map.lengths[i]; j++)
                    {
                        writeTempBuff[j] = Convert.ToBoolean(writebuff[index * 2]);
                        index++;
                    }

                    for (int reqCnt = 0; reqCnt < requestCount; reqCnt++)
                    {
                        try
                        {
                            client.WriteMultipleCoils(map.addrs[i], writeTempBuff);
                            break;
                        }
                        catch (Exception ex)
                        {
                            IronUtilites.LogManager.Manager.WriteLog("Exception", "WriteCoils failed");
                        }
                    }
                }

                return true;
            }
        }

        public bool SetHoldingRegister(int addr, int length, ref byte[] writebuff, int requestCount)
        {
            lock(this)
            {
                int[] addrs = { addr };
                int[] lengths = { length };

                MemoryManager.MemoryMap map = MemoryManager.CreateMemoryMap(addrs, lengths, BYTE_WRITE_MAXSIZE);

                int index = 0;

                for (int i = 0; i < map.addrs.Count; i++)
                {
                    int[] writeTempBuff = new int[map.lengths[i]];

                    for(int j = 0; j < map.lengths[i]; j++)
                    {

                        Buffer.BlockCopy(writebuff, index, writeTempBuff, j * 4, 2);
                        
                        index += 2;
                    }

                    for (int reqCnt = 0; reqCnt < requestCount; reqCnt++)
                    {
                        try
                        {
                            client.WriteMultipleRegisters(map.addrs[i], writeTempBuff);
                            break;
                        }
                        catch (Exception ex)
                        {
                            IronUtilites.LogManager.Manager.WriteLog("Exception", "WriteCoils failed");
                        }
                    }
                }

                return true;
            }
        }
    }
}
