using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.Data;
using DevExpress.Skins;
using DevExpress.Utils;
using DevExpress.Utils.MVVM;
using Configurator.Data;
using Configurator.Model;
using Configurator.Utils;
using Configurator.ViewModels;
using DevExpress.XtraBars;
using DevExpress.XtraBars.ToastNotifications;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Controls;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Tile;
using DevExpress.XtraGrid.Views.Tile.ViewInfo;
using DevExpress.XtraLayout;
using Tag = Configurator.Data.Tag;
using DevExpress.XtraBars.Navigation;
using System.Collections.Generic;
using System.Data;
using DevExpress.XtraGrid;
using System.ComponentModel;

namespace Configurator.Modules
{
    public partial class TagViewer : BaseModule
    {
        public delegate void rowDele(CurrentRowEventArgs e);
        public event rowDele rowEditing =null;
        List<string> tagMessage = new List<string>();
        public TagViewer() : base(typeof(TagViewerViewModel))
        {
            InitializeComponent();
            CurrentRowEventArgs currentRowEventArgs = new CurrentRowEventArgs();

            ViewModel.TagChanged += ViewModel_TagChanged;
            ViewModel.TagDeleted += ViewModel_TagDeleted;
            TagViewTable.MainView.DataController.ListChanged += DataController_ListChanged;
            
        }

        private void DataController_ListChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
        {
            DeviceDataModel.SendTag();
        }

        private void ViewModel_TagDeleted(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
        private void ViewModel_TagChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
        public TagViewerViewModel ViewModel
        {
            get { return GetViewModel<TagViewerViewModel>(); }
        }
        public void ChangeFile(string fileName)
        {
            ViewModel.FileName = fileName;
            TagViewTable.DataSource = DeviceDataModel.Tags;
        }
        private void LoadButton_Click(object sender, EventArgs e)
        {
            xtraOpenFileDialog1.InitialDirectory = Application.StartupPath + "\\Config\\";
            if (xtraOpenFileDialog1.ShowDialog() == DialogResult.OK)
            {
                settingPath(xtraOpenFileDialog1.FileName);
                TagViewTable.DataSource = DeviceDataModel.Tags;
                DeviceDataModel.SendTag();
            }
        }

        

        private void SaveButton_Click(object sender, EventArgs e)
        {
            DeviceDataModel.Tags = (List<Tag>)TagViewTable.DataSource;
        }

        private void SaveAsButton_Click(object sender, EventArgs e)
        {
            xtraSaveFileDialog1.InitialDirectory = Application.StartupPath + "\\Config\\";
            if (xtraSaveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                settingPath(xtraSaveFileDialog1.FileName);
                DeviceDataModel.Tags = (List<Tag>)TagViewTable.DataSource;
                DeviceDataModel.SendTag();
            }
        }




        private void settingPath(string path)
        {
            DeviceDataModel.fileName = path.Split('\\')[path.Split('\\').Length - 1];
            string filePath = path.Replace("\\" + DeviceDataModel.fileName, "");
            DeviceDataModel.Path = filePath;
        }



        private void AddTag_Click(object sender, EventArgs e)
        {
            TagViewTable.DataSource = DeviceDataModel.AddTag();
            TagViewTable.RefreshDataSource();
            DeviceDataModel.SendTag();
        }

        private void DeleteTag_Click(object sender, EventArgs e)
        {
            TagViewTable.MainView.DataController.DeleteSelectedRows();
            DeviceDataModel.SendTag();
        }
        private void Items_ListChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
        {
            CurrentMsg.Items.Add(DeviceDataModel.TagMessage);
            if (CurrentMsg.Items.Count>1000)
            {
                CurrentMsg.Items.RemoveAt(0);
            }
        }

        private void btnCopyTag_Click(object sender, EventArgs e)
        {
            TagViewTable.FocusedView.CopyToClipboard();

        }

        private void btnPasteTag_Click(object sender, EventArgs e)
        {

        }
    }
}
