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

namespace Configurator.Modules
{
    public partial class TagViewer : BaseModule
    {
        public TagViewer() : base(typeof(TagViewerViewModel))
        {
            InitializeComponent();

            gridControl1.DataSource = DeviceDataModel.Tags;

            ViewModel.TagChanged += ViewModel_TagChanged;
            ViewModel.TagDeleted += ViewModel_TagDeleted;
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
    }
}
