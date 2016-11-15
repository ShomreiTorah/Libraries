using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ShomreiTorah.Common;

namespace ShomreiTorah.WinForms.Forms {
	///<summary>Executes operations with progress.</summary>
	public static class ProgressWorker {
		///<summary>Executes an operation and displays its progress.</summary>
		///<param name="method">The method to execute on the background thread.</param>
		///<param name="cancellable">Indicates the visibility of the cancel button.</param>
		///<returns>False if the cancel button was clicked.</returns>
		public static bool Execute(Action<IProgressReporter> method, bool cancellable) { return Execute(null, method, cancellable); }
		///<summary>Executes an operation and displays its progress.</summary>
		///<param name="parent">The form that will own the progress display.  This parameter can be null.</param>
		///<param name="method">The method to execute on the background thread.</param>
		///<param name="cancellable">Indicates the visibility of the cancel button.</param>
		///<returns>False if the cancel button was clicked.</returns>
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Cross-thread exception")]
		public static bool Execute(IWin32Window parent, Action<IProgressReporter> method, bool cancellable) {
			if (method == null) throw new ArgumentNullException("method");

			Exception exception = null;

			bool canceled = false;
			using (var dialog = new ProgressForm() { CanCancel = cancellable }) {

				dialog.FadeIn();
				ThreadPool.QueueUserWorkItem(delegate {
					try {
						method(dialog);
					} catch (Exception ex) {
						exception = ex;
					} finally {
						dialog.Finished = true;
						dialog.FadeOut();
					}
					canceled = dialog.WasCanceled;
				});
				if (!dialog.IsDisposed && !dialog.Finished)
					dialog.ShowDialog(parent);
			}
			if (exception != null)
				throw new TargetInvocationException(exception);
			return !canceled;
		}
		///<summary>Executes an asynchronous operation and displays its progress.</summary>
		///<param name="method">The method to execute.  This should not do any immediate blocking work.</param>
		public static Task ExecuteAsync(Func<IProgressReporter, CancellationToken, Task> method) => ExecuteAsync(null, method);
		///<summary>Executes a cancellable asynchronous operation and displays its progress.</summary>
		///<param name="parent">The form that will own the progress display.  This parameter can be null.</param>
		///<param name="method">The method to execute.  This should not do any immediate blocking work.</param>
		///<returns>False if the cancel button was clicked.</returns>
		public static async Task ExecuteAsync(IWin32Window parent, Func<IProgressReporter, CancellationToken, Task> method) {
			if (method == null) throw new ArgumentNullException("method");

			using (var dialog = new ProgressForm() { CanCancel = true }) {
				dialog.FadeIn();
				try {
					var task = method(dialog, dialog.CancellationSource.Token);
					await Task.WhenAny(task, Task.Delay(5));    // If it finishes quickly, don't bother showing the dialog.
					if (!task.IsCompleted)
						dialog.ShowDialog(parent);
					await task;
				} finally {
					dialog.Finished = true;
					dialog.FadeOut();
				}
				if (dialog.WasCanceled)
					throw new TaskCanceledException();
			}
		}
		class ProgressForm : ProgressDialog, IProgressReporter {
			public CancellationTokenSource CancellationSource { get; } = new CancellationTokenSource();
			public bool Finished { get; set; }

			protected override void OnFadedIn() {
				base.OnFadedIn();
				if (Finished) FadeOut();
			}

			public new string Caption {
				get { return base.Caption; }
				set { MyInvoke(() => base.Caption = value); }
			}

			//TODO: Scale longs to int ranges; maintain private actual values
			public new long Maximum {
				get { return base.Maximum; }
				set { MyInvoke(() => base.Maximum = (int)value); }
			}
			public long Progress {
				get { return base.Value; }
				set { MyInvoke(() => base.Value = (int)value); }
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
				CancellationSource.Cancel();
			}
			public bool WasCanceled { get; private set; }


			public bool CanCancel {
				get { return CancelState != ButtonMode.Hidden; }
				set {
					MyInvoke(() => {
						if (value)
							CancelState = WasCanceled ? ButtonMode.Disabled : ButtonMode.Normal;
						else
							CancelState = ButtonMode.Hidden;
					});
				}
			}
		}
	}
}
