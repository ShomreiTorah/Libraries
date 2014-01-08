using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using ShomreiTorah.Data;
using ShomreiTorah.Singularity;

namespace ShomreiTorah.Statements {
	public class StatementInfo {
		readonly DataContext data;
		public StatementInfo(Person person, DateTime startDate, StatementKind kind) {
			if (person == null) throw new ArgumentNullException("person");

			Person = person;
			data = person.Table.Context;
			if (data == null)
				throw new ArgumentException("The person must be attached to a DataContext (for now)", "person");

			switch (kind) {
				case StatementKind.Bill:
					if (person.Pledges.Any()
					 && !person.Pledges.Any(p => p.Date >= startDate))
						startDate = new DateTime(person.Pledges.OrderBy(p => p.Date).Last().Date.Year, 1, 1);
					StartDate = startDate;
					EndDate = DateTime.MaxValue;
					break;
				case StatementKind.Receipt:
					StartDate = new DateTime(startDate.Year, 1, 1);
					EndDate = new DateTime(startDate.Year + 1, 1, 1).AddTicks(-1);
					break;
			}

			Kind = kind;

			Recalculate();
		}

		public Person Person { get; private set; }
		public DateTime StartDate { get; private set; }
		public DateTime EndDate { get; private set; }
		public StatementKind Kind { get; private set; }

		public virtual bool ShouldSend { get { return Accounts.Count > 0; } }

		public void Recalculate() {
			if (Person.Schema.Columns.Contains("BalanceDue"))
				TotalBalance = Math.Max(0, Person.Field<decimal>("BalanceDue"));
			else
				TotalBalance = Person.Pledges.Concat<ITransaction>(Person.Payments).Sum(t => t.SignedAmount);

			Accounts = new ReadOnlyCollection<StatementAccount>(
				Names.AccountNames.Select(a => new StatementAccount(this, a))
								  .Where(ba => ba.ShouldInclude)
								  .ToArray()
			);

			TotalPledged = Accounts.Sum(a => a.TotalPledged);
			TotalPaid = Accounts.Sum(a => a.TotalPaid);

			Deductibility = String.Join("  ", CalcDeductibility());

			if (data.Table<Payment>().Rows.Any())
				LastEnteredPayment = data.Table<Payment>().Rows.Max(p => p.Modified);
			else if (data.Table<Pledge>().Rows.Any())
				LastEnteredPayment = data.Table<Pledge>().Rows.Max(p => p.Modified);
			else
				LastEnteredPayment = new DateTime(1970, 1, 1);
		}

		private IEnumerable<String> CalcDeductibility() {
			var exlcudedPledges = Accounts.SelectMany(a => a.Pledges.Where(p =>
				p.SubType.Replace("-", "").StartsWith(Names.NonDeductibleSubType.Replace("-", ""), StringComparison.OrdinalIgnoreCase))
			).ToList();

			var exlcudedPledgeSum = exlcudedPledges.Sum(p => p.Amount);
			if (exlcudedPledgeSum == 0) {
				yield return "No goods or services have been provided.";
			} else {
				yield return String.Format(CultureInfo.CurrentCulture,
										   "This receipt covers {0:c} of payments for goods or services rendered ({2}), leaving a total of {1:c} of tax-deductible contributions.",
										   exlcudedPledgeSum, Math.Max(0, TotalPaid - exlcudedPledgeSum), String.Join(", ", exlcudedPledges.Select(p => p.Type).Distinct()));
			}
			if (Accounts.Any(a => a.Pledges.Any(p => p.Amount > 0 && p.Type.StartsWith("Melave Malka", StringComparison.CurrentCultureIgnoreCase))))
				yield return "If you attended the Melave Malka, " + (exlcudedPledgeSum > 0 ? "an additional " : "") + " $25 per reservation is not tax-deductible.";
		}

		///<summary>Gets the date of the most recently entered payment in the system.</summary>
		public DateTime LastEnteredPayment { get; private set; }

		///<summary>Gets the total value of the pledges in the statement.</summary>
		public decimal TotalPledged { get; private set; }
		///<summary>Gets the total value of the payments in the statement.</summary>
		public decimal TotalPaid { get; private set; }
		///<summary>Gets the person's balance due.</summary>
		public decimal TotalBalance { get; private set; }
		///<summary>Gets the accounts in the statement.</summary>
		public ReadOnlyCollection<StatementAccount> Accounts { get; private set; }
		///<summary>Gets a disclaimer describing how much of the contributions are tax-deductible.</summary>
		public string Deductibility { get; private set; }

		//This method is called in a loop.
		//I want the times to be identical
		//for all of the calls so that it
		//will sort nicely.
		[ThreadStatic]
		static DateTime lastGenTime;
		public void LogStatement(string media, string kind, string userName = null) {
			var now = DateTime.Now;
			if ((now - lastGenTime) > TimeSpan.FromSeconds(5))
				lastGenTime = now;
			data.Table<LoggedStatement>().Rows.Add(new LoggedStatement {
				Person = Person,
				DateGenerated = lastGenTime,
				Media = media,
				StatementKind = kind,
				StartDate = StartDate,
				EndDate = EndDate,
				UserName = userName ?? Environment.UserName
			});
		}
	}
	///<summary>Contains data about a specific account in a statement.</summary>
	public class StatementAccount {
		internal StatementAccount(StatementInfo parent, string accountName) {
			Parent = parent;
			AccountName = accountName;
			OutstandingBalance = Parent.Person.GetBalance(Parent.StartDate, AccountName);
			BalanceDue = Math.Max(0, Parent.Person.GetBalance(AccountName));

			Func<ITransaction, bool> filter = t => t.Date >= Parent.StartDate && t.Date < Parent.EndDate && t.Account == AccountName;

			Pledges = new ReadOnlyCollection<Pledge>(Parent.Person.Pledges.Where(p => filter(p)).OrderBy(p => p.Date).ToArray());
			Payments = new ReadOnlyCollection<Payment>(Parent.Person.Payments.Where(p => filter(p)).OrderBy(p => p.Date).ToArray());

			TotalPledged = OutstandingBalance + Pledges.Sum(p => p.Amount);
			TotalPaid = Payments.Sum(p => p.Amount);
		}
		internal bool ShouldInclude {
			get {
				if (Parent.Kind == StatementKind.Bill)
					return OutstandingBalance != 0 || Pledges.Any() || Payments.Any();
				else
					return Payments.Any();
			}
		}

		///<summary>Gets the statement containing the account.</summary>
		public StatementInfo Parent { get; private set; }
		///<summary>Gets the name of the account.</summary>
		public string AccountName { get; private set; }

		///<summary>Gets the outstanding balance in the account.</summary>
		public decimal OutstandingBalance { get; private set; }
		public decimal BalanceDue { get; private set; }

		///<summary>Gets the total value of the pledges in the account.</summary>
		public decimal TotalPledged { get; private set; }
		///<summary>Gets the total value of the payments in the account.</summary>
		public decimal TotalPaid { get; private set; }

		///<summary>Gets the pledges that belong to the account.</summary>
		public ReadOnlyCollection<Pledge> Pledges { get; private set; }
		///<summary>Gets the payments that belong to the account.</summary>
		public ReadOnlyCollection<Payment> Payments { get; private set; }
	}
	public enum StatementKind {
		Bill,
		Receipt
	}
}