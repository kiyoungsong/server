using DriverInterface;
using IronUtilites;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BoschCISS
{
    class Communication
    {

        public static byte[] char1_1 = new byte[15];
        public static byte[] char1_2 = new byte[15];
        public static byte[] char1_3 = new byte[15];
        public static byte[] char2_1 = new byte[15];
        public static byte[] char2_2 = new byte[15];
        public static byte[] char2_3 = new byte[15];
        public static byte[] char3_1 = new byte[15];
        public static byte[] char3_2 = new byte[15];
        public static byte[] char3_3 = new byte[15];
        public static byte[] char4_1 = new byte[15];
        public static byte[] char4_2 = new byte[15];
        public static byte[] char4_3 = new byte[15];
        public static byte[] char5_1 = new byte[15];
        public static byte[] char5_2 = new byte[15];
        public static byte[] char5_3 = new byte[15];

        public static string[] macIndex = new string[5];
        public static string[] charIndex = new string[3];
        static int macIndexCount = 0;

        public static ConcurrentDictionary<string,List<string>> serviceUUIDList = new ConcurrentDictionary<string, List<string>>();
        public static ConcurrentDictionary<string, List<string>> deviceMacList = new ConcurrentDictionary<string, List<string>>();

        internal static void SettingIndex()
        {
            int i = 0;
            charIndex[i++] = "00007502-0000-1000-8000-00805f9b34fb";
            charIndex[i++] = "00007504-0000-1000-8000-00805f9b34fb";
            charIndex[i++] = "0000750a-0000-1000-8000-00805f9b34fb";
        }

        internal static void SettingDevice(string mac, string path)
        {
            macIndex[macIndexCount++] = mac;

            DirectoryInfo di = new DirectoryInfo(path + Path.DirectorySeparatorChar + Definition.RESOURCE);

            if (!di.Exists)
            {
                di.Create();
            }

            using (FileStream fs = new FileStream(path + Path.DirectorySeparatorChar + Definition.RESOURCE + Path.DirectorySeparatorChar + "CISS" + Definition.WRITEFILE + Definition.TEXT, FileMode.Create, FileAccess.Write))
            {
                StringBuilder sb = new StringBuilder();
                StreamWriter sw = new StreamWriter(fs);
                
                foreach (var macAddress in macIndex)
                {
                    if (!string.IsNullOrEmpty(macAddress))
                    {
                        sb.Append(macAddress + Environment.NewLine);
                    }
                }

                sw.WriteLine(sb.ToString());
                sw.Flush();
            }
        }

        public static async Task WriteValue(string path, string mac, byte[] data)
        {
            try
            {
                List<byte> temp = new List<byte>();

                DirectoryInfo di = new DirectoryInfo(path + Path.DirectorySeparatorChar + Definition.RESOURCE);

                if (!di.Exists)
                {
                    di.Create();
                }

                using (FileStream fs = new FileStream(path + Path.DirectorySeparatorChar + Definition.RESOURCE + Path.DirectorySeparatorChar + mac + Definition.WRITEFILE + Definition.TEXT, FileMode.Create, FileAccess.Write))
                {
                    StreamWriter sw = new StreamWriter(fs);
                    sw.Write(data[0].ToString());
                    sw.Flush();
                }

                return;
            }
            catch (Exception ex)
            {
                LogManager.Manager.WriteLog("Exception", $"WriteValue Message : {ex.Message}");
            }
        }

        // Driver File Read & Data Store in Memory //
        internal static async Task<bool> ReadValue(string mac, string path)
        {
            try
            {
                int firstIndex = Array.IndexOf(macIndex, mac);

                string fileName = path;

                FileInfo fileInfo = new FileInfo(fileName);

                if (!fileInfo.Exists)
                {
                    return false;
                }

                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    var ms = new MemoryStream();
                    StreamReader sr = new StreamReader(fs);
                    sr.BaseStream.CopyTo(ms);

                    byte[] data = ms.GetBuffer();

                    byte[] data1 = data.ToList().GetRange(0, 20).ToArray();
                    byte[] data2 = data.ToList().GetRange(20, 20).ToArray();
                    byte[] data3 = data.ToList().GetRange(40, 20).ToArray();
                    
                    switch (firstIndex)
                    {
                        case 0:
                            char1_1 = data1;
                            char1_2 = data2;
                            char1_3 = data3;
                            break;
                        case 1:
                            char2_1 = data1;
                            char2_2 = data2;
                            char2_3 = data3;
                            break;
                        case 2:
                            char3_1 = data1;
                            char3_2 = data2;
                            char3_3 = data3;
                            break;
                        case 3:
                            char4_1 = data1;
                            char4_2 = data2;
                            char4_3 = data3;
                            break;
                        case 4:
                            char5_1 = data1;
                            char5_2 = data2;
                            char5_3 = data3;
                            break;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Manager.WriteLog("Exception", $"ReadValue Message : {ex.Message}");
                return false;
            }
        }

        internal static byte[] GetData(string mac, string charuuid)
        {
            int firstIndex = Array.IndexOf(macIndex, mac);
            int secondIndex = Array.IndexOf(charIndex, charuuid);

            switch (firstIndex)
            {
                case 0:
                    switch (secondIndex)
                    {
                        case 0:
                            return char1_1;
                        case 1:
                            return char1_2;
                        case 2:
                            return char1_3;
                    }
                    break;
                case 1:
                    switch (secondIndex)
                    {
                        case 0:
                            return char2_1;
                        case 1:
                            return char2_2;
                        case 2:
                            return char2_3;
                    }
                    break;
                case 2:
                    switch (secondIndex)
                    {
                        case 0:
                            return char3_1;
                        case 1:
                            return char3_2;
                        case 2:
                            return char3_3;
                    }
                    break;
                case 3:
                    switch (secondIndex)
                    {
                        case 0:
                            return char4_1;
                        case 1:
                            return char4_2;
                        case 2:
                            return char4_3;
                    }
                    break;
                case 4:
                    switch (secondIndex)
                    {
                        case 0:
                            return char5_1;
                        case 1:
                            return char5_2;
                        case 2:
                            return char5_3;
                    }
                    break;
            }

            return new byte[] { 0x00 };
        }
    }
}
