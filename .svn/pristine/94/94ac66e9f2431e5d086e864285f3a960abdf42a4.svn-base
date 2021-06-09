using System;
using DevExpress.Data.Filtering;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using Configurator.Data;
using Configurator.Model;
using Configurator.Modules;
using Configurator.Properties;
using Configurator.Utils;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraLayout.Utils;
using DevExpress.XtraRichEdit;
using DevExpress.XtraRichEdit.API.Native;
using DevExpress.XtraRichEdit.Import.Doc;

namespace Configurator.ViewModels
{
    public class TagViewerViewModel
    {
        public event EventHandler TagDeleted;
        public event EventHandler TagChanged;

        DeviceDataModel model;
        public TagViewerViewModel()
        {
            model = new DeviceDataModel();
        }
        public string FileName
        {
            set
            {
                model.FileName = value;
            }
        }
    }
}
