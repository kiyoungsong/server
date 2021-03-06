using DriverInterface;
using IronUtilites;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IronServer
{
    public class DeviceManager
    {
        bool start = false;
        bool pause = false;

        IronSAN ironServerInfo = null;

        Dictionary<string, Dictionary<string, DeviceInfo>> deviceInfoDic = new Dictionary<string, Dictionary<string, DeviceInfo>>();
        Dictionary<string, Dictionary<string, Device>> deviceDic = new Dictionary<string, Dictionary<string, Device>>();
        
        internal Dictionary<string, Dictionary<string, DeviceInfo>> DeviceInfoDic { get => deviceInfoDic; }
        internal Dictionary<string, Dictionary<string, Device>> DeviceDic { get => deviceDic; }
        public IronSAN IronServerInfo { get => ironServerInfo; }

        public void Start(string path)
        {
            start = true;

            if (path.LastIndexOf(Path.DirectorySeparatorChar) != path.Length + 1)
            {
                path = path.TrimEnd(Path.DirectorySeparatorChar);
            }

            ironServerInfo = GetServerInfo(path);

            if (ironServerInfo == null)
            {
                IronUtilites.LogManager.Manager.WriteLog("Error", path + @"\ChannelInfo.xml File Not Exist");
                return;
            }

            foreach (_tagChannelInfo chInfo in ironServerInfo.TagChannelnfo)
            {
                string channelName = chInfo.Name;

                foreach (_tagDeviceInfo Dvinfo in ironServerInfo.TagDeviceInfo)
                {
                    if (chInfo.IId == Dvinfo.IchannelID)
                    {
                        FileInfo fi = new FileInfo(path + Path.DirectorySeparatorChar + Definition.DRIVER + Path.DirectorySeparatorChar + Dvinfo.StrDriver + Definition.DllExtension);

                        string driverName = Dvinfo.DriverName;
                        DeviceInfo deviceInfo = new DeviceInfo();

                        if (fi.Exists)
                        {
                            try
                            {
                                deviceInfo.SetChannelInfo(chInfo);
                                deviceInfo.SetDeviceInfo(Dvinfo);

                                if (!deviceInfoDic.ContainsKey(chInfo.Name))
                                {
                                    deviceInfoDic.Add(chInfo.Name, new Dictionary<string, DeviceInfo>());
                                }

                                deviceInfoDic[chInfo.Name].Add(driverName, deviceInfo);

                                Device device = new Device();
                                device.LoadDriver(path, Dvinfo);

                                if (!deviceDic.ContainsKey(chInfo.Name))
                                {
                                    deviceDic.Add(chInfo.Name, new Dictionary<string, Device>());
                                }

                                deviceDic[chInfo.Name].Add(driverName, device);
                            }
                            catch (Exception ex)
                            {
                                IronUtilites.LogManager.Manager.WriteLog("Exception", "Setting DeviceInfo : " + ex.Message);
                            }
                        }
                    }
                }
            }
        }

        public void Stop()
        {

        }

        public void Pause()
        {

        }

        public void Resume()
        {

        }

        private IronSAN GetServerInfo(string path)
        {
            if (path.LastIndexOf(Path.DirectorySeparatorChar) != -1)
            {
                path = path.TrimEnd(Path.DirectorySeparatorChar);
            }
            string configPath = path + Path.DirectorySeparatorChar + Definition.CONFIG + Path.DirectorySeparatorChar + Definition.DeviceInfoName;
            ironServerInfo = (IronSAN)XMLParser.DeserializeXML(configPath, typeof(IronSAN));

            return ironServerInfo;
        }
    }
}
