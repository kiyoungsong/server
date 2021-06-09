using System;
using System.Threading.Tasks;
using System.Threading;
using NationalInstruments;
using NationalInstruments.Tdms;
using System.IO;
using DriverBase;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace TOS_FCM
{
    public class Communication : TCP
    {
        delegate void CompleteWaferDele(string recipeName);

        Thread gatheringThread = null;
        Thread saveTdmsThread = null;
        ConcurrentQueue<byte[]> storeData = new ConcurrentQueue<byte[]>();
        MemoryStream memstream;
        int memstreamOffset = 0;

        string pmIndex = "";
        Dictionary<int, int> valveSetting = new Dictionary<int, int>();
        bool stopThread = false;

        string recipeName = "Default";
        string jobID = "-";
        DateTime baseTime = DateTime.Now;
        UInt32 totalCycle = 0;

        TdmsFile file = null;
        TdmsChannelGroup digitalGroup;
        TdmsChannelGroup analogGroup;
        TdmsChannelGroup[] cycleGroup;
        TdmsChannel[] digitalChannels;
        TdmsChannel[] analogChannels;
        List<TdmsChannel[]> cycleChannels;
        PM_Config config = null;

        public Communication(string ip, ushort port, string pm) : base(ip, port)
        {
            pmIndex = pm;

            for (int i = 0; i < 16; i++)
            {
                valveSetting.Add(i, i + 16);
            }
        }

        ~Communication()
        {
            Close();
        }

        public string ProjectPath { get; set; }

        public int[] ValveSetting
        {
            get { return valveSetting.Values.ToArray();  }
            set
            {
                for (int i = 0; i < 16; i++)
                {
                    valveSetting[i] = value[i];
                }
            }
        }
        public string RecipeName
        {
            get { return recipeName;  }
            set { recipeName = value;  }
        }

        public string JobID
        {
            get { return jobID; }
            set { jobID = value; }
        }

        public bool EnableCommunicationLog { get; set; }

        public bool Processing { get; set; }

        public int MaxFilter { get; set; }

        public int MinFilter { get; set; }

        public int Index { get; set; }

        public bool StartGathering()
        {
            Processing = false;

            gatheringThread = new Thread(GatheringThread);
            saveTdmsThread = new Thread(SaveTdmsThread);
            gatheringThread.Start();
            saveTdmsThread.Start();

            return true;
        }

        public void StopGathering()
        {
            stopThread = true;
            gatheringThread.Join();
            saveTdmsThread.Join();
        }

        void GatheringThread()
        {
            while (!stopThread)
            {
                if (IsOpen())
                {
                    int buff = InWaiting();

                    if (buff > 0)
                    {
                        byte[] byRead = new byte[buff];

                        if (Read(ref byRead, buff))
                        {
                            storeData.Enqueue(byRead);
                        }
                    }
                }
                else
                {
                    Open();
                }

                Thread.Sleep(1);
            }
        }

        void SaveTdmsThread()
        {
            bool endData = false;
            memstream = new MemoryStream();
            bool change = false;

            while (!stopThread)
            {
                byte[] data = null;

                while (storeData.TryDequeue(out data))
                {
                    int dataSize = data.Length;

                    memstream.Seek(memstreamOffset, SeekOrigin.Begin);
                    memstream.Write(data, 0, dataSize);
                    memstreamOffset += dataSize;
                    change = true;
                }

                if (!change)
                {
                    Thread.Sleep(1);
                    continue;
                }

                change = false;

                memstream.Seek(0, SeekOrigin.Begin);

                StreamReader reader = new StreamReader(memstream);

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    if (line == "</DATA>")
                    {
                        Processing = false;
                        endData = true;
                    }
                    else if (line == "<DATA>")
                    {
                        Processing = true;
                        baseTime = DateTime.Now;
                    }
                }

                if (endData)
                {
                    endData = false;

                    memstream.Seek(0, SeekOrigin.Begin);

                    //Write TDMS
                    string waferData = "";
                    byte[] byteArray = new byte[memstream.Length];
                    memstream.Read(byteArray, 0, (int)memstream.Length);
                    waferData = Encoding.ASCII.GetString(byteArray);

                    string path = "D:\\Log\\" + DateTime.Now.ToString("yyyyMMdd") + "\\" + pmIndex + "\\" + recipeName + "\\";
                    string fileName = DateTime.Now.ToString("HHmmss");

                    if (EnableCommunicationLog)
                    {
                        DirectoryInfo dir = new DirectoryInfo(path);

                        if (!dir.Exists)
                        {
                            dir.Create();
                        }

                        using (FileStream f = new FileStream(path + fileName + ".log", FileMode.Create, FileAccess.Write, FileShare.Read))
                        {
                            f.Write(byteArray, 0, (int)memstream.Length);
                            f.Flush();
                        }
                    }

                    string[] splitData = waferData.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    CompleteWaferData(path, fileName, ref splitData);

                    memstream = new MemoryStream();
                    memstreamOffset = 0;
                }

                Thread.Sleep(1);
            }
        }

        private void SetUpTDMSFile(string filename)
        {
            if (File.Exists(filename))
                TdmsFile.Delete(filename);

            file = new TdmsFile(filename, new TdmsFileOptions());
            file.AutoSave = true;

            TdmsProperty propStartTime = new TdmsProperty("Start Time", TdmsPropertyDataType.DateTime, baseTime);
            file.AddProperty(propStartTime);

            TdmsChannelGroupCollection channelGroups = file.GetChannelGroups();
            
            digitalGroup = new TdmsChannelGroup("Digital Value");
            analogGroup = new TdmsChannelGroup("Analog Value");
            cycleGroup = new TdmsChannelGroup[16];

            channelGroups.Add(digitalGroup);
            channelGroups.Add(analogGroup);

            for (int i = 0; i < 16; i++)
            {
                cycleGroup[i] = new TdmsChannelGroup("Cycle Valve" + (i + 1).ToString());
                channelGroups.Add(cycleGroup[i]);
            }

            //Digital Value
            TdmsChannelCollection tdmsChannels = digitalGroup.GetChannels();
            digitalChannels = new TdmsChannel[32];
            digitalGroup.WaveformLayout = TdmsWaveformLayout.NoTimeChannel;

            for (int i = 0; i < 32; i++)
            {
                string channelName = "digital input " + i.ToString();
                digitalChannels[i] = new TdmsChannel(channelName, TdmsDataType.UInt8);

                tdmsChannels.Add(digitalChannels[i]);
            }

            //Analog Value
            tdmsChannels = analogGroup.GetChannels();
            analogChannels = new TdmsChannel[16];
            analogGroup.WaveformLayout = TdmsWaveformLayout.NoTimeChannel;

            for (int i = 0; i < 16; i++)
            {
                string channelName = "analog input " + i.ToString();
                analogChannels[i] = new TdmsChannel(channelName, TdmsDataType.Int32);

                tdmsChannels.Add(analogChannels[i]);
            }

            cycleChannels = new List<TdmsChannel[]>();

            for (int i = 0; i < 16; i++)
            {
                cycleChannels.Add(new TdmsChannel[101]);

                //Cycle
                tdmsChannels = cycleGroup[i].GetChannels();
                cycleGroup[i].WaveformLayout = TdmsWaveformLayout.NoTimeChannel;


                cycleChannels[i][0] = new TdmsChannel("Index".ToString(), TdmsDataType.Int32);
                tdmsChannels.Add(cycleChannels[i][0]);

                cycleChannels[i][1] = new TdmsChannel("T1".ToString(), TdmsDataType.Int32);
                tdmsChannels.Add(cycleChannels[i][1]);

                cycleChannels[i][2] = new TdmsChannel("T2".ToString(), TdmsDataType.Int32);
                tdmsChannels.Add(cycleChannels[i][2]);

                cycleChannels[i][3] = new TdmsChannel("T1Alarm".ToString(), TdmsDataType.Int32);
                tdmsChannels.Add(cycleChannels[i][3]);

                cycleChannels[i][4] = new TdmsChannel("T2Alarm".ToString(), TdmsDataType.Int32);
                tdmsChannels.Add(cycleChannels[i][4]);

                for (int j = 0; j < 16; j++)
                {
                    cycleChannels[i][5 + j * 6] = new TdmsChannel("A" + (j + 1).ToString() + "_Min".ToString(), TdmsDataType.Int32);
                    tdmsChannels.Add(cycleChannels[i][5 + j * 6]);

                    cycleChannels[i][6 + j * 6] = new TdmsChannel("A" + (j + 1).ToString() + "_Max".ToString(), TdmsDataType.Int32);
                    tdmsChannels.Add(cycleChannels[i][6 + j * 6]);

                    cycleChannels[i][7 + j * 6] = new TdmsChannel("A" + (j + 1).ToString() + "_Avg".ToString(), TdmsDataType.Double);
                    tdmsChannels.Add(cycleChannels[i][7 + j * 6]);

                    cycleChannels[i][8 + j * 6] = new TdmsChannel("A" + (j + 1).ToString() + "_MinAlarm".ToString(), TdmsDataType.Int32);
                    tdmsChannels.Add(cycleChannels[i][8 + j * 6]);

                    cycleChannels[i][9 + j * 6] = new TdmsChannel("A" + (j + 1).ToString() + "_MaxAlarm".ToString(), TdmsDataType.Int32);
                    tdmsChannels.Add(cycleChannels[i][9 + j * 6]);

                    cycleChannels[i][10 + j * 6] = new TdmsChannel("A" + (j + 1).ToString() + "_AvgAlarm".ToString(), TdmsDataType.Int32);
                    tdmsChannels.Add(cycleChannels[i][10 + j * 6]);
                }
            }
        }

        void CompleteWaferData(string path, string fileName, ref string[] data)
        {
            DirectoryInfo dir = new DirectoryInfo(path);

            if (!dir.Exists)
            {
                dir.Create();
            }

            FileInfo fi = new FileInfo(ProjectPath + @"\Config\PM_Config\" + recipeName + ".xml");

            fi.CopyTo(path + fileName + ".xml", true);

            config = (PM_Config)IronUtilites.Serialize.DeserializeXML(path + fileName + ".xml", typeof(PM_Config));

            SetUpTDMSFile(path + fileName + ".tdms");

            AnalogWaveform<byte>[] digitalData = new AnalogWaveform<byte>[32];
            AnalogWaveform<int>[] analogData = new AnalogWaveform<int>[16];
            AnalogWaveform<int>[] delayData = new AnalogWaveform<int>[16];

            int totalData = data.Length - 3;

            byte[,] di = new byte[32, totalData];
            int[,] ai = new int[16, totalData];
            Dictionary<int, List<int>> cycleIndex = new Dictionary<int, List<int>>();
            Dictionary<int, List<int>> t1Delay = new Dictionary<int, List<int>>();
            Dictionary<int, List<int>> t2Delay = new Dictionary<int, List<int>>();
            Dictionary<int, List<int>> t1Alarm = new Dictionary<int, List<int>>();
            Dictionary<int, List<int>> t2Alarm = new Dictionary<int, List<int>>();
            Dictionary<int, Dictionary<int, List<int>>> analogMin = new Dictionary<int, Dictionary<int, List<int>>>();
            Dictionary<int, Dictionary<int, List<int>>> analogMax = new Dictionary<int, Dictionary<int, List<int>>>();
            Dictionary<int, Dictionary<int, List<double>>> analogAvg = new Dictionary<int, Dictionary<int, List<double>>>();
            Dictionary<int, Dictionary<int, List<int>>> analogMinAlarm = new Dictionary<int, Dictionary<int, List<int>>>();
            Dictionary<int, Dictionary<int, List<int>>> analogMaxAlarm = new Dictionary<int, Dictionary<int, List<int>>>();
            Dictionary<int, Dictionary<int, List<int>>> analogAvgAlarm = new Dictionary<int, Dictionary<int, List<int>>>();
            
            if (totalData >= 100000)
            {
                di = new byte[32, 100000];
                ai = new int[16, 100000];
            }

            int index = 0;
            int mul = 0;

            int[] cycleCount = new int[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            int[] preCycleCount = new int[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            int[] cycleTime = new int[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            int[] t1Time = new int[16] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
            int[] t2Time = new int[16] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
            Dictionary<int, Dictionary<int, List<int>>> cycleAnalog = new Dictionary<int, Dictionary<int, List<int>>>();
            
            for (int i = 0; i < 16; i++)
            {
                cycleIndex.Add(i, new List<int>());
                t1Delay.Add(i, new List<int>());
                t2Delay.Add(i, new List<int>());
                t1Alarm.Add(i, new List<int>());
                t2Alarm.Add(i, new List<int>());

                analogMin.Add(i, new Dictionary<int, List<int>>());
                analogMax.Add(i, new Dictionary<int, List<int>>());
                analogAvg.Add(i, new Dictionary<int, List<double>>());
                analogMinAlarm.Add(i, new Dictionary<int, List<int>>());
                analogMaxAlarm.Add(i, new Dictionary<int, List<int>>());
                analogAvgAlarm.Add(i, new Dictionary<int, List<int>>());
                cycleAnalog.Add(i, new Dictionary<int, List<int>>());

                for (int j = 0; j < 16; j++)
                {
                    analogMin[i].Add(j, new List<int>());
                    analogMax[i].Add(j, new List<int>());
                    analogAvg[i].Add(j, new List<double>());
                    analogMinAlarm[i].Add(j, new List<int>());
                    analogMaxAlarm[i].Add(j, new List<int>());
                    analogAvgAlarm[i].Add(j, new List<int>());
                    cycleAnalog[i].Add(j, new List<int>());
                }
            }

            foreach (string d in data)
            {
                Console.WriteLine(d);

                if (d.IndexOf("<Title=") != -1)
                {
                    totalCycle = Convert.ToUInt32(d.Substring(8, d.Length - 10));

                    continue;
                }
                else if (d.IndexOf("<DATA=") != -1)
                {
                    int cycle = Convert.ToInt32(d.Substring(7, 4), 16);
                    uint digital = Convert.ToUInt32(d.Substring(11, 8), 16);

                    for (int j = 0; j < 32; j++)
                    {
                        di[j, index] = (digital & 1 << j) > 0 ? (byte)1 : (byte)0;
                    }

                    for (int j = 0; j < 16; j++)
                    {
                        ai[j, index] = Convert.ToInt32(d.Substring(19 + j * 4, 4), 16);
                    }

                    for (int j = 0; j < 16; j++)
                    {
                        if (t1Delay != null)
                        {
                            byte temp = 0;

                            temp |= di[j, index]; //Command
                            temp |= (byte)(di[valveSetting[j], index] << 1); //Status

                            switch (temp)
                            {
                                case 1:
                                    t1Time[j]++;
                                    break;

                                case 3:
                                    if (t1Time[j] >= 0 || index == 0)
                                    {
                                        if (index == 0)
                                        {
                                            t1Delay[j].Add(0);
                                            t1Alarm[j].Add(0);
                                        }
                                        else
                                        {
                                            t1Delay[j].Add(t1Time[j]);

                                            double diffMin = config.Digital_CH_list[j].SpecValue - (config.Digital_CH_list[j].SpecValue * config.Digital_CH_list[j].SpecPercent / 100);
                                            double diffMax = config.Digital_CH_list[j].SpecValue + (config.Digital_CH_list[j].SpecValue * config.Digital_CH_list[j].SpecPercent / 100);

                                            if (t1Time[j] < diffMin || t1Time[j] > diffMax)
                                            {
                                                t1Alarm[j].Add(1);
                                            }
                                            else
                                            {
                                                t1Alarm[j].Add(0);
                                            }
                                        }

                                        cycleIndex[j].Add(cycle);
                                        t1Time[j] = -1;
                                        cycleCount[j]++;
                                    }

                                    t2Time[j] = 0;
                                    break;

                                case 2:
                                    t2Time[j]++;
                                    break;

                                case 0:
                                    if (t2Time[j] >= 0)
                                    {
                                        t2Delay[j].Add(t2Time[j]);

                                        double diffMin = config.Digital_CH_list[j].SpecValue - (config.Digital_CH_list[j].SpecValue * config.Digital_CH_list[j].SpecPercent / 100);
                                        double diffMax = config.Digital_CH_list[j].SpecValue + (config.Digital_CH_list[j].SpecValue * config.Digital_CH_list[j].SpecPercent / 100);

                                        if (t2Time[j] < diffMin || t2Time[j] > diffMax)
                                        {
                                            t2Alarm[j].Add(1);
                                        }
                                        else
                                        {
                                            t2Alarm[j].Add(0);
                                        }

                                        t2Time[j] = -1;
                                    }

                                    t1Time[j] = 0;
                                    break;
                            }
                        }
                    }

                    for (int j = 0; j < 16; j++)
                    {
                        if (cycleCount[j] > 1 && cycleCount[j] != preCycleCount[j])
                        {
                            preCycleCount[j] = cycleCount[j];

                            for (int k = 0; k < 16; k++)
                            {
                                cycleAnalog[j][k].Sort();

                                if (MinFilter > 0)
                                {
                                    int minIndex = cycleAnalog[j][k].Count * MinFilter / 100;
                                    cycleAnalog[j][k].RemoveRange(0, minIndex);
                                }

                                if (MaxFilter > 0)
                                {
                                    int maxIndex = cycleAnalog[j][k].Count * MaxFilter / 100;
                                    cycleAnalog[j][k].RemoveRange(cycleAnalog[j][k].Count - maxIndex - 1, maxIndex);
                                }

                                int min = int.MaxValue;
                                int max = int.MinValue;
                                int avg = 0;

                                foreach (int a in cycleAnalog[j][k])
                                {
                                    if (a < min)
                                    {
                                        min = a;
                                    }

                                    if (a > max)
                                    {
                                        max = a;
                                    }

                                    avg += a;
                                }

                                analogMin[j][k].Add(min);
                                analogMax[j][k].Add(max);
                                analogAvg[j][k].Add(avg / (double)cycleAnalog[j][k].Count);

                                double gain = (config.Analog_CH_list[j].Display_Max - config.Analog_CH_list[j].Display_Min) / (config.Analog_CH_list[j].Raw_Max - config.Analog_CH_list[j].Raw_Min);
                                double offset = config.Analog_CH_list[j].Display_Min - config.Analog_CH_list[j].Raw_Min;

                                double diffMinMin = config.Analog_CH_list[j].MIN_Value - (config.Analog_CH_list[j].MIN_Value * config.Analog_CH_list[j].MIN_Percent / 100);
                                double diffMinMax = config.Analog_CH_list[j].MIN_Value + (config.Analog_CH_list[j].MIN_Value * config.Analog_CH_list[j].MIN_Percent / 100);

                                double value = min * gain - offset;

                                if (value < diffMinMin || value > diffMinMax)
                                {
                                    analogMinAlarm[j][k].Add(1);
                                }
                                else
                                {
                                    analogMinAlarm[j][k].Add(0);
                                }

                                double diffMaxMin = config.Analog_CH_list[j].MAX_Value - (config.Analog_CH_list[j].MAX_Value * config.Analog_CH_list[j].MAX_Percent / 100);
                                double diffMaxMax = config.Analog_CH_list[j].MAX_Value + (config.Analog_CH_list[j].MAX_Value * config.Analog_CH_list[j].MAX_Percent / 100);

                                value = max * gain - offset;

                                if (value < diffMaxMin || value > diffMaxMax)
                                {
                                    analogMaxAlarm[j][k].Add(1);
                                }
                                else
                                {
                                    analogMaxAlarm[j][k].Add(0);
                                }

                                double diffAvgMin = config.Analog_CH_list[j].AVG_Value - (config.Analog_CH_list[j].AVG_Value * config.Analog_CH_list[j].AVG_Percent / 100);
                                double diffAvgMax = config.Analog_CH_list[j].AVG_Value + (config.Analog_CH_list[j].AVG_Value * config.Analog_CH_list[j].AVG_Percent / 100);

                                value = avg / (double)cycleAnalog[j][k].Count * gain - offset;

                                if (value < diffAvgMin || value > diffAvgMax)
                                {
                                    analogAvgAlarm[j][k].Add(1);
                                }
                                else
                                {
                                    analogAvgAlarm[j][k].Add(0);
                                }
                            }

                            cycleTime[j] = 0;
                        }

                        if (cycleCount[j] > 0)
                        {
                            cycleTime[j]++;

                            for (int k = 0; k < 16; k++)
                            {
                                cycleAnalog[j][k].Add(ai[k, index]);
                            }
                        }
                    }
                }
                else
                {
                    continue;
                }

                index++;

                if (index % 100000 == 0)
                {
                    digitalData = AnalogWaveform<byte>.FromArray2D(di);
                    analogData = AnalogWaveform<int>.FromArray2D(ai);

                    try
                    {
                        digitalGroup.AppendAnalogWaveforms<byte>(digitalChannels, digitalData);
                        analogGroup.AppendAnalogWaveforms<int>(analogChannels, analogData);

                        for (int i = 0; i < 16; i++)
                        {
                            if (t1Delay[i].Count > 0)
                            {
                                cycleGroup[i].AppendAnalogWaveform<int>(cycleChannels[i][0], AnalogWaveform<int>.FromArray1D(cycleIndex[i].ToArray()));
                                cycleGroup[i].AppendAnalogWaveform<int>(cycleChannels[i][1], AnalogWaveform<int>.FromArray1D(t1Delay[i].ToArray()));
                            }

                            if (t2Delay[i].Count > 0)
                            {
                                cycleGroup[i].AppendAnalogWaveform<int>(cycleChannels[i][2], AnalogWaveform<int>.FromArray1D(t2Delay[i].ToArray()));
                            }

                            if (t1Alarm[i].Count > 0)
                            {
                                cycleGroup[i].AppendAnalogWaveform<int>(cycleChannels[i][3], AnalogWaveform<int>.FromArray1D(t1Alarm[i].ToArray()));
                            }

                            if (t2Alarm[i].Count > 0)
                            {
                                cycleGroup[i].AppendAnalogWaveform<int>(cycleChannels[i][4], AnalogWaveform<int>.FromArray1D(t2Alarm[i].ToArray()));
                            }

                            for (int j = 0; j < 16; j++)
                            {
                                if (analogMin[i][j].Count > 0)
                                {
                                    cycleGroup[i].AppendAnalogWaveform<int>(cycleChannels[i][5 + j * 6], AnalogWaveform<int>.FromArray1D(analogMin[i][j].ToArray()));
                                }

                                if (analogMax[i][j].Count > 0)
                                {
                                    cycleGroup[i].AppendAnalogWaveform<int>(cycleChannels[i][6 + j * 6], AnalogWaveform<int>.FromArray1D(analogMax[i][j].ToArray()));
                                }

                                if (analogAvg[i][j].Count > 0)
                                {
                                    cycleGroup[i].AppendAnalogWaveform<double>(cycleChannels[i][7 + j * 6], AnalogWaveform<double>.FromArray1D(analogAvg[i][j].ToArray()));
                                }

                                if (analogMinAlarm[i][j].Count > 0)
                                {
                                    cycleGroup[i].AppendAnalogWaveform<int>(cycleChannels[i][8 + j * 6], AnalogWaveform<int>.FromArray1D(analogMinAlarm[i][j].ToArray()));
                                }

                                if (analogMaxAlarm[i][j].Count > 0)
                                {
                                    cycleGroup[i].AppendAnalogWaveform<int>(cycleChannels[i][9 + j * 6], AnalogWaveform<int>.FromArray1D(analogMaxAlarm[i][j].ToArray()));
                                }

                                if (analogAvgAlarm[i][j].Count > 0)
                                {
                                    cycleGroup[i].AppendAnalogWaveform<int>(cycleChannels[i][10 + j * 6], AnalogWaveform<int>.FromArray1D(analogAvgAlarm[i][j].ToArray()));
                                }
                            }
                        }
                    }
                    catch (Exception tdmsException)
                    {
                        Console.WriteLine(tdmsException.Message);
                    }

                    if ((totalData - 100000 * ++mul) >= 100000)
                    {
                        di = new byte[32, 100000];
                        ai = new int[16, 100000];
                    }
                    else
                    {
                        di = new byte[32, totalData - 100000 * mul];
                        ai = new int[16, totalData - 100000 * mul];
                    }

                    index = 0;
                }
            }

            for (int j = 0; j < 16; j++)
            {
                if (cycleCount[j] > 1)
                {
                    preCycleCount[j] = cycleCount[j];

                    for (int k = 0; k < 16; k++)
                    {
                        cycleAnalog[j][k].Sort();

                        if (MinFilter > 0)
                        {
                            int minIndex = cycleAnalog[j][k].Count * MinFilter / 100;
                            cycleAnalog[j][k].RemoveRange(0, minIndex);
                        }

                        if (MaxFilter > 0)
                        {
                            int maxIndex = cycleAnalog[j][k].Count * MaxFilter / 100;
                            cycleAnalog[j][k].RemoveRange(cycleAnalog[j][k].Count - maxIndex - 1, maxIndex);
                        }

                        int min = int.MaxValue;
                        int max = int.MinValue;
                        int avg = 0;

                        foreach (int a in cycleAnalog[j][k])
                        {
                            if (a < min)
                            {
                                min = a;
                            }

                            if (a > max)
                            {
                                max = a;
                            }

                            avg += a;
                        }

                        analogMin[j][k].Add(min);
                        analogMax[j][k].Add(max);
                        analogAvg[j][k].Add(avg / (double)cycleAnalog[j][k].Count);

                        double gain = (config.Analog_CH_list[j].Display_Max - config.Analog_CH_list[j].Display_Min) / (config.Analog_CH_list[j].Raw_Max - config.Analog_CH_list[j].Raw_Min);
                        double offset = config.Analog_CH_list[j].Display_Min - config.Analog_CH_list[j].Raw_Min;

                        double diffMinMin = config.Analog_CH_list[j].MIN_Value - (config.Analog_CH_list[j].MIN_Value * config.Analog_CH_list[j].MIN_Percent / 100);
                        double diffMinMax = config.Analog_CH_list[j].MIN_Value + (config.Analog_CH_list[j].MIN_Value * config.Analog_CH_list[j].MIN_Percent / 100);

                        double value = min * gain - offset;

                        if (value < diffMinMin || value > diffMinMax)
                        {
                            analogMinAlarm[j][k].Add(1);
                        }
                        else
                        {
                            analogMinAlarm[j][k].Add(0);
                        }

                        double diffMaxMin = config.Analog_CH_list[j].MAX_Value - (config.Analog_CH_list[j].MAX_Value * config.Analog_CH_list[j].MAX_Percent / 100);
                        double diffMaxMax = config.Analog_CH_list[j].MAX_Value + (config.Analog_CH_list[j].MAX_Value * config.Analog_CH_list[j].MAX_Percent / 100);

                        value = max * gain - offset;

                        if (value < diffMaxMin || value > diffMaxMax)
                        {
                            analogMaxAlarm[j][k].Add(1);
                        }
                        else
                        {
                            analogMaxAlarm[j][k].Add(0);
                        }

                        double diffAvgMin = config.Analog_CH_list[j].AVG_Value - (config.Analog_CH_list[j].AVG_Value * config.Analog_CH_list[j].AVG_Percent / 100);
                        double diffAvgMax = config.Analog_CH_list[j].AVG_Value + (config.Analog_CH_list[j].AVG_Value * config.Analog_CH_list[j].AVG_Percent / 100);

                        value = avg / (double)cycleAnalog[j][k].Count * gain - offset;

                        if (value < diffAvgMin || value > diffAvgMax)
                        {
                            analogAvgAlarm[j][k].Add(1);
                        }
                        else
                        {
                            analogAvgAlarm[j][k].Add(0);
                        }
                    }

                    cycleTime[j] = 0;
                }
            }

            digitalData = AnalogWaveform<byte>.FromArray2D(di);
            analogData = AnalogWaveform<int>.FromArray2D(ai);

            try
            {
                digitalGroup.AppendAnalogWaveforms<byte>(digitalChannels, digitalData);
                analogGroup.AppendAnalogWaveforms<int>(analogChannels, analogData);

                for (int i = 0; i < 16; i++)
                {
                    if (t1Delay[i].Count > 0)
                    {
                        cycleGroup[i].AppendAnalogWaveform<int>(cycleChannels[i][0], AnalogWaveform<int>.FromArray1D(cycleIndex[i].ToArray()));
                        cycleGroup[i].AppendAnalogWaveform<int>(cycleChannels[i][1], AnalogWaveform<int>.FromArray1D(t1Delay[i].ToArray()));
                    }

                    if (t2Delay[i].Count > 0)
                    {
                        cycleGroup[i].AppendAnalogWaveform<int>(cycleChannels[i][2], AnalogWaveform<int>.FromArray1D(t2Delay[i].ToArray()));
                    }

                    if (t1Alarm[i].Count > 0)
                    {
                        cycleGroup[i].AppendAnalogWaveform<int>(cycleChannels[i][3], AnalogWaveform<int>.FromArray1D(t1Alarm[i].ToArray()));
                    }

                    if (t2Alarm[i].Count > 0)
                    {
                        cycleGroup[i].AppendAnalogWaveform<int>(cycleChannels[i][4], AnalogWaveform<int>.FromArray1D(t2Alarm[i].ToArray()));
                    }

                    for (int j = 0; j < 16; j++)
                    {
                        if (analogMin[i][j].Count > 0)
                        {
                            cycleGroup[i].AppendAnalogWaveform<int>(cycleChannels[i][5 + j * 6], AnalogWaveform<int>.FromArray1D(analogMin[i][j].ToArray()));
                        }

                        if (analogMax[i][j].Count > 0)
                        {
                            cycleGroup[i].AppendAnalogWaveform<int>(cycleChannels[i][6 + j * 6], AnalogWaveform<int>.FromArray1D(analogMax[i][j].ToArray()));
                        }

                        if (analogAvg[i][j].Count > 0)
                        {
                            cycleGroup[i].AppendAnalogWaveform<double>(cycleChannels[i][7 + j * 6], AnalogWaveform<double>.FromArray1D(analogAvg[i][j].ToArray()));
                        }

                        if (analogMinAlarm[i][j].Count > 0)
                        {
                            cycleGroup[i].AppendAnalogWaveform<int>(cycleChannels[i][8 + j * 6], AnalogWaveform<int>.FromArray1D(analogMinAlarm[i][j].ToArray()));
                        }

                        if (analogMaxAlarm[i][j].Count > 0)
                        {
                            cycleGroup[i].AppendAnalogWaveform<int>(cycleChannels[i][9 + j * 6], AnalogWaveform<int>.FromArray1D(analogMaxAlarm[i][j].ToArray()));
                        }

                        if (analogAvgAlarm[i][j].Count > 0)
                        {
                            cycleGroup[i].AppendAnalogWaveform<int>(cycleChannels[i][10 + j * 6], AnalogWaveform<int>.FromArray1D(analogAvgAlarm[i][j].ToArray()));
                        }
                    }
                }
            }
            catch (Exception tdmsException)
            {
                Console.WriteLine(tdmsException.Message);
            }

            //fire property
            TdmsProperty propCycleNum = new TdmsProperty("Cycle Count", TdmsPropertyDataType.UInt32, totalCycle);
            file.AddProperty(propCycleNum);
            TdmsProperty propJobID = new TdmsProperty("JobID", TdmsPropertyDataType.String, jobID);
            file.AddProperty(propJobID);

            file.Close();
        }
    }
}
