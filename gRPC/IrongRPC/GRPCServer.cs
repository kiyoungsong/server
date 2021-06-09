using Grpc.Core;
using Grpc.Core.Utils;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UIInterface;

namespace IrongRPC
{
    public class GRPCServer
    {
        #region Field
        static int remoteCk = 0; // 0 : local , 1 : remote
        static bool bRemotestop = false;
        static bool bLocalstop = false;
        static Server remoteServer;
        static Server localServer;
        Thread thread = null;

        static int size = 0;
        static int index = 0;
        static string path = System.IO.Directory.GetCurrentDirectory();
        
        static DirectoryInfo di = new DirectoryInfo(path + Path.DirectorySeparatorChar + UI_Definition.Config);
        static FileInfo[] fileInfos;

        public static bool BRemotestop { get => bRemotestop; set => bRemotestop = value; }
        public static bool BLocalstop { get => bLocalstop; set => bLocalstop = value; }
        #endregion
        #region EventField
        public static event SetXmlEventHandler SetXmlData;
        #endregion
        public void StartModule(string ipAddress, int port, int remoteck)
        {
            thread = new Thread(async () => await RunAsync(ipAddress, port, remoteck));
            thread.Start();
        }

        private static async Task RunAsync(string ipAddress, int port, int remoteck)
        {
            remoteCk = remoteck;

            Server server = new Grpc.Core.Server
            {
                Ports = { { ipAddress, port, ServerCredentials.Insecure } },
                Services =
                {
                ServerServiceDefinition.CreateBuilder()
                    .AddMethod(Descriptors.ConnectMethod, async (requestStream, responseStream, context) =>
                    {
                        await requestStream.ForEachAsync(async connectRequest =>
                        {
                            switch(connectRequest.Command)
                            {
                                case 1:
                                    await responseStream.WriteAsync(GetXML());
                                    break;
                                case 11:
                                    await responseStream.WriteAsync(SetXML(connectRequest.FileName, connectRequest.FileData, connectRequest.Command));
                                    break;
                                case 12:
                                    await responseStream.WriteAsync(SetXML(connectRequest.FileName, connectRequest.FileData, connectRequest.Command));
                                    break;
	                        }
                        });
                    })
                    .Build()
                }
            };

            server.Start();
            
            if (remoteck == 0)
            {
                remoteServer = server;

                while (bLocalstop)
                {
                }
            }
            else if (remoteck == 1)
            {
                localServer = server;

                while (bRemotestop)
                {
                }
            }
        }

        private static ConnectResponse SetXML(string fileName, byte[] fileData, int command)
        {
            ConnectResponse cr = new ConnectResponse();
            cr.FileData = fileData;
            cr.FileName = fileName;

            try
            {
                DataSet ds = UI_Definition.DeserailizeByteArrayToDataSet(fileData);
                ds.WriteXml(path + Path.DirectorySeparatorChar + fileName);
                cr.Answer = 11;
                SetXmlData(fileName, command);
                return cr;
            }
            catch (Exception ex)
            {
                IronUtilites.LogManager.Manager.WriteLog("Exception", "SetChannel RPCServer : " + ex.Message);
                cr.Answer = 12;
                return cr;
            }
        }

        private static ConnectResponse GetXML()
        {
            ConnectResponse cr = new ConnectResponse();

            try
            {
                if (di.Exists)
                {
                    if (index == 0)
                    {
                        fileInfos = di.GetFiles("*.xml");
                        if (fileInfos[fileInfos.Length - 1].Name == UI_Definition.UIConfigName)
                        {
                            size = fileInfos.Length - 1;
                        }
                        else
                        {
                            size = fileInfos.Length;
                        }
                    }

                    for (int i = index; i < size; i++)
                    {
                        string fileName = fileInfos[i].Name;

                        if (fileName == UI_Definition.UIConfigName)
                        {
                            continue;
                        }
                        else
                        {
                            DataSet dataSet = new DataSet();
                            dataSet.ReadXml(fileInfos[i].FullName);

                            cr.FileData = UI_Definition.ConvertDataSetToByteArray(dataSet);
                            DataSet test = (DataSet)UI_Definition.DeserailizeByteArrayToDataSet(cr.FileData);
                            if (index == size-1)
                            {
                                cr.Answer = 2;
                                index = 0;
                            }
                            else
                            {
                                cr.Answer = 1;
                            }
                            cr.FileName = fileName;
                            index++;
                            return cr;
                        }
                    }
                }
                index++;
                return cr;
            }
            catch (Exception ex)
            {
                index = 0;
                IronUtilites.LogManager.Manager.WriteLog("Exception", "RPC Connection Func : " + ex.Message);
                return null;
            }
        }

        public async void Stop(int remoteck)
        {
            if (remoteck == 0)
            {
                bLocalstop = false;
                await localServer.ShutdownAsync();
            }
            else if (remoteck == 1)
            {
                bRemotestop = false;
                await remoteServer.ShutdownAsync();
            }

            try
            {
                thread.Join();
            }
            catch (Exception ex)
            {
                thread.Abort();
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
