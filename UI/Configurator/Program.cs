using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using DevExpress.Internal;
using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.Skins.Info;
using Configurator.Utils;
using DevExpress.XtraBars.FluentDesignSystem;
using DevExpress.XtraEditors;

namespace Configurator
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        const string AppName = "Configurator";
        [STAThread]
        static void Main()
        {
            bool exit;
            using (DevAVDataDirectoryHelper.SingleInstanceApplicationGuard(AppName, out exit))
            {
                if (exit) return;
            }
            AppDomain.CurrentDomain.AssemblyResolve += OnCurrentDomainAssemblyResolve;

            WindowsFormsSettings.ForceDirectXPaint();
            WindowsFormsSettings.SetDPIAware();
            WindowsFormsSettings.EnableFormSkins();
            RegisterSkin();
            SetSkinPalette();
            DevExpress.Utils.AppearanceObject.DefaultFont = new Font("Segoe UI", AppProvider.GetDefaultSize());

            WindowsFormsSettings.ScrollUIMode = ScrollUIMode.Touch;
            WindowsFormsSettings.CustomizationFormSnapMode = DevExpress.Utils.Controls.SnapMode.OwnerControl;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using (new StartUpProcess())
            {
                using (StartUpProcess.Status.Subscribe(new DemoStartUp()))
                {
                    Application.Run(new MainForm());
                }
            }
        }
        static void SetSkinPalette()
        {
            UserLookAndFeel.Default.SetSkinStyle("fluentmailclient");
            var skin = CommonSkins.GetSkin(WindowsFormsSettings.DefaultLookAndFeel);
            DevExpress.Utils.Svg.SvgPalette palette = skin.CustomSvgPalettes["Default"];
            skin.SvgPalettes[Skin.DefaultSkinPaletteName].SetCustomPalette(palette);
            LookAndFeelHelper.ForceDefaultLookAndFeelChanged();
        }
        static void RegisterSkin()
        {
            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            string fluentMailClientSkinDataPath = string.Format("{0}.SkinData.fluentmailclient.fluentmailclient.SkinData.", assemblyName);
            SkinBlobXmlCreator skinCreator = new SkinBlobXmlCreator("fluentmailclient", fluentMailClientSkinDataPath, typeof(Program).Assembly, null);
            SkinManager.Default.RegisterSkin(skinCreator);
            DevExpress.XtraSplashScreen.SplashScreenManager.RegisterUserSkin(skinCreator);
            FluentDesignFormCompatibleSkins.Skins.Add("fluentmailclient");
        }
        static Assembly OnCurrentDomainAssemblyResolve(object sender, ResolveEventArgs args)
        {
            string partialName = DevExpress.Utils.AssemblyHelper.GetPartialName(args.Name).ToLower();
            if (partialName == "entityframework" || partialName == "system.data.sqlite" || partialName == "system.data.sqlite.ef6")
            {
                string path = Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), "Dll", partialName + ".dll");
                return Assembly.LoadFrom(path);
            }
            return null;
        }
    }
}
