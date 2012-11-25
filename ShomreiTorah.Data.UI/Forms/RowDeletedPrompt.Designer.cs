namespace ShomreiTorah.Data.UI.Forms {
	partial class RowDeletedPrompt {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
			this.delete = new DevExpress.XtraEditors.SimpleButton();
			this.undelete = new DevExpress.XtraEditors.SimpleButton();
			this.rowDisplay = new ShomreiTorah.Data.UI.Forms.ColumnValueDisplay();
			((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
			this.panelControl1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.rowDisplay)).BeginInit();
			this.SuspendLayout();
			// 
			// labelControl1
			// 
			this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Vertical;
			this.labelControl1.Dock = System.Windows.Forms.DockStyle.Top;
			this.labelControl1.Location = new System.Drawing.Point(0, 0);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Padding = new System.Windows.Forms.Padding(13);
			this.labelControl1.Size = new System.Drawing.Size(318, 65);
			this.labelControl1.TabIndex = 0;
			this.labelControl1.Text = "A row that you have edited was deleted by someone else.\r\n\r\nWhat do you want to do" +
    "?";
			// 
			// panelControl1
			// 
			this.panelControl1.Controls.Add(this.undelete);
			this.panelControl1.Controls.Add(this.delete);
			this.panelControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelControl1.Location = new System.Drawing.Point(0, 297);
			this.panelControl1.Name = "panelControl1";
			this.panelControl1.Size = new System.Drawing.Size(318, 69);
			this.panelControl1.TabIndex = 1;
			// 
			// delete
			// 
			this.delete.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.delete.Location = new System.Drawing.Point(12, 34);
			this.delete.Name = "delete";
			this.delete.Size = new System.Drawing.Size(294, 23);
			this.delete.TabIndex = 0;
			this.delete.Text = "Remain deleted (discard my changes)";
			this.delete.Click += new System.EventHandler(this.delete_Click);
			// 
			// undelete
			// 
			this.undelete.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.undelete.Location = new System.Drawing.Point(12, 5);
			this.undelete.Name = "undelete";
			this.undelete.Size = new System.Drawing.Size(294, 23);
			this.undelete.TabIndex = 1;
			this.undelete.Text = "Undelete row (keep my changes)";
			// 
			// rowDisplay
			// 
			this.rowDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rowDisplay.Location = new System.Drawing.Point(0, 65);
			this.rowDisplay.Name = "rowDisplay";
			this.rowDisplay.Size = new System.Drawing.Size(318, 232);
			this.rowDisplay.TabIndex = 2;
			// 
			// RowDeletedPrompt
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(318, 366);
			this.ControlBox = false;
			this.Controls.Add(this.rowDisplay);
			this.Controls.Add(this.panelControl1);
			this.Controls.Add(this.labelControl1);
			this.Name = "RowDeletedPrompt";
			this.Text = "Database Conflict";
			((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
			this.panelControl1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.rowDisplay)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private DevExpress.XtraEditors.LabelControl labelControl1;
		private DevExpress.XtraEditors.PanelControl panelControl1;
		private DevExpress.XtraEditors.SimpleButton undelete;
		private DevExpress.XtraEditors.SimpleButton delete;
		private ColumnValueDisplay rowDisplay;
	}
}