using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections;

namespace ShomreiTorah.Common.Collections {
	///<summary>
	/// Contains all of the properties of a class that 
	/// are used to provide value semantics.
	///</summary>
	///<remarks>
	/// You can create a static readonly ValueComparer for your class,
	/// then call into it from Equals, GetHashCode, and CompareTo.
	///</remarks>
	public class ValueComparer<T> : IComparer<T>, IEqualityComparer<T> {
		///<summary>Creates a ValueComparer that compares a set of properties.</summary>
		public ValueComparer(params Func<T, object>[] props) {
			Properties = new ReadOnlyCollection<Func<T, object>>(props);
		}

		///<summary>Gets the properties used by the comparer.</summary>
		public ReadOnlyCollection<Func<T, object>> Properties { get; private set; }

		///<summary>Checks whether two values are equivalent.</summary>
		public bool Equals(T x, T y) {
			if (default(T) == null && ReferenceEquals(x, y)) return true;

			if (x == null || y == null) return false;
			//Object.Equals handles strings and nulls correctly
			return Properties.All(f => Equals(f(x), f(y)));
		}

		///<summary>Gets a hash value for an object.</summary>
		public int GetHashCode(T obj) {
			if (obj == null) return -42;
			//http://stackoverflow.com/questions/263400/263416#263416
			unchecked {
				int hash = 17;
				foreach (var prop in Properties) {
					object value = prop(obj);
					if (value == null)
						hash = hash * 23 - 1;
					else
						hash = hash * 23 + value.GetHashCode();
				}
				return hash;
			}
		}

		///<summary>Checks the ordering of two values.</summary>
		public int Compare(T x, T y) {
			foreach (var prop in Properties) {
				//The properties can be any type including null.
				var comp = Comparer.DefaultInvariant.Compare(prop(x), prop(y));
				if (comp != 0)
					return comp;
			}
			return 0;
		}
	}
}
