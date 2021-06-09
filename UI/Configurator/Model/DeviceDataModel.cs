using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DevExpress.DevAV;
using DevExpress.Utils;
using DevExpress.Utils.Drawing;
using DevExpress.XtraEditors;
using Device = Configurator.Data.Device;
using Tag = Configurator.Data.Tag;
using DevExpress.Internal;
using System.Data.Entity;

namespace Configurator.Model
{
    class DeviceDataModel
    {
        public static string Path = Application.StartupPath + "\\Config";
        public static string fileName = "";
        public static string TagMessage = "";
        public static ListChangedEventHandler eventHandler = null;
        static List<Device> devices = null;
        static List<Tag> tags = null;
        static string table =null;
        static DataSet dataSet =null;
        

        internal static DataSet DeviceTable
        {
            get
            {
                table = "Tag";
                DataSet _tags = CreateDataTable(table);
                return _tags;
            }
        }
        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }
        public static List<Device> Devices
        {
            get
            {
                try
                {
                    devices = new List<Device>();
                    DataTable tbl = DeviceTable.Tables["Device"];
                    if (tbl != null)
                    {
                        for (int i = 0; i < tbl.Rows.Count; i++)
                        {
                            Device device = new Device(tbl.Rows[i]);
                            devices.Add(device);
                        }
                    }
                }
                catch (Exception e)
                {
                    XtraMessageBox.Show(e.Message, e.Source);
                    devices = new List<Device>();
                }
                return devices;
            }
        }
        public static List<Tag> Tags
        {
            set
            {
                tags = value;
                if (tags != null && dataSet != null)
                {
                    int i = 0;
                    foreach (DataRow row in dataSet.Tables[table].Rows)
                    {
                        row["id"] = tags[i].ID;
                        row["memory"] = tags[i].Memory;
                        row["addr"] = tags[i].Addr;
                        row["scanrate"] = tags[i].Scanrate;
                        row["type"] = tags[i].Type;
                        row["redis"] = tags[i].Redis;
                        row["size"] = tags[i].TagSize;
                        i++;
                    }
                    WriteDataTable(dataSet);

                }
            }
            get
            {
                try
                {
                    tags = new List<Tag>();
                    DataTable tbl = DeviceTable.Tables[table];
                    if (tbl != null)
                    {
                        for (int i = 0; i < tbl.Rows.Count; i++)
                        {
                            Tag tag = new Tag(tbl.Rows[i]);
                            tags.Add(tag);
                        }
                    }
                    TagMessage = DateTime.Now.ToString("yyyyMMdd HH:mm:ss " + fileName + " load complete");

                }
                catch (Exception e)
                {
                    XtraMessageBox.Show(e.Message, e.Source);
                    TagMessage = DateTime.Now.ToString("yyyyMMdd HH:mm:ss " + fileName + " load fail");

                    tags = new List<Tag>();
                }


                return tags;
            }
        }
        static DataSet CreateDataTable(string table)
        {
            DirectoryInfo di = new DirectoryInfo(Path);

            if (di.Exists)
            {
                if (!fileName.Contains(".xml")) fileName += ".xml";

                dataSet = new DataSet();
                dataSet.ReadXml(Path + "\\" + fileName);
                return dataSet;
            }

            return null;
        }
        static void WriteDataTable(DataSet dataSet)
        {
            DirectoryInfo di = new DirectoryInfo(Path);
            if (di.Exists)
            {
                try
                {
                    if (!fileName.Contains(".xml")) fileName += ".xml";
                    dataSet.WriteXml(di.FullName + "\\" + fileName);
                    TagMessage = DateTime.Now.ToString("yyyyMMdd HH:mm:ss "+ fileName + " save complete");

                }
                catch (Exception)
                {
                    TagMessage = DateTime.Now.ToString("yyyyMMdd HH:mm:ss " + fileName + " save fail");
                }
            }
            
        }

        public static List<Tag> AddTag()
        {
            DataRow dr = dataSet.Tables[table].NewRow();
            dr["id"] = "new";
            dr["memory"] = "";
            dr["addr"] = "";
            dr["scanrate"] = 0;
            dr["type"] = "";
            dr["redis"] = true;
            dr["size"] = 0;
            tags.Add(new Tag(dr));
            return tags;
        }

        public static void SendTag()
        {
            //if (Configurator.Data.gRPC.connFlag)
            //{
            //    DataSet ds = new DataSet();
            //    Configurator.Data.gRPC.dsFileData = dataSet;
            //    Configurator.Data.gRPC.fileName = fileName;
            //    ds.ReadXml(Application.StartupPath + "\\Config\\ChannelInfo.xml");
            //    Configurator.Data.gRPC.dschannelInfo = ds;
            //    Configurator.Data.gRPC.SendTagGRPC(); 
            //}
        }
    }
}
