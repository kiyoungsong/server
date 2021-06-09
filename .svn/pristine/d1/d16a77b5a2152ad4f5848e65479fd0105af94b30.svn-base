using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IronServer.Tray
{
    public partial class IronTray : Form
    {
        bool systemShutDown = false;
        ServiceController sc;
        string programName = "IronServer.Tray.exe";
        string serverName = "IronServer";
        string serverStop = "Server Stop";
        string serverRun = "Server Running";
        bool createdNew = false;

        public IronTray()
        {
            Mutex mutex = new Mutex(true, programName, out createdNew);

            if (createdNew)
            {
                mutex.ReleaseMutex();
            }
            else
            {
                MessageBox.Show("Application is Already Executing.");
                Application.ExitThread();
                Environment.Exit(0);
                return;
            }

            InitializeComponent();

            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += Timer_Elapsed;
            timer.Interval = 1000;
            timer.Start();

            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Visible = false;

            sc = new ServiceController();
            sc.ServiceName = "IronServer.Service";

            try
            {
                if (sc.Status != ServiceControllerStatus.Running)
                {
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 5));
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{sc.ServiceName} : failed to start, {ex.Message}, {sc.Status}");
            }
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Process[] processes = Process.GetProcessesByName(serverName);

            // Stop
            if (processes.Length < 1)
            {
                this.Invoke(new Action(delegate ()
                {
                    serverRunningToolStripMenuItem.Text = serverStop;
                    notifyIcon1.Icon = Properties.Resources.AppStop;
                }));
            }
            else //running
            {
                this.Invoke(new Action(delegate ()
                {
                    serverRunningToolStripMenuItem.Text = serverRun;
                    notifyIcon1.Icon = Properties.Resources.AppRunning;
                }));
            }
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (sc.Status != ServiceControllerStatus.Running)
                {
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0,0,5));
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{sc.ServiceName} : failed to start, {ex.Message}, {sc.Status}");
            }
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sc.Status == ServiceControllerStatus.Running)
            {
                sc.Stop();
                sc.Refresh();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            systemShutDown = true;
            this.Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (systemShutDown)
            {
                e.Cancel = false;

                if (sc.Status == ServiceControllerStatus.Running)
                {
                    sc.Stop();
                    sc.Refresh();
                }
            }
            else
            {
                e.Cancel = true;
                this.Visible = false;
            }
        }
    }
}
