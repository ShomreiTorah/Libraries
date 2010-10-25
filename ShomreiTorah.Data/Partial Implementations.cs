using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace ShomreiTorah.Data {
	partial class Person {
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
	}

	#region Comparable
	[SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Justification = "Comparable for grids only")]
	partial class Person : IComparable<Person>, IComparable {
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
	#endregion

	#region Empty deposit cleanup
	partial class Deposit {
		///<summary>Called before the row is removed from its table.</summary>
		protected override void OnRemoving() {
			foreach (var payment in Payments.ToArray()) {	//The loop will modify the collection
				payment.Deposit = null;
			}
		}
	}
	partial class Payment {
		///<summary>Called before the row is removed from its table.</summary>
		protected override void OnRemoving() {
			if (Deposit != null && Deposit.Payments.Count == 1)
				Deposit.RemoveRow();
		}
		partial void OnDepositChanged(Deposit oldValue, Deposit newValue) {
			if (oldValue.Payments.Count == 0)
				oldValue.RemoveRow();
		}
	}
	#endregion

	partial class EmailAddress {
		partial void ValidateEmail(string newValue, Action<string> error) {
			try {
				new MailAddress(newValue).ToString();
			} catch (FormatException) {
				error("Invalid email address");
				return;
			}
		}
		partial void OnEmailChanged(string oldValue, string newValue) {
			if (newValue == null & Table == null) return;
			Email = new MailAddress(newValue).Address;
		}

		///<summary>Gets a MailAddress object for this email address.</summary>
		public MailAddress MailAddress { get { return new MailAddress(Email, Name); } }
	}
}