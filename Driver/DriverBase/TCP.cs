using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DriverBase
{
    public class TCP : EthernetBase
	{
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public static ManualResetEvent connectDone = new ManualResetEvent(false);
        public static ManualResetEvent acceptDone = new ManualResetEvent(false);

        Socket hSocket = null;
		public IPAddress ipAddress = null;
		public bool bServer = false;
		public ushort nPort = 1024;
        bool bSwitchServer = false;
        public bool isConnect = false;
        public bool isError = false;


        public TCP(string addr = "127.0.0.1", ushort port = 1024, bool log = false, string name = "", string path = ".", bool server = false, int timeout = 1, int connectTime = 1)
        {
            ipAddress = IPAddress.Parse(addr);
            nPort = port;
            nTimeout = timeout;
            nConnectTime = connectTime * 1000;

            if (bServer)
            {
                bSwitchServer = true;
            }
        }
        public override bool Open()
        {
            if (bServer)
            {
                // 새로운 쓰레드 생성 서버일경우 
                // 서버쪽은 다시 확인해야함
                ThreadPool.QueueUserWorkItem(ThreadProc, this);
            }
            else
            {
                try
                {
                    IPEndPoint remoteEP = new IPEndPoint(ipAddress, nPort);
                    this.isConnect = false;
                    hSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                    try
                    {
                        IAsyncResult asyncResult = hSocket.BeginConnect(remoteEP, null, null);

                        if (!asyncResult.AsyncWaitHandle.WaitOne(nConnectTime, false))
                    	{
        	                StackTrace st = new StackTrace(new StackFrame(true));
    	                    StackFrame sf = st.GetFrame(0);
                            //ironLog.WriteLog("Error", $"Socket Connection is failed!({sf.GetFileName()}:{sf.GetFileLineNumber()}");
    	                    return false;
                        }
                        else
                        {
                            hSocket.EndConnect(asyncResult);
                            this.isConnect = hSocket.Connected;
                        }
                    }
                    catch (Exception e)
                    {
	                    StackTrace st = new StackTrace(new StackFrame(true));
	                    StackFrame sf = st.GetFrame(0);
                        //ironLog.WriteLog("Exception", $"Sokcet Access Failed : {ex}");
                        Console.WriteLine("Unexpected exception : {0}", e.ToString());
                        return false;
                    }
					
					bOpen = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return false;
                }
            }

            bOpen = true;
            isError = false;
            return true;
        }

        public override void Close()
        {
            try
            {
                hSocket.Shutdown(SocketShutdown.Both);
                hSocket.Close();
				bOpen = false;
                isError = false;
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }
        }
		
        void ThreadProc(object obj)
        {

            try
            {
                Socket listen_sock;
                listen_sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, nPort);

                if (obj is TCP pTCP)
                {
                    
                    listen_sock.Bind(remoteEP);
                    listen_sock.Listen(1);
                    acceptDone.Reset();

                    listen_sock.BeginAccept(new AsyncCallback(AcceptCallback), pTCP);
                    acceptDone.WaitOne();

                    
                    while (bSwitchServer)
                    {
                        
                        Thread.Sleep(100);
                    }

                    hSocket.Close();
                    bSwitchServer = false;
                }
            }
            catch (ObjectDisposedException ex)
            {
                StackTrace st = new StackTrace(new StackFrame(true));
                StackFrame sf = st.GetFrame(0);
                //ironLog.WriteLog("Exception", $"listen_Socket is closed!({sf.GetFileName()}:{sf.GetFileLineNumber()}");
            }

        }
		
        public void AcceptCallback(IAsyncResult ar)
        {
            // 수락된것을 hSocket에 넣어주면 되는데 ...
            Console.WriteLine("Accept");
            bSwitchServer = true;
            hSocket = (Socket)ar.AsyncState;

            acceptDone.Set();
        }
		
        override public int InWaiting()
        {
            if (hSocket == null)
            {
                return 0;
            }

            return hSocket.Available;
        }

        override public string Read(int nSize = 0)
        {
            if(nSize < 0)
            {
                return "";
            }

            byte[] readBuffer = new byte[nSize];
            bool readResult = Read(ref readBuffer, nSize);

            return Encoding.ASCII.GetString(readBuffer);
        }

        public override bool Read(ref byte[] byRead, int nSize)
        {
            if (hSocket == null)
            {
                return false;
            }

            if (nSize < 0)
            {
                return false;
            }

            byRead.Initialize();

            if (!hSocket.Connected)
            {
                SetBlocking(false);
            }
            else
            {
                SetBlocking(true);
            }

            hSocket.ReceiveTimeout = nTimeout * 1000;
            
            int index = 0;
            DateTime startTime = DateTime.Now;
            DateTime checkTime = startTime + new TimeSpan(nTimeout * 10000000);
            int nReadSize = 0;

            while (DateTime.Now < checkTime)
            {
                try
                {
                    byte[] buff = new byte[nSize];

                    nReadSize = hSocket.Receive(buff);

                    Buffer.BlockCopy(buff, 0, byRead, index, nReadSize);

                    if (nReadSize == nSize)
                    {
                        break;
                    }
                    else
                    {
                        index = index + nReadSize;
                        nSize = nSize - nReadSize;
                    }
                }
                catch (SocketException ex)
                {
                    Trace.WriteLine(ex.Message);
                    isError = true;

                    if (DateTime.Now > checkTime)
                    {
                        return false;
                    }

                    continue;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                    isError = true;
                    return false;
                }

                Thread.Sleep(0);
            }
            isError = false;
            return true;
        }

        public override bool Write(string write)
        {
            byte[] msg = Encoding.ASCII.GetBytes(write);
            bool writeResult = Write(ref msg);
            return writeResult;
        }

        public override bool Write(ref byte[] write)
        {
            if (hSocket == null)
            {
                return false;
            }

            int bytesSent = 0;
            hSocket.SendTimeout = nTimeout * 1000;

            try
            {
                bytesSent = hSocket.Send(write);
            }
            catch (SocketException ex)
            {
                Trace.WriteLine(ex.Message);
                isError = true;
                return false;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                isError = true;
                return false;
            }

            //WriteLog(strWrite, "SEND");
            //Console.WriteLine("Writereturn  : " + true);
            isError = false;
            return true;
        }

        public bool InternalSelect(bool bWrite)
        {
            return false;
        }

        public override void SetBlocking(bool block)
        {
            hSocket.Blocking = block;
        }

        public override void FlushInput()
        {
            if (hSocket == null)
            {
                return;
            }

            if (hSocket.Available > 0)
            {
                byte[] outval = new byte[hSocket.Available];

                try
                {
                    int len = hSocket.Receive(outval);
                }
                catch
                {
                    return;
                }
            }
        }

        public override bool IsOpen()
        {
            bOpen = hSocket.Connected;
            return bOpen;
        }

        public override bool IsCommunication()
        {
            return bCommunication;
        }
    }
}
