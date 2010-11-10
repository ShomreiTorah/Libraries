namespace ShomreiTorah.Data.UI.Forms {
	partial class SimplePersonDetails {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			DevExpress.Utils.SuperToolTip superToolTip1 = new DevExpress.Utils.SuperToolTip();
			DevExpress.Utils.ToolTipTitleItem toolTipTitleItem1 = new DevExpress.Utils.ToolTipTitleItem();
			DevExpress.Utils.ToolTipItem toolTipItem1 = new DevExpress.Utils.ToolTipItem();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SimplePersonDetails));
			this.detailText = new DevExpress.XtraEditors.MemoEdit();
			this.map = new ShomreiTorah.WinForms.Controls.GoogleMapControl();
			this.showEditor = new DevExpress.XtraEditors.SimpleButton();
			this.editorPanel = new DevExpress.XtraEditors.PanelControl();
			this.closeEditor = new DevExpress.XtraEditors.SimpleButton();
			this.editor = new ShomreiTorah.Data.UI.Controls.PersonEditor();
			((System.ComponentModel.ISupportInitialize)(this.detailText.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.editorPanel)).BeginInit();
			this.editorPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// detailText
			// 
			this.detailText.Dock = System.Windows.Forms.DockStyle.Top;
			this.detailText.Location = new System.Drawing.Point(0, 0);
			this.detailText.Name = "detailText";
			this.detailText.Properties.ReadOnly = true;
			this.detailText.Size = new System.Drawing.Size(261, 96);
			this.detailText.TabIndex = 0;
			// 
			// map
			// 
			this.map.Dock = System.Windows.Forms.DockStyle.Fill;
			this.map.Location = new System.Drawing.Point(0, 338);
			this.map.Name = "map";
			this.map.Size = new System.Drawing.Size(261, 24);
			this.map.TabIndex = 1;
			this.map.Text = "googleMapControl1";
			// 
			// showEditor
			// 
			this.showEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.showEditor.Image = global::ShomreiTorah.Data.UI.Properties.Resources.Edit16;
			this.showEditor.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.showEditor.Location = new System.Drawing.Point(238, 0);
			this.showEditor.Name = "showEditor";
			this.showEditor.Size = new System.Drawing.Size(23, 23);
			toolTipTitleItem1.Text = "Edit";
			toolTipItem1.LeftIndent = 6;
			toolTipItem1.Text = "Edits the person\'s name or address";
			superToolTip1.Items.Add(toolTipTitleItem1);
			superToolTip1.Items.Add(toolTipItem1);
			this.showEditor.SuperTip = superToolTip1;
			this.showEditor.TabIndex = 7;
			this.showEditor.Text = "Edit";
			this.showEditor.Click += new System.EventHandler(this.showEditor_Click);
			// 
			// editorPanel
			// 
			this.editorPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.editorPanel.Controls.Add(this.closeEditor);
			this.editorPanel.Controls.Add(this.editor);
			this.editorPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.editorPanel.Location = new System.Drawing.Point(0, 96);
			this.editorPanel.Name = "editorPanel";
			this.editorPanel.Size = new System.Drawing.Size(261, 242);
			this.editorPanel.TabIndex = 8;
			this.editorPanel.Visible = false;
			// 
			// closeEditor
			// 
			this.closeEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.closeEditor.Location = new System.Drawing.Point(183, 216);
			this.closeEditor.Name = "closeEditor";
			this.closeEditor.Size = new System.Drawing.Size(75, 23);
			this.closeEditor.TabIndex = 1;
			this.closeEditor.Text = "Done";
			this.closeEditor.Click += new System.EventHandler(this.closeEditor_Click);
			// 
			// editor
			// 
			this.editor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.editor.Location = new System.Drawing.Point(0, 0);
			this.editor.Name = "editor";
			this.editor.Size = new System.Drawing.Size(260, 210);
			this.editor.TabIndex = 0;
			// 
			// SimplePersonDetails
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(261, 362);
			this.Controls.Add(this.map);
			this.Controls.Add(this.editorPanel);
			this.Controls.Add(this.showEditor);
			this.Controls.Add(this.detailText);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SimplePersonDetails";
			this.Text = "SimplePersonDetails";
			((System.ComponentModel.ISupportInitialize)(this.detailText.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.editorPanel)).EndInit();
			this.editorPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DevExpress.XtraEditors.MemoEdit detailText;
		private WinForms.Controls.GoogleMapControl map;
		private DevExpress.XtraEditors.SimpleButton showEditor;
		private DevExpress.XtraEditors.PanelControl editorPanel;
		private DevExpress.XtraEditors.SimpleButton closeEditor;
		private Controls.PersonEditor editor;
	}
}