using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ShomreiTorah.Common;

namespace ShomreiTorah.WinForms.Forms {
	///<summary>Executes operations with progress.</summary>
	public static class ProgressWorker {
		///<summary>Executes an operation and displays its progress.</summary>
		///<param name="method">The method to execute on the background thread.</param>
		///<param name="cancellable">Indicates the visibilty of the cancel button.</param>
		///<returns>False if the cancel button was clicked.</returns>
		public static bool Execute(Action<IProgressReporter> method, bool cancellable) { return Execute(null, method, cancellable); }
		///<summary>Executes an operation and displays its progress.</summary>
		///<param name="parent">The form that will own the progress display.  This parameter can be null.</param>
		///<param name="method">The method to execute on the background thread.</param>
		///<param name="cancellable">Indicates the visibilty of the cancel button.</param>
		///<returns>False if the cancel button was clicked.</returns>
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Cross-thread exception")]
		public static bool Execute(IWin32Window parent, Action<IProgressReporter> method, bool cancellable) {
			if (method == null) throw new ArgumentNullException("method");

			Exception exception = null;

			bool cancelled = false;
			using (var dialog = new ProgressForm() { CancelState = cancellable ? ButtonMode.Normal : ButtonMode.Hidden }) {
				object syncer = new object();

				dialog.FadeIn();
				ThreadPool.QueueUserWorkItem(delegate {
					try {
						method(dialog);
					} catch (Exception ex) {
						exception = ex;
					} finally {
						lock (syncer) {
							dialog.Finished = true;
							dialog.FadeOut();
						}
					}
					lock (syncer) {
						if (!dialog.IsDisposed)
							dialog.ShowDialog(parent);
					}
					cancelled = dialog.WasCanceled;
				});
			}
			if (exception != null)
				throw new TargetInvocationException(exception);
			return !cancelled;
		}
		class ProgressForm : ProgressDialog, IProgressReporter {
			public bool Finished { get; set; }

			protected override void OnFadedIn() {
				base.OnFadedIn();
				if (Finished) FadeOut();
			}

			public new string Caption {
				get { return base.Caption; }
				set { MyInvoke(() => base.Caption = value); }
			}

			public new int Maximum {
				get { return base.Maximum; }
				set { MyInvoke(() => base.Maximum = value); }
			}
			public int Progress {
				get { return base.Value; }
				set { MyInvoke(() => base.Value = value); }
			}
			void MyInvoke(Action method) {
				if (IsHandleCreated && InvokeRequired)
					BeginInvoke(method);
				else
					method();
			}

			protected override void OnCancelClicked(EventArgs e) {
				base.OnCancelClicked(e);
				WasCanceled = true;
				CancelState = ButtonMode.Disabled;
			}
			public bool WasCanceled { get; private set; }
		}
	}
}
