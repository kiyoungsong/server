using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DriverBase
{
    public abstract class DriverBase
    {
		IronLog ironLog;
        protected int nTimeout = -1;
        protected bool bLog = false;
        protected bool bOpen = false;
        protected bool bCommunication = true;
        protected string strName = "";
        protected string strPath = "";
        protected int nConnectTime = 1;

        public DriverBase(bool bLog = false, string strName = "", string strPath = ".")
        {
            ironLog = null;
        }
		public abstract bool Open();
        public abstract void Close();
        public abstract string Read(int nSize = 0);
        public abstract bool Read(ref byte[] byRead, int nSize);
        public abstract bool Write(string strWrite);
        public abstract bool Write(ref byte[] byWrite);
        public abstract int InWaiting();        
        public abstract void FlushInput();
        public abstract void SetTimeout(int timeout);
        public abstract int GetTimeout();
        public abstract bool IsOpen();
        public abstract bool IsCommunication();
        public string ReadLine(char cEOL = '\n', int nSize = 0)
        {
            string strLine = "";

            bool preLog = bLog;

            if (bLog)
                bLog = false;

            while (true)
            {
                string c = Read(1);

                if (c.Length > 0)
                {
                    strLine += c;

                    if (c[0] == cEOL)
                    {
                        break;
                    }

                    if (nSize != 0 && strLine.Length >= nSize)
                    {
                        break;
                    }
                }
                else
                    break;
            }

            if (preLog)
            {
                bLog = true;
                //WriteLog(strLine, "RECV");
            }

            return strLine;
        }
        public List<string> ReadLines(char cEOL = '\n', int nSizeHint = 0)
        {
            List<string> lstrLines = new List<string>();

            if (nTimeout < 0)
            {
                StackTrace st = new StackTrace(new StackFrame(true));
                StackFrame sf = st.GetFrame(0);
                //m_pLog.WriteLog("Error", $"Port must have enabled timeout for this function!({sf.GetFileName()}:{sf.GetFileLineNumber()}");
                return lstrLines;
            }

            while (true)
            {
                string strLine = ReadLine(cEOL);

                if (strLine.Length > 0)
                {
                    lstrLines.Add(strLine);

                    char cEnd = strLine[strLine.Length - 1];

                    if (cEnd == cEOL)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            return lstrLines;
        }
        public List<string> xreadlines(int nSizeHint = 0)
        {
            return ReadLines();
        }
        public bool WriteLines(List<string> listWrite)
        {
            if(listWrite.Count > 0)
            {
            foreach  (string line in listWrite)
            {
                if (!Write(line))
                {
                    return false;
                }
            }

            return true;
            }

            return false;
        }
        public void SetWriteLog(bool log)
        {
            bLog = log;
        }
        public bool GetWriteLog()
        {
            return bLog;
        }
        public void WriteLog(ref string sLog, string sTag)
        {
            if (bLog && ironLog != null)
            {
                //이부분은 수정 필요할 것으로 보임
                //ironLog.WriteLog(sLog, sTag);
            }
        }
        public void WriteLog(ref byte sLog, int nSize, string sTag)
        {
            if (bLog && ironLog != null)
            {
                //이부분은 수정 필요할 것으로 보임
                //m_pLog.WriteLog(sLog, nSize, sTag);
            }
        }

    }
}
