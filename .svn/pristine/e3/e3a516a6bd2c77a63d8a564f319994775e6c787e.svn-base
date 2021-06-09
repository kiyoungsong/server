using IronUtilites;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IronWeb.Data
{
    public static class DriverManager
    {
        const string configFolder = "config";
        const string channelFileName = "ChannelInfo.xml";

        public static void LoadXML()
        {
            try
            {
                // 경로 체크 용
                string path = Directory.GetCurrentDirectory();

                // 경로 수정 필요
                string configPath = @"C:\Users\IronAuto\Source\Repos\IronServer\bin\config\";
                //string configPath = "..\\bin" + Path.DirectorySeparatorChar + configFolder + Path.DirectorySeparatorChar;

                XmlModel xmlInfo = (XmlModel)XMLParser.DeserializeXML(configPath + channelFileName, typeof(XmlModel));

                if (xmlInfo != null)
                {
                    foreach (var channel in xmlInfo.TagChannelnfo)
                    {
                        XMLInfo.ChannelList[channel.IId] = new Channel() { Name = channel.Name, DriverList = new Dictionary<int, Device>() };

                        foreach (var device in xmlInfo.TagDeviceInfo)
                        {
                            if (channel.IId == device.IchannelID)
                            {
                                XMLInfo.ChannelList[channel.IId].DriverList[device.IId] = new Device()
                                {
                                    Name = device.DriverName,
                                    IOFileName = device.StrIOFile,
                                    TagList = new List<Tag>()
                                };

                                Driver driver = (Driver)XMLParser.DeserializeXML(configPath + device.StrIOFile, typeof(Driver));

                                if (driver != null)
                                {
                                    //Comm 추가
                                    XMLInfo.ChannelList[channel.IId].DriverList[device.IId].TagList = driver.Tag;

                                    XMLInfo.ChannelList[channel.IId].DriverList[device.IId].TableTags = new Dictionary<string, TableTag>();

                                    int idx = 0;

                                    foreach (var tag in XMLInfo.ChannelList[channel.IId].DriverList[device.IId].TagList)
                                    {
                                        if (XMLInfo.ChannelList[channel.IId].DriverList[device.IId].TableTags.ContainsKey($"{channel.Name}.{device.DriverName}.{tag.id}"))
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            XMLInfo.ChannelList[channel.IId].DriverList[device.IId].TableTags.Add($"{channel.Name}.{device.DriverName}.{tag.id}", new TableTag()
                                            {
                                                Index = idx,
                                                id = tag.id,
                                                Value = "0",
                                                type = tag.type,
                                                ServerTime = DateTime.Now.ToString("hh:mm:ss.fffff"),
                                                SourceTime = DateTime.Now.ToString("hh:mm:ss.fffff"),
                                                IsTrend = false,
                                                FullName = $"{channel.Name}.{device.DriverName}.{tag.id}"
                                            });
                                        }

                                        idx++;
                                    }

                                    XMLInfo.ChannelList[channel.IId].DriverList[device.IId].TableTagList = new List<TableTag>(XMLInfo.ChannelList[channel.IId].DriverList[device.IId].TableTags.Values);
                                }
                                else
                                {
                                    Data.LogManager.WriteLog($"Missed A XML File Path : {configPath + device.StrIOFile}", false);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Data.LogManager.WriteLog($"Missed A XML File Path : {configPath + channelFileName}", false);
                }
            }
            catch (Exception ex)
            {
                Data.LogManager.WriteLog($"Failed Load XML File : {ex.Message}", false);
            }

        }

        public static void ResetTags()
        {
            string strPage = Data.SettingInfo.CurrentPage;

            if (strPage != string.Empty)
            {
                string[] strArray = strPage.Split('.');

                if (XMLInfo.ChannelList.Count > 0)
                {
                    foreach (var channel in XMLInfo.ChannelList)
                    {
                        if (channel.Value.Name == strArray[0])
                        {
                            foreach (var driver in channel.Value.DriverList)
                            {
                                if (driver.Value.Name == strArray[1])
                                {
                                    foreach (var tag in driver.Value.TableTagList)
                                    {
                                        tag.IsTrend = false;
                                        tag.Status = "Bad";
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
