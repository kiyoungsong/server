using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bond;
using Bond.Protocols;
using Bond.IO.Unsafe;
using Grpc.Core;
using Grpc.Core.Utils;
using System.Data;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;

namespace Configurator.Data
{

    [Schema]
    public class ConnectRequest
    {
        [Id(0)]
        public byte[] FileData { get; set; }

        [Id(1)]
        public string FileName { get; set; }
        [Id(2)]
        public int Command { get; set; }
    }

    [Schema]
    public class ConnectResponse
    {
        [Id(0)]
        public byte[] FileData { get; set; }

        [Id(1)]
        public string FileName { get; set; }
        [Id(2)]
        public int Answer { get; set; }
    }

    public static class gRPC
    {
        static Grpc.Core.Channel channel = null;
        static DefaultCallInvoker invoker = null;
        static AsyncDuplexStreamingCall<ConnectRequest, ConnectResponse> call = null;
        public static bool connFlag = false;
        public static bool disConnFlag = false;
        static bool messageFlag = false;
        public static string ip = "192.168.0.1";
        public static int port= 5000;
        private static Thread thread = null;
        public static string fileName = "";
        public static DataSet dsFileData = null;
        public static DataSet dschannelInfo = null;


        public static void ConnectGRPC()
        {
            if (thread == null)
            {
                thread = new Thread(ConnectGRPCAsync);
            }

            if (thread.ThreadState == ThreadState.Unstarted)
            {
                connFlag = true;
                messageFlag = true;
                thread.Start();
            }

            if (thread.ThreadState == ThreadState.Stopped)
            {
                connFlag = true;
                messageFlag = true;
                thread = new Thread(ConnectGRPCAsync);
                thread.Start();
            }

        }
        public static void SendTagGRPC()
        {
            messageFlag = true;
        }
        public static void DisconnectGRPC()
        {
            disConnFlag = true;
            messageFlag = true;
        }
        private static byte[] ConvertDataSetToByteArray(DataSet dataSet)
        {
            byte[] binaryDataResult = null;
            using (MemoryStream memStream = new MemoryStream())
            {
                BinaryFormatter brFormatter = new BinaryFormatter();
                brFormatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Full;
                dataSet.RemotingFormat = SerializationFormat.Xml;
                brFormatter.Serialize(memStream, dataSet);
                binaryDataResult = memStream.ToArray();
            }
            return binaryDataResult;
        }
        private static DataSet DeserailizeByteArrayToDataSet(byte[] byteArrayData)
        {
            DataSet tempDataSet=null;
            // Deserializing into datatable    
            if (byteArrayData == null)
            {
                return null;
            }
            using (MemoryStream stream = new MemoryStream(byteArrayData))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                try
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    tempDataSet = (DataSet)bformatter.Deserialize(stream);
                }
                catch (Exception e )
                {
                    MessageBox.Show(e.ToString());
                }

            }
            // Adding DataTable into DataSet    
            return tempDataSet;
        }

        private static async void ConnectGRPCAsync()
        {
            channel = new Grpc.Core.Channel(ip, port, ChannelCredentials.Insecure);

            invoker = new DefaultCallInvoker(channel);
            
            bool dataPollingFlag = true ;
            bool ConnPulseFlag = true;
            int dataChangeCnt = 1;
            Task responseCompleted = null;

            DataSet ds = new DataSet();


            using (var call = invoker.AsyncDuplexStreamingCall(Descriptors.Method, null, new CallOptions { }))
            {
                if (channel.State != ChannelState.Ready)
                {
                    await channel.ShutdownAsync();
                    connFlag = false;
                    messageFlag = false;
                    return;
                }
                while (connFlag)
                {
                    if (messageFlag)
                    {
                        if (dataPollingFlag)
                        {
                            responseCompleted = call.ResponseStream.ForEachAsync(async response =>
                                                                        {

                                                                            switch (response.Answer)
                                                                            {
                                                                                case 2://Finish
                                                                                    messageFlag = false;
                                                                                    ds = DataSetToByte(response, ds);
                                                                                    ConnPulseFlag = false;
                                                                                    break;
                                                                                case 1://Remain
                                                                                    messageFlag = true;
                                                                                    ds = DataSetToByte(response, ds);
                                                                                    break;
                                                                                case 3://Disconnect
                                                                                    messageFlag = false;
                                                                                    disConnFlag = false;
                                                                                    connFlag = false;
                                                                                    ds = DataSetToByte(response, ds);
                                                                                    break;
                                                                                case 11: //Success
                                                                                    ds = DataSetToByte(response, ds);
                                                                                    dataChangeCnt++;

                                                                                    break;
                                                                                case 12: //Fail
                                                                                    break;
                                                                            }
                                                                            if (dataChangeCnt == 13)
                                                                            {
                                                                                messageFlag = false;
                                                                                dataChangeCnt = 1;
                                                                            }
                                                                            dataPollingFlag = true;

                                                                        });
                            //Application.StartupPath + "\\Config";
                            try
                            {
                                if (connFlag && ConnPulseFlag && !disConnFlag)
                                {
                                    await call.RequestStream.WriteAsync(new ConnectRequest { FileData = ConvertDataSetToByteArray(ds), FileName = fileName, Command = 1 });//cmd : Connect
                                }
                                else if(connFlag && !ConnPulseFlag &&!disConnFlag)
                                {
                                    switch (10 + dataChangeCnt)
                                    {
                                        case 11:
                                            ds = dsFileData;
                                            break;
                                        case 12:
                                            ds = dschannelInfo;
                                            fileName = "ChannelInfo.xml";
                                            break;
                                    }
                                    await call.RequestStream.WriteAsync(new ConnectRequest { FileData = ConvertDataSetToByteArray(ds), FileName = fileName, Command = 10+ dataChangeCnt }); //cmd : 11 Channel, 12 Tag

                                }
                                else if(connFlag && !ConnPulseFlag && disConnFlag)
                                {
                                    await call.RequestStream.WriteAsync(new ConnectRequest { FileData = ConvertDataSetToByteArray(ds), FileName = fileName, Command = 2 });//cmd : Disconnect

                                }
                            }
                            catch (Grpc.Core.RpcException)
                            {

                            }

                            dataPollingFlag = false;
                        }
                    }

                    Thread.Sleep(1);
                }
                await call.RequestStream.CompleteAsync();
                await responseCompleted;
                await channel.ShutdownAsync();
            }
        

        }

        private static DataSet DataSetToByte(ConnectResponse response, DataSet ds)
        {
            if (response.FileData != null)
            {
                ds = DeserailizeByteArrayToDataSet(response.FileData);
                ds.WriteXml(Application.StartupPath + "\\Config\\" + response.FileName);
            }
            else
            {
                MessageBox.Show("error");
            }

            return ds;
        }
    }


    

    public static class Descriptors
    {
        public static Method<ConnectRequest, ConnectResponse> Method =
            new Method<ConnectRequest, ConnectResponse>(
                type: MethodType.DuplexStreaming,
                serviceName: "IronService",
                name: "AdditionMethod",
                requestMarshaller: Marshallers.Create(
                    serializer: Serializer<ConnectRequest>.ToBytes,
                    deserializer: Serializer<ConnectRequest>.FromBytes),
                responseMarshaller: Marshallers.Create(
                    serializer: Serializer<ConnectResponse>.ToBytes,
                    deserializer: Serializer<ConnectResponse>.FromBytes));

    }

    public static class Serializer<T>
    {
        public static byte[] ToBytes(T obj)
        {
            var buffer = new OutputBuffer();
            var writer = new FastBinaryWriter<OutputBuffer>(buffer);
            Serialize.To(writer, obj);
            var output = new byte[buffer.Data.Count];
            Array.Copy(buffer.Data.Array, 0, output, 0, (int)buffer.Position);
            return output;
        }

        public static T FromBytes(byte[] bytes)
        {
            var buffer = new InputBuffer(bytes);
            var data = Deserialize<T>.From(new FastBinaryReader<InputBuffer>(buffer));
            return data;
        }
    }
}
