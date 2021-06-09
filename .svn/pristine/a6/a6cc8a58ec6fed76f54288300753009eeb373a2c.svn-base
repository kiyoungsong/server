using DriverInterface;
using IronUtilites;
using Opc.Ua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace IronServer
{
    public struct _tagIOInfo
    {
        string strName;
        Type tType;
        bool bRedis;
        int iSize;
        string strMemory;

        public string StrName { get => strName; set => strName = value; }
        public string StrMemory { get => strMemory; set => strMemory = value; }
        public Type TType { get => tType; set => tType = value; }
        public bool BRedis { get => bRedis; set => bRedis = value; }
        public int ISize { get => iSize; set => iSize = value; }
    }

    class Device
    {
        IDriver driver = null;
        Dictionary<string, _tagIOInfo> tagInfoDic = new Dictionary<string, _tagIOInfo>();
        private const string connectionTag = "_commStatus";

        public Dictionary<string, _tagIOInfo> TagInfoDic { get => tagInfoDic; }
        
        public void LoadDriver(string path,_tagDeviceInfo deviceInfo)
        {
            if (path.LastIndexOf(Path.DirectorySeparatorChar) != -1)
            {
                path = path.TrimEnd(Path.DirectorySeparatorChar);
            }

            try
            {
                Assembly asm = Assembly.LoadFrom(path + Path.DirectorySeparatorChar + Definition.DRIVER + Path.DirectorySeparatorChar + deviceInfo.StrDriver + Definition.DllExtension);
                driver = (IDriver)asm.CreateInstance(asm.ManifestModule.ScopeName.Replace(Definition.DllExtension, "" ) + "." + deviceInfo.StrClassName);
            }
            catch (Exception ex)
            {
                IronUtilites.LogManager.Manager.WriteLog("Exception", "Device - Reflection Assembly/Driver dll : " + ex.Message);
            }
            
            Setting(deviceInfo.UHexNum, path + Path.DirectorySeparatorChar + Definition.CONFIG + Path.DirectorySeparatorChar + deviceInfo.StrIOFile);

            if (deviceInfo.BReal)
            {
                try
                {
                    Connect();
                }
                catch (Exception ex)
                {
                    IronUtilites.LogManager.Manager.WriteLog("Exception", "Connection Exception : " + ex.Message);
                }
            }
        }

        public void Connect()
        {
            driver.Connect();
        }

        public void DisConnect()
        {
            driver.Disconnect();
        }

        void Setting(uint hexNum, string path)
        {
            string[] strName = new string[0];
            string[] strType = new string[0];
            string[] strMemory = new string[0];
            uint[] uScantime = new uint[0];
            bool[] bRedis = new bool[0];
            int[] iSize = new int[0];


            try
            {
                if (driver != null)
                {
                    driver.SetDriverInfo(hexNum > 0, 0, path);
                    driver.GetTagInfo(ref strName, ref strType, ref bRedis, ref iSize, ref strMemory);
                    IronUtilites.LogManager.Manager.WriteLog("Console", $"IronServer-Device- {path}");
                }
                else
                {
                    IronUtilites.LogManager.Manager.WriteLog("Exception", "IronServer-Device- driver object is null");
                }
            }
            catch (Exception ex)
            {
                IronUtilites.LogManager.Manager.WriteLog("Exception", "Device - driver Func Exception : " + ex.Message);
            }

            for (int i = 0; i < strName.Length; i++)
            {
                _tagIOInfo tagIO = new _tagIOInfo();
                tagIO.StrName = strName[i];
                tagIO.BRedis = bRedis[i];
                tagIO.ISize = iSize[i];
                tagIO.StrMemory = strMemory[i];

                switch (strType[i])
                {
                    case DriverInterface.Definition.BOOL:
                        tagIO.TType = typeof(bool);
                        break;
                    case DriverInterface.Definition.SHORT:
                        tagIO.TType = typeof(short);
                        break;
                    case DriverInterface.Definition.INT:
                        tagIO.TType = typeof(int);
                        break;
                    case DriverInterface.Definition.LONG:
                        tagIO.TType = typeof(long);
                        break;
                    case DriverInterface.Definition.FLOAT:
                        tagIO.TType = typeof(float);
                        break;
                    case DriverInterface.Definition.DOUBLE:
                        tagIO.TType = typeof(double);
                        break;
                    case DriverInterface.Definition.STRING:
                        tagIO.TType = typeof(string);
                        break;
                    case DriverInterface.Definition.USHORT:
                        tagIO.TType = typeof(ushort);
                        break;
                    case DriverInterface.Definition.UINT:
                        tagIO.TType = typeof(uint);
                        break;
                    case DriverInterface.Definition.ULONG:
                        tagIO.TType = typeof(ulong);
                        break;
                }

                tagInfoDic.Add(tagIO.StrName, tagIO);
            }
        }

        public bool CommunicationStatus()
        {
            return driver.CommunicationStatus();
        }

        public bool ReadTag(string tagName, ref object data)
        {
            return driver.ReadTag(tagName, ref data);
        }

        public bool ReadTags(string[] tagNames, ref object[] datas)
        {
            return driver.ReadTags(tagNames, ref datas);
        }

        public bool WriteTag(bool bSwap, string tagName, object data)
        {
            object paramObj = Definition.ObjectToBytes(bSwap, tagInfoDic[tagName].ISize, tagInfoDic[tagName].TType, data);

            return driver.WriteTag(tagName, paramObj);
        }

        public bool WriteTags(bool bSwap, string[] tagNames, object[] datas)
        {
            object[] paramObjs = new object[datas.Length];

            for (int i = 0; i < datas.Length; i++)
            {
                paramObjs[i] = Definition.ObjectToBytes(bSwap, tagInfoDic[tagNames[i]].ISize, tagInfoDic[tagNames[i]].TType, datas[i]);
            }

            return driver.WriteTags(tagNames, paramObjs);
        }
    }
}
