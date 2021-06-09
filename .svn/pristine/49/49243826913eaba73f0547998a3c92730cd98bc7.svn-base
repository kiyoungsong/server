using System;

namespace DriverInterface
{
    public interface IDriver
    {
		void Connect();
        void Disconnect();
        void SetDriverInfo(bool bHexAddress, int nScanMode, string param);
        void GetTagInfo(ref string[] strName, ref string[] strType, ref bool[] bRedis, ref int[] iSize, ref string[] strMemory);
        bool CommunicationStatus();
        bool ReadTag(string tagName, ref object data);
        bool ReadTags(string[] tagNames, ref object[] datas);
        bool WriteTag(string tagName, object data);
        bool WriteTags(string[] tagNames, object[] datas);
    }
}
