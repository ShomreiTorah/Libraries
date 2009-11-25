using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.Utils.Drawing;
using DevExpress.Utils.ViewInfo;
using DevExpress.XtraEditors;

namespace ShomreiTorah.WinForms.Forms {
	///<summary>Specifies the phase of a FadingPopup.</summary>
	public enum Phase {
		///<summary>The popup is not shown.</summary>
		None,
		///<summary>The popup is fading in.</summary>
		FadingIn,
		///<summary>The popup is visible at 100% opacity.</summary>
		Visible,
		///<summary>The popup is fading out.</summary>
		FadingOut
	}
	///<summary>A popup form that fades in and out.</summary>
	public class FadingPopup : SuperTipForm {
		///<summary>Creates a new FadingPopup.</summary>
		[SuppressMessage("Microsoft.Mobility", "CA1601:DoNotUseTimersThatPreventPowerStateChanges", Justification = "Animation")]
		public FadingPopup() {
			base.TopMost = true;
			base.ControlBox = false;
			ShowInTaskbar = Application.OpenForms.Count == 0;

			timer = new Timer { Interval = 10 };
			timer.Tick += delegate { OnTick(); };
			Opacity = 0;
		}
		///<summary>Raises the Load event.</summary>
		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);
			if (DesignMode) ShowInTaskbar = true;
		}
		Timer timer;
		///<summary>Raises the Shown event.</summary>
		protected override void OnShown(EventArgs e) {
			base.OnShown(e);
			if (DesignMode) return;
			if (Owner != null)
				CenterToParent();
			else {
				Screen desktop = null;

				if (Owner != null)
					desktop = Screen.FromControl(Owner);
				else
					desktop = Screen.FromPoint(Control.MousePosition);

				var width = desktop.WorkingArea.Width;
				if (desktop.Bounds.Width > 2 * desktop.Bounds.Height)
					width /= 2;
				Rectangle screenRect = desktop.WorkingArea;
				Location = new Point(
					Math.Max(screenRect.X, screenRect.X + (width - Width) / 2),
					Math.Max(screenRect.Y, screenRect.Y + (screenRect.Height - Height) / 2)
				);
			}
			FadeIn();
		}
		///<summary>Gets the current phase of the popup.</summary>
		public Phase Phase { get; protected set; }
		///<summary>Gets or sets whether the animation timer is running.</summary>
		protected bool TimerRunning { get { return timer.Enabled; } set { timer.Enabled = value; } }
		///<summary>Starts the timer.</summary>
		protected void StartTimer() { timer.Start(); }
		///<summary>Stops the timer.</summary>
		protected void StopTimer() { timer.Stop(); }

		///<summary>Begins the fade-in phase.</summary>
		public void FadeIn() {
			if (Opacity >= 1) return;
			Phase = Phase.FadingIn;
			StartTimer();
		}
		///<summary>Called after the fade-in is finished.</summary>
		protected virtual void OnFadedIn() {
			Phase = Phase.Visible;
			timer.Stop();
		}
		///<summary>Begins the fade-out phase, which culminates in closing the form.</summary>
		[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "FadeOut")]
		public void FadeOut() {
			if (InvokeRequired) {
				BeginInvoke(new Action(FadeOut));
				return;
			}
			if (Opacity <= 0) return;
			Phase = Phase.FadingOut;
			StartTimer();
		}


		///<summary>Called after the fade-out is finished.</summary>
		protected virtual void OnFadedOut() {
			Close();
			Phase = Phase.None;
			timer.Stop();
		}
		///<summary>Performs the animation.</summary>
		protected virtual void OnTick() {
			switch (Phase) {
				case Phase.FadingIn:
					Opacity += (timer.Interval / 1000.0) / FadeInDuration;
					if (Opacity >= 1) {
						OnFadedIn();
					}
					break;
				case Phase.FadingOut:
					Opacity -= (timer.Interval / 1000.0) / FadeoutDuration;
					if (Opacity <= 0)
						OnFadedOut();
					break;
			}
		}

		///<summary>Gets the duration of the fade in animation in seconds.</summary>
		protected virtual double FadeInDuration { get { return .5; } }
		///<summary>Gets the duration of the fade out animation in seconds.</summary>
		protected virtual double FadeoutDuration { get { return .5; } }

		///<summary>Releases the managed resources used by the FadingPopup.</summary>
		protected override void Dispose(bool disposing) {
			if (disposing) {
				if (timer != null) timer.Dispose();
			}
			base.Dispose(disposing);
		}
	}
	///<summary>An XtraForm that is painted as a SuperTip.</summary>
	public class SuperTipForm : XtraForm {
		ObjectPainter superTipPainter = new ToolTipContainerPainter();
		ToolTipContainerViewInfo superTipViewInfo;
		ToolTipContainerInfoArgs superTipInfoArgs;

		///<summary>Creates a new SuperTipForm.</summary>
		public SuperTipForm() {
			base.FormBorderStyle = FormBorderStyle.None;

			superTipViewInfo = new ToolTipContainerViewInfo(new SuperToolTip());
			superTipInfoArgs = new ToolTipContainerInfoArgs(null, superTipViewInfo);
		}
		///<summary>Raises the Paint event.</summary>
		protected override void OnPaint(PaintEventArgs e) {
			base.OnPaint(e);
			superTipInfoArgs.Bounds = ClientRectangle;
			superTipInfoArgs.Cache = new GraphicsCache(e);
			superTipPainter.DrawObject(superTipInfoArgs);
		}
		///<summary>Gets the border style of the form.</summary>
		[Browsable(false)]
		public new FormBorderStyle FormBorderStyle { get { return base.FormBorderStyle; } }

		///<summary>Processes Windows Messages.</summary>
		protected override void WndProc(ref Message m) {
			if (m.Msg == 0x0084) {//NCHITTEST
				m.Result = new IntPtr((int)HitTest.Caption);
			}
			if (m.Msg == 0x00A3) {//WM_NCLBUTTONDBLCLK 
				OnDoubleClick(EventArgs.Empty);
				return;
			}
			base.WndProc(ref m);
		}
	}
}
