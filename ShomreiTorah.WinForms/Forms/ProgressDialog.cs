using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Taskbar;
using System.Diagnostics;

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
		// Microsoft.WindowsAPICodePack.Shell can give strange loading errors.
		// Swallow these errors so that they don't block save.  Every function
		// that references types from that assembly can only be called through
		// this wrapper, to swallow load failures from the JITter.
		static void TaskBarAction(Action a) {
			try {
				a();
			} catch { }
		}

		IntPtr handleForTaskbar;

		///<summary>Raises the Shown event.</summary>
		protected override void OnShown(EventArgs e) {
			base.OnShown(e);
			TaskBarAction(UpdateTaskbar);
		}
		///<summary>Raises the Closed event.</summary>
		protected override void OnClosed(EventArgs e) {
			base.OnClosed(e);
			TaskBarAction(() => {
				if (TaskbarManager.IsPlatformSupported && handleForTaskbar != IntPtr.Zero)
					TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress, handleForTaskbar);
			});
		}
		void UpdateTaskbar() {
			if (!TaskbarManager.IsPlatformSupported) return;

			if (handleForTaskbar == IntPtr.Zero)
				handleForTaskbar = Process.GetCurrentProcess().MainWindowHandle;
			if (handleForTaskbar == IntPtr.Zero)
				handleForTaskbar = Handle;

			if (!Visible)
				TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress, handleForTaskbar);
			else if (Maximum < 0)
				TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Indeterminate, handleForTaskbar);
			else {
				TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal, handleForTaskbar);
				TaskbarManager.Instance.SetProgressValue(Value, Maximum, handleForTaskbar);
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
				TaskBarAction(UpdateTaskbar);
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
				TaskBarAction(UpdateTaskbar);
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
