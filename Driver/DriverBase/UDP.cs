using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
namespace DriverBase
{
    public class UDP : EthernetBase
    {
        Socket socket = null;
        IPAddress ipAddress = null;
        IPAddress clientIpAddress = null;
        bool bServer = false;
        ushort nPort = 1024;
        ushort nClientPort = 1024;
        IPEndPoint serverEP = null;
        IPEndPoint clientEP = null;

        public UDP(bool log = false, string name = "", string path = ".", bool server = false, string addr = "127.0.0.1", ushort port = 1024, string clientAddr = "127.0.0.1", ushort clientPort = 1024, int timeout = 1, int connectTime = 1)
        {
            ipAddress = IPAddress.Parse(addr);
            nPort = port;

            clientIpAddress = IPAddress.Parse(clientAddr);
            nClientPort = clientPort;

            nTimeout = timeout;
            nConnectTime = connectTime;
        }
        override public bool Open()
        {
            try
            {
                serverEP = new IPEndPoint(ipAddress, nPort);
                clientEP = new IPEndPoint(clientIpAddress, nClientPort);

                socket = new Socket(ipAddress.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                
                if (bServer)
                {

                }
                else
                {
                    try
                    {
                        socket.Bind(clientEP);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Unexpected exception : {0}", e.ToString());
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }

            bOpen = true;

            return true;
        }
        override public void Close()
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }
        }
        override public int InWaiting()
        {
            if (socket == null)
            {
                return 0;
            }

            return socket.Available;
        }
        public override string Read(int nSize = 0)
        {
            if (nSize < 0)
            {
                return "";
            }

            byte[] readBuffer = new byte[nSize];

            if (Read(ref readBuffer, nSize))
            {
                return Encoding.Default.GetString(readBuffer);
            }

            return "";
        }
        public override bool Read(ref byte[] byRead, int nSize)
        {
            if (socket == null)
            {
                return false;
            }

            if (nSize < 0)
            {
                return false;
            }

            if (!socket.Connected)
            {
                SetBlocking(false);
            }
            else
            {
                SetBlocking(true);
            }

            byRead.Initialize();

            socket.ReceiveTimeout = nTimeout * 1000;

            int index = 0;
            DateTime startTime = DateTime.Now;
            DateTime checkTime = startTime + new TimeSpan(nTimeout * 10000000);

            while (DateTime.Now < checkTime)
            {
                try
                {
                    byte[] buff = new byte[nSize];

                    EndPoint remoteEP;

                    if (bServer)
                    {
                        remoteEP = (EndPoint)clientEP;
                    }
                    else
                    {
                        remoteEP = (EndPoint)serverEP;
                    }

                    int nReadSize = socket.ReceiveFrom(buff, ref remoteEP);

                    Buffer.BlockCopy(buff, 0, byRead, index, nReadSize);

                    if (nReadSize == nSize)
                    {
                        break;
                    }
                    else
                    {
                        index += nReadSize;
                        nSize -= nReadSize;
                    }
                }
                catch (SocketException ex)
                {
                    bCommunication = false;
                    continue;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                    bCommunication = false;
                    return false;
                }

                Thread.Sleep(0);
            }

            bCommunication = DateTime.Now < checkTime;
            
            //WriteLog(string(readBuff), "RECV");
            return true;
        }
        override public bool Write(string write)
        {
            byte[] msg = Encoding.ASCII.GetBytes(write);

            return Write(ref msg);
        }
        override public bool Write(ref byte[] write)
        {
            if (socket == null)
            {
                //throw new Exception("Socket is not open.");
                return false;
            }

            socket.SendTimeout = nTimeout * 1000;

            try
            {
                EndPoint remoteEP;

                if (bServer)
                {
                    remoteEP = (EndPoint)clientEP;
                }
                else
                {
                    remoteEP = (EndPoint)serverEP;
                }

                int bytesSent = socket.SendTo(write, remoteEP);
            }
            catch (SocketException ex)
            {
                Trace.WriteLine(ex.Message);
                //throw new Exception(ex.Message, ex);
                bCommunication = false;
                return false;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                Console.WriteLine(ex.Message);
                //throw new Exception(ex.Message, ex);
                bCommunication = false;
                return false;
            }
            bCommunication = true;
            //WriteLog(strWrite, "SEND");
            return true;
        }
        public bool InternalSelect(bool bWrite)
        {
            return false;
        }
        override public void SetBlocking(bool block)
        {
            socket.Blocking = block;
        }
        override public void FlushInput()
        {
            if (socket == null)
            {
                return;
            }

            if (socket.Available > 0)
            {
                byte[] outval = new byte[socket.Available];

                try
                {
                    int len = socket.Receive(outval);
                }
                catch
                {
                    return;
                }
            }
        }
        override public bool IsOpen()
        {
            return bOpen;
        }
        override public bool IsCommunication()
        {
            return bCommunication;
        }
    }
}
