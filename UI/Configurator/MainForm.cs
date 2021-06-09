using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.Utils;
using DevExpress.Utils.DirectXPaint;
using DevExpress.Utils.Drawing;
using DevExpress.Utils.MVVM;
using DevExpress.Utils.Taskbar;
using DevExpress.Utils.Taskbar.Core;
using Configurator.Modules;
using Configurator.Services;
using Configurator.Utils;
using Configurator.ViewModels;
using DevExpress.XtraBars.Navigation;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using Configurator.Model;
using Channel = Configurator.Data.Channel;
using DeviceInfo = Configurator.Data.DeviceInfo;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels;
using DevExpress.XtraBars;

namespace Configurator
{
    public partial class MainForm : DevExpress.XtraBars.FluentDesignSystem.FluentDesignForm, IMainModule
    {
        public MainForm()
        {
            TaskbarHelper.InitDemoJumpList(TaskbarAssistant.Default, this);
            AppProvider.MainForm = this;
            StartUpProcess.OnStart("When Only the Best Will Do");
            InitializeComponent();
            ConfigureDateNavigatorControl();
            StartUpProcess.OnRunning("Initializing...");
            mvvmContext.ViewModelConstructorParameter = this;
            this.OptionsAdaptiveLayout.AdaptiveLayout = true;
            Icon = AppProvider.AppIcon;
            
            ViewModel.ChannelChanged += ViewModel_ChannelChanged;
            ViewModel.ChannelDeleted += ViewModel_ChannelDeleted;

            ViewModel.ModuleAdded += ViewModelOnModuleAdded;
            ViewModel.ModuleRemoved += ViewModelOnModuleRemoved;
            ViewModel.CurrentTagChanged += ViewModel_CurrentTagChanged;


            BindCommands();
            InitBindings();
        }

        private void ViewModel_ChannelDeleted(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ViewModel_ChannelChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void ViewModel_CurrentTagChanged(object sender, EventArgs e)
        {
            SelectNavigationControlElementByTag(accordionControl, ViewModel.CurrentTag);
        }
        void SelectNavigationControlElementByTag(AccordionControl control, object tag)
        {
            var element = control.GetElements().FirstOrDefault(x => Equals(x.Tag, tag));
            control.SelectElement(element);
        }
        protected override void OnLookAndFeelChangedCore()
        {
            base.OnLookAndFeelChangedCore();
        }
        void ConfigureDateNavigatorControl()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            List<Channel> channels = ChannelDataModel.Channels;
            List<DeviceInfo> devices = ChannelDataModel.Devices;

            foreach (Channel channel in channels)
            {
                AccordionControlElement element = elementConnectivity.Elements.Add();

                element.Name = "accordionControlElement" + channel.Name;
                element.Style = ElementStyle.Group;
                element.Text = channel.Name;
                //element.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("accordionControlElement1.ImageOptions.SvgImage")));

                foreach (DeviceInfo device in devices)
                {
                    if (device.ChannelID == channel.Id)
                    {
                        AccordionControlElement subElement = new AccordionControlElement();

                        subElement.Name = "accordionControlElement" + channel.Name + "_" + device.Name;
                        subElement.Style = ElementStyle.Item;
                        subElement.Text = device.Name;
                        subElement.Tag = device.IOFile;

                        elementConnectivity.Elements.Add(subElement);
                    }
                }
            }
        }
        void ViewModelOnModuleRemoved(object sender, EventArgs e)
        {
            Control module = sender as Control;
            if (module != null)
            {
                module.Parent = null;
            }
            this.accordionControl.Refresh();
        }
        void ViewModelOnModuleAdded(object sender, EventArgs e)
        {
            Control module = sender as Control;
            if (module != null)
            {
                ConfigureAddedModule(module);
                module.Dock = DockStyle.Fill;
                module.Parent = fluentDesignFormContainer;
            }
        }
        void ConfigureAddedModule(Control module)
        {
            if (module is DeviceModule)
            {
                ((DeviceModule)module).Init();
                accordionControl.OptionsMinimizing.State = AccordionControlState.Minimized;
            }
        }

