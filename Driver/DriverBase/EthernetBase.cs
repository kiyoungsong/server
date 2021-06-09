using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DriverBase
{
    public abstract class EthernetBase : DriverBase
    {
        protected static double defaultTimeout;
        public abstract void SetBlocking(bool v);
        public EthernetBase(bool bLog = false, string strName = "", string strPath = ".", string sAddr = "127.0.01", ushort nPort = 1024, int nTimeout = -1, int connectTime = 1)
        {
            bOpen = false;

            if (bLog)
            {
                string logFile = sAddr;
                // 수정필요
                logFile += "_" ;
                //m_pLog = LogManager.Manager;
                //m_pLog.WriteLog("Console", $"Driver name : {logFile}" + $" Folder name : {m_strPath}" + "\\Log\\Driver\\" + m_strName + "\\");
            }
        }

        public override void SetTimeout(int timeout)
		{
			if (timeout != -1.0 && timeout < 0.0)
            {
                StackTrace st = new StackTrace(new StackFrame(true));
                StackFrame sf = st.GetFrame(0);
                //m_pLog.WriteLog("Error", $"Timeout value out of range.({sf.GetFileName()}:{sf.GetFileLineNumber()})");
                return;
            }
		
			 nTimeout = timeout;

			SetBlocking(nTimeout < 0);
		}
        public override int GetTimeout()
        {
			return nTimeout;
        }
	}
}
