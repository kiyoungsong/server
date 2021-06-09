using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Threading;

namespace DriverBase
{
    public class Serial : SerialBase
    {
        public SerialPort hSerial = null;
        public bool bServer = false;
        bool bSwitchServer = false;

        public Serial(string addr = "127.0.0.1", ushort port = 1024, string strPort="COM1",ushort baudRate = 9600, bool log = false, string name = "", string path = ".", bool server = false, int timeout = 1, int connectTime = 1)
        {
            string[] portNames = SerialPort.GetPortNames();

            if(portNames != null)
            {
                string portList = "";

                foreach (string ptName in portNames)
                {
                    if (ptName == strPort)
                    {
                        this.strPort = strPort;
                        nBaudRate = baudRate;
                    }
                    portList += ptName + ","; 
                }

                IronUtilites.LogManager.Manager.WriteLog("Console", $"Avaliable Port : {portList}");
            }
            
            nTimeout = timeout;
            nConnectTime = connectTime * 1000;

            if (bServer)
            {
                bSwitchServer = true;
            }
        }

        public override bool Open()
        {
            try
            {
                hSerial = new SerialPort(strPort, nBaudRate, (Parity)nParity, nDataBits);

                hSerial.Open();

                if (!hSerial.IsOpen)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                IronUtilites.LogManager.Manager.WriteLog("Exception", "Serial Open Failed : " + ex.Message);
            }

            return true;
        }

        public override void Close()
        {
            if(hSerial != null)
            {
                hSerial.Close();
            }
        }

        public override bool IsOpen()
        {
            if (hSerial != null)
            {
                bOpen = hSerial.IsOpen;
                return bOpen;
            }
            else
            {
                return false;
            }
        }

        public override bool IsCommunication()
        {
            return bCommunication;
        }

        public override void FlushInput()
        {
            if (hSerial == null)
            {
                return;
            }

            try
            {
                if (hSerial.BytesToRead > 0)
                {
                    hSerial.DiscardInBuffer();
                }

                if (hSerial.BytesToWrite > 0)
                {
                    hSerial.DiscardOutBuffer();
                }
            }
            catch(Exception ex)
            {
                IronUtilites.LogManager.Manager.WriteLog("Exception", "Serial Flush Failed : " + ex.Message);
            }
        }

        public override int InWaiting()
        {
            if (hSerial == null)
            {
                return 0;
            }

            return hSerial.BytesToRead;
        }

        #region Get/Set Attribute
        public override int[] GetSupportedBaudrates()
        {
            return this.BAUDRATES;
        }

        public override int[] GetSupportedByteSizes()
        {
            return this.BYTESIZES;
        }

        public override int[] GetSupportedParities()
        {
            return this.PARITIES;
        }

        public override int[] GetSupportedStopbits()
        {
            return this.STOPBITS;
        }


        public override void SetBaudrate(ushort baudRate)
        {
            this.nBaudRate = baudRate;
        }

        public override ushort GetBaudrate()
        {
            return nBaudRate;
        }

        public override void SetPort(string strPort)
        {
            this.strPort = strPort;
        }

        public override string GetPort()
        {
            return strPort;
        }

        public override void SetTimeout(int timeout)
        {
            this.nTimeout = timeout;
        }

        public override int GetTimeout()
        {
            return nTimeout;
        }

        public override void SetReadTimeout(int nReadTimeout)
        {
            this.nReadTimeout = nReadTimeout;
        }

        public override int GetReadTimeout()
        {
            return this.nReadTimeout;
        }

        public override void SetWriteTimeout(int nWriteTimeout)
        {
            this.nWriteTimeout = nWriteTimeout;
        }

        public override int GetWriteTimeout()
        {
            return nWriteTimeout;
        }
        #endregion

        public override string Read(int nSize = 0)
        {
            if (nSize < 0)
            {
                return "";
            }

            byte[] readBuffer = new byte[nSize];
            bool readResult = Read(ref readBuffer, nSize);

            return Encoding.ASCII.GetString(readBuffer);
        }

        public override bool Read(ref byte[] byRead, int nSize)
        {
            if (hSerial == null || !hSerial.IsOpen)
            {
                return false;
            }

            if (nSize < 0)
            {
                return false;
            }

            byRead.Initialize();

            hSerial.ReadTimeout = nReadTimeout * 1000;

            int index = 0;
            DateTime startTime = DateTime.Now;
            DateTime checkTime = startTime + new TimeSpan(hSerial.ReadTimeout * 10000000);
            int nReadSize = 0;
            int offset = 0;

            while (DateTime.Now < checkTime)
            {
                try
                {
                    byte[] buff = new byte[nSize];

                    nReadSize = hSerial.Read(buff, offset, buff.Length);

                    Buffer.BlockCopy(buff, 0, byRead, index, nReadSize);

                    if (nReadSize == nSize)
                    {
                        break;
                    }
                    else
                    {
                        index = index + nReadSize;
                        nSize = nSize - nReadSize;
                        offset += buff.Length;
                    }
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine(ex.Message);

                    if (DateTime.Now > checkTime)
                    {
                        return false;
                    }

                    continue;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }

                Thread.Sleep(0);
            }

            return true;
        }

        public override bool Write(string strWrite)
        {
            byte[] msg = Encoding.ASCII.GetBytes(strWrite);
            bool writeResult = Write(ref msg);
            return writeResult;
        }

        public override bool Write(ref byte[] byWrite)
        {
            if (hSerial == null || !hSerial.IsOpen)
            {
                return false;
            }

            hSerial.WriteTimeout = nWriteTimeout * 1000;
            int offset = 0;

            try
            {
                hSerial.Write(byWrite, offset, byWrite.Length);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            return true;
        }
    }
}
