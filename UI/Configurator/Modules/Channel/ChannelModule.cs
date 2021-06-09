using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using DevExpress.Office.Utils;
using DevExpress.Utils.Drawing;
using DevExpress.Utils.Drawing.Helpers;
using DevExpress.Utils.Svg;
using Configurator.Data;
using Configurator.Properties;
using Configurator.ViewModels;
using DevExpress.XtraEditors;
using DevExpress.XtraRichEdit.API.Native;

namespace Configurator.Modules
{
    public partial class DeviceModule : BaseModule
    {
        static Regex EmailRegex = new Regex(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", RegexOptions.Compiled);
        public DeviceModule() : base(typeof(TagViewModel))
        {
            InitializeComponent();
        }
        public TagViewModel ViewModel
        {
            get { return GetViewModel<TagViewModel>(); }
        }
        public bool SaveMessageToDrafts()
        {
            return false;
        }
        public void Init()
        {
        }
        public void ShowTag(Tag tag)
        {
        }

        void tEditTo_BeforeShowPopupPanel(object sender, DevExpress.XtraEditors.TokenEditBeforeShowPopupPanelEventArgs e)
        {
        }
    }
}
