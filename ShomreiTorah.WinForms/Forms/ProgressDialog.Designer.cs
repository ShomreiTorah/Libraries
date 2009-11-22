namespace ShomreiTorah.Forms {
	partial class ProgressDialog {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProgressDialog));
			this.label = new DevExpress.XtraEditors.LabelControl();
			this.cancel = new DevExpress.XtraEditors.SimpleButton();
			this.marqueeBar = new DevExpress.XtraEditors.MarqueeProgressBarControl();
			this.progressBar = new DevExpress.XtraEditors.ProgressBarControl();
			((System.ComponentModel.ISupportInitialize)(this.marqueeBar.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.progressBar.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// label
			// 
			this.label.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.label.Appearance.Options.UseTextOptions = true;
			this.label.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.label.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.label.Location = new System.Drawing.Point(12, 12);
			this.label.Name = "label";
			this.label.Size = new System.Drawing.Size(287, 28);
			this.label.TabIndex = 0;
			this.label.Text = "Please wait...";
			// 
			// cancel
			// 
			this.cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancel.Location = new System.Drawing.Point(224, 49);
			this.cancel.Name = "cancel";
			this.cancel.Size = new System.Drawing.Size(75, 23);
			this.cancel.TabIndex = 1;
			this.cancel.Text = "Cancel";
			this.cancel.Click += new System.EventHandler(this.cancel_Click);
			// 
			// marqueeBar
			// 
			this.marqueeBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.marqueeBar.EditValue = 0;
			this.marqueeBar.Location = new System.Drawing.Point(12, 51);
			this.marqueeBar.Name = "marqueeBar";
			this.marqueeBar.Size = new System.Drawing.Size(206, 18);
			this.marqueeBar.TabIndex = 2;
			// 
			// progressBar
			// 
			this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.progressBar.Location = new System.Drawing.Point(12, 51);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(206, 18);
			this.progressBar.TabIndex = 3;
			// 
			// ProgressDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(311, 84);
			this.ControlBox = false;
			this.Controls.Add(this.progressBar);
			this.Controls.Add(this.marqueeBar);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.label);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "ProgressDialog";
			this.ShowInTaskbar = false;
			this.Text = "Please Wait...";
			((System.ComponentModel.ISupportInitialize)(this.marqueeBar.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.progressBar.Properties)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private DevExpress.XtraEditors.LabelControl label;
		private DevExpress.XtraEditors.SimpleButton cancel;
		private DevExpress.XtraEditors.MarqueeProgressBarControl marqueeBar;
		private DevExpress.XtraEditors.ProgressBarControl progressBar;
	}
}