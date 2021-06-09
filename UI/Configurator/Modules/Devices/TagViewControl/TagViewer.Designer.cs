namespace Configurator.Modules
{
    partial class TagViewer
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TagViewer));
            this.TagViewTable = new DevExpress.XtraGrid.GridControl();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.AddTag = new System.Windows.Forms.ToolStripMenuItem();
            this.DeleteTag = new System.Windows.Forms.ToolStripMenuItem();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.ID = new DevExpress.XtraGrid.Columns.GridColumn();
            this.Memory = new DevExpress.XtraGrid.Columns.GridColumn();
            this.Addr = new DevExpress.XtraGrid.Columns.GridColumn();
            this.Scanrate = new DevExpress.XtraGrid.Columns.GridColumn();
            this.Type = new DevExpress.XtraGrid.Columns.GridColumn();
            this.Redis = new DevExpress.XtraGrid.Columns.GridColumn();
            this.TagSize = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemTextEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.splitContainerControl1 = new DevExpress.XtraEditors.SplitContainerControl();
            this.DeleteTagButton = new DevExpress.XtraEditors.SimpleButton();
            this.AddTagButton = new DevExpress.XtraEditors.SimpleButton();
            this.LoadButton = new DevExpress.XtraEditors.SimpleButton();
            this.SaveAsButton = new DevExpress.XtraEditors.SimpleButton();
            this.SaveButton = new DevExpress.XtraEditors.SimpleButton();
            this.CurrentMsg = new DevExpress.XtraEditors.ListBoxControl();
            this.xtraOpenFileDialog1 = new DevExpress.XtraEditors.XtraOpenFileDialog(this.components);
            this.xtraSaveFileDialog1 = new DevExpress.XtraEditors.XtraSaveFileDialog(this.components);
            this.btnCopyTag = new DevExpress.XtraEditors.SimpleButton();
            this.btnPasteTag = new DevExpress.XtraEditors.SimpleButton();
            this.copyTagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteTagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.mvvmContext)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TagViewTable)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).BeginInit();
            this.splitContainerControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CurrentMsg)).BeginInit();
            this.SuspendLayout();
            // 
            // TagViewTable
            // 
            this.TagViewTable.AllowRestoreSelectionAndFocusedRow = DevExpress.Utils.DefaultBoolean.False;
            this.TagViewTable.ContextMenuStrip = this.contextMenuStrip1;
            this.TagViewTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TagViewTable.Location = new System.Drawing.Point(0, 0);
            this.TagViewTable.MainView = this.gridView1;
            this.TagViewTable.Name = "TagViewTable";
            this.TagViewTable.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemTextEdit1});
            this.TagViewTable.ShowOnlyPredefinedDetails = true;
            this.TagViewTable.Size = new System.Drawing.Size(1035, 557);
            this.TagViewTable.TabIndex = 2;
            this.TagViewTable.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AddTag,
            this.copyTagToolStripMenuItem,
            this.pasteTagToolStripMenuItem,
            this.DeleteTag});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(181, 114);
            // 
            // AddTag
            // 
            this.AddTag.Image = global::Configurator.Properties.Resources.AddHeader_16x16;
            this.AddTag.Name = "AddTag";
            this.AddTag.Size = new System.Drawing.Size(165, 22);
            this.AddTag.Text = "Add Tag";
            this.AddTag.Click += new System.EventHandler(this.AddTag_Click);
            // 
            // DeleteTag
            // 
            this.DeleteTag.Image = global::Configurator.Properties.Resources.DeleteHeader_16x16;
            this.DeleteTag.Name = "DeleteTag";
            this.DeleteTag.Size = new System.Drawing.Size(165, 22);
            this.DeleteTag.Text = "Delete Tag";
            this.DeleteTag.Click += new System.EventHandler(this.DeleteTag_Click);
            // 
            // gridView1
            // 
            this.gridView1.Appearance.VertLine.ForeColor = System.Drawing.Color.Transparent;
            this.gridView1.AppearancePrint.Row.Options.UseTextOptions = true;
            this.gridView1.AppearancePrint.Row.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.ID,
            this.Memory,
            this.Addr,
            this.Scanrate,
            this.Type,
            this.Redis,
            this.TagSize});
            this.gridView1.GridControl = this.TagViewTable;
            this.gridView1.GroupCount = 1;
            this.gridView1.GroupFormat = "{1}";
            this.gridView1.GroupSummary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[] {
            new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Custom, "Memory", this.Memory, "{0}: {1}", "<Null>")});
            this.gridView1.Name = "gridView1";
            this.gridView1.OptionsBehavior.AlignGroupSummaryInGroupRow = DevExpress.Utils.DefaultBoolean.True;
            this.gridView1.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.True;
            this.gridView1.OptionsBehavior.AllowDeleteRows = DevExpress.Utils.DefaultBoolean.True;
            this.gridView1.OptionsBehavior.AllowFixedGroups = DevExpress.Utils.DefaultBoolean.False;
            this.gridView1.OptionsBehavior.EditingMode = DevExpress.XtraGrid.Views.Grid.GridEditingMode.Inplace;
            this.gridView1.OptionsPrint.AllowMultilineHeaders = true;
            this.gridView1.OptionsPrint.ExpandAllGroups = false;
            this.gridView1.OptionsPrint.PrintFilterInfo = true;
            this.gridView1.OptionsPrint.PrintFooter = false;
            this.gridView1.OptionsSelection.MultiSelect = true;
            this.gridView1.OptionsView.GroupDrawMode = DevExpress.XtraGrid.Views.Grid.GroupDrawMode.Standard;
            this.gridView1.OptionsView.ShowChildrenInGroupPanel = true;
            this.gridView1.OptionsView.ShowGroupedColumns = true;
            this.gridView1.OptionsView.ShowVerticalLines = DevExpress.Utils.DefaultBoolean.False;
            this.gridView1.SortInfo.AddRange(new DevExpress.XtraGrid.Columns.GridColumnSortInfo[] {
            new DevExpress.XtraGrid.Columns.GridColumnSortInfo(this.Memory, DevExpress.Data.ColumnSortOrder.Ascending)});
            // 
            // ID
            // 
            this.ID.FieldName = "ID";
            this.ID.Name = "ID";
            this.ID.Visible = true;
            this.ID.VisibleIndex = 1;
            // 
            // Memory
            // 
            this.Memory.FieldName = "Memory";
            this.Memory.GroupFormat.FormatString = "{0}: {1}";
            this.Memory.GroupFormat.FormatType = DevExpress.Utils.FormatType.Custom;
            this.Memory.GroupInterval = DevExpress.XtraGrid.ColumnGroupInterval.Value;
            this.Memory.Name = "Memory";
            this.Memory.OptionsColumn.AllowGroup = DevExpress.Utils.DefaultBoolean.True;
            this.Memory.Visible = true;
            this.Memory.VisibleIndex = 0;
            // 
            // Addr
            // 
            this.Addr.FieldName = "Addr";
            this.Addr.Name = "Addr";
            this.Addr.Visible = true;
            this.Addr.VisibleIndex = 2;
            // 
            // Scanrate
            // 
            this.Scanrate.FieldName = "Scanrate";
            this.Scanrate.Name = "Scanrate";
            this.Scanrate.Visible = true;
            this.Scanrate.VisibleIndex = 3;
            // 
            // Type
            // 
            this.Type.FieldName = "Type";
            this.Type.Name = "Type";
            this.Type.Visible = true;
            this.Type.VisibleIndex = 4;
            // 
            // Redis
            // 
            this.Redis.FieldName = "Redis";
            this.Redis.Name = "Redis";
            this.Redis.Visible = true;
            this.Redis.VisibleIndex = 5;
            // 
            // TagSize
            // 
            this.TagSize.FieldName = "TagSize";
            this.TagSize.Name = "TagSize";
            this.TagSize.Visible = true;
            this.TagSize.VisibleIndex = 6;
            // 
            // repositoryItemTextEdit1
            // 
            this.repositoryItemTextEdit1.AutoHeight = false;
            this.repositoryItemTextEdit1.Name = "repositoryItemTextEdit1";
            // 
            // splitContainerControl1
            // 
            this.splitContainerControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerControl1.Horizontal = false;
            this.splitContainerControl1.Location = new System.Drawing.Point(0, 0);
            this.splitContainerControl1.Name = "splitContainerControl1";
            this.splitContainerControl1.Panel1.Controls.Add(this.btnPasteTag);
            this.splitContainerControl1.Panel1.Controls.Add(this.btnCopyTag);
            this.splitContainerControl1.Panel1.Controls.Add(this.DeleteTagButton);
            this.splitContainerControl1.Panel1.Controls.Add(this.AddTagButton);
            this.splitContainerControl1.Panel1.Controls.Add(this.LoadButton);
            this.splitContainerControl1.Panel1.Controls.Add(this.SaveAsButton);
            this.splitContainerControl1.Panel1.Controls.Add(this.SaveButton);
            this.splitContainerControl1.Panel1.Controls.Add(this.TagViewTable);
            this.splitContainerControl1.Panel1.Text = "Panel1";
            this.splitContainerControl1.Panel2.Controls.Add(this.CurrentMsg);
            this.splitContainerControl1.Panel2.Text = "Panel2";
            this.splitContainerControl1.Size = new System.Drawing.Size(1035, 681);
            this.splitContainerControl1.SplitterPosition = 557;
            this.splitContainerControl1.TabIndex = 3;
            // 
            // DeleteTagButton
            // 
            this.DeleteTagButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("DeleteTagButton.ImageOptions.Image")));
            this.DeleteTagButton.Location = new System.Drawing.Point(220, 10);
            this.DeleteTagButton.Name = "DeleteTagButton";
            this.DeleteTagButton.Size = new System.Drawing.Size(22, 24);
            this.DeleteTagButton.TabIndex = 7;
            this.DeleteTagButton.Click += new System.EventHandler(this.DeleteTag_Click);
            // 
            // AddTagButton
            // 
            this.AddTagButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("AddTagButton.ImageOptions.Image")));
            this.AddTagButton.Location = new System.Drawing.Point(195, 10);
            this.AddTagButton.Name = "AddTagButton";
            this.AddTagButton.Size = new System.Drawing.Size(22, 24);
            this.AddTagButton.TabIndex = 6;
            this.AddTagButton.Click += new System.EventHandler(this.AddTag_Click);
            // 
            // LoadButton
            // 
            this.LoadButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("LoadButton.ImageOptions.Image")));
            this.LoadButton.Location = new System.Drawing.Point(162, 10);
            this.LoadButton.Name = "LoadButton";
            this.LoadButton.Size = new System.Drawing.Size(22, 24);
            this.LoadButton.TabIndex = 5;
            this.LoadButton.Click += new System.EventHandler(this.LoadButton_Click);
            // 
            // SaveAsButton
            // 
            this.SaveAsButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("SaveAsButton.ImageOptions.Image")));
            this.SaveAsButton.Location = new System.Drawing.Point(137, 10);
            this.SaveAsButton.Name = "SaveAsButton";
            this.SaveAsButton.Size = new System.Drawing.Size(22, 24);
            this.SaveAsButton.TabIndex = 4;
            this.SaveAsButton.Click += new System.EventHandler(this.SaveAsButton_Click);
            // 
            // SaveButton
            // 
            this.SaveButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("SaveButton.ImageOptions.Image")));
            this.SaveButton.Location = new System.Drawing.Point(112, 10);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(22, 24);
            this.SaveButton.TabIndex = 3;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // CurrentMsg
            // 
            this.CurrentMsg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CurrentMsg.Location = new System.Drawing.Point(0, 0);
            this.CurrentMsg.Name = "CurrentMsg";
            this.CurrentMsg.Size = new System.Drawing.Size(1035, 114);
            this.CurrentMsg.TabIndex = 8;
            // 
            // xtraOpenFileDialog1
            // 
            this.xtraOpenFileDialog1.FileName = "xtraOpenFileDialog1";
            // 
            // xtraSaveFileDialog1
            // 
            this.xtraSaveFileDialog1.FileName = "xtraSaveFileDialog1";
            // 
            // btnCopyTag
            // 
            this.btnCopyTag.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("simpleButton1.ImageOptions.Image")));
            this.btnCopyTag.Location = new System.Drawing.Point(245, 10);
            this.btnCopyTag.Name = "btnCopyTag";
            this.btnCopyTag.Size = new System.Drawing.Size(22, 24);
            this.btnCopyTag.TabIndex = 8;
            this.btnCopyTag.Click += new System.EventHandler(this.btnCopyTag_Click);
            // 
            // btnPasteTag
            // 
            this.btnPasteTag.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("simpleButton2.ImageOptions.Image")));
            this.btnPasteTag.Location = new System.Drawing.Point(270, 10);
            this.btnPasteTag.Name = "btnPasteTag";
            this.btnPasteTag.Size = new System.Drawing.Size(22, 24);
            this.btnPasteTag.TabIndex = 9;
            this.btnPasteTag.Click += new System.EventHandler(this.btnPasteTag_Click);
            // 
            // copyTagToolStripMenuItem
            // 
            this.copyTagToolStripMenuItem.Name = "copyTagToolStripMenuItem";
            this.copyTagToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyTagToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.copyTagToolStripMenuItem.Text = "Copy Tag";
            this.copyTagToolStripMenuItem.Click += new System.EventHandler(this.btnCopyTag_Click);
            // 
            // pasteTagToolStripMenuItem
            // 
            this.pasteTagToolStripMenuItem.Name = "pasteTagToolStripMenuItem";
            this.pasteTagToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.pasteTagToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.pasteTagToolStripMenuItem.Text = "Paste Tag";
            this.pasteTagToolStripMenuItem.Click += new System.EventHandler(this.btnPasteTag_Click);
            // 
            // TagViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainerControl1);
            this.Name = "TagViewer";
            this.Size = new System.Drawing.Size(1035, 681);
            ((System.ComponentModel.ISupportInitialize)(this.mvvmContext)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TagViewTable)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).EndInit();
            this.splitContainerControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.CurrentMsg)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private DevExpress.XtraGrid.GridControl TagViewTable;
        private DevExpress.XtraEditors.SplitContainerControl splitContainerControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraGrid.Columns.GridColumn ID;
        private DevExpress.XtraGrid.Columns.GridColumn Memory;
        private DevExpress.XtraGrid.Columns.GridColumn Addr;
        private DevExpress.XtraGrid.Columns.GridColumn Scanrate;
        private DevExpress.XtraGrid.Columns.GridColumn Type;
        private DevExpress.XtraGrid.Columns.GridColumn Redis;
        private DevExpress.XtraGrid.Columns.GridColumn TagSize;
        private DevExpress.XtraEditors.SimpleButton LoadButton;
        private DevExpress.XtraEditors.SimpleButton SaveAsButton;
        private DevExpress.XtraEditors.SimpleButton SaveButton;
        private DevExpress.XtraEditors.XtraOpenFileDialog xtraOpenFileDialog1;
        private DevExpress.XtraEditors.XtraSaveFileDialog xtraSaveFileDialog1;
        private DevExpress.XtraEditors.SimpleButton DeleteTagButton;
        private DevExpress.XtraEditors.SimpleButton AddTagButton;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem AddTag;
        private System.Windows.Forms.ToolStripMenuItem DeleteTag;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit repositoryItemTextEdit1;
        private DevExpress.XtraEditors.ListBoxControl CurrentMsg;
        private System.Windows.Forms.ToolStripMenuItem copyTagToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteTagToolStripMenuItem;
        private DevExpress.XtraEditors.SimpleButton btnPasteTag;
        private DevExpress.XtraEditors.SimpleButton btnCopyTag;
    }
}
