using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShomreiTorah.Singularity {
	///<summary>A dictionary that supports null keys.</summary>
	class NullableDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : class {
		readonly IDictionary<object, TValue> inner = new Dictionary<object, TValue>();

		///<summary>Used as a key instead of null.</summary>
		static readonly object Null = new object();

		public TValue this[TKey key] {
			get => inner[key ?? Null];
			set => inner[key ?? Null] = value;
		}

		public ICollection<TKey> Keys => throw new NotSupportedException();

		public ICollection<TValue> Values => inner.Values;

		public int Count => inner.Count;

		public bool IsReadOnly => inner.IsReadOnly;

		public void Add(TKey key, TValue value) => inner.Add(key ?? Null, value);


		static KeyValuePair<object, TValue> ToInnerPair(KeyValuePair<TKey, TValue> pair) {
			return new KeyValuePair<object, TValue>(pair.Key ?? Null, pair.Value);

		}
		public void Add(KeyValuePair<TKey, TValue> item) => inner.Add(ToInnerPair(item));

		public void Clear() => inner.Clear();

		public bool Contains(KeyValuePair<TKey, TValue> item) => inner.Contains(ToInnerPair(item));

		public bool ContainsKey(TKey key) => inner.ContainsKey(key ?? Null);

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
			throw new NotSupportedException();
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
			return inner
				.Select(item => new KeyValuePair<TKey, TValue>(item.Key == Null ? null : (TKey)item.Key, item.Value))
				.GetEnumerator();
		}

		public bool Remove(TKey key) {
			return inner.Remove(key ?? null);
		}

		public bool Remove(KeyValuePair<TKey, TValue> item) => inner.Remove(ToInnerPair(item));

		public bool TryGetValue(TKey key, out TValue value) {
			return inner.TryGetValue(key ?? Null, out value);
		}
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
