namespace ShomreiTorah.Forms {
	partial class InfoMessage {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InfoMessage));
			this.label = new DevExpress.XtraEditors.LabelControl();
			this.SuspendLayout();
			// 
			// label
			// 
			this.label.Appearance.Options.UseTextOptions = true;
			this.label.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.label.Appearance.TextOptions.HotkeyPrefix = DevExpress.Utils.HKeyPrefix.None;
			this.label.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.label.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.label.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label.Location = new System.Drawing.Point(0, 0);
			this.label.Name = "label";
			this.label.Padding = new System.Windows.Forms.Padding(4);
			this.label.Size = new System.Drawing.Size(240, 86);
			this.label.TabIndex = 0;
			this.label.Text = "Hello!";
			this.label.UseMnemonic = false;
			this.label.DoubleClick += new System.EventHandler(this.label_DoubleClick);
			this.label.MouseLeave += new System.EventHandler(this.label_MouseLeave);
			this.label.MouseEnter += new System.EventHandler(this.label_MouseEnter);
			// 
			// InfoMessage
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(240, 86);
			this.Controls.Add(this.label);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "InfoMessage";
			this.Text = "InfoMessage";
			this.ResumeLayout(false);

		}

		#endregion

		private DevExpress.XtraEditors.LabelControl label;
	}
}