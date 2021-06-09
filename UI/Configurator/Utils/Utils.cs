using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using DevExpress.Data;
using DevExpress.Skins;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;

namespace Configurator.Utils
{
    public delegate void OptionValueChangedEventHandler();
    public class SortInfo
    {
        public GridColumn Column { get; set; }
        public ColumnSortOrder Order { get; set; }
    }
    public static class Utils
    {
        public static string MessageFrom = "maildemo@dx-mail.com";
        static string _startMhtText = null;
        static string DefaultTextResourceName = "StartMhtText.txt";
        public static string StartMhtText
        {
            get
            {
                if (string.IsNullOrEmpty(_startMhtText))
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    using (Stream stream = assembly.GetManifestResourceStream(Assembly.GetExecutingAssembly().GetName().Name + ".Data." + DefaultTextResourceName))
                    {
                        using (StreamReader sr = new StreamReader(stream))
                        {
                            _startMhtText = sr.ReadToEnd();
                        }
                    }
                }
                return _startMhtText;
            }
            set { _startMhtText = value; }
        }
        static bool _useAsEmailSignature = true;
        public static bool UseAsEmailSignature { get { return _useAsEmailSignature; } set { _useAsEmailSignature = value; } }
        static bool _showNotifications = true;
        public static bool ShowNotifications { get { return _showNotifications; } set { _showNotifications = value; } }
    }
    static class ColorProvider
    {
        public static Color GetControlColor(DevExpress.LookAndFeel.UserLookAndFeel provider)
        {
            return DevExpress.LookAndFeel.LookAndFeelHelper.GetSystemColor(provider, SystemColors.Control);
        }
        public static Color TextColor
        {
            get { return CommonSkins.GetSkin(DevExpress.LookAndFeel.UserLookAndFeel.Default).Colors.GetColor(CommonColors.ControlText); }
        }
        public static Color WindowColor
        {
            get { return CommonSkins.GetSkin(DevExpress.LookAndFeel.UserLookAndFeel.Default).Colors.GetColor(CommonColors.Window); }
        }
        public static Color WindowTextColor
        {
            get { return CommonSkins.GetSkin(DevExpress.LookAndFeel.UserLookAndFeel.Default).Colors.GetColor(CommonColors.WindowText); }
        }
        public static Color DisabledTextColor
        {
            get { return CommonSkins.GetSkin(DevExpress.LookAndFeel.UserLookAndFeel.Default).Colors.GetColor(CommonColors.DisabledText); }
        }
        public static Color CriticalColor
        {
            get { return CommonColors.GetCriticalColor(DevExpress.LookAndFeel.UserLookAndFeel.Default); }
        }
        public static Color WarningColor
        {
            get { return CommonColors.GetWarningColor(DevExpress.LookAndFeel.UserLookAndFeel.Default); }
        }
        public static Color QuestionColor
        {
            get
            {
                return CommonColors.GetQuestionColor(DevExpress.LookAndFeel.UserLookAndFeel.Default);
            }
        }
        public static Color InformationColor
        {
            get { return CommonColors.GetInformationColor(DevExpress.LookAndFeel.UserLookAndFeel.Default); }
        }
    }
    static class FontProvider
    {
        static IDictionary<string, Font> cache;
        static FontProvider()
        {
            cache = new Dictionary<string, Font>();
        }
        public static Font GetSegoeUIFont(FontStyle fontStyle)
        {
            float defaultSize = DevExpress.Utils.AppearanceObject.DefaultFont.Size;
            return GetFont("Segoe UI", defaultSize, fontStyle);
        }
        public static Font GetSegoeUIFont(float sizeGrow = 0)
        {
            float defaultSize = DevExpress.Utils.AppearanceObject.DefaultFont.Size;
            return GetFont("Segoe UI", defaultSize + sizeGrow);
        }
        public static Font GetSegoeUILightFont(float sizeGrow = 0)
        {
            float defaultSize = DevExpress.Utils.AppearanceObject.DefaultFont.Size;
            return GetFont("Segoe UI Light", defaultSize + sizeGrow);
        }
        public static Font GetFont(string familyName, float size, FontStyle style = FontStyle.Regular)
        {
            string key = familyName + "#" + size;
            if (style != FontStyle.Regular)
                key += "#" + style;
            Font result;
            if (!cache.TryGetValue(key, out result))
            {
                try
                {
                    var family = FindFontFamily(familyName);
                    result = new Font(family ?? FontFamily.GenericSansSerif, size, style);
                }
                catch (ArgumentException) { result = DevExpress.Utils.AppearanceObject.DefaultFont; }
                cache.Add(key, result);
            }
            return result;
        }
        static FontFamily FindFontFamily(string familyName)
        {
            return Array.Find(FontFamily.Families, (f) => f.Name == familyName);
        }
    }
    class FilterTabController
    {
        LabelControl[] labels;
        public FilterTabController(object eValue, params LabelControl[] list)
        {
            this.labels = list;
            EditValue = eValue;
            foreach (LabelControl lb in list)
                lb.Click += (s, e) => EditValue = ((LabelControl)s).Tag;
        }
        object editValueCore;
        public object EditValue
        {
            get { return editValueCore; }
            set
            {
                if (object.Equals(editValueCore, value)) return;
                editValueCore = value;
                OnEditValueChanged();
            }
        }
        void OnEditValueChanged()
        {
            UpdateAppearance();
            RaiseEditValueChanged();
        }
        void UpdateAppearance()
        {
            foreach (LabelControl lc in labels)
            {
                bool isSelected = EditValue.Equals(lc.Tag);
                lc.Font = FontProvider.GetFont(lc.Font.FontFamily.Name, 10.25f, isSelected ? FontStyle.Bold : FontStyle.Regular);
                lc.Appearance.ForeColor = isSelected ? ColorProvider.QuestionColor : Color.Empty;
            }
        }
        public event EventHandler EditValueChanged;
        void RaiseEditValueChanged()
        {
            EventHandler handler = EditValueChanged;
            if (handler != null) handler(EditValue, EventArgs.Empty);
        }
    }
}
