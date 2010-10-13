using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace ShomreiTorah.Data.UI.DisplaySettings {
	///<summary>Verifies that code is running on a single UI thread.</summary>
	internal static class UIThread {
		static long? threadId;

		///<summary>In DEBUG builds, ensures that the code is running on the UI thread.</summary>
		[Conditional("DEBUG")]
		public static void Verify() {
			if (!threadId.HasValue)
				threadId = Thread.CurrentThread.ManagedThreadId;
			else if (threadId != Thread.CurrentThread.ManagedThreadId)
				throw new InvalidOperationException("This code can only be used from the UI thread.");
		}
	}
}
