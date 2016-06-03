using System;
using System.Threading;

namespace ShomreiTorah.Common {
	///<summary>Runs delegates in sequence, preventing re-entrancy issues.  This class is not thread-safe.</summary>
	public class Sequentializer {
		///<summary>True if any delegate (queued or otherwise) is being executed.</summary>
		bool isRunning;

		///<summary>
		/// The delegates, if any, that have been enqueued while a delegate was already running.
		/// Will only be non-null if isRunning is true.
		///</summary>
		Action queue;

		///<summary>Executes a delegate, or enqueues it to run after an already-executing delegate.</summary>
		public void Execute(Action a) {
			if (isRunning) {
				queue += a;
				return;
			}

			try {
				isRunning = true;
				queue = a;

				Action next;
				// Run the delegate, then keep checking in case another delegate was enqueued by it.
				while (null != (next = Interlocked.Exchange(ref queue, null)))
					next();
			} finally {
				isRunning = false;
			}
		}
	}
}