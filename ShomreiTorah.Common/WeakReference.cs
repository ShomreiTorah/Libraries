using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace ShomreiTorah.Common {
	///<summary>Maintains a weak reference to an object, while allowing the object to be garbage-collected.</summary>
	public class WeakReference<T> where T : class {
		readonly WeakReference wr;
		///<summary>Creates a weak reference to the specified object.</summary>
		public WeakReference(T target) {
			if (target == null) throw new ArgumentNullException("target");

			wr = new WeakReference(target);
		}

		///<summary>Gets the object referenced.</summary>
		public T Target { get { return wr.Target as T; } }
		///<summary>Indicates whether the object has been garbage-collected.</summary>
		public bool IsAlive { get { return wr.IsAlive; } }

		///<summary>Gets the object referenced by a typed WeakReference.</summary>
		[SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Justification = "Target proeprty")]
		public static implicit operator T(WeakReference<T> weakRef) { return weakRef.Target; }
	}
}
