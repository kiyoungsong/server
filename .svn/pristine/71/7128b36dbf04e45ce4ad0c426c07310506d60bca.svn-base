using System;
using System.Collections.Generic;
using System.Data;
using System.Xml.Serialization;
using Configurator.Model;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraRichEdit;

namespace Configurator.Data
{
    public class Channel
    {
        DataRow _row;
        int id;
        string name;
        public Channel()
        {
        }
        public Channel(DataRow row)
        {
            this._row = row;
            id = (int)row["ID"];
            name = string.Format("{0}", row["Name"]);
        }
        public int Id { get { return id; } set { id = value; } }
        public string Name { get { return name; } set { name = value; } }

    }
    public class DeviceInfo
    {
        DataRow _row;
        int id, channelId;
        string name, ioFile;
        public DeviceInfo()
        {
        }
        public DeviceInfo(DataRow row)
        {
            this._row = row;
            id = (int)row["ID"];
            channelId = (int)row["ChannelID"];
            name = string.Format("{0}", row["Name"]);
            ioFile = string.Format("{0}", row["IOFile"]);
        }
        public int ID { get { return id; } set { id = value; } }
        public int ChannelID { get { return channelId; } set { channelId = value; } }
        public string Name { get { return name; } set { name = value; } }
        public string IOFile { get { return ioFile; } set { ioFile = value; } }
    }
    public class Device
    {
        DataRow _row;
        string id;
        public Device()
        {
        }
        public Device(DataRow row)
        {
            this._row = row;
            id = string.Format("{0}", row["type"]);
        }
        public string ID { get { return id; } set { id = value; } }
    }
    public class Tag
    {
        DataRow _row;
        string id;
        string memory;
        string addr;
        int scanrate;
        string type;
        bool redis;
        int tagSize;

        public Tag()
        {
        }
        public Tag(DataRow row)
        {
            this._row = row;
            id = string.Format("{0}", row["id"]);
            memory = string.Format("{0}", row["memory"]);
            addr = string.Format("{0}", row["addr"]);
            scanrate = int.Parse(row["scanrate"].ToString());
            type = string.Format("{0}", row["type"]);
            redis = bool.Parse(row["redis"].ToString());
            tagSize = int.Parse(row["size"].ToString());
        }
        
        public string ID { get { return id; } set { id = value; } }
        public string Memory { get { return memory; } set { memory = value; } }
        public string Addr { get { return addr; } set { addr = value; } }
        public int Scanrate { get { return scanrate; } set { scanrate = value; } }
        public string Type { get { return type; } set { type = value; } }
        public bool Redis { get { return redis; } set { redis = value; } }
        public int TagSize { get { return tagSize; } set { tagSize = value; } }
    }
    public class ObjectHelper
    {
        static RichEditDocumentServer rich = new RichEditDocumentServer();
        public static string GetPlainTextFromMHT(string mhtText)
        {
            rich.MhtText = mhtText;
            return rich.Text.TrimStart();
        }
        public static void GetChildDataRowHandles(GridView view, int rowHandle, List<Tag> list)
        {
            for (int i = 0; i < view.GetChildRowCount(rowHandle); i++)
            {
                int row = view.GetChildRowHandle(rowHandle, i);
                if (row >= 0)
                    list.Add(view.GetRow(row) as Tag);
                else
                    GetChildDataRowHandles(view, row, list);
            }
        }
    }
}
