using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace ShomreiTorah.Data {
	[SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Justification = "Comparable for grids only")]
	partial class Person : IComparable<Person>, IComparable {
		#region String properties
		///<summary>Gets the family's full name, including both spouses.</summary>
		public string VeryFullName {
			get {
				StringBuilder retVal = new StringBuilder((HisName ?? "").Length + (HerName ?? "").Length + (LastName ?? "").Length + 6);

				if (!String.IsNullOrEmpty(HisName)) {
					retVal.Append(HisName);
					if (!String.IsNullOrEmpty(HerName))
						retVal.Append(" and ");
				}

				if (!String.IsNullOrEmpty(HerName))
					retVal.Append(HerName);

				if (retVal.Length != 0 && !String.IsNullOrEmpty(LastName)) retVal.Append(" ");

				if (!String.IsNullOrEmpty(LastName)) retVal.Append(LastName);
				return retVal.ToString();
			}
		}
		///<summary>Gets the person's full mailing address.</summary>
		public string MailingAddress {
			get {
				var retVal = new StringBuilder();
				if (!String.IsNullOrEmpty(FullName)) retVal.AppendLine(FullName);
				if (!String.IsNullOrEmpty(Address)) retVal.AppendLine(Address);
				if (!String.IsNullOrEmpty(City)
				 && !String.IsNullOrEmpty(State)) {
					retVal.Append(City).Append(", ").Append(State);
					if (!String.IsNullOrEmpty(Zip)) retVal.Append(" ").Append(Zip);
				}
				return retVal.ToString();
			}
		}
		#endregion

		partial void OnFullNameChanged(string oldValue, string newValue) {
			if (Table == null || Table.Context == null)
				return;
			var emails = Table.Context.Table<EmailAddress>();
			if (emails != null) return;
			foreach (var email in emails.Rows) {
				if (String.IsNullOrWhiteSpace(email.Name) || email.Name.Trim() == oldValue)
					email.Name = newValue;
			}
		}

		int IComparable.CompareTo(object obj) { return CompareTo(obj as Person); }
		///<summary>Compares this person to another Person instance.</summary>
		public int CompareTo(Person other) {
			if (other == null) return 1;
			if (this == other) return 0;

			if (LastName == other.LastName)
				return String.CompareOrdinal(HisName, other.HisName);

			return String.CompareOrdinal(LastName, other.LastName);
		}

	}
	[SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Justification = "Comparable for grids only")]
	partial class Deposit : IComparable<Deposit>, IComparable {
		int IComparable.CompareTo(object obj) { return CompareTo(obj as Deposit); }
		///<summary>Compares this person to another Person instance.</summary>
		public int CompareTo(Deposit other) {
			if (other == null) return 1;
			if (this == other) return 0;

			var result = Date.CompareTo(other.Date);
			return result == 0 ? Number.CompareTo(other.Number) : result;
		}
	}
}