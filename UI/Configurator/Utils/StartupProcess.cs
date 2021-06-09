using DevExpress.XtraSplashScreen;
using System;

namespace Configurator.Utils
{
    class StartUpProcess : IProcess, IDisposable
    {
        static StartUpProcess process;
        IDisposable tracker;
        public StartUpProcess()
        {
            process = this;
            tracker = StartUpProcessTracker.Instance.StartTracking(this);
        }
        void IDisposable.Dispose()
        {
            tracker.Dispose();
            process = null;
        }
        public static IObservable<string> Status
        {
            get { return StartUpProcessTracker.Instance; }
        }
        public static void OnStart(string status)
        {
            if (process != null)
                process.RaiseStart(status);
        }
        public static void OnRunning(string status)
        {
            if (process != null)
                process.RaiseRunning(status);
        }
        public static void OnComplete()
        {
            if (process != null)
                process.RaiseComplete();
        }
        #region ProcessTracker

        class StartUpProcessTracker : ProcessTracker
        {
            internal static StartUpProcessTracker Instance = new StartUpProcessTracker();
        }

        #endregion ProcessTracker
        #region IProcess Members

        ProcessStatusEventHandler startCore;
        event ProcessStatusEventHandler IProcess.Start
        {
            add { startCore += value; }
            remove { startCore -= value; }
        }
        ProcessStatusEventHandler runningCore;
        event ProcessStatusEventHandler IProcess.Running
        {
            add { runningCore += value; }
            remove { runningCore -= value; }
        }
        EventHandler completeCore;
        event EventHandler IProcess.Complete
        {
            add { completeCore += value; }
            remove { completeCore -= value; }
        }
        void RaiseStart(string status)
        {
            if (startCore != null)
                startCore(this, new ProcessStatusEventArgs(status));
        }
        void RaiseRunning(string status)
        {
            if (runningCore != null)
                runningCore(this, new ProcessStatusEventArgs(status));
        }
        void RaiseComplete()
        {
            if (completeCore != null)
                completeCore(this, EventArgs.Empty);
        }

        #endregion
    }
    class DemoStartUp : IObserver<string>
    {
        FluentSplashScreenOptions options = null;
        public DemoStartUp()
        {
            options = new FluentSplashScreenOptions()
            {
                Title = "IronServer",
                Subtitle = "v1.0.1",
                RightFooter = "Starting...",
                LeftFooter = Environment.NewLine + "All Rights reserved.",
                LoadingIndicatorType = FluentLoadingIndicatorType.Ring,
                OpacityColor = System.Drawing.Color.FromArgb(16, 110, 190),
                Opacity = 130,
            };
            options.AppearanceLeftFooter.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            //options.LogoImageOptions.SvgImage = Properties.Resources.DevExpress_Logo_Mono;
        }
        void IObserver<string>.OnCompleted()
        {
            DevExpress.XtraSplashScreen.SplashScreenManager.CloseForm(false, 300, AppProvider.MainForm);
        }
        void IObserver<string>.OnNext(string status)
        {
            if (DevExpress.XtraSplashScreen.SplashScreenManager.Default == null)
            {
                DevExpress.XtraSplashScreen.SplashScreenManager.ShowFluentSplashScreen(options, null, AppProvider.MainForm);
            }
            else
            {
                options.RightFooter = status;
                DevExpress.XtraSplashScreen.SplashScreenManager.Default.SendCommand(FluentSplashScreenCommand.UpdateOptions, options);
            }
        }
        void IObserver<string>.OnError(Exception error) { throw error; }
    }
}
