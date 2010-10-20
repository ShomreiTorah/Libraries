namespace ShomreiTorah.Data.UI.Forms {
	partial class ExceptionReporter {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExceptionReporter));
			this.emailWorker = new System.ComponentModel.BackgroundWorker();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.loadingPanel = new DevExpress.XtraEditors.PanelControl();
			this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
			this.loadingCircle1 = new ShomreiTorah.WinForms.Controls.LoadingCircle();
			this.sendStatusLabel = new DevExpress.XtraEditors.LabelControl();
			this.errorDetails = new DevExpress.XtraEditors.MemoEdit();
			this.groupControl1 = new DevExpress.XtraEditors.GroupControl();
			((System.ComponentModel.ISupportInitialize)(this.loadingPanel)).BeginInit();
			this.loadingPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.errorDetails.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.groupControl1)).BeginInit();
			this.groupControl1.SuspendLayout();
			this.SuspendLayout();
			// 
			// emailWorker
			// 
			this.emailWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.emailWorker_DoWork);
			this.emailWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.emailWorker_RunWorkerCompleted);
			// 
			// labelControl1
			// 
			this.labelControl1.Appearance.Image = global::ShomreiTorah.Data.UI.Properties.Resources.RedX32;
			this.labelControl1.Appearance.Options.UseImage = true;
			this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Vertical;
			this.labelControl1.Dock = System.Windows.Forms.DockStyle.Top;
			this.labelControl1.ImageAlignToText = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
			this.labelControl1.LineLocation = DevExpress.XtraEditors.LineLocation.Bottom;
			this.labelControl1.LineVisible = true;
			this.labelControl1.Location = new System.Drawing.Point(0, 0);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Padding = new System.Windows.Forms.Padding(4, 5, 4, 4);
			this.labelControl1.Size = new System.Drawing.Size(462, 51);
			this.labelControl1.TabIndex = 0;
			this.labelControl1.Text = "Unfortunately, an error occurred.\r\nIn principle, no data has been lost; you shoul" +
				"d be able to continue working.\r\nI apologize for the problem.";
			// 
			// loadingPanel
			// 
			this.loadingPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.loadingPanel.Controls.Add(this.labelControl2);
			this.loadingPanel.Controls.Add(this.loadingCircle1);
			this.loadingPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.loadingPanel.Location = new System.Drawing.Point(0, 51);
			this.loadingPanel.Name = "loadingPanel";
			this.loadingPanel.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
			this.loadingPanel.Size = new System.Drawing.Size(462, 22);
			this.loadingPanel.TabIndex = 2;
			// 
			// labelControl2
			// 
			this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelControl2.Location = new System.Drawing.Point(20, 0);
			this.labelControl2.Name = "labelControl2";
			this.labelControl2.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
			this.labelControl2.Size = new System.Drawing.Size(442, 22);
			this.labelControl2.TabIndex = 3;
			this.labelControl2.Text = "Sending error report...";
			// 
			// loadingCircle1
			// 
			this.loadingCircle1.Active = true;
			this.loadingCircle1.Color = System.Drawing.Color.DarkGray;
			this.loadingCircle1.Dock = System.Windows.Forms.DockStyle.Left;
			this.loadingCircle1.InnerCircleRadius = 4;
			this.loadingCircle1.Location = new System.Drawing.Point(4, 0);
			this.loadingCircle1.Name = "loadingCircle1";
			this.loadingCircle1.OuterCircleRadius = 7;
			this.loadingCircle1.RotationSpeed = 100;
			this.loadingCircle1.Size = new System.Drawing.Size(16, 22);
			this.loadingCircle1.SpokeCount = 10;
			this.loadingCircle1.SpokeThickness = 2;
			this.loadingCircle1.TabIndex = 2;
			this.loadingCircle1.Text = "loadingCircle1";
			// 
			// sendStatusLabel
			// 
			this.sendStatusLabel.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Vertical;
			this.sendStatusLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.sendStatusLabel.Location = new System.Drawing.Point(0, 73);
			this.sendStatusLabel.Name = "sendStatusLabel";
			this.sendStatusLabel.Padding = new System.Windows.Forms.Padding(4, 3, 3, 6);
			this.sendStatusLabel.Size = new System.Drawing.Size(462, 22);
			this.sendStatusLabel.TabIndex = 3;
			this.sendStatusLabel.Text = "You can\'t see me.";
			this.sendStatusLabel.Visible = false;
			// 
			// errorDetails
			// 
			this.errorDetails.Dock = System.Windows.Forms.DockStyle.Fill;
			this.errorDetails.Location = new System.Drawing.Point(2, 22);
			this.errorDetails.Name = "errorDetails";
			this.errorDetails.Properties.ReadOnly = true;
			this.errorDetails.Properties.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.errorDetails.Properties.WordWrap = false;
			this.errorDetails.Size = new System.Drawing.Size(458, 169);
			this.errorDetails.TabIndex = 4;
			this.errorDetails.KeyUp += new System.Windows.Forms.KeyEventHandler(this.errorDetails_KeyUp);
			// 
			// groupControl1
			// 
			this.groupControl1.AppearanceCaption.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.groupControl1.AppearanceCaption.Options.UseFont = true;
			this.groupControl1.Controls.Add(this.errorDetails);
			this.groupControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupControl1.Location = new System.Drawing.Point(0, 95);
			this.groupControl1.Name = "groupControl1";
			this.groupControl1.Size = new System.Drawing.Size(462, 193);
			this.groupControl1.TabIndex = 5;
			this.groupControl1.Text = "Error Details";
			// 
			// ExceptionReporter
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(462, 288);
			this.Controls.Add(this.groupControl1);
			this.Controls.Add(this.sendStatusLabel);
			this.Controls.Add(this.loadingPanel);
			this.Controls.Add(this.labelControl1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(420, 102);
			this.Name = "ExceptionReporter";
			this.Text = "Error";
			((System.ComponentModel.ISupportInitialize)(this.loadingPanel)).EndInit();
			this.loadingPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.errorDetails.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.groupControl1)).EndInit();
			this.groupControl1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.ComponentModel.BackgroundWorker emailWorker;
		private DevExpress.XtraEditors.LabelControl labelControl1;
		private DevExpress.XtraEditors.PanelControl loadingPanel;
		private DevExpress.XtraEditors.LabelControl labelControl2;
		private WinForms.Controls.LoadingCircle loadingCircle1;
		private DevExpress.XtraEditors.LabelControl sendStatusLabel;
		private DevExpress.XtraEditors.MemoEdit errorDetails;
		private DevExpress.XtraEditors.GroupControl groupControl1;
	}
}