using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace ModbusRTU
{
    [XmlRoot(ElementName = "Driver")]
    public class Driver
    {
        [XmlAttribute("port")]
        public string port { get; set; }

        [XmlAttribute("baudrate")]
        public string baudrate  { get; set; }

        [XmlAttribute("scanmode")]
        public string scanmode { get; set; }

        [XmlAttribute("desc")]
        public string desc { get; set; }

        [XmlAttribute("connectTime")]
        public string connectTime { get; set; }

        [XmlAttribute("requestTime")]
        public string requestTime { get; set; }

        [XmlAttribute("requestCount")]
        public string requestCount { get; set; }

        private List<Tag> tag = new List<Tag>();

        [XmlElement("Tag")]
        public List<Tag> Tag
        {
            get { return tag; }
            set { tag = value; }
        }
    }

    public class Tag
    {
        [XmlAttribute("id")]
        public string id { get; set; }

        [XmlAttribute("memory")]
        public string memory { get; set; }

        [XmlAttribute("addr")]
        public string addr { get; set; }

        [XmlAttribute("scanrate")]
        public string scanrate { get; set; }

        [XmlAttribute("type")]
        public string type { get; set; }

        [XmlAttribute("redis")]
        public string redis { get; set; }

        [XmlAttribute("size")]
        public string size { get; set; }
    }
}
