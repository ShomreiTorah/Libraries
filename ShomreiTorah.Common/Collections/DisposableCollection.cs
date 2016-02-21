using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShomreiTorah.Common.Collections {
	///<summary>A read-only collection which disposes its elements.</summary>
	public sealed class DisposableCollection<T> : ReadOnlyCollection<T>, IDisposable where T : IDisposable {
		///<summary>Creates a DisposableCollection containing the items in a sequence.</summary>
		public DisposableCollection(IEnumerable<T> items) : base(items.ToList()) { }

		///<summary>Disposes every item in the collection.</summary>
		public void Dispose() {
			foreach (var item in this)
				item.Dispose();
		}
	}
}
