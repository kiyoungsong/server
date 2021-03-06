using DriverInterface;
using IronUtilites;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace BoschCISS
{
    public class CISS : DriverInterface.IDriver
    {
        Driver xmlInfo = null;
        
        bool bFlag = false;
        string path = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Definition.DRIVER;
        
        Dictionary<string, Tag> tagInfos = new Dictionary<string, Tag>();
        List<string> macList = new List<string>();
        Thread[] th;
        DateTime[] dts;
        
        public void Connect()
        {
            try
            {
                bFlag = true;
                CreateWrite();

                // Thread Setting - 1 Device 1 Thread [File Read Thread]
                th = new Thread[xmlInfo.Device.Count];
                dts = new DateTime[xmlInfo.Device.Count];

                for (int i = 0; i < th.Length; i++)
                {
                    if (th[i] == null)
                    {
                        string mac = xmlInfo.Device[i].mac;
                        th[i] = new Thread(() => RunAsync(mac));
                        th[i].Start();
                    }
                    else if (th[i].ThreadState != System.Threading.ThreadState.Running)
                    {
                        th[i].Start();
                    }
                }

                // FileReadTime Setting
                for (int i = 0; i < dts.Length; i++)
                {
                    dts[i] = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                bFlag = false;
                
                foreach (Thread thread in th)
                {
                    thread.Join();
                }
                LogManager.Manager.WriteLog("Error", $"Plaese Check {xmlInfo.adapter} Power On");
                Console.WriteLine(ex.Message);
            }
        }

        public void Disconnect()
        {
            bFlag = false;

            foreach (Thread thread in th)
            {
                thread.Join();
            }
        }

        private void CreateWrite()
        {
            string dirName = path + Path.DirectorySeparatorChar + Definition.RESOURCE;
            DirectoryInfo di = new DirectoryInfo(dirName);

            if (!di.Exists)
            {
                di.Create();
            }

            foreach (var mac in Communication.macIndex)
            {
                if(!String.IsNullOrEmpty(mac))
                {
                    FileInfo fileInfo = new FileInfo(dirName + Path.DirectorySeparatorChar + mac + Definition.WRITEFILE + Definition.TEXT);

                    if (!fileInfo.Exists)
                    {
                        using (FileStream fs = new FileStream(dirName + Path.DirectorySeparatorChar + mac + Definition.WRITEFILE + Definition.TEXT, FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            StreamWriter sw = new StreamWriter(fs);
                            sw.Write('2');
                            sw.Flush();
                        }
                    }
                }
            }
        }

        public bool CommunicationStatus()
        {
            bool check = false;
            
            DirectoryInfo di = new DirectoryInfo(path + Path.DirectorySeparatorChar + Definition.RESOURCE);
            
            if (di.Exists)
            {
                FileInfo[] fis = di.GetFiles();
            
                if (fis.Length > 0)
                {
                    foreach (var file in fis)
                    {
                        if (macList.IndexOf(file.Name.Split('.')[0]) >= 0 && !file.Name.Contains("_WR"))
                        {
                            if(dts[macList.IndexOf(file.Name.Split('.')[0])] != file.LastWriteTime)
                            {
                                dts[macList.IndexOf(file.Name.Split('.')[0])] = file.LastWriteTime;
                            }

                            TimeSpan t = new TimeSpan(0, 0, 3);

                            if ((DateTime.Now - file.LastWriteTime).TotalSeconds < t.TotalSeconds)
                            {
                                check = true;
                            }
                        }
                    }
                }
            }

            return check;
        }

        public void SetDriverInfo(bool bHexAddress, int nScanMode, string param)
        {
            try
            {
                xmlInfo = (Driver)XMLParser.DeserializeXML(param, typeof(Driver));

                Communication.SettingIndex();

                foreach (Device device in xmlInfo.Device)
                {
                    Communication.SettingDevice(device.mac , path);
                    macList.Add(device.mac);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Read Xml File Failed Error Message : " + ex.Message);
            }
        }

        public void GetTagInfo(ref string[] strName, ref string[] strType, ref bool[] bRedis, ref int[] iSize, ref string[] strMemory)
        {
            List<string> tagList = new List<string>();
            List<string> typeList = new List<string>();
            List<uint> scantimeList = new List<uint>();
            List<bool> redisList = new List<bool>();
            List<int> sizeList = new List<int>();
            List<string> memoryList = new List<string>();

            try
            {
                Communication.serviceUUIDList["00007500-0000-1000-8000-00805f9b34fb"] = new List<string>();

                Communication.serviceUUIDList["00007500-0000-1000-8000-00805f9b34fb"].Add("00007502-0000-1000-8000-00805f9b34fb");
                Communication.serviceUUIDList["00007500-0000-1000-8000-00805f9b34fb"].Add("00007504-0000-1000-8000-00805f9b34fb");
                Communication.serviceUUIDList["00007500-0000-1000-8000-00805f9b34fb"].Add("0000750a-0000-1000-8000-00805f9b34fb");

                foreach (Device device_ in xmlInfo.Device)
                {
                    string adapterName = xmlInfo.adapter;

                    if (!Communication.deviceMacList.ContainsKey(adapterName))
                    {
                        Communication.deviceMacList[adapterName] = new List<string>();
                    }

                    Communication.deviceMacList[adapterName].Add(device_.mac);

                    string device = device_.id;
                    Setting(device_, device);

                    #region 7502
                    tagList.Add(device + "_AccelerometerX");
                    typeList.Add("short");
                    scantimeList.Add(100);
                    redisList.Add(true);
                    sizeList.Add(1);
                    memoryList.Add("7502");
                    tagList.Add(device + "_AccelerometerY");
                    typeList.Add("short");
                    scantimeList.Add(100);
                    redisList.Add(true);
                    sizeList.Add(1);
                    memoryList.Add("7502");
                    tagList.Add(device + "_AccelerometerZ");
                    typeList.Add("short");
                    scantimeList.Add(100);
                    redisList.Add(true);
                    sizeList.Add(1);
                    memoryList.Add("7502");
                    tagList.Add(device + "_GyroscopeX");
                    typeList.Add("short");
                    scantimeList.Add(100);
                    redisList.Add(true);
                    sizeList.Add(1);
                    memoryList.Add("7502");
                    tagList.Add(device + "_GyroscopeY");
                    typeList.Add("short");
                    scantimeList.Add(100);
                    redisList.Add(true);
                    sizeList.Add(1);
                    memoryList.Add("7502");
                    tagList.Add(device + "_GyroscopeZ");
                    typeList.Add("short");
                    scantimeList.Add(100);
                    redisList.Add(true);
                    sizeList.Add(1);
                    memoryList.Add("7502");
                    tagList.Add(device + "_MagnetometerX");
                    typeList.Add("short");
                    scantimeList.Add(100);
                    redisList.Add(true);
                    sizeList.Add(1);
                    memoryList.Add("7502");
                    tagList.Add(device + "_MagnetometerY");
                    typeList.Add("short");
                    scantimeList.Add(100);
                    redisList.Add(true);
                    sizeList.Add(1);
                    memoryList.Add("7502");
                    tagList.Add(device + "_MagnetometerZ");
                    typeList.Add("short");
                    scantimeList.Add(100);
                    redisList.Add(true);
                    sizeList.Add(1);
                    memoryList.Add("7502");
                    #endregion

                    #region 7504
                    tagList.Add(device + "_Temperature");
                    typeList.Add("short");
                    scantimeList.Add(100);
                    redisList.Add(true);
                    sizeList.Add(1);
                    memoryList.Add("7504");
                    tagList.Add(device + "_Humidity");
                    typeList.Add("ushort");
                    scantimeList.Add(100);
                    redisList.Add(true);
                    sizeList.Add(1);
                    memoryList.Add("7504");
                    tagList.Add(device + "_Pressure");
                    typeList.Add("uint");
                    scantimeList.Add(100);
                    redisList.Add(true);
                    sizeList.Add(1);
                    memoryList.Add("7504");
                    tagList.Add(device + "_Noise");
                    typeList.Add("short");
                    scantimeList.Add(100);
                    redisList.Add(true);
                    sizeList.Add(1);
                    memoryList.Add("7504");
                    tagList.Add(device + "_Light");
                    typeList.Add("uint");
                    scantimeList.Add(100);
                    redisList.Add(true);
                    sizeList.Add(1);
                    memoryList.Add("7504");
                    #endregion

                    #region 750a
                    tagList.Add(device + "_FrequencyMode");
                    typeList.Add("short");
                    scantimeList.Add(100);
                    redisList.Add(true);
                    sizeList.Add(1);
                    memoryList.Add("750a");
                    #endregion
                }

                strName = tagList.ToArray();
                strType = typeList.ToArray();
                bRedis = redisList.ToArray();
                iSize = sizeList.ToArray();
                strMemory = memoryList.ToArray();
            }
            catch (Exception ex)
            {
                LogManager.Manager.WriteLog("Exception", $"GetTagInfo : {ex.Message}");
            }
        }

        private void Setting(Device device_, string device)
        {
            #region don't Open
            Tag t = new Tag()
            {
                tag = device + "_AccelerometerX",
                addr = "0",
                type = "short",
                scantime = 100,
                redis = true,
                size = 16,
                memory = "7502",
                charuuid = "00007502-0000-1000-8000-00805f9b34fb",
                serviceuuid = "00007500-0000-1000-8000-00805f9b34fb",
                mac = device_.mac
            };
            device_.tags.Add(t);
            tagInfos[t.tag] = t;

            t = new Tag()
            {
                tag = device + "_AccelerometerY",
                addr = "16",
                type = "short",
                scantime = 100,
                redis = true,
                size = 16,
                memory = "7502",
                charuuid = "00007502-0000-1000-8000-00805f9b34fb",
                serviceuuid = "00007500-0000-1000-8000-00805f9b34fb",
                mac = device_.mac
            };
            device_.tags.Add(t);
            tagInfos[t.tag] = t;

            t = new Tag()
            {
                tag = device + "_AccelerometerZ",
                addr = "32",
                type = "short",
                scantime = 100,
                redis = true,
                size = 16,
                memory = "7502",
                charuuid = "00007502-0000-1000-8000-00805f9b34fb",
                serviceuuid = "00007500-0000-1000-8000-00805f9b34fb",
                mac = device_.mac
            };
            device_.tags.Add(t);
            tagInfos[t.tag] = t;

            t = (new Tag()
            {
                tag = device + "_GyroscopeX",
                addr = "48",
                type = "short",
                scantime = 100,
                redis = true,
                size = 12,
                memory = "7502",
                charuuid = "00007502-0000-1000-8000-00805f9b34fb",
                serviceuuid = "00007500-0000-1000-8000-00805f9b34fb",
                mac = device_.mac
            });
            device_.tags.Add(t);
            tagInfos[t.tag] = t;

            t = (new Tag()
            {
                tag = device + "_GyroscopeY",
                addr = "60",
                type = "short",
                scantime = 100,
                redis = true,
                size = 12,
                memory = "7502",
                charuuid = "00007502-0000-1000-8000-00805f9b34fb",
                serviceuuid = "00007500-0000-1000-8000-00805f9b34fb",
                mac = device_.mac
            });
            device_.tags.Add(t);
            tagInfos[t.tag] = t;

            t = (new Tag()
            {
                tag = device + "_GyroscopeZ",
                addr = "72",
                type = "short",
                scantime = 100,
                redis = true,
                size = 12,
                memory = "7502",
                charuuid = "00007502-0000-1000-8000-00805f9b34fb",
                serviceuuid = "00007500-0000-1000-8000-00805f9b34fb",
                mac = device_.mac
            });
            device_.tags.Add(t);
            tagInfos[t.tag] = t;

            t = (new Tag()
            {
                tag = device + "_MagnetometerX",
                addr = "84",
                type = "short",
                scantime = 100,
                redis = true,
                size = 14,
                memory = "7502",
                charuuid = "00007502-0000-1000-8000-00805f9b34fb",
                serviceuuid = "00007500-0000-1000-8000-00805f9b34fb",
                mac = device_.mac
            });
            device_.tags.Add(t);
            tagInfos[t.tag] = t;

            t = (new Tag()
            {
                tag = device + "_MagnetometerY",
                addr = "98",
                type = "short",
                scantime = 100,
                redis = true,
                size = 14,
                memory = "7502",
                charuuid = "00007502-0000-1000-8000-00805f9b34fb",
                serviceuuid = "00007500-0000-1000-8000-00805f9b34fb",
                mac = device_.mac
            });
            device_.tags.Add(t);
            tagInfos[t.tag] = t;

            t = (new Tag()
            {
                tag = device + "_MagnetometerZ",
                addr = "112",
                type = "short",
                scantime = 100,
                redis = true,
                size = 16,
                memory = "7502",
                charuuid = "00007502-0000-1000-8000-00805f9b34fb",
                serviceuuid = "00007500-0000-1000-8000-00805f9b34fb",
                mac = device_.mac
            });
            device_.tags.Add(t);
            tagInfos[t.tag] = t;

            t = (new Tag()
            {
                tag = device + "_Temperature",
                addr = "0",
                type = "short",
                scantime = 100,
                redis = true,
                size = 16,
                memory = "7504",
                charuuid = "00007504-0000-1000-8000-00805f9b34fb",
                serviceuuid = "00007500-0000-1000-8000-00805f9b34fb",
                mac = device_.mac
            });
            device_.tags.Add(t);
            tagInfos[t.tag] = t;

            t = (new Tag()
            {
                tag = device + "_Humidity",
                addr = "16",
                type = "ushort",
                scantime = 100,
                redis = true,
                size = 16,
                memory = "7504",
                charuuid = "00007504-0000-1000-8000-00805f9b34fb",
                serviceuuid = "00007500-0000-1000-8000-00805f9b34fb",
                mac = device_.mac
            });
            device_.tags.Add(t);
            tagInfos[t.tag] = t;

            t = (new Tag()
            {
                tag = device + "_Pressure",
                addr = "4",
                type = "uint",
                scantime = 100,
                redis = true,
                size = 1,
                memory = "7504",
                charuuid = "00007504-0000-1000-8000-00805f9b34fb",
                serviceuuid = "00007500-0000-1000-8000-00805f9b34fb",
                mac = device_.mac
            });
            device_.tags.Add(t);
            tagInfos[t.tag] = t;

            t = (new Tag()
            {
                tag = device + "_Noise",
                addr = "64",
                type = "short",
                scantime = 100,
                redis = true,
                size = 16,
                memory = "7504",
                charuuid = "00007504-0000-1000-8000-00805f9b34fb",
                serviceuuid = "00007500-0000-1000-8000-00805f9b34fb",
                mac = device_.mac
            });
            device_.tags.Add(t);
            tagInfos[t.tag] = t;

            t = (new Tag()
            {
                tag = device + "_Light",
                addr = "10",
                type = "uint",
                scantime = 100,
                redis = true,
                size = 1,
                memory = "7504",
                charuuid = "00007504-0000-1000-8000-00805f9b34fb",
                serviceuuid = "00007500-0000-1000-8000-00805f9b34fb",
                mac = device_.mac
            });
            device_.tags.Add(t);
            tagInfos[t.tag] = t;

            t = (new Tag()
            {
                tag = device + "_FrequencyMode",
                addr = "16",
                type = "short",
                scantime = 100,
                redis = true,
                size = 8,
                memory = "750a",
                charuuid = "0000750a-0000-1000-8000-00805f9b34fb",
                serviceuuid = "00007500-0000-1000-8000-00805f9b34fb",
                mac = device_.mac
            });
            device_.tags.Add(t);
            tagInfos[t.tag] = t;
            #endregion
        }

        public bool ReadTag(string tagName, ref object data)
        {
            try
            {
                string mac = tagInfos[tagName].mac;
                string charuuid = tagInfos[tagName].charuuid;
                
                byte[] driverData = Communication.GetData(mac, charuuid);

                BitArray bitArr = new BitArray(driverData);
                byte[] bytes = bitArr.Cast<bool>().Select(bit => bit ? (byte)1 : (byte)0).ToArray<byte>();

                bool[] tempByte = null;

                tempByte = new bool[tagInfos[tagName].size];

                int srcOffset = Convert.ToInt32(tagInfos[tagName].addr);

                for (int j = 0; j < tempByte.Length; j++)
                {
                    tempByte[j] = bytes[srcOffset + j] == (byte)1 ? true : false;
                }

                byte[] setBytes = new byte[tempByte.Length / 8 + (tempByte.Length % 8 > 0 ? 1 : 0)];
                bitArr = new BitArray(tempByte);
                bitArr.CopyTo(setBytes, 0);

                int length = (int)Enum.Parse(typeof(DriverInterface.Definition.DataType), tagInfos[tagName].type.ToUpper()) * 2;
                if (length > setBytes.Length)
                {
                    Array.Resize(ref setBytes, length);
                }

                if (!tagName.Contains("_FrequencyMode"))
                {
                    Array.Reverse(setBytes);
                }
                data = setBytes;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool ReadTags(string[] tagNames, ref object[] datas)
        {
            string logTagName = "";

            try
            {
                int cnt = 0;
                TimeSpan ts = new TimeSpan(0, 0, 3);

                foreach (string tag in tagNames)
                {
                    logTagName = tag;

                    try
                    {
                        string mac = tagInfos[tag].mac;
                        double mainTime = (DateTime.Now - dts[macList.IndexOf(mac)]).TotalSeconds;

                        if (mainTime > ts.TotalSeconds)
                        {
                            throw new Exception($"Disconnect Device! mac : {mac} , tag : {tag}");
                        }

                        string charuuid = tagInfos[tag].charuuid;

                        byte[] data = Communication.GetData(mac, charuuid);

                        int addr = Convert.ToInt32(tagInfos[tag].addr);
                        int size = Convert.ToInt32(tagInfos[tag].size);

                        string type = tagInfos[tag].type;

                        byte[] tempByte = null;

                        if (type == "int" || type == "uint")
                        {
                            tempByte = BitConverter.GetBytes(Utilities.GetIntData(ref data, addr));
                        }
                        else if (type == "ushort")
                        {
                            tempByte = BitConverter.GetBytes(Utilities.GetuShortData(ref data, addr, size));
                        }
                        else
                        {
                            tempByte = BitConverter.GetBytes(Utilities.GetShortData(ref data, addr, size));
                        }

                        datas[cnt++] = tempByte;
                    }
                    catch (Exception ex)
                    {
                        LogManager.Manager.WriteLog("Else", ex.Message);
                        datas[cnt++] = new byte[] { 0x00 };
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                IronUtilites.LogManager.Manager.WriteLog("Exception", $"readTags  tagName {logTagName} , {ex.Message}");
                return false;
            }
        }

        public bool WriteTag(string tagName, object data)
        {
            if (tagInfos[tagName].charuuid == "0000750a-0000-1000-8000-00805f9b34fb")
            {
                if (data is byte[] writeData)
                {
                    byte[] temp = { writeData[0] };
                    var r = Communication.WriteValue(path, tagInfos[tagName].mac, temp);
                    return true;
                }
            }

            return false;
        }

        public bool WriteTags(string[] tagNames, object[] datas)
        {
            return false;
        }

        private async void RunAsync(string mac)
        {
            while (bFlag)
            {
                try
                {
                    Stopwatch watch = new Stopwatch();
                    watch.Start();

                    string filepath = path + Path.DirectorySeparatorChar 
                        + Definition.RESOURCE + Path.DirectorySeparatorChar + mac + Definition.TEXT;
                    
                    bool result = await Communication.ReadValue(mac, filepath);

                    double checkTime = (double)watch.ElapsedMilliseconds;
                    int setInterval = Convert.ToInt32(Math.Max(1, 100 - checkTime));

                    Thread.Sleep(setInterval);
                }
                catch (Exception ex)
                {
                    LogManager.Manager.WriteLog("Error", $"ReadValue Thread mac : {mac} , ExceptionMessage : {ex.Message}");
                }
            }
        }
    }
}
