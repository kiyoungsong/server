using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IronWeb.Data
{
    public static class XMLInfo
    {
        public static Dictionary<int, Channel> ChannelList { get; set; } = new Dictionary<int, Channel>();
    }

    public class Channel
    {
        public string Name { get; set; }
        public Dictionary<int, Device> DriverList { get; set; }
    }

    public class Device
    {
        public string Name { get; set; }
        public string IOFileName { get; set; }
        public List<Tag> TagList { get; set; }
        public Dictionary<string, TableTag> TableTags { get; set; }
        public List<TableTag> TableTagList { get; set; }
    }
    
}
