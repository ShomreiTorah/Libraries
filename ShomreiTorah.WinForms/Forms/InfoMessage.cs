using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ShomreiTorah.Forms {
	///<summary>A FadingPopup that displays a notification.</summary>
	public sealed partial class InfoMessage : FadingPopup {
		///<summary>The amount of time that an InfoMessage is visible for by default.</summary>
		public static readonly TimeSpan DefaultDuration = TimeSpan.FromSeconds(5);

		///<summary>Displays a notification.</summary>
		///<param name="text">The text to display.</param>
		public static void Show(string text) { Show(text, false, DefaultDuration); }
		///<summary>Displays a notification.</summary>
		///<param name="text">The text to display.</param>
		///<param name="isHtml">Whether the text should be displayed as HTML.</param>
		public static void Show(string text, bool isHtml) { Show(text, isHtml, DefaultDuration); }
		///<summary>Displays a notification.</summary>
		///<param name="text">The text to display.</param>
		///<param name="duration">The length of time to display the notification.</param>
		public static void Show(string text, TimeSpan duration) { Show(text, false, duration); }
		///<summary>Displays a notification.</summary>
		///<param name="text">The text to display.</param>
		///<param name="isHtml">Whether the text should be displayed as HTML.</param>
		///<param name="duration">The length of time to display the notification.</param>
		public static void Show(string text, bool isHtml, TimeSpan duration) { Show(null, text, isHtml, duration); }

		///<summary>Displays a notification.</summary>
		///<param name="owner">The form that will own the notification.</param>
		///<param name="text">The text to display.</param>
		public static void Show(IWin32Window owner, string text) { Show(owner, text, false, DefaultDuration); }
		///<summary>Displays a notification.</summary>
		///<param name="owner">The form that will own the notification.</param>
		///<param name="text">The text to display.</param>
		///<param name="isHtml">Whether the text should be displayed as HTML.</param>
		public static void Show(IWin32Window owner, string text, bool isHtml) { Show(owner, text, isHtml, DefaultDuration); }
		///<summary>Displays a notification.</summary>
		///<param name="owner">The form that will own the notification.</param>
		///<param name="text">The text to display.</param>
		///<param name="duration">The length of time to display the notification.</param>
		public static void Show(IWin32Window owner, string text, TimeSpan duration) { Show(owner, text, false, duration); }
		///<summary>Displays a notification.</summary>
		///<param name="owner">The form that will own the notification.</param>
		///<param name="text">The text to display.</param>
		///<param name="isHtml">Whether the text should be displayed as HTML.</param>
		///<param name="duration">The length of time to display the notification.</param>
		public static void Show(IWin32Window owner, string text, bool isHtml, TimeSpan duration) {
			var form = new InfoMessage { duration = duration };
			form.label.Text = text;
			form.label.AllowHtmlString = isHtml;

			form.Show(owner);
			form.FadeIn();
		}


		private InfoMessage() { InitializeComponent(); }

		TimeSpan duration;
		DateTime visibleEnd;

		///<summary>Called after the fade-in is finished.</summary>
		protected override void OnFadedIn() {
			Phase = Phase.Visible;
			visibleEnd = DateTime.Now + duration;
		}
		///<summary>Performs the animation.</summary>
		protected override void OnTick() {
			base.OnTick();
			if (Phase == Phase.Visible && DateTime.Now >= visibleEnd && !Bounds.Contains(MousePosition))
				FadeOut();
		}

		private void label_DoubleClick(object sender, EventArgs e) {
			FadeOut();
		}

		private void label_MouseLeave(object sender, EventArgs e) {
			if (Phase == Phase.Visible && DateTime.Now >= visibleEnd && !Bounds.Contains(MousePosition))
				FadeOut();
		}

		private void label_MouseEnter(object sender, EventArgs e) {
			if (Phase == Phase.FadingOut) {
				StopTimer();
				Opacity = 1;
				Phase = Phase.Visible;
			}
		}
		///<summary>Gets the duration of the fade out animation in seconds.</summary>
		protected override double FadeoutDuration { get { return 1.3; } }
	}
}
