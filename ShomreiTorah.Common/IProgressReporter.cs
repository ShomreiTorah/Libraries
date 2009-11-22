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
	}
}
