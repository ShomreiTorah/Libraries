using DevExpress.XtraEditors;
using System.Windows.Forms;
namespace ShomreiTorah.WinForms.Controls {
	partial class Lookup {
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Mobility", "CA1601:DoNotUseTimersThatPreventPowerStateChanges")]
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			this.popupCanvas = new ShomreiTorah.WinForms.Controls.Canvas();
			this.resizeAnimator = new System.Windows.Forms.Timer(this.components);
			this.Title = new DevExpress.XtraEditors.SimpleButton();
			this.input = new DevExpress.XtraEditors.TextEdit();
			this.popupScroller = new DevExpress.XtraEditors.VScrollBar();
			this.popupPanel = new System.Windows.Forms.Panel();
			this.autoScrollTimer = new System.Windows.Forms.Timer(this.components);
			((System.ComponentModel.ISupportInitialize)(this.input.Properties)).BeginInit();
			this.popupPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// popupCanvas
			// 
			this.popupCanvas.Dock = System.Windows.Forms.DockStyle.Fill;
			this.popupCanvas.Location = new System.Drawing.Point(0, 0);
			this.popupCanvas.Name = "popupCanvas";
			this.popupCanvas.Size = new System.Drawing.Size(291, 61);
			this.popupCanvas.TabIndex = 0;
			this.popupCanvas.Paint += new System.Windows.Forms.PaintEventHandler(this.PopupCanvas_Paint);
			this.popupCanvas.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PopupCanvas_MouseMove);
			this.popupCanvas.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PopupCanvas_MouseClick);
			this.popupCanvas.MouseDown += new System.Windows.Forms.MouseEventHandler(this.popupCanvas_MouseDown);
			this.popupCanvas.MouseUp += new System.Windows.Forms.MouseEventHandler(this.popupCanvas_MouseUp);
			this.popupCanvas.SizeChanged += new System.EventHandler(this.RecreateBrushes);
			// 
			// resizeAnimator
			// 
			this.resizeAnimator.Interval = 10;
			this.resizeAnimator.Tick += new System.EventHandler(this.ResizeAnimator_Tick);
			// 
			// Title
			// 
			this.Title.Appearance.Font = new System.Drawing.Font("Tahoma", 8F);
			this.Title.Appearance.Options.UseFont = true;
			this.Title.Dock = System.Windows.Forms.DockStyle.Left;
			this.Title.Location = new System.Drawing.Point(0, 0);
			this.Title.Name = "Title";
			this.Title.Size = new System.Drawing.Size(79, 392);
			this.Title.TabIndex = 1;
			this.Title.Text = "Type a name:";
			this.Title.Click += new System.EventHandler(this.Title_Click);
			// 
			// input
			// 
			this.input.Dock = System.Windows.Forms.DockStyle.Fill;
			this.input.Location = new System.Drawing.Point(79, 0);
			this.input.Margin = new System.Windows.Forms.Padding(0);
			this.input.Name = "input";
			this.input.Size = new System.Drawing.Size(503, 20);
			this.input.TabIndex = 0;
			this.input.Leave += new System.EventHandler(this.Input_Leave);
			this.input.Enter += new System.EventHandler(this.Input_Enter);
			this.input.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Input_KeyPress);
			this.input.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Input_KeyUp);
			this.input.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Input_KeyDown);
			this.input.Click += new System.EventHandler(this.Input_Click);
			// 
			// popupScroller
			// 
			this.popupScroller.Dock = System.Windows.Forms.DockStyle.Right;
			this.popupScroller.LargeChange = 34;
			this.popupScroller.Location = new System.Drawing.Point(291, 0);
			this.popupScroller.Maximum = 34;
			this.popupScroller.Name = "popupScroller";
			this.popupScroller.ScrollBarAutoSize = true;
			this.popupScroller.Size = new System.Drawing.Size(17, 61);
			this.popupScroller.SmallChange = 17;
			this.popupScroller.TabIndex = 2;
			this.popupScroller.Value = 2;
			this.popupScroller.ValueChanged += new System.EventHandler(this.popupScroller_ValueChanged);
			// 
			// popupPanel
			// 
			this.popupPanel.Controls.Add(this.popupCanvas);
			this.popupPanel.Controls.Add(this.popupScroller);
			this.popupPanel.Location = new System.Drawing.Point(141, 122);
			this.popupPanel.Name = "popupPanel";
			this.popupPanel.Size = new System.Drawing.Size(308, 61);
			this.popupPanel.TabIndex = 3;
			// 
			// autoScrollTimer
			// 
			this.autoScrollTimer.Interval = 10;
			this.autoScrollTimer.Tick += new System.EventHandler(this.autoScrollTimer_Tick);
			// 
			// Lookup
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.popupPanel);
			this.Controls.Add(this.input);
			this.Controls.Add(this.Title);
			this.MinimumSize = new System.Drawing.Size(448, 20);
			this.Name = "Lookup";
			this.Size = new System.Drawing.Size(582, 392);
			((System.ComponentModel.ISupportInitialize)(this.input.Properties)).EndInit();
			this.popupPanel.ResumeLayout(false);
			this.popupPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Timer resizeAnimator;
		private Canvas popupCanvas;
		private SimpleButton Title;
		private TextEdit input;
		private DevExpress.XtraEditors.VScrollBar popupScroller;
		private Panel popupPanel;
		private Timer autoScrollTimer;
	}
}
