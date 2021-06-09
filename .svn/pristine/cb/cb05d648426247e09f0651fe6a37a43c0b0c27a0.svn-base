using Bond;
using Bond.IO.Safe;
using Bond.Protocols;
using Grpc.Core;
using System;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace UIInterface
{
    #region Delegate
    public delegate bool SetXmlEventHandler(string fileName, int command);
    #endregion

    [XmlRoot(ElementName = "UIConfig")]
    public class UIConfig
    {
        [XmlElement("RemoteIpAddress")]
        public string RemoteIpAddress { get; set; }
        [XmlElement("LocalIpAddress")]
        public string LocalIpAddress { get; set; }
        [XmlElement("Port")]
        public int Port { get; set; }

    }

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

    public static class Descriptors
    {
        public static Method<ConnectRequest, ConnectResponse> ConnectMethod =
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

    public static class UI_Definition
    {
        public const string Config = "config";
        public const string UIConfigName = "UIConfig.xml";

        public static byte[] ConvertDataSetToByteArray(DataSet dataSet)
        {
            byte[] binaryDataResult = null;
            using (MemoryStream memStream = new MemoryStream())
            {
                BinaryFormatter brFormatter = new BinaryFormatter();
                dataSet.RemotingFormat = SerializationFormat.Xml;
                brFormatter.Serialize(memStream, dataSet);
                binaryDataResult = memStream.ToArray();
            }
            return binaryDataResult;
        }
        public static DataSet DeserailizeByteArrayToDataSet(byte[] byteArrayData)
        {
            DataSet tempDataSet = new DataSet();

            if (byteArrayData == null)
            {
                return null;
            }
            using (MemoryStream stream = new MemoryStream(byteArrayData))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                tempDataSet = (DataSet)bformatter.Deserialize(stream);

            }
            return tempDataSet;
        }
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
