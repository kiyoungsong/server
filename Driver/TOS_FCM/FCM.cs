using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using DriverInterface;
using IronUtilites;

namespace TOS_FCM
{
    public class FCM : IDriver
    {
        Communication comm = null;
		bool hexAddress = false;
		Driver xmlInfo = null;
        public Dictionary<string, Tag> tagInfos = new Dictionary<string, Tag>();

        public void Connect()
        {
            comm = new Communication(xmlInfo.ipServerAddr, ushort.Parse(xmlInfo.serverPort), xmlInfo.id);
			comm.ProjectPath = xmlInfo.path;

			comm.Open();
			comm.StartGathering();
        }
        public void Disconnect()
        {
			comm.StopGathering();
            comm.Close();
        }
        public void SetDriverInfo(bool bHexAddress, int nScanMode, string param)
        {
            try
            {
				hexAddress = bHexAddress;
				string strFilename = param;

                XmlDocument doc = new XmlDocument();
                doc.Load(strFilename);

                xmlInfo = new Driver();
                xmlInfo = (Driver)XMLParser.DeserializeXML(strFilename, typeof(Driver));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Read Xml File Failed Error Message : " + ex.Message);
            }
        }

        public void GetTagInfo(ref string[] strName, ref string[] strType, ref bool[] bRedis, ref int[] iSize, ref string[] strMemory)
        {
			try
			{
				List<string> nameList = new List<string>();
				List<string> typeList = new List<string>();
				List<bool> redisList = new List<bool>();
				List<int> sizeList = new List<int>();
				List<string> memoryList = new List<string>();

				foreach (var tag in xmlInfo.Tag)
				{
					try
					{
						int size = int.Parse(tag.size);

						if (size > 0)
						{
							tag.memory = tag.memory.ToLower();
							tag.type = tag.type.ToLower();

							int tagType = (int)Enum.Parse(typeof(Definition.DataType), tag.type.ToUpper());

							int addr = 0;

							if (hexAddress)
							{
								addr = Definition.HexToDec(tag.addr);
							}
							else
							{
								addr = (Convert.ToInt32(tag.addr));
							}

							tag.addr = addr.ToString();

							uint.TryParse(tag.scanrate, out uint scantime);
							bool.TryParse(tag.redis, out bool redis);

							if (tagInfos.ContainsKey(tag.id))
							{
								IronUtilites.LogManager.Manager.WriteLog("Warning", $"This Tag Name- {tag.id} alread used");
							}
							else
							{
								nameList.Add(tag.id);
								typeList.Add(tag.type);
								redisList.Add(redis);
								sizeList.Add(size);
								tagInfos[tag.id] = tag;
								memoryList.Add(tag.memory);
							}
						}
						else
						{
							IronUtilites.LogManager.Manager.WriteLog("Warning", "Tag Size must be over 0");
						}
					}
					catch (FormatException ex)
					{
						IronUtilites.LogManager.Manager.WriteLog("Exception", "Only integer type can be used for size. : " + ex.Message);
					}
				}

				strName = nameList.ToArray();
				strType = typeList.ToArray();
				bRedis = redisList.ToArray();
				iSize = sizeList.ToArray();
				strMemory = memoryList.ToArray();
			}
			catch (Exception ex)
			{
				IronUtilites.LogManager.Manager.WriteLog("Exception", "Get Driver Info Failed messaged : " + ex.Message);
				Debug.WriteLine("Get Driver Info Failed messaged : " + ex.Message);
			}
		}
		public bool CommunicationStatus()
		{
			if (comm == null || !comm.IsOpen())
			{
				return false;
			}

			return comm.IsCommunication();
		}
		public bool ReadTag(string tagName, ref object data)
        {
			byte[] readbuff = null;

			switch (tagName)
            {
				case "recipe":
					readbuff = new byte[comm.RecipeName.Length + 1];
					Buffer.BlockCopy(Encoding.ASCII.GetBytes(comm.RecipeName), 0, readbuff, 0, comm.RecipeName.Length);
					break;

				case "index":
					readbuff = new byte[4];
					Buffer.BlockCopy(BitConverter.GetBytes(comm.Index), 0, readbuff, 0, 4);
					break;

				case "valveSetting":
					readbuff = new byte[16 * 4];
					Buffer.BlockCopy(comm.ValveSetting, 0, readbuff, 0, 16 * 4);
					break;

				case "communicationLog":
					readbuff = BitConverter.GetBytes(comm.EnableCommunicationLog);
					break;

				case "filterMin":
					readbuff = BitConverter.GetBytes(comm.MinFilter);
					break;

				case "filterMax":
					readbuff = BitConverter.GetBytes(comm.MaxFilter);
					break;

				case "processing":
					readbuff = BitConverter.GetBytes(comm.Processing);
					break;
				default:
					return false;
			}

			data = readbuff;

			return true;
        }

        public bool ReadTags(string[] tagNames, ref object[] datas)
        {
			Dictionary<string, object> valueLists = new Dictionary<string, object>();

			foreach (string tagName in tagNames)
			{
				object value = new object();

				if (!ReadTag(tagName, ref value))
				{
					return false;
				}

				valueLists.Add(tagName, value);
			}

			for (int i = 0; i < tagNames.Length; i++)
			{
				if (valueLists.ContainsKey(tagNames[i]))
				{
					datas[i] = valueLists[tagNames[i]];
				}
			}

			return true;
		}

        public bool WriteTag(string tagName, object data)
        {
			if(data is byte[] dataList)
            {
				switch (tagName)
				{

					case "valveSetting":
						int[] temp = new int[16];
						Buffer.BlockCopy(dataList, 0, temp, 0, 16 * 4);
						comm.ValveSetting = temp;
						break;
					case "communicationLog":
						comm.EnableCommunicationLog = BitConverter.ToBoolean(dataList, 0);
						break;

					case "filterMin":
						comm.MinFilter = BitConverter.ToInt32(dataList, 0);
						break;

					case "filterMax":
						comm.MaxFilter = BitConverter.ToInt32(dataList,0);
						break;

					default:
						return false;
				}
			}

			return true;
		}

        public bool WriteTags(string[] tagNames, object[] datas)
        {
            for (int i = 0; i < tagNames.Length; i++)
            {
				if(!WriteTag(tagNames[i], datas[i]))
                {
					return false;
                }
            }

			return true;
        }
    }
}
