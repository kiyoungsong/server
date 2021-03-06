using System;
using System.Collections.Generic;
using System.Text;

namespace DriverBase
{
    public abstract class SerialBase : DriverBase
    {
        public readonly int[] BAUDRATES = { 50, 75, 110, 134, 150, 200, 300, 600, 1200, 1800, 2400, 4800, 9600, 19200, 38400,
                                         57600, 115200, 230400, 460800, 500000, 576000, 921600, 1000000, 1152000, 1500000,
                                         2000000, 2500000, 3000000, 3500000, 4000000 };
        public readonly int[] BYTESIZES = { 5, 6, 7, 8 };
        public readonly int[] PARITIES = { 0, 1, 2, 3, 4 };
        public readonly int[] STOPBITS = { 1, 2 };

        public string strPort;
        public ushort nBaudRate;
        public ushort nParity;
        public ushort nDataBits;
        public ushort nStopBits;
        public int nReadTimeout;
        public int nWriteTimeout;

        public SerialBase(bool bLog = false, string strName = "", string strPath = ".", string strPort = "", ushort nBaudRate = 9600, ushort nParity = 0,
                          int nBytesize = 8, ushort nDataBits = 8, ushort nStopBits = 1, int nTimeout = -1, int nXOnXOff = 0,
                          int nRtsCts = 0, int nWriteTimeout = -1, int nReadTimeout = -1,int nDsrDtr = 0)
        {
            bOpen = false;
            this.strPort = strPort;
            this.nBaudRate = nBaudRate;
            this.nParity = nParity;
            this.nDataBits = nDataBits;
            this.nStopBits = nStopBits;
            this.nReadTimeout = nReadTimeout;
            this.nWriteTimeout = nWriteTimeout;
        }

        public abstract int[] GetSupportedBaudrates();
        
        public abstract int[] GetSupportedByteSizes();

        public abstract int[] GetSupportedStopbits();

        public abstract int[] GetSupportedParities();

        public abstract void SetPort(string nPort);

        public abstract string GetPort();

        public abstract void SetBaudrate(ushort nPort);

        public abstract ushort GetBaudrate();

        public abstract void SetReadTimeout(int nReadTimeout);

        public abstract int GetReadTimeout();

        public abstract void SetWriteTimeout(int nWriteTimeout);

        public abstract int GetWriteTimeout();
    }
}
