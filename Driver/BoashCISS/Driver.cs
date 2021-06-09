using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BoschCISS
{
    [XmlRoot(ElementName = "Driver")]
    public class Driver
    {
        [XmlAttribute("id")]
        public string id { get; set; }

        [XmlAttribute("adapter")]
        public string adapter { get; set; }


        private List<Device> device = new List<Device>();
        [XmlElement("Device")]
        public List<Device> Device
        {
            get { return device; }
            set { device = value; }
        }
    }

    public class Device
    {
        [XmlAttribute("id")]
        public string id { get; set; }

        [XmlAttribute("mac")]
        public string mac { get; set; }

        public List<Tag> tags { get; set; }
    }

    public class Tag
    {
        public string tag { get; set; }
        public string addr { get; set; }
        public string type { get; set; }
        public uint scantime { get; set; }
        public bool redis { get; set; }
        public int size { get; set; }
        public string memory { get; set; }
        public string charuuid { get; set; }
        public string serviceuuid { get; set; }
        public string mac { get; set; }
    }
}
