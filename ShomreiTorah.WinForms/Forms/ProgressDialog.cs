using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace ShomreiTorah.WinForms.Forms {
	///<summary>A FadingPopup that displays the progress of an operation.</summary>
	public partial class ProgressDialog : FadingPopup {
		readonly int progressBarWidthWithCancel;
		///<summary>Creates a new ProgressDialog form.</summary>
		public ProgressDialog() {
			InitializeComponent();
			progressBarWidthWithCancel = progressBar.Width;
			Maximum = -1;
		}

		#region Windows 7
		///<summary>Rasiases the Shown event.</summary>
		protected override void OnShown(EventArgs e) {
			base.OnShown(e);
			UpdateTaskbar();
		}
		///<summary>Rasiases the Closed event.</summary>
		protected override void OnClosed(EventArgs e) {
			base.OnClosed(e);
			if (TaskbarManager.IsPlatformSupported)
				TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress, Handle);
		}
		void UpdateTaskbar() {
			if (!TaskbarManager.IsPlatformSupported) return;
			if (!Visible)
				TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress, Handle);
			else if (Maximum < 0)
				TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Indeterminate, Handle);
			else {
				TaskbarManager.Instance.SetProgressValue(Value, Maximum, Handle);
				TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal, Handle);
			}
		}
		#endregion

		///<summary>Gets or sets the text displayed above the progress bar.</summary>
		public string Caption {
			get { return label.Text; }
			set { label.Text = value; }
		}
		///<summary>Gets or sets whether the caption should be displayed as HTML.</summary>
		public bool IsCaptionHtml {
			get { return label.AllowHtmlString; }
			set { label.AllowHtmlString = value; }
		}

		///<summary>Gets or sets the value of the progress bar.</summary>
		public int Value {
			get { return progressBar.Position; }
			set {
				progressBar.Position = value;
				UpdateTaskbar();
			}
		}
		///<summary>Gets or sets the progress bar's maximum value, or -1 to display a marquee.</summary>
		public int Maximum {
			get { return marqueeBar.Visible ? -1 : progressBar.Properties.Maximum; }
			set {
				if (value < 0) {
					progressBar.Hide();
					marqueeBar.Show();
				} else {
					progressBar.Show();
					marqueeBar.Hide();

					progressBar.Properties.Maximum = value;
				}
				UpdateTaskbar();
			}
		}

		///<summary>Gets the state of the Cancel button.</summary>
		public ButtonMode CancelState {
			get {
				if (!cancel.Visible)
					return ButtonMode.Hidden;
				else if (!cancel.Enabled)
					return ButtonMode.Disabled;
				else
					return ButtonMode.Normal;
			}
			set {
				switch (value) {
					case ButtonMode.Normal:
						progressBar.Width = marqueeBar.Width = progressBarWidthWithCancel;
						cancel.Visible = cancel.Enabled = true;
						break;
					case ButtonMode.Disabled:
						progressBar.Width = marqueeBar.Width = progressBarWidthWithCancel;
						cancel.Enabled = false;
						cancel.Visible = true;
						break;
					case ButtonMode.Hidden:
						progressBar.Width = marqueeBar.Width = cancel.Right - progressBar.Left;
						cancel.Visible = false;
						break;
				}
			}
		}

		private void cancel_Click(object sender, EventArgs e) { OnCancelClicked(); }

		///<summary>Occurs when the user clicks Cancel.</summary>
		public event EventHandler CancelClicked;
		///<summary>Raises the CancelClicked event.</summary>
		protected virtual void OnCancelClicked() { OnCancelClicked(EventArgs.Empty); }
		///<summary>Raises the CancelClicked event.</summary>
		///<param name="e">An EventArgs object that provides the event data.</param>
		protected virtual void OnCancelClicked(EventArgs e) {
			if (CancelClicked != null)
				CancelClicked(this, e);
		}
	}

	///<summary>Specifies the state of a button.</summary>
	public enum ButtonMode {
		///<summary>The button is visible and enabled.</summary>
		Normal,
		///<summary>The button is disabled.</summary>
		Disabled,
		///<summary>The button is hidden.</summary>
		Hidden
	}
}
