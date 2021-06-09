using System;
using System.Collections.Generic;
using System.Text;

namespace IronClient
{
    public class DataTree
    {
        public string nodeId;
        public string idType;
        public string value;
        public string dataType;
        public string displayName;
        public string browseName;
        public string sourceTimestamp;
        public string serverTimestamp;
        public string statusCode;
        public bool isFolder = false;
        public DataTree()
        {
            this.ChildNodes = new List<DataTree>();
        }
        public List<DataTree> ChildNodes { get; set; }
    }
}
