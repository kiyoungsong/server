using DriverInterface;
using IronUtilites;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace IronServer
{
    [XmlRoot(ElementName = "IronServer")]
    public class IronSAN
    {
        List<_tagDeviceInfo> tagDeviceInfo;
        List<_tagChannelInfo> tagChannelInfo;

        [XmlElement("Device")]
        public List<_tagDeviceInfo> TagDeviceInfo
        {
            get { return tagDeviceInfo; }
            set { tagDeviceInfo = value; }
        }
        [XmlElement("Channel")]
        public List<_tagChannelInfo> TagChannelnfo
        {
            get { return tagChannelInfo; }
            set { tagChannelInfo = value; }
        }
    }
    public struct _tagChannelInfo
    {
        int iId;
        string name;

        [XmlElement("ID")]
        public int IId { get => iId; set => iId = value; }
        [XmlElement("Name")]
        public string Name { get => name; set => name = value; }
    }

    public struct _tagDeviceInfo
	{
        uint iId;
        int ichannelId;
        string driverName;
        bool bReal;
        bool bThread;
        bool bLog;
        bool bSwap;
        uint uHexNum;
        uint uScanMode;
        uint uScanRate;
        string strDriver;
        string strClassName;
        string strIOFile;

        [XmlElement("ID")]
        public uint IId { get => iId; set => iId = value; }
        [XmlElement("ChannelID")]
        public int IchannelID { get => ichannelId; set => ichannelId = value; }
        [XmlElement("Name")]
        public string DriverName { get => driverName; set => driverName = value; }
        [XmlElement("Real")]
        public bool BReal { get => bReal; set => bReal = value; }
        [XmlElement("Thread")]
        public bool BThread { get => bThread; set => bThread = value; }
        [XmlElement("Log")]
        public bool BLog { get => bLog; set => bLog = value; }
        [XmlElement("HexNum")]
        public uint UHexNum { get => uHexNum; set => uHexNum = value; }
        [XmlElement("Swap")]
        public bool BSwap { get => bSwap; set => bSwap = value; }
        [XmlElement("ScanMode")]
        public uint UScanMode { get => uScanMode; set => uScanMode = value; }
        [XmlElement("ScanRate")]
        public uint UScanRate { get => uScanRate; set => uScanRate = value; }

        [XmlElement("IOFile")]
        public string StrIOFile { get => strIOFile; set => strIOFile = value; }
        [XmlElement("DLL")]
        public string StrDriver { get => strDriver; set => strDriver = value; }
        [XmlElement("ClassName")]
        public string StrClassName { get => strClassName; set => strClassName = value; }
    }
	public class DeviceInfo
    {
		_tagDeviceInfo tagDeviceInfo = new _tagDeviceInfo();
        _tagChannelInfo tagChannelInfo = new _tagChannelInfo();

        public _tagDeviceInfo TagDeviceInfo { get => tagDeviceInfo; }
        public _tagChannelInfo TagChannelInfo { get => tagChannelInfo; }

        public void SetDeviceInfo(_tagDeviceInfo info)
        {
            tagDeviceInfo.BLog = info.BLog;
            tagDeviceInfo.BReal = info.BReal;
            tagDeviceInfo.BSwap = info.BSwap;
            tagDeviceInfo.BThread = info.BThread;
            tagDeviceInfo.DriverName = info.DriverName;
            tagDeviceInfo.IchannelID = info.IchannelID;
            tagDeviceInfo.IId = info.IId;
            tagDeviceInfo.StrClassName = info.StrClassName;
            tagDeviceInfo.StrDriver = info.StrDriver;
            tagDeviceInfo.StrIOFile = info.StrIOFile;
            tagDeviceInfo.UHexNum = info.UHexNum;
            tagDeviceInfo.UScanMode = info.UScanMode;
            tagDeviceInfo.UScanRate = info.UScanRate;
        }
        public void SetChannelInfo(_tagChannelInfo ch)
        {
            tagChannelInfo.IId = ch.IId;
            tagChannelInfo.Name = ch.Name;
        }
    }
}
