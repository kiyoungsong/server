using DevExpress.Utils;
using Configurator.Controls;
using Configurator.ViewModels;
using DevExpress.XtraBars.Navigation;
using DevExpress.XtraEditors;

namespace Configurator
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.repositoryItemToggleSwitch4 = new DevExpress.XtraEditors.Repository.RepositoryItemToggleSwitch();
            this.repositoryItemRadioGroup3 = new DevExpress.XtraEditors.Repository.RepositoryItemRadioGroup();
            this.repositoryItemToggleSwitch2 = new DevExpress.XtraEditors.Repository.RepositoryItemToggleSwitch();
            this.repositoryItemButtonEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit();
            this.repositoryItemToggleSwitch1 = new DevExpress.XtraEditors.Repository.RepositoryItemToggleSwitch();
            this.repositoryItemRadioGroup1 = new DevExpress.XtraEditors.Repository.RepositoryItemRadioGroup();
            this.repositoryItemRadioGroup2 = new DevExpress.XtraEditors.Repository.RepositoryItemRadioGroup();
            this.repositoryItemTextEdit3 = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.repositoryItemTextEdit2 = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.repositoryItemTextEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.fluentDesignFormContainer = new DevExpress.XtraBars.FluentDesignSystem.FluentDesignFormContainer();
            this.accordionControl = new DevExpress.XtraBars.Navigation.AccordionControl();
            this.elementDevice = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.elementConnectivity = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.elementTrend = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.elementSettings = new DevExpress.XtraBars.Navigation.AccordionControlElement();
            this.fluentDesignFormControl1 = new DevExpress.XtraBars.FluentDesignSystem.FluentDesignFormControl();
            this.barSubItem1 = new DevExpress.XtraBars.BarSubItem();
            this.ConnectServer = new DevExpress.XtraBars.BarToggleSwitchItem();
            this.SelectedRemote = new DevExpress.XtraBars.BarSubItem();
            this.ConnectIP = new DevExpress.XtraBars.BarEditItem();
            this.ConnectPort = new DevExpress.XtraBars.BarEditItem();
            this.ConnectLocal = new DevExpress.XtraBars.BarEditItem();
            this.ConnectRemote = new DevExpress.XtraBars.BarEditItem();
            this.barEditItem3 = new DevExpress.XtraBars.BarEditItem();
            this.barToggleSwitchItem2 = new DevExpress.XtraBars.BarToggleSwitchItem();
            this.skinDropDownButtonItem1 = new DevExpress.XtraBars.SkinDropDownButtonItem();
            this.barCheckItem1 = new DevExpress.XtraBars.BarCheckItem();
            this.barEditItem1 = new DevExpress.XtraBars.BarEditItem();
            this.barEditItem4 = new DevExpress.XtraBars.BarEditItem();
            this.barToolbarsListItem1 = new DevExpress.XtraBars.BarToolbarsListItem();
            this.barButtonItem1 = new DevExpress.XtraBars.BarButtonItem();
            this.barCheckItem2 = new DevExpress.XtraBars.BarCheckItem();
            this.barButtonItem2 = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonItem3 = new DevExpress.XtraBars.BarButtonItem();
            this.barToggleSwitchItem1 = new DevExpress.XtraBars.BarToggleSwitchItem();
            this.barToolbarsListItem2 = new DevExpress.XtraBars.BarToolbarsListItem();
            this.barButtonItem4 = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonItem5 = new DevExpress.XtraBars.BarButtonItem();
            this.mvvmContext = new DevExpress.Utils.MVVM.MVVMContext(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemToggleSwitch4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemRadioGroup3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemToggleSwitch2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemButtonEdit1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemToggleSwitch1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemRadioGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemRadioGroup2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.accordionControl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fluentDesignFormControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mvvmContext)).BeginInit();
            this.SuspendLayout();
            // 
            // repositoryItemToggleSwitch4
            // 
            this.repositoryItemToggleSwitch4.AutoHeight = false;
            this.repositoryItemToggleSwitch4.Name = "repositoryItemToggleSwitch4";
            this.repositoryItemToggleSwitch4.OffText = "Local";
            this.repositoryItemToggleSwitch4.OnText = "Remote";
            // 
            // repositoryItemRadioGroup3
            // 
            this.repositoryItemRadioGroup3.Name = "repositoryItemRadioGroup3";
            // 
            // repositoryItemToggleSwitch2
            // 
            this.repositoryItemToggleSwitch2.AutoHeight = false;
            this.repositoryItemToggleSwitch2.Name = "repositoryItemToggleSwitch2";
            this.repositoryItemToggleSwitch2.OffText = "Off";
            this.repositoryItemToggleSwitch2.OnText = "On";
            // 
            // repositoryItemButtonEdit1
            // 
            this.repositoryItemButtonEdit1.AutoHeight = false;
            this.repositoryItemButtonEdit1.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.repositoryItemButtonEdit1.Name = "repositoryItemButtonEdit1";
            // 
            // repositoryItemToggleSwitch1
            // 
            this.repositoryItemToggleSwitch1.AutoHeight = false;
            this.repositoryItemToggleSwitch1.Name = "repositoryItemToggleSwitch1";
            this.repositoryItemToggleSwitch1.OffText = "Disconnect";
            this.repositoryItemToggleSwitch1.OnText = "Connect";
            // 
            // repositoryItemRadioGroup1
            // 
            this.repositoryItemRadioGroup1.Name = "repositoryItemRadioGroup1";
            // 
            // repositoryItemRadioGroup2
            // 
            this.repositoryItemRadioGroup2.Name = "repositoryItemRadioGroup2";
            // 
            // repositoryItemTextEdit3
            // 
            this.repositoryItemTextEdit3.AutoHeight = false;
            this.repositoryItemTextEdit3.Name = "repositoryItemTextEdit3";
            // 
            // repositoryItemTextEdit2
            // 
            this.repositoryItemTextEdit2.AutoHeight = false;
            this.repositoryItemTextEdit2.Name = "repositoryItemTextEdit2";
            // 
            // repositoryItemTextEdit1
            // 
            this.repositoryItemTextEdit1.AutoHeight = false;
            this.repositoryItemTextEdit1.Name = "repositoryItemTextEdit1";
            // 
            // fluentDesignFormContainer
            // 
            this.fluentDesignFormContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fluentDesignFormContainer.Location = new System.Drawing.Point(228, 31);
            this.fluentDesignFormContainer.Name = "fluentDesignFormContainer";
            this.fluentDesignFormContainer.Size = new System.Drawing.Size(1053, 811);
            this.fluentDesignFormContainer.TabIndex = 0;
            // 
            // accordionControl
            // 
            this.accordionControl.AllowItemSelection = true;
            this.accordionControl.Appearance.AccordionControl.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.accordionControl.Appearance.AccordionControl.Options.UseFont = true;
            this.accordionControl.Appearance.Group.Disabled.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.accordionControl.Appearance.Group.Disabled.Options.UseFont = true;
            this.accordionControl.Appearance.Group.Hovered.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.accordionControl.Appearance.Group.Hovered.Options.UseFont = true;
            this.accordionControl.Appearance.Group.Normal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.accordionControl.Appearance.Group.Normal.Options.UseFont = true;
            this.accordionControl.Appearance.Group.Pressed.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.accordionControl.Appearance.Group.Pressed.Options.UseFont = true;
            this.accordionControl.Appearance.Hint.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.accordionControl.Appearance.Hint.Options.UseFont = true;
            this.accordionControl.Appearance.Item.Disabled.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.accordionControl.Appearance.Item.Disabled.Options.UseFont = true;
            this.accordionControl.Appearance.Item.Hovered.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.accordionControl.Appearance.Item.Hovered.Options.UseFont = true;
            this.accordionControl.Appearance.Item.Normal.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.accordionControl.Appearance.Item.Normal.Options.UseFont = true;
            this.accordionControl.Appearance.Item.Pressed.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.accordionControl.Appearance.Item.Pressed.Options.UseFont = true;
            this.accordionControl.Dock = System.Windows.Forms.DockStyle.Left;
            this.accordionControl.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] {
            this.elementDevice,
            this.elementTrend,
            this.elementSettings});
            this.accordionControl.ExpandGroupOnHeaderClick = false;
            this.accordionControl.ExpandItemOnHeaderClick = false;
            this.accordionControl.Location = new System.Drawing.Point(0, 31);
            this.accordionControl.Margin = new System.Windows.Forms.Padding(2);
            this.accordionControl.Name = "accordionControl";
            this.accordionControl.OptionsMinimizing.AllowMinimizeMode = DevExpress.Utils.DefaultBoolean.True;
            this.accordionControl.OptionsMinimizing.NormalWidth = 341;
            this.accordionControl.RootDisplayMode = DevExpress.XtraBars.Navigation.AccordionControlRootDisplayMode.Footer;
            this.accordionControl.ScrollBarMode = DevExpress.XtraBars.Navigation.ScrollBarMode.Touch;
            this.accordionControl.ShowGroupExpandButtons = false;
            this.accordionControl.ShowItemExpandButtons = false;
            this.accordionControl.ShowToolTips = false;
            this.accordionControl.Size = new System.Drawing.Size(228, 811);
            this.accordionControl.TabIndex = 1;
            this.accordionControl.ViewType = DevExpress.XtraBars.Navigation.AccordionControlViewType.HamburgerMenu;
            this.accordionControl.CustomDrawElement += new DevExpress.XtraBars.Navigation.CustomDrawElementEventHandler(this.accordionControl_CustomDrawElement);
            // 
            // elementDevice
            // 
            this.elementDevice.Elements.AddRange(new DevExpress.XtraBars.Navigation.AccordionControlElement[] {
            this.elementConnectivity});
            this.elementDevice.Expanded = true;
            this.elementDevice.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("elementDevice.ImageOptions.SvgImage")));
            this.elementDevice.ImageOptions.SvgImageSize = new System.Drawing.Size(24, 24);
            this.elementDevice.Name = "elementDevice";
            this.elementDevice.Tag = 3;
            this.elementDevice.Text = "Element1";
            // 
            // elementConnectivity
            // 
            this.elementConnectivity.Expanded = true;
            this.elementConnectivity.Height = -1;
            this.elementConnectivity.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("elementConnectivity.ImageOptions.SvgImage")));
            this.elementConnectivity.ImageOptions.SvgImageSize = new System.Drawing.Size(24, 24);
            this.elementConnectivity.Name = "elementConnectivity";
            this.elementConnectivity.Tag = 3;
            this.elementConnectivity.Text = "Connectivity";
            // 
            // elementTrend
            // 
            this.elementTrend.Expanded = true;
            this.elementTrend.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("elementTrend.ImageOptions.SvgImage")));
            this.elementTrend.ImageOptions.SvgImageSize = new System.Drawing.Size(24, 24);
            this.elementTrend.Name = "elementTrend";
            this.elementTrend.Tag = 15;
            this.elementTrend.Text = "Trend";
            // 
            // elementSettings
            // 
            this.elementSettings.ControlFooterAlignment = DevExpress.XtraBars.Navigation.AccordionItemFooterAlignment.Far;
            this.elementSettings.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("elementSettings.ImageOptions.SvgImage")));
            this.elementSettings.ImageOptions.SvgImageSize = new System.Drawing.Size(24, 24);
            this.elementSettings.Name = "elementSettings";
            this.elementSettings.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
            this.elementSettings.Text = "Settings";
            // 
            // fluentDesignFormControl1
            // 
            this.fluentDesignFormControl1.FluentDesignForm = this;
            this.fluentDesignFormControl1.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.barSubItem1,
            this.ConnectServer,
            this.SelectedRemote,
            this.ConnectLocal,
            this.ConnectRemote,
            this.barEditItem3,
            this.ConnectPort,
            this.ConnectIP,
            this.barToggleSwitchItem2,
            this.skinDropDownButtonItem1,
            this.barCheckItem1,
            this.barEditItem1,
            this.barEditItem4,
            this.barToolbarsListItem1,
            this.barButtonItem1,
            this.barCheckItem2,
            this.barButtonItem2,
            this.barButtonItem3,
            this.barToggleSwitchItem1,
            this.barToolbarsListItem2,
            this.barButtonItem4,
            this.barButtonItem5});
            this.fluentDesignFormControl1.Location = new System.Drawing.Point(0, 0);
            this.fluentDesignFormControl1.Name = "fluentDesignFormControl1";
            this.fluentDesignFormControl1.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemRadioGroup1,
            this.repositoryItemRadioGroup2,
            this.repositoryItemTextEdit3,
            this.repositoryItemTextEdit2,
            this.repositoryItemTextEdit1,
            this.repositoryItemButtonEdit1,
            this.repositoryItemToggleSwitch1,
            this.repositoryItemToggleSwitch2,
            this.repositoryItemToggleSwitch4,
            this.repositoryItemRadioGroup3});
            this.fluentDesignFormControl1.Size = new System.Drawing.Size(1281, 31);
            this.fluentDesignFormControl1.TabIndex = 2;
            this.fluentDesignFormControl1.TabStop = false;
            this.fluentDesignFormControl1.TitleItemLinks.Add(this.SelectedRemote, true);
            this.fluentDesignFormControl1.TitleItemLinks.Add(this.ConnectServer);
            // 
            // barSubItem1
            // 
            this.barSubItem1.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Right;
            this.barSubItem1.Caption = "barSubItem1";
            this.barSubItem1.Id = 0;
            this.barSubItem1.Name = "barSubItem1";
            // 
            // ConnectServer
            // 
            this.ConnectServer.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Right;
            this.ConnectServer.Border = DevExpress.XtraEditors.Controls.BorderStyles.Default;
            this.ConnectServer.Caption = "Connect";
            this.ConnectServer.Id = 0;
            this.ConnectServer.ItemAppearance.Normal.ForeColor = System.Drawing.Color.White;
            this.ConnectServer.ItemAppearance.Normal.Options.UseForeColor = true;
            this.ConnectServer.Name = "ConnectServer";
            this.ConnectServer.CheckedChanged += new DevExpress.XtraBars.ItemClickEventHandler(this.ConnectServer_CheckedChanged);
            // 
            // SelectedRemote
            // 
            this.SelectedRemote.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Right;
            this.SelectedRemote.Id = 1;
            this.SelectedRemote.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("SelectedRemote.ImageOptions.Image")));
            this.SelectedRemote.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("SelectedRemote.ImageOptions.LargeImage")));
            this.SelectedRemote.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(((DevExpress.XtraBars.BarLinkUserDefines)((DevExpress.XtraBars.BarLinkUserDefines.Caption | DevExpress.XtraBars.BarLinkUserDefines.PaintStyle))), this.ConnectIP, "IP", true, true, true, 0, null, DevExpress.XtraBars.BarItemPaintStyle.CaptionInMenu),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.ConnectPort, DevExpress.XtraBars.BarItemPaintStyle.CaptionInMenu)});
            this.SelectedRemote.Name = "SelectedRemote";
            this.SelectedRemote.PaintStyle = DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph;
            // 
            // ConnectIP
            // 
            this.ConnectIP.AutoFillWidth = true;
            this.ConnectIP.AutoFillWidthInMenu = DevExpress.Utils.DefaultBoolean.True;
            this.ConnectIP.AutoHideEdit = false;
            this.ConnectIP.Caption = "IP";
            this.ConnectIP.Edit = this.repositoryItemTextEdit1;
            this.ConnectIP.EditValue = "192.168.0.1";
            this.ConnectIP.EditWidth = 90;
            this.ConnectIP.Id = 6;
            this.ConnectIP.Name = "ConnectIP";
            this.ConnectIP.Size = new System.Drawing.Size(100, 50);
            // 
            // ConnectPort
            // 
            this.ConnectPort.Caption = "Port";
            this.ConnectPort.Edit = this.repositoryItemTextEdit2;
            this.ConnectPort.EditValue = ((short)(5000));
            this.ConnectPort.Id = 5;
            this.ConnectPort.Name = "ConnectPort";
            // 
            // ConnectLocal
            // 
            this.ConnectLocal.Caption = "Local";
            this.ConnectLocal.Edit = this.repositoryItemRadioGroup1;
            this.ConnectLocal.EditValue = true;
            this.ConnectLocal.Id = 2;
            this.ConnectLocal.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("ConnectLocal.ImageOptions.Image")));
            this.ConnectLocal.ItemAppearance.Normal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.ConnectLocal.ItemAppearance.Normal.Options.UseBackColor = true;
            this.ConnectLocal.Name = "ConnectLocal";
            // 
            // ConnectRemote
            // 
            this.ConnectRemote.Caption = "Remote";
            this.ConnectRemote.Edit = this.repositoryItemRadioGroup2;
            this.ConnectRemote.EditValue = false;
            this.ConnectRemote.Id = 3;
            this.ConnectRemote.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("ConnectRemote.ImageOptions.Image")));
            this.ConnectRemote.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("ConnectRemote.ImageOptions.LargeImage")));
            this.ConnectRemote.ItemAppearance.Normal.BackColor = System.Drawing.Color.Silver;
            this.ConnectRemote.ItemAppearance.Normal.Options.UseBackColor = true;
            this.ConnectRemote.ItemAppearance.Pressed.BackColor = System.Drawing.Color.Blue;
            this.ConnectRemote.ItemAppearance.Pressed.Options.UseBackColor = true;
            this.ConnectRemote.Name = "ConnectRemote";
            // 
            // barEditItem3
            // 
            this.barEditItem3.Caption = "IP";
            this.barEditItem3.Edit = this.repositoryItemTextEdit3;
            this.barEditItem3.Id = 4;
            this.barEditItem3.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("barEditItem3.ImageOptions.Image")));
            this.barEditItem3.Name = "barEditItem3";
            // 
            // barToggleSwitchItem2
            // 
            this.barToggleSwitchItem2.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Right;
            this.barToggleSwitchItem2.Caption = "barToggleSwitchItem2";
            this.barToggleSwitchItem2.Id = 0;
            this.barToggleSwitchItem2.Name = "barToggleSwitchItem2";
            // 
            // skinDropDownButtonItem1
            // 
            this.skinDropDownButtonItem1.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Right;
            this.skinDropDownButtonItem1.Id = 1;
            this.skinDropDownButtonItem1.Name = "skinDropDownButtonItem1";
            // 
            // barCheckItem1
            // 
            this.barCheckItem1.Caption = "barCheckItem1";
            this.barCheckItem1.Id = 0;
            this.barCheckItem1.Name = "barCheckItem1";
            // 
            // barEditItem1
            // 
            this.barEditItem1.Edit = this.repositoryItemToggleSwitch4;
            this.barEditItem1.Id = 0;
            this.barEditItem1.Name = "barEditItem1";
            // 
            // barEditItem4
            // 
            this.barEditItem4.Caption = "barEditItem4";
            this.barEditItem4.Edit = this.repositoryItemRadioGroup3;
            this.barEditItem4.Id = 1;
            this.barEditItem4.Name = "barEditItem4";
            // 
            // barToolbarsListItem1
            // 
            this.barToolbarsListItem1.Caption = "barToolbarsListItem1";
            this.barToolbarsListItem1.Id = 0;
            this.barToolbarsListItem1.Name = "barToolbarsListItem1";
            // 
            // barButtonItem1
            // 
            this.barButtonItem1.Caption = "barButtonItem1";
            this.barButtonItem1.Id = 1;
            this.barButtonItem1.Name = "barButtonItem1";
            // 
            // barCheckItem2
            // 
            this.barCheckItem2.Caption = "barCheckItem2";
            this.barCheckItem2.Id = 2;
            this.barCheckItem2.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("barCheckItem2.ImageOptions.Image")));
            this.barCheckItem2.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("barCheckItem2.ImageOptions.LargeImage")));
            this.barCheckItem2.Name = "barCheckItem2";
            // 
            // barButtonItem2
            // 
            this.barButtonItem2.Caption = "barButtonItem2";
            this.barButtonItem2.Id = 0;
            this.barButtonItem2.Name = "barButtonItem2";
            // 
            // barButtonItem3
            // 
            this.barButtonItem3.Caption = "barButtonItem3";
            this.barButtonItem3.Id = 1;
            this.barButtonItem3.Name = "barButtonItem3";
            // 
            // barToggleSwitchItem1
            // 
            this.barToggleSwitchItem1.Caption = "barToggleSwitchItem1";
            this.barToggleSwitchItem1.Id = 2;
            this.barToggleSwitchItem1.Name = "barToggleSwitchItem1";
            // 
            // barToolbarsListItem2
            // 
            this.barToolbarsListItem2.Caption = "barToolbarsListItem2";
            this.barToolbarsListItem2.Id = 0;
            this.barToolbarsListItem2.Name = "barToolbarsListItem2";
            // 
            // barButtonItem4
            // 
            this.barButtonItem4.Caption = "barButtonItem4";
            this.barButtonItem4.Id = 1;
            this.barButtonItem4.Name = "barButtonItem4";
            // 
            // barButtonItem5
            // 
            this.barButtonItem5.Caption = "barButtonItem5";
            this.barButtonItem5.Id = 2;
            this.barButtonItem5.Name = "barButtonItem5";
            // 
            // mvvmContext
            // 
            this.mvvmContext.ContainerControl = this;
            this.mvvmContext.ViewModelType = typeof(Configurator.ViewModels.MainViewModel);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1281, 842);
            this.ControlContainer = this.fluentDesignFormContainer;
            this.Controls.Add(this.fluentDesignFormContainer);
            this.Controls.Add(this.accordionControl);
            this.Controls.Add(this.fluentDesignFormControl1);
            this.FluentDesignFormControl = this.fluentDesignFormControl1;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MainForm";
            this.NavigationControl = this.accordionControl;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "IronServer Configurator";
            this.TransparencyKey = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(255)))));
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemToggleSwitch4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemRadioGroup3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemToggleSwitch2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemButtonEdit1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemToggleSwitch1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemRadioGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemRadioGroup2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.accordionControl)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fluentDesignFormControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mvvmContext)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private DevExpress.XtraBars.FluentDesignSystem.FluentDesignFormContainer fluentDesignFormContainer;
        private DevExpress.XtraBars.Navigation.AccordionControl accordionControl;
        private DevExpress.XtraBars.FluentDesignSystem.FluentDesignFormControl fluentDesignFormControl1;
        private DevExpress.Utils.MVVM.MVVMContext mvvmContext;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elementConnectivity;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elementDevice;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elementTrend;
        private DevExpress.XtraBars.Navigation.AccordionControlElement elementSettings;
        private DevExpress.XtraBars.BarSubItem barSubItem1;
        private DevExpress.XtraBars.BarToggleSwitchItem ConnectServer;
        private DevExpress.XtraBars.BarSubItem SelectedRemote;
        private DevExpress.XtraBars.BarEditItem ConnectLocal;
        private DevExpress.XtraBars.BarEditItem ConnectRemote;
        private DevExpress.XtraBars.BarEditItem ConnectIP;
        private DevExpress.XtraBars.BarEditItem ConnectPort;
        private DevExpress.XtraBars.BarEditItem barEditItem3;
        private DevExpress.XtraEditors.Repository.RepositoryItemRadioGroup repositoryItemRadioGroup1;
        private DevExpress.XtraEditors.Repository.RepositoryItemRadioGroup repositoryItemRadioGroup2;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit repositoryItemTextEdit3;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit repositoryItemTextEdit2;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit repositoryItemTextEdit1;
        private DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit repositoryItemButtonEdit1;
        private DevExpress.XtraEditors.Repository.RepositoryItemToggleSwitch repositoryItemToggleSwitch1;
        private DevExpress.XtraBars.BarToggleSwitchItem barToggleSwitchItem2;
        private DevExpress.XtraBars.SkinDropDownButtonItem skinDropDownButtonItem1;
        private DevExpress.XtraEditors.Repository.RepositoryItemToggleSwitch repositoryItemToggleSwitch2;
        private DevExpress.XtraBars.BarCheckItem barCheckItem1;
        private DevExpress.XtraBars.BarEditItem barEditItem1;
        private DevExpress.XtraBars.BarEditItem barEditItem4;
        private DevExpress.XtraEditors.Repository.RepositoryItemToggleSwitch repositoryItemToggleSwitch4;
        private DevExpress.XtraEditors.Repository.RepositoryItemRadioGroup repositoryItemRadioGroup3;
        private DevExpress.XtraBars.BarButtonItem barButtonItem1;
        private DevExpress.XtraBars.BarCheckItem barCheckItem2;
        private DevExpress.XtraBars.BarToolbarsListItem barToolbarsListItem1;
        private DevExpress.XtraBars.BarButtonItem barButtonItem2;
        private DevExpress.XtraBars.BarButtonItem barButtonItem3;
        private DevExpress.XtraBars.BarToggleSwitchItem barToggleSwitchItem1;
        private DevExpress.XtraBars.BarToolbarsListItem barToolbarsListItem2;
        private DevExpress.XtraBars.BarButtonItem barButtonItem4;
        private DevExpress.XtraBars.BarButtonItem barButtonItem5;
    }
}
