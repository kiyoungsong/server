using System;

namespace Mistubishi_Melsec
{
    class Program
    {
        static void Main(string[] args)
        {
            Melsec m = new Melsec();

            m.SetDriverInfo(false, 0, "Melsec.xml");
            //m.SetDriverInfo(false, 0, "Modbus.xml");

            string[] strName = null;
            string[] strType = null;
            bool[] bRedis = null;
            int[] iSize = null;
            string[] strMemory = null;

            m.GetTagInfo(ref strName, ref strType, ref bRedis, ref iSize, ref strMemory);
            m.Connect();

            byte[] list = new byte[10];
            object result = list;
            byte[] list1 = new byte[10];
            object result1 = list1;
            byte[] list2 = new byte[10];
            object result2 = list2;
            byte[] list3 = new byte[10];
            object result3 = list3;
            string[] strs = { "Test0", "Test1" };
            object[] objs = { result1, result2 };
            //m.ReadTags(strs, ref objs);
            //m.ReadTag("Test0", ref result1);
            //m.ReadTag("Test1", ref result1);
            //m.ReadTag("Test2", ref result2);
            //m.ReadTag("Test3", ref result3);
            result = new byte[4] { 20, 0, 21, 0 };//, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0};
            m.WriteTag("Test0", result);
        }
    }
}
