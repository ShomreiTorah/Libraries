using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using ShomreiTorah.Common;
using ShomreiTorah.Singularity;

namespace ShomreiTorah.Data {
	partial class RaffleTicket {
		///<summary>Calculates the standard price for a number of tickets.</summary>
		public static decimal CalcPrice(int ticketCount) {
			if (ticketCount == 0)
				return 0;
			if (ticketCount == 1)
				return 60;

			return 50 + 25 * ticketCount;
		}
	}

	///<summary>Contains the core fields of a person in the master directory.</summary>
	public interface IPerson {
		///<summary>Gets or sets the husband's name.</summary>
		string HisName { get; set; }
		///<summary>Gets or sets the wife's name.</summary>
		string HerName { get; set; }
		///<summary>Gets or sets the family name.</summary>
		string LastName { get; set; }
	}

	partial class Person : IPerson {
		partial void OnFullNameChanged(string oldValue, string newValue) {
			if (Table == null || Table.Context == null || Table.IsLoadingData)
				return;
			var emails = Table.Context.Table<EmailAddress>();
			if (emails == null) return;
			foreach (var email in emails.Rows) {
				if (String.IsNullOrWhiteSpace(email.Name) || email.Name.Trim() == oldValue)
					email.Name = newValue;
			}
		}

		#region String properties
		///<summary>Gets the actual salutation to use for the person.</summary>
		public string ActualSalutation { get { return String.IsNullOrWhiteSpace(Salutation) ? FullName : Salutation; } }

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

