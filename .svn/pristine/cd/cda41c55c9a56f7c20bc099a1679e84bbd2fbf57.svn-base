using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWeb.Data
{
    public static class LogManager
    {
        public static List<LogData> LogList { get; set; } = new List<LogData>();

        public static void WriteLog(string message, bool isServer)
        {
            if (isServer)
            {

            }
            else
            {
                LogList.Add(new LogData() { Time = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"),
                                               Server = "Server", Source = "Source", Message = message
                });
            }
        }

        public static void Clear()
        {
            LogList.Clear();
        }
    }

    public struct LogData
    {
        public string Time { get; set; }
        public string Source { get; set; }
        public string Server { get; set; }
        public string Message { get; set; }
    }
}
