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
		int Progress { get; set; }
		///<summary>Gets or sets the maximum progress of the operation.</summary>
		int Maximum { get; set; }
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
		int IProgressReporter.Progress { get; set; }
		int IProgressReporter.Maximum { get; set; }
		bool IProgressReporter.WasCanceled { get { return false; } }
		bool IProgressReporter.CanCancel { get; set; }
	}
}
