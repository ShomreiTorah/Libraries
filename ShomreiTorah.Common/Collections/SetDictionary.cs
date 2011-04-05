using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShomreiTorah.Common.Collections {
	///<summary>A dictionary mapping keys to unordered sets of items.</summary>
	public class SetDictionary<TKey, TItem> {
		private readonly Dictionary<TKey, HashSet<TItem>> dict;
		private readonly IEqualityComparer<TItem> itemComparer;

		///<summary>Creates a SetDictionary using standard comparers.</summary>
		public SetDictionary() : this(null, null) { }
		///<summary>Creates a SetDictionary using the specified comparers for the keys and items.</summary>
		public SetDictionary(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TItem> itemComparer) {
			dict = new Dictionary<TKey, HashSet<TItem>>(keyComparer);
			this.itemComparer = itemComparer ?? EqualityComparer<TItem>.Default;
		}

		///<summary>Adds an item to the set associated with a key.</summary>
		///<returns>False if the item was already in the set.</returns>
		public bool Add(TKey key, TItem item) {
			return this[key].Add(item);
		}
		///<summary>Merges a set of items with the set associated with a key.</summary>
		public void Add(TKey key, IEnumerable<TItem> items) {
			this[key].UnionWith(items);
		}

		///<summary>Removes an item from the set associated with a key.</summary>
		///<returns>False if the item was already not in the set.</returns>
		public bool Remove(TKey key, TItem item) {
			HashSet<TItem> set;
			if (dict.TryGetValue(key, out set))
				return set.Remove(item);
			return false;	//The entire set is not there.
		}

		///<summary>Clears all keys and items from the dictionary.</summary>
		public void Clear() {
			dict.Clear();
		}

		///<summary>Clears all items from the given key.</summary>
		public void ClearItems(TKey key) {
			dict.Remove(key);
		}

		///<summary>Gets the set associated with a key.</summary>
		public HashSet<TItem> this[TKey key] {
			get {
				HashSet<TItem> retVal;
				if (!dict.TryGetValue(key, out retVal))
					dict.Add(key, (retVal = new HashSet<TItem>(itemComparer)));
				return retVal;
			}
		}

		///<summary>Gets all of the keys that exist in the dictionary.  Some of these keys may not have values.</summary>
		public Dictionary<TKey, HashSet<TItem>>.KeyCollection Keys {
			get { return dict.Keys; }
		}
		///<summary>Gets all of the items in the sets in the dictionary.</summary>
		public IEnumerable<TItem> AllItems {
			get { return dict.Values.SelectMany(s => s); }
		}
	}
}
