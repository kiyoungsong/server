using DriverInterface;
using IronUtilites;
using Modbus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Melsecneth
{
    class Communication
    {
        public bool GetBit(List<Tag> tagInfo, int nPath, int nNetwork, int nStation, int devType, ref int nPrevBitError, ref byte[] buff, int requestCount)
        {
            lock (this)
            {
                int nErr = 0;
                int cnt = 0;
                int[] addrs = new int[tagInfo.Count];
                int[] lengths = new int[tagInfo.Count];

                foreach (Tag tag in tagInfo)
                {
                    addrs[cnt] = Convert.ToInt32(tag.addr);
                    lengths[cnt] = Convert.ToInt32(tag.size) * (int)Enum.Parse(typeof(Definition.DataType), tag.type.ToUpper());

                    cnt++;
                }

                MemoryManager.MemoryMap map = MemoryManager.CreateMemoryMap(addrs, lengths, Definition.MAX_MELSEC_BIT_SIZE - 8);

                int totalLength = 0;

                foreach (int l in map.lengths)
                {
                    totalLength += l;
                }

                ushort[] readData = new ushort[totalLength];

                int index = 0;

                for (int i = 0; i < map.addrs.Count; i++)
                {
                    int length = (map.lengths[i] / Definition.MELSEC_BIT_UNIT) + (map.lengths[i] % Definition.MELSEC_BIT_UNIT > 0 ? 1 : 0);
                    int addr = (map.addrs[i] / Definition.MELSEC_BIT_UNIT) * Definition.MELSEC_BIT_UNIT;
                    int offset = map.addrs[i] % Definition.MELSEC_BIT_UNIT;
                    byte[] idata = new byte[length];

                    for (int reqCnt = 0; reqCnt < requestCount; reqCnt++)
                    {
                        nErr = Melsecneth.mdreceiveex(nPath, nNetwork, nStation, devType, addr, ref length, idata);

                        if (nErr != 0)
                        {
                            IronUtilites.LogManager.Manager.WriteLog("Error", "nErr : " + nErr + ", start Address is " + addr);

                            if(reqCnt == requestCount - 1)
                            {
                                return false;
                            }
                        }
                    }

                    BitArray bitArr = new BitArray(idata);
                    ushort[] bytes = bitArr.Cast<bool>().Select(bit => bit ? (ushort)1 : (ushort)0).ToArray<ushort>();
                    
                    if ((readData.Length - index / 2) > bytes.Length - offset)
                    {
                        Buffer.BlockCopy(bytes, offset * 2, readData, index, (bytes.Length - offset) * 2);
                        index += (bytes.Length - offset) * 2;
                    }
                    else
                    {
                        Buffer.BlockCopy(bytes, offset * 2, readData, index, map.lengths[i] * 2);
                        index += bytes.Length * 2;
                    }
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

        public bool GetWord(List<Tag> tagInfo, int nPath, int nNetwork, int nStation, int devType, ref int nPrevWordError, ref byte[] buff, int requestCount)
        {
            lock (this)
            {
                int[] addrs = new int[tagInfo.Count];
                int[] lengths = new int[tagInfo.Count];
                bool[] dwords = new bool[tagInfo.Count];
                int cnt = 0;
                int nErr = 0;
                int totalLength = 0;

                foreach (Tag tag in tagInfo)
                {
                    addrs[cnt] = Convert.ToInt32(tag.addr);
                    lengths[cnt] = Convert.ToInt32(tag.size) * (int)Enum.Parse(typeof(Definition.DataType), tag.type.ToUpper());
                    dwords[cnt] = tag.dword == "1" ? true : false;

                    cnt++;
                }

                MemoryManager.MemoryMap map = MemoryManager.CreateMemoryMap(addrs, lengths, Definition.MAX_MELSEC_WORD_SIZE / 2);
                
                foreach (int l in map.lengths)
                {
                    totalLength += l;
                }

                byte[] readData = new byte[totalLength *2];
                int index = 0;

                for (int i = 0; i < map.addrs.Count; i++)
                {
                    int length = map.lengths[i] * 2;
                    int addr = map.addrs[i];
                    byte[] idata = new byte[length];


                    for (int reqCnt = 0; reqCnt < requestCount; reqCnt++)
                    {
                        nErr = Melsecneth.mdreceiveex(nPath, nNetwork, nStation, devType, addr, ref length, idata);

                        if (nErr != 0)
                        {
                            IronUtilites.LogManager.Manager.WriteLog("Error", "nErr : " + nErr + ", start Address is " + addr);

                            if(reqCnt == requestCount - 1)
                            {
                                return false;
                            }
                        }
                    }

                    Buffer.BlockCopy(idata, 0, readData, index, idata.Length);

                    index += idata.Length * 2;
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

                return nErr == 0 ? true : false;
            }
        }

        public bool SetWord(Tag tag, int nPath, int nNetwork, int nStation, int devType, ref int nPrevWordError, byte[] data, int requestCount)
        {
            lock (this)
            {
                int.TryParse(tag.addr, out int startAddr);
                int.TryParse(tag.size, out int size);
                int[] addrs = { startAddr };
                int[] lengths = { size * (int)Enum.Parse(typeof(Definition.DataType), tag.type.ToUpper()) };

                MemoryManager.MemoryMap map = MemoryManager.CreateMemoryMap(addrs, lengths, Definition.MAX_MELSEC_WORD_SIZE);

                int index = 0;
                int nErr = 0;

                for (int i = 0; i < map.addrs.Count; i++)
                {
                    int length = map.lengths[i] * 2;
                    byte[] writeTempBuff = new byte[map.lengths[i] * 2];

                    for (int j = 0; j < map.lengths[i]; j++)
                    {
                        Buffer.BlockCopy(data, index, writeTempBuff, j * 2, 2);
                        index += 2;
                    }

                    for (int reqCnt = 0; reqCnt < requestCount; reqCnt++)
                    {
                        nErr = Melsecneth.mdsendex(nPath, nNetwork, nStation, devType, map.addrs[i], ref length, writeTempBuff);

                        if (nErr != 0 && nErr != nPrevWordError)
                        {
                            IronUtilites.LogManager.Manager.WriteLog("Error", "WriteWordData Error , Error Code is " + nErr);
                            nPrevWordError = nErr;

                            if (reqCnt == requestCount - 1)
                            {
                                return false;
                            }
                        }
                    }
                }

                return true;
            }
        }

        public bool SetBit(Tag tag, int nPath, int nNetwork, int nStation, int devType, ref int nPrevBitError, byte[] data, int requestCount)
        {
            lock (this)
            {
                int nErr = 0;
                int offset = 0;
                int.TryParse(tag.addr, out int startAddr);
                int.TryParse(tag.size, out int size);
                int[] addrs = { startAddr };
                int[] lengths = { size * (int)Enum.Parse(typeof(Definition.DataType), tag.type.ToUpper()) };

                MemoryManager.MemoryMap map = MemoryManager.CreateMemoryMap(addrs, lengths, Definition.MAX_MELSEC_BIT_SIZE - 8);

                int totalLength = 0;

                foreach (int l in map.lengths)
                {
                    totalLength += l;
                }

                ushort[] readData = new ushort[totalLength];

                int index = 0;

                for (int i = 0; i < map.addrs.Count; i++)
                {
                    int length = (map.lengths[i] / Definition.MELSEC_BIT_UNIT) + (map.lengths[i] % Definition.MELSEC_BIT_UNIT > 0 ? 1 : 0);
                    int addr = (map.addrs[i] / Definition.MELSEC_BIT_UNIT) * Definition.MELSEC_BIT_UNIT;
                    offset = map.addrs[i] % Definition.MELSEC_BIT_UNIT;
                    byte[] idata = new byte[length];

                    for (int reqCnt = 0; reqCnt < requestCount; reqCnt++)
                    {
                        nErr = Melsecneth.mdreceiveex(nPath, nNetwork, nStation, devType, addr, ref length, idata);

                        if (nErr != 0 && nErr != nPrevBitError)
                        {
                            IronUtilites.LogManager.Manager.WriteLog("Error", "Read BitData Error , Error Code is " + nErr);
                            nPrevBitError = nErr;

                            if(reqCnt == requestCount - 1)
                            {
                                return false;
                            }
                        }
                    }
                    
                    BitArray bitArr = new BitArray(idata);
                    bool[] bytes = bitArr.Cast<bool>().Select(bit => bit ? true : false).ToArray();

                    for (int j = offset; j < bytes.Length; j++)
                    {
                        if (data.Length > index * 2)
                        {
                            bytes[j] = data[index++ * 2] == 1 ? true : false;
                        }
                        else
                        {
                            break;
                        }
                    }
                    
                    byte[] setBytes = new byte[bytes.Length / 8];
                    bitArr = new BitArray(bytes);
                    bitArr.CopyTo(setBytes, 0);

                    nErr = Melsecneth.mdsendex(nPath, nNetwork, nStation, devType, addr, ref length, setBytes);

                    if (nErr != 0 && nErr != nPrevBitError)
                    {
                        IronUtilites.LogManager.Manager.WriteLog("Error", "Write WordData Error , Error Code is " + nErr);
                        nPrevBitError = nErr;
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
