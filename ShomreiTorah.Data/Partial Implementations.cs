using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using ShomreiTorah.Singularity;

namespace ShomreiTorah.Data {
	partial class Person {
		partial void OnFullNameChanged(string oldValue, string newValue) {
			if (Table == null || Table.Context == null || Table.IsLoadingData)
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

		#region Transaction properties
		///<summary>Gets all of the person's transactions.</summary>
		public IEnumerable<ITransaction> Transactions { get { return Pledges.Cast<ITransaction>().Concat(Payments); } }

		///<summary>Gets the accounts in which this person has outstanding balance.</summary>
		public IEnumerable<string> OpenAccounts {
			get {
				return from t in Transactions
					   group t by t.Account into a
					   where a.Sum(t => t.SignedAmount) != 0
					   select a.Key;
			}
		}

		///<summary>Gets the person's total outstanding balance as of the given date.</summary>
		public decimal GetBalance(DateTime until) {
			return Transactions.Where(p => p.Date < until).Sum(p => p.SignedAmount);
		}
		///<summary>Gets the person's current outstanding balance in the given account.</summary>
		public decimal GetBalance(string account) {
			return Transactions.Where(p => p.Account == account).Sum(p => p.SignedAmount);
		}
		///<summary>Gets the person's outstanding balance in the given account as of the given date.</summary>
		public decimal GetBalance(DateTime until, string account) {
			return Transactions.Where(p => p.Date < until && p.Account == account).Sum(p => p.SignedAmount);
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
			if (!Table.IsLoadingData && Deposit != null && Deposit.Payments.Count == 1)
				Deposit.RemoveRow();
		}
		partial void OnDepositChanged(Deposit oldValue, Deposit newValue) {
			if (Table != null && !Table.IsLoadingData
			 && oldValue != null && oldValue.Payments.Count == 0)
				oldValue.RemoveRow();
		}
	}
	#endregion

	#region Modification tracking
	partial class Pledge {
		///<summary>Called when the row is added to a table.</summary>
		protected override void OnAdded() {
			if (!Table.IsLoadingData) {
				Modifier = Environment.UserName;
				Modified = DateTime.UtcNow;
			}
		}
		partial void OnColumnChanged(Column column, object oldValue, object newValue) {
			if ((Modifier == null || (Table != null && !Table.IsLoadingData))	//Don't overwrite the values when loading the table
			 && column != ModifiedColumn && column != ModifierColumn) {
				Modifier = Environment.UserName;
				Modified = DateTime.UtcNow;
			}
		}
	}
	partial class Payment {
		///<summary>Called when the row is added to a table.</summary>
		protected override void OnAdded() {
			if (!Table.IsLoadingData) {
				Modifier = Environment.UserName;
				Modified = DateTime.UtcNow;
			}
		}
		partial void OnColumnChanged(Column column, object oldValue, object newValue) {
			if ((Modifier == null || (Table != null && !Table.IsLoadingData))	//Don't overwrite the values when loading the table
			 && column != ModifiedColumn && column != ModifierColumn && column != DepositColumn) {
				Modifier = Environment.UserName;
				Modified = DateTime.UtcNow;
			}
		}
	}
	#endregion

	partial class Pledge {
		partial void ValidateAmount(decimal newValue, Action<string> error) {
			if (newValue < 0) error("Amount cannot be negative");
		}
	}
	partial class Payment {
		partial void ValidateAmount(decimal newValue, Action<string> error) {
			if (newValue < 0) error("Amount cannot be negative");
		}
	}

	partial class EmailAddress {
		partial void Initialize() {
			RandomCode = Guid.NewGuid().ToString("N").Remove(20).ToUpperInvariant();
			DateAdded = DateTime.Now;
		}
		partial void OnPersonChanged(Person oldValue, Person newValue) {
			if (String.IsNullOrWhiteSpace(Name) && newValue != null)
				Name = newValue.FullName;
		}
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

	#region Interfaces
	partial class Pledge : ITransaction {
		///<summary>Gets the amount with the sign as reflected in the balance due.</summary>
		public decimal SignedAmount { get { return Amount; } }
	}
	partial class Payment : ITransaction {
		///<summary>Gets the amount with the sign as reflected in the balance due.</summary>
		public decimal SignedAmount { get { return -Amount; } }
	}
	partial class MelaveMalkaInvitation : IOwnedObject { }
	partial class EmailAddress : IOwnedObject { }
	partial class LoggedStatement : IOwnedObject { }
	partial class SeatingReservation : IOwnedObject {
		///<summary>Gets the person that placed the reservation.</summary>
		public Person Person { get { return Pledge == null ? null : Pledge.Person; } }

		///<summary>Gets the total number of seats in the reservation.</summary>
		public int TotalSeats { get { return MensSeats + WomensSeats + BoysSeats + GirlsSeats; } }
	}


	///<summary>Represents a billing transaction (a pledge or payment).</summary>
	public interface ITransaction : IOwnedObject {
		///<summary>Gets the date that the transaction was made.</summary>
		[SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Date", Justification = "For internal consumption")]
		DateTime Date { get; }
		///<summary>Gets the account that the transaction is applied to.</summary>
		string Account { get; }
		///<summary>Gets the amount with the sign as reflected in the balance due.</summary>
		decimal SignedAmount { get; }
	}
	///<summary>Represents an object associated with a person.</summary>
	public interface IOwnedObject {
		///<summary>Gets the person that owns the object.</summary>
		Person Person { get; }
	}
	#endregion
}