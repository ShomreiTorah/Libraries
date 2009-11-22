namespace ShomreiTorah.WinForms.Controls {
	partial class GoogleMapControl {
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
			this.pictureBox = new System.Windows.Forms.PictureBox();
			this.message = new DevExpress.XtraEditors.LabelControl();
			((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
			this.panelControl1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// panelControl1
			// 
			this.panelControl1.Controls.Add(this.pictureBox);
			this.panelControl1.Controls.Add(this.message);
			this.panelControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelControl1.Location = new System.Drawing.Point(0, 0);
			this.panelControl1.Name = "panelControl1";
			this.panelControl1.Size = new System.Drawing.Size(226, 200);
			this.panelControl1.TabIndex = 0;
			// 
			// pictureBox
			// 
			this.pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pictureBox.InitialImage = global::ShomreiTorah.WinForms.Properties.Resources.Loading32;
			this.pictureBox.Location = new System.Drawing.Point(2, 2);
			this.pictureBox.Name = "pictureBox";
			this.pictureBox.Size = new System.Drawing.Size(222, 196);
			this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pictureBox.TabIndex = 0;
			this.pictureBox.TabStop = false;
			this.pictureBox.LoadCompleted += new System.ComponentModel.AsyncCompletedEventHandler(this.pictureBox_LoadCompleted);
			this.pictureBox.Click += new System.EventHandler(this.pictureBox_Click);
			// 
			// message
			// 
			this.message.Appearance.Options.UseTextOptions = true;
			this.message.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.message.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.message.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.message.Dock = System.Windows.Forms.DockStyle.Fill;
			this.message.Location = new System.Drawing.Point(2, 2);
			this.message.Name = "message";
			this.message.Size = new System.Drawing.Size(222, 196);
			this.message.TabIndex = 1;
			// 
			// GoogleMapControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.panelControl1);
			this.Name = "GoogleMapControl";
			this.Size = new System.Drawing.Size(226, 200);
			((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
			this.panelControl1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private DevExpress.XtraEditors.PanelControl panelControl1;
		private System.Windows.Forms.PictureBox pictureBox;
		private DevExpress.XtraEditors.LabelControl message;
	}
}
