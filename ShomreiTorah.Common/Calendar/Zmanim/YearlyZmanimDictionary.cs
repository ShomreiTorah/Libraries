using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace ShomreiTorah.Common.Calendar.Zmanim {
	///<summary>An efficient collection of Zmanim for a single year.</summary>
	public class YearlyZmanimDictionary : IDictionary<DateTime, ZmanimInfo> {
		///<summary>Creates a YearlyZmanimDictionary.</summary>
		///<param name="year">The English year of the cache.</param>
		///<param name="values">The Zmanim instances for the year.  There must be one instance for each day of the year, sorted.</param>
		public YearlyZmanimDictionary(int year, IList<ZmanimInfo> values) {
			if (values == null) throw new ArgumentNullException("values");
			if (values.Count != (DateTime.IsLeapYear(year) ? 366 : 365))
				throw new ArgumentException("values.Count must equal the number of days in the year.", "values");
			if (year != values[0].Date.Year) throw new ArgumentException("values must describe year", "values");
			if (values[0].Date.DayOfYear != 1) throw new ArgumentException("values must describe year", "values");

			Values = new ReadOnlyCollection<ZmanimInfo>(values);
			Year = year;
		}


		///<summary>Gets the year that this instance contains.</summary>
		public int Year { get; private set; }
		///<summary>Gets the values of the collection.</summary>
		public ReadOnlyCollection<ZmanimInfo> Values { get; private set; }
		ICollection<ZmanimInfo> IDictionary<DateTime, ZmanimInfo>.Values { get { return Values; } }

		#region Not supported
		///<summary>Not supported.</summary>
		public void Add(DateTime key, ZmanimInfo value) { throw new NotSupportedException(); }
		///<summary>Not supported.</summary>
		public void Add(KeyValuePair<DateTime, ZmanimInfo> item) { throw new NotSupportedException(); }
		///<summary>Not supported.</summary>
		public void Clear() { throw new NotSupportedException(); }
		///<summary>Not supported.</summary>
		public bool Remove(KeyValuePair<DateTime, ZmanimInfo> item) { throw new NotSupportedException(); }
		///<summary>Not supported.</summary>
		public bool Remove(DateTime key) { throw new NotSupportedException(); }
		#endregion

		///<summary>Checks whether this collection contains the given date.</summary>
		public bool ContainsKey(DateTime key) { return key.Year == Year; }
		///<summary>Checks whether this collection contains the given date.</summary>
		public bool Contains(KeyValuePair<DateTime, ZmanimInfo> item) { return ContainsKey(item.Key) && this[item.Key] == item.Value; }
		///<summary>Gets the dates contained in this collection.</summary>
		public ICollection<DateTime> Keys { get { return new KeyCollection(Year); } }

		class KeyCollection : ICollection<DateTime> {
			int year;
			public KeyCollection(int year) { this.year = year; }

			public void Add(DateTime item) { throw new NotSupportedException(); }
			public bool Remove(DateTime item) { throw new NotSupportedException(); }
			public void Clear() { throw new NotSupportedException(); }

			public bool Contains(DateTime item) { return item.Year == year; }
			public void CopyTo(DateTime[] array, int arrayIndex) {
				if (array == null) throw new ArgumentNullException("array");

				foreach (var date in this)
					array[arrayIndex + date.DayOfYear - 1] = date;
			}

			public int Count { get { return DateTime.IsLeapYear(year) ? 366 : 365; } }
			public bool IsReadOnly { get { return true; } }
			public IEnumerator<DateTime> GetEnumerator() { var start = new DateTime(year, 1, 1); return Enumerable.Range(0, Count).Select(d => start.AddDays(d)).GetEnumerator(); }
			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
		}

		///<summary>Gets the value associated with the specified key.</summary>
		///<param name="key">The date to look for.</param>
		///<param name="value">A reference to put the value in.</param>
		///<returns>True if the collection contained the date.</returns>
		public bool TryGetValue(DateTime key, out ZmanimInfo value) {
			if (ContainsKey(key)) {
				value = this[key];
				return true;
			} else {
				value = null;
				return false;
			}
		}


		///<summary>Gets the Zmanim instance for a given date.</summary>
		public ZmanimInfo this[DateTime key] {
			get {
				if (key.Year != Year) throw new ArgumentOutOfRangeException("key");
				return Values[key.DayOfYear - 1];
			}
			set { throw new NotSupportedException(); }
		}

		///<summary>Copies the key-value pairs in the collection to an array.</summary>
		public void CopyTo(KeyValuePair<DateTime, ZmanimInfo>[] array, int arrayIndex) {
			if (array == null) throw new ArgumentNullException("array");
		
			foreach (var kvp in this)
				array[arrayIndex + kvp.Key.DayOfYear - 1] = kvp;
		}

		///<summary>Gets the number of days in the year.</summary>
		public int Count { get { return Values.Count; } }

		///<summary>Returns true</summary>
		public bool IsReadOnly { get { return true; } }


		///<summary>Creates an enumerator for this collections.</summary>
		public IEnumerator<KeyValuePair<DateTime, ZmanimInfo>> GetEnumerator() {
			for (int d = 0; d < Count; d++)
				yield return new KeyValuePair<DateTime, ZmanimInfo>(Values[d].Date, Values[d]);
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}
}
