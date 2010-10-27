namespace ShomreiTorah.Data.UI.Forms {
	partial class PersonCreator {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PersonCreator));
			this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
			this.ok = new DevExpress.XtraEditors.SimpleButton();
			this.cancel = new DevExpress.XtraEditors.SimpleButton();
			this.personEditor = new ShomreiTorah.Data.UI.Controls.PersonEditor();
			((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
			this.panelControl1.SuspendLayout();
			this.SuspendLayout();
			// 
			// panelControl1
			// 
			this.panelControl1.Controls.Add(this.ok);
			this.panelControl1.Controls.Add(this.cancel);
			this.panelControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelControl1.Location = new System.Drawing.Point(0, 202);
			this.panelControl1.Name = "panelControl1";
			this.panelControl1.Size = new System.Drawing.Size(369, 40);
			this.panelControl1.TabIndex = 1;
			// 
			// ok
			// 
			this.ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ok.Image = ((System.Drawing.Image)(resources.GetObject("ok.Image")));
			this.ok.Location = new System.Drawing.Point(201, 5);
			this.ok.Name = "ok";
			this.ok.Size = new System.Drawing.Size(75, 23);
			this.ok.TabIndex = 1;
			this.ok.Text = "Create";
			this.ok.Click += new System.EventHandler(this.ok_Click);
			// 
			// cancel
			// 
			this.cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.Location = new System.Drawing.Point(282, 5);
			this.cancel.Name = "cancel";
			this.cancel.Size = new System.Drawing.Size(75, 23);
			this.cancel.TabIndex = 0;
			this.cancel.Text = "Cancel";
			// 
			// personEditor
			// 
			this.personEditor.Dock = System.Windows.Forms.DockStyle.Top;
			this.personEditor.Location = new System.Drawing.Point(0, 0);
			this.personEditor.Name = "personEditor";
			this.personEditor.Size = new System.Drawing.Size(369, 211);
			this.personEditor.TabIndex = 0;
			// 
			// PersonCreator
			// 
			this.AcceptButton = this.ok;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(369, 242);
			this.ControlBox = false;
			this.Controls.Add(this.panelControl1);
			this.Controls.Add(this.personEditor);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximumSize = new System.Drawing.Size(900, 310);
			this.MinimumSize = new System.Drawing.Size(377, 270);
			this.Name = "PersonCreator";
			this.Text = "New Person";
			((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
			this.panelControl1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Controls.PersonEditor personEditor;
		private DevExpress.XtraEditors.PanelControl panelControl1;
		private DevExpress.XtraEditors.SimpleButton ok;
		private DevExpress.XtraEditors.SimpleButton cancel;
	}
}