        #region MVVM
        public MainViewModel ViewModel
        {
            get { return mvvmContext.GetViewModel<MainViewModel>(); }
        }
        void BindCommands()
        {
            mvvmContext.BindCommand<MainViewModel>(elementConnectivity, x => x.SelectModule(ModuleType.DeviceModule), p => ModuleType.DeviceModule);
            mvvmContext.BindCommand<MainViewModel>(elementSettings, x => x.ShowPanel(PanelType.Settings), p => PanelType.Settings);

            foreach (AccordionControlElement element in elementConnectivity.Elements)
            {
                if (element.Style == ElementStyle.Item)
                {
                    mvvmContext.BindCommand<MainViewModel>(element, x => x.SelectFile(element.Tag.ToString()), p => element.Tag.ToString());
                }
            }

            mvvmContext.BindCommand<MainViewModel>(elementDevice, x => x.SelectModule(ModuleType.DeviceModule), p => ModuleType.DeviceModule);
            mvvmContext.BindCommand<MainViewModel>(elementTrend, x => x.SelectModule(ModuleType.TrendModule), p => ModuleType.TrendModule);
        }
        void InitBindings()
        {
            MVVMContextFluentAPI<MainViewModel> fluentAPI = mvvmContext.OfType<MainViewModel>();
            fluentAPI.SetBinding(accordionControl, x => x.SelectedElement, x => x.CurrentTag, GetAccordionControlElementFromTag, x => x != null ? x.Tag : -1);
        }
        AccordionControlElement GetAccordionControlElementFromTag(object tag)
        {
            var element = accordionControl.GetElements().FirstOrDefault(x => Equals(x.Tag, tag));
            if (!Equals(element, null)) return element;
            return accordionControl.GetElements().FirstOrDefault(x => Equals(x.Tag, elementDevice.Tag));
        }

        #endregion
        #region Properties

        protected override bool ExtendNavigationControlToFormTitle
        {
            get { return false; }
        }
        internal bool ExtendNavigationControlToFormTitleInternal { get { return ExtendNavigationControlToFormTitle; } }

        protected override FormShowMode ShowMode
        {
            get { return FormShowMode.AfterInitialization; }
        }

        #endregion
        #region Events

        int loading = 0;
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            loading++;
            try
            {
                ViewModel.SelectedModuleType = ModuleType.DeviceModule;
            }
            finally
            {
                StartUpProcess.OnRunning("Successfully loaded.");
                loading--;
            }
        }
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            StartUpProcess.OnComplete();
        }
        protected override void OnClosed(EventArgs e)
        {
            ViewModel.SelectedModuleType = ModuleType.Unknown;
            ViewModel.ModuleAdded -= ViewModelOnModuleAdded;
            ViewModel.ModuleRemoved -= ViewModelOnModuleRemoved;
            ViewModel.CurrentTagChanged -= ViewModel_CurrentTagChanged;
            base.OnClosed(e);
        }

        #endregion
        void ISupportModuleLayout.SaveLayoutToStream(MemoryStream ms) { }
        void ISupportModuleLayout.RestoreLayoutFromStream(MemoryStream ms) { }

        void accordionControl_CustomDrawElement(object sender, CustomDrawElementEventArgs e)
        {
            int selectionWidth = 3;
            SkinElement elem = HamburgerMenuSkins.GetSkin(UserLookAndFeel.Default)[HamburgerMenuSkins.SkinItem];
        }

        private void ConnectServer_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //if (((BarToggleSwitchItem)sender).Checked == true)
            //{
            //    Configurator.Data.gRPC.ip = ConnectIP.EditValue.ToString();
            //    Configurator.Data.gRPC.port = (int)ConnectPort.EditValue;
            //    Configurator.Data.gRPC.ConnectGRPC();
            //}
            //else
            //{
            //    Configurator.Data.gRPC.DisconnectGRPC();
            //}
        }

    }
}
