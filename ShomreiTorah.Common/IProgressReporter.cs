using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShomreiTorah.Common {
	///<summary>A form that displays the progress of an operation.</summary>
	public interface IProgressReporter {
		///<summary>Gets or sets the caption to display above the progress bar.</summary>
		string Caption { get; set; }
		///<summary>Gets or sets the current progress of the operation.</summary>
		long Progress { get; set; }
		///<summary>Gets or sets the maximum progress of the operation.</summary>
		long Maximum { get; set; }
		///<summary>Gets whether the cancel button has been clicked.</summary>
		bool WasCanceled { get; }
		///<summary>Gets or sets whether the operation can be canceled.</summary>
		bool CanCancel { get; set; }
	}
	///<summary>An IProgressReporter that doesn't do anything.</summary>
	///<remarks>This is used in methods that optionally take an IProgressReporter, like this:
	///<code>progress = progress ?? new EmptyProgressReporter();</code></remarks>
	public sealed class EmptyProgressReporter : IProgressReporter {
		///<summary>Creates a new EmptyProgressReporter instance.</summary>
		public EmptyProgressReporter() { ((IProgressReporter)this).Maximum = -1; }

		string IProgressReporter.Caption { get; set; }
		long IProgressReporter.Progress { get; set; }
		long IProgressReporter.Maximum { get; set; }
		bool IProgressReporter.WasCanceled { get { return false; } }
		bool IProgressReporter.CanCancel { get; set; }
	}
	///<summary>Contains extension methods for progress reporters.</summary>
	public static class ProgressReporterExtensions {
		class NonScalingOperationReporter : IProgressReporter {
			public NonScalingOperationReporter(IProgressReporter parent) {
				this.parent = parent;
				baseProgress = parent.Progress;
			}

			readonly IProgressReporter parent;
			readonly long baseProgress;

			public string Caption {
				get { return parent.Caption; }
				set { parent.Caption = value; }
			}

			public long Progress {
				get { return parent.Progress - baseProgress; }
				set { parent.Progress = value + baseProgress; }
			}

			public long Maximum { get; set; }	//The caller is expected to have set the parent reporter's maximum in advance.

			public bool WasCanceled { get { return parent.WasCanceled; } }

			public bool CanCancel {
				get { return parent.CanCancel; }
				set { parent.CanCancel = value; }
			}
		}


		///<summary>Returns an IProgressReporter that adds progress to an existing reporter without affecting the maximum.</summary>
		///<remarks>The new reporter will add its progress directly to the existing reporter without scaling for the maximum; the
		///maximum of the original reporter is expected to equal the sum of the maximums of the child operations.</remarks>
		public static IProgressReporter ChildOperation(this IProgressReporter reporter) {
			if (reporter == null) return new EmptyProgressReporter();
			return new NonScalingOperationReporter(reporter);
		}
	}
}
