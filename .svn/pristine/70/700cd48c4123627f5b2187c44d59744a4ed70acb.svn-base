using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IronServer.Service
{
    public partial class Service1 : ServiceBase
    {
        public Process myProcess { get; }
        string x86Path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        string x64Path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        string programName = ConfigurationManager.AppSettings["programName"];
        string currentPath = ConfigurationManager.AppSettings["IronServerPath"];
        public Service1()
        {
            InitializeComponent();

            myProcess = new Process();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                Process[] p = Process.GetProcessesByName(programName.Replace(".exe", ""));
                if (!(p.Length > 0))
                {
                    myProcess.StartInfo.FileName = programName;
                    myProcess.StartInfo.WorkingDirectory = currentPath;
                    myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    myProcess.Start();
                    myProcess.WaitForExit(3000);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Exception : " + ex.Message);
            }
        }

        protected override void OnStop()
        {
            try
            {
                Process[] p = Process.GetProcessesByName(programName.Replace(".exe", ""));
                if (p.Length > 0)
                {
                    myProcess.Kill();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Exception : " + ex.Message);
            }
        }
    }
}