		///<summary>Returns a string containing all of the information about the person.</summary>
		public string ToFullString() {
			StringBuilder retVal = new StringBuilder();

			retVal.Append(VeryFullName);
			if (!String.IsNullOrEmpty(Phone))
				retVal.Append(", ").Append(Phone);

			retVal.AppendLine().AppendLine();
			retVal.Append(MailingAddress);

			return retVal.ToString().Trim();
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
		///<remarks>This method allows Person columns to be sorted in grids.</remarks>
		public int CompareTo(Person other) {
			if (other == null) return 1;
			if (this == other) return 0;

			if (LastName == other.LastName)
				return String.CompareOrdinal(HisName, other.HisName);

			return String.CompareOrdinal(LastName, other.LastName);
		}
	}
	[SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Justification = "Comparable for grids only")]
	partial class Caller : IComparable, IComparable<Caller> {
		int IComparable.CompareTo(object obj) { return CompareTo(obj as Caller); }
		///<summary>Compares this caller to another Caller instance.</summary>
		///<remarks>This method allows the call list grid to be sorted by caller.</remarks>
		public int CompareTo(Caller other) {
			if (other == null) return 1;
			if (this == other) return 0;

			return Person.CompareTo(other.Person);
		}
	}
	[SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Justification = "Comparable for grids only")]
	partial class Deposit : IComparable<Deposit>, IComparable {
		int IComparable.CompareTo(object obj) { return CompareTo(obj as Deposit); }
		///<summary>Compares this person to another Person instance.</summary>
		///<remarks>This method allows payment grids to be sorted by Deposit.</remarks>
		public int CompareTo(Deposit other) {
			if (other == null) return 1;
			if (this == other) return 0;

			var result = Date.CompareTo(other.Date);
			return result == 0 ? Number.CompareTo(other.Number) : result;
		}
	}
	#endregion
	partial class Payment {
		///<summary>Gets the full description of this payment, including the payment method and the appropriately-formatted details.</summary>
		///<remarks>This string is displayed in receipts.</remarks>
		public string MethodDescription {
			get {
				if (Method == "Unknown")    // Imported legacy data
					return "?";

				if (string.IsNullOrWhiteSpace(CheckNumber))
					return Method;

				switch (Method) {
					case "Check":
						return Method + " #" + CheckNumber;

					case "Credit Card":
						return Method + " (" + CheckNumber + ")";

					case "Goods and/or Services":
						return "Goods / services: " + CheckNumber;

					case "Cash":
					default:
						return Method + " – " + CheckNumber;
				}
			}
		}

		///<summary>Called before the row is removed from its table.</summary>
		protected override void OnRemoving() {
			OnRemoving_Deposits();
			OnRemoving_Links();
		}
	}
	#region Empty deposit cleanup
	partial class Deposit {
		///<summary>Clears a deposit's payments when the deposit is removed.</summary>
		protected override void OnRemoving() {
			foreach (var payment in Payments.ToList()) {    //The loop will modify the collection
				payment.Deposit = null;
			}
		}
	}
	partial class Payment {
		///<summary>Deletes a deposit if its last payment is removed.</summary>
		void OnRemoving_Deposits() {
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
			if ((Modifier == null || (Table != null && !Table.IsLoadingData))   //Don't overwrite the values when loading the table
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
			if ((Modifier == null || (Table != null && !Table.IsLoadingData))   //Don't overwrite the values when loading the table
			 && column != ModifiedColumn && column != ModifierColumn && column != DepositColumn) {
				Modifier = Environment.UserName;
				Modified = DateTime.UtcNow;
			}
		}

		///<summary>Indicates whether this payment needs to be deposited.</summary>
		public bool NeedsDeposit {
			get { return Amount > 0 && !Names.UndepositedPayments.Contains(Method, StringComparer.OrdinalIgnoreCase); }
		}
	}
	#endregion

	#region Melave Malka
	partial class Caller {
		///<summary>Gets the string used to represent this caller in a dropdown list.</summary>
		public override string ToString() { return Person.HisName[0] + " " + Person.LastName; }

		///<summary>Clears a caller's assigned callees when the caller is deleted..</summary>
		protected override void OnRemoving() {
			foreach (var callee in Callees.ToList()) //The loop will modify the collection
				callee.Caller = null;
		}

		///<summary>Gets the caller's name.</summary>
		public string Name { get { return Person.HisName + " " + Person.LastName; } }
	}
	partial class MelaveMalkaInvitation {
		partial void OnShouldCallChanged(bool? oldValue, bool? newValue) {
			if (newValue == false)
				Caller = null;
		}
	}

	partial class MelaveMalkaInfo {
		///<summary>Gets the year of the current Melave Malka.</summary>
		///<remarks>Melave Malka work starts in December of the previous year.</remarks>
		public static int CurrentYear { get { return DateTime.Now.AddMonths(5).Year; } }

		///<summary>Gets all of the honorees of this Melave Malka.</summary>
		public IEnumerable<Person> Honorees {
			get {
				yield return Honoree;
				if (Honoree2 != null)
					yield return Honoree2;
			}
		}

		//If the date is after November, it should wrap to the previous year.
		partial void OnAdDeadlineChanged(DateTime? oldValue, DateTime? newValue) {
			if (newValue != null && this["Year"] != null)
				AdDeadline = new DateTime(Year - (newValue.Value.Month > 10 ? 1 : 0), newValue.Value.Month, newValue.Value.Day);
		}
		partial void OnMelaveMalkaDateChanged(DateTime? oldValue, DateTime? newValue) {
			if (newValue != null && this["Year"] != null)
				MelaveMalkaDate = new DateTime(Year - (newValue.Value.Month > 10 ? 1 : 0), newValue.Value.Month, newValue.Value.Day)
								+ newValue.Value.TimeOfDay; //Preserve the Melave Malka's TimeOfDay when normalizing the year
		}

		///<summary>Gets the relative path to the ad blank PDF on the website.</summary>
		public Uri AdBlankPath { get { return new Uri(String.Format(CultureInfo.InvariantCulture, "/Files/Ad-Blank-{0:yyyy}.pdf", MelaveMalkaDate), UriKind.Relative); } }
	}
	#endregion

	#region Pledge Links
	partial class Pledge {
		partial void OnAccountChanged(string oldValue, string newValue) {
			if (Table == null || Table.Context == null || Table.IsLoadingData)
				return;
			foreach (var link in LinkedPayments.ToList()) //The loop will modify the collection
				link.RemoveRow();
		}
		partial void OnPersonChanged(Person oldValue, Person newValue) {
			if (Table == null || Table.Context == null || Table.IsLoadingData)
				return;
			foreach (var link in LinkedPayments.ToList()) //The loop will modify the collection
				link.RemoveRow();
		}

		///<summary>Clears a pledge's payment links when the pledge is removed.</summary>
		protected override void OnRemoving() {
			if (Table.Context.Table<PledgeLink>() == null)  // Don't choke in DirectoryManager on refresh
				return;
			foreach (var link in LinkedPayments.ToList()) //The loop will modify the collection
				link.RemoveRow();
		}
	}
	partial class Payment {
		partial void ValidateAmount(decimal newValue, Action<string> error) {
			if (newValue < 0) error("Amount cannot be negative");
		}
		partial void OnAccountChanged(string oldValue, string newValue) {
			if (Table == null || Table.Context == null || Table.IsLoadingData)
				return;
			foreach (var link in LinkedPledges.ToList()) //The loop will modify the collection
				link.RemoveRow();
		}
		partial void OnPersonChanged(Person oldValue, Person newValue) {
			if (Table == null || Table.Context == null || Table.IsLoadingData)
				return;
			foreach (var link in LinkedPledges.ToList()) //The loop will modify the collection
				link.RemoveRow();
		}

		///<summary>Clears a payment's pledge links when the payment is removed.</summary>
		void OnRemoving_Links() {
			if (Table.Context.Table<PledgeLink>() == null)  // Don't choke in DirectoryManager on refresh
				return;
			foreach (var link in LinkedPledges.ToList()) //The loop will modify the collection
				link.RemoveRow();
		}
	}
	partial class PledgeLink {
		partial void ValidateAmount(decimal newValue, Action<string> error) {
			if (newValue < 0) error("Amount cannot be negative");
		}
	}
	#endregion


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
	partial class MelaveMalkaSeat {
		///<summary>Gets the organization-specific caption for the Mens' Seats column.</summary>
		public static string MensSeatsCaption => Config.ReadAttribute("Journal", "Seats", "MensSeatsCaption");
		///<summary>Gets the organization-specific caption for the Womens' Seats column.</summary>
		public static string WomensSeatsCaption => Config.ReadAttribute("Journal", "Seats", "WomensSeatsCaption");
	}

	#region Interfaces
	partial class MelaveMalkaInvitation : IOwnedObject { }
	partial class MelaveMalkaSeat : IOwnedObject { }
	partial class EmailAddress : IOwnedObject { }
	partial class LoggedStatement : IOwnedObject { }
	partial class RaffleTicket : IOwnedObject { }
	partial class Caller : IOwnedObject { }
	partial class MelaveMalkaInfo : IOwnedObject {
		Person IOwnedObject.Person { get { return Honoree; } }
	}

	partial class Pledge : ITransaction {
		///<summary>Gets the amount with the sign as reflected in the balance due.</summary>
		public decimal SignedAmount { get { return Amount; } }
	}
	partial class Payment : ITransaction {
		///<summary>Gets the amount with the sign as reflected in the balance due.</summary>
		public decimal SignedAmount { get { return -Amount; } }
	}
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

	partial class JournalAd {
		///<summary>Infers the journal year most likely for a specific date.</summary>
		///<remarks>Since journals start around January, any date after September is inferred as the following year.</remarks>
		public static int InferYear(DateTime date) => date.AddMonths(4).Year;

		partial void OnAdTypeChanged(string oldValue, string newValue) {
			if (String.IsNullOrEmpty(oldValue) || String.IsNullOrEmpty(newValue)) return;
			if (Table == null) return;
			if (Table.IsLoadingData) return;
			if (Table.Context == null) return;

			var oldSubType = Names.AdTypes.First(t => t.Name == oldValue).PledgeSubType;
			var newSubType = Names.AdTypes.First(t => t.Name == newValue).PledgeSubType;
			foreach (var pledge in Pledges) {
				if (pledge.SubType == oldSubType)
					pledge.SubType = newSubType;
			}
		}

		///<summary>Creates a pledge for this ad.</summary>
		public Pledge CreatePledge() {
			var mmi = Table.Context.Table<MelaveMalkaInfo>().Rows.FirstOrDefault(m => m.Year == Year);
			var honorees = mmi?.Honorees?.Select(h => h.FullName)?.Join(" and ");
			return new Pledge {
				Type = "Melave Malka Journal",
				SubType = Names.AdTypes.First(t => t.Name == AdType).PledgeSubType,
				Note = string.IsNullOrEmpty(honorees) ? null : String.Format(Config.ReadAttribute("Journal", "PledgeNoteFormat"), honorees),
				Account = Names.DefaultAccount,
				ExternalSource = "Journal " + Year,
				ExternalId = ExternalId,
				Date = DateAdded
			};
		}
		///<summary>Creates a payment for this ad.</summary>
		public Payment CreatePayment() {
			return new Payment {
				Account = Names.DefaultAccount,
				ExternalSource = "Journal " + Year,
				ExternalId = ExternalId,
				Date = DateAdded
			};
		}
		///<summary>Gets the pledges linked to this ad.</summary>
		public IEnumerable<Pledge> Pledges {
			get { return Table.Context.Table<Pledge>().Rows.Where(p => p.ExternalSource == "Journal " + Year && p.ExternalId == ExternalId); }
		}
		///<summary>Gets the payments linked to this ad.</summary>
		public IEnumerable<Payment> Payments {
			get { return Table.Context.Table<Payment>().Rows.Where(p => p.ExternalSource == "Journal " + Year && p.ExternalId == ExternalId); }
		}

		partial void OnExternalIdChanged(int? oldValue, int? newValue) {
			if (oldValue == null || newValue == null) return;
			if (Table == null) return;
			if (Table.IsLoadingData) return;
			if (Table.Context == null) return;

			//I can't use the properties since ExternalId has already changed.
			//I need to force eager evaluation.
			var pledges = Table.Context.Table<Pledge>().Rows
							.Where(p => p.ExternalSource == "Journal " + Year && p.ExternalId == oldValue).ToArray();
			foreach (var row in pledges)
				row.ExternalId = newValue.Value;

			var payments = Table.Context.Table<Payment>().Rows
							.Where(p => p.ExternalSource == "Journal " + Year && p.ExternalId == oldValue).ToArray();
			foreach (var row in payments)
				row.ExternalId = newValue.Value;
		}
		partial void ValidateExternalId(int newValue, Action<string> error) {
			if (Table.Rows.Any(r => r != this && r.Year == Year && r.ExternalId == newValue))
				error("There is already an ad with external ID " + newValue);
		}
	}
}