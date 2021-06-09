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
using Channel = Configurator.Data.Channel;
using DeviceInfo = Configurator.Data.DeviceInfo;
using DevExpress.Internal;
using System.Data.Entity;

namespace Configurator.Model
{
    class ChannelDataModel
    {
        static List<Channel> channels = null;
        static List<DeviceInfo> devices = null;
        internal static DataTable ChannelTable
        {
            get
            {
                string table = "Channel";
                DataTable _channels = CreateDataTable(table);
                return _channels;
            }
        }
        internal static DataTable DeviceTable
        {
            get
            {
                string table = "Device";
                DataTable _devices = CreateDataTable(table);
                return _devices;
            }
        }
        public static List<Channel> Channels
        {
            get
            {
                try
                {
                    if (channels == null)
                    {
                        channels = new List<Channel>();
                        DataTable tbl = ChannelTable;
                        if (tbl != null)
                        {
                            for (int i = 0; i < tbl.Rows.Count; i++)
                            {
                                Channel channel = new Channel(tbl.Rows[i]);
                                channels.Add(channel);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    XtraMessageBox.Show(e.Message, e.Source);
                    channels = new List<Channel>();
                }
                return channels;
            }
        }
        public static List<DeviceInfo> Devices
        {
            get
            {
                try
                {
                    if (devices == null)
                    {
                        devices = new List<DeviceInfo>();
                        DataTable tbl = DeviceTable;
                        if (tbl != null)
                        {
                            for (int i = 0; i < tbl.Rows.Count; i++)
                            {
                                DeviceInfo device = new DeviceInfo(tbl.Rows[i]);
                                devices.Add(device);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    XtraMessageBox.Show(e.Message, e.Source);
                    devices = new List<DeviceInfo>();
                }
                return devices;
            }
        }
        static DataTable CreateDataTable(string table)
        {
            DataSet dataSet = new DataSet();
            string dataFile = FilesHelper.FindingFileName(Application.StartupPath, "Config\\ChannelInfo.xml");
            if (dataFile != string.Empty)
            {
                FileInfo fi = new FileInfo(dataFile);
                dataSet.ReadXml(fi.FullName);
                return dataSet.Tables[table];
            }
            return null;
        }
    }
}
