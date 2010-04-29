using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace ShomreiTorah.Singularity.DataBinding {
	///<summary>A collection that wraps another collection and optionally adds a phantom item.</summary>
	///<remarks>Tightly coupled with RowCollectionBinder.</remarks>
	class PhantomCollection<T> : IList<T> where T : class {
		protected IList<T> Inner { get; private set; }
		public PhantomCollection(IList<T> inner) { Inner = inner; }

		T phantomItem;

		public bool HasPhantomItem { get { return phantomItem != null; } }

		public void Insert(int index, T item) { throw new NotSupportedException(); }

		public void AddPhantom(T item) {
			if (phantomItem != null) throw new InvalidOperationException("Cannot add two phantom items");
			phantomItem = item;
		}
		public void CommitPhantom() {
			if (phantomItem == null) throw new InvalidOperationException("There is no phantom item");
			var item = phantomItem;
			phantomItem = null;
			Add(item);
		}
		public void ResetPhantom() {
			if (phantomItem == null) throw new InvalidOperationException("There is no phantom item");
			phantomItem = null;
		}

		public virtual void Add(T item) { Inner.Add(item); }
		protected virtual bool RemoveInner(T item) { return Inner.Remove(item); }

		public void Clear() {
			for (int i = Count - 1; i >= 0; i--)
				RemoveAt(i);
		}

		public void RemoveAt(int index) { Remove(this[index]); }
		public bool Remove(T item) {
			if (HasPhantomItem && item == phantomItem) {
				phantomItem = null;
				return true;
			}

			return RemoveInner(item);
		}

		public virtual bool IsReadOnly { get { return Inner.IsReadOnly; } }

		#region Read passthrough (handles PhantomItem)
		public T this[int index] {
			get {
				if (HasPhantomItem && index == Inner.Count)
					return phantomItem;
				return Inner[index];
			}
			set { throw new NotSupportedException(); }
		}
		public int IndexOf(T item) {
			if (HasPhantomItem && item == phantomItem)
				return Inner.Count;
			return Inner.IndexOf(item);
		}
		public bool Contains(T item) {
			if (HasPhantomItem && item == phantomItem)
				return true;
			return Inner.Contains(item);
		}
		public void CopyTo(T[] array, int arrayIndex) {
			if (array == null) throw new ArgumentNullException("array");

			Inner.CopyTo(array, arrayIndex);
			if (HasPhantomItem)
				array[arrayIndex + Inner.Count] = phantomItem;
		}
		public int Count { get { return Inner.Count + (HasPhantomItem ? 1 : 0); } }

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
		public IEnumerator<T> GetEnumerator() {
			if (!HasPhantomItem)
				return Inner.GetEnumerator();
			else
				return GetPhantomEnumerator();
		}
		IEnumerator<T> GetPhantomEnumerator() {
			foreach (var item in Inner) {
				yield return item;
			}
			yield return phantomItem;
		}
		#endregion
	}
}
