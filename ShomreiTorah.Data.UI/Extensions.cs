using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using DevExpress.Utils;
using DevExpress.XtraBars;
using ShomreiTorah.Singularity;
using ShomreiTorah.WinForms;

namespace ShomreiTorah.Data.UI {
	///<summary>Contains extension methods for data UIs.</summary>
	public static class Extensions {
		///<summary>Sets up a button that can use data for a range of years.</summary>
		///<typeparam name="TRow">The strongly-typed Singularity row that the feature uses.  These rows are used to populate the years dropdown.</typeparam>
		///<param name="button">The button that initiates the action.</param>
		///<param name="yearGetter">A delegate that gets the year represented by a row.  This is called to populate the years dropdown.</param>
		///<param name="showForm">The method bound to the button.</param>
		///<param name="defaultYear">The year used when clicking the button itself. This year will also be bolded in the dropdown.  Defaults to the current year.</param>
		public static void SetupYearlyButton<TRow>(this BarButtonItem button, Func<TRow, int?> yearGetter, Action<int> showForm, int? defaultYear = null) where TRow : Row {
			if (button == null) throw new ArgumentNullException("button");
			if (yearGetter == null) throw new ArgumentNullException("yearGetter");
			if (showForm == null) throw new ArgumentNullException("showForm");

			defaultYear = defaultYear ?? DateTime.Now.Year;
			var menu = new PopupMenu(button.Manager);
			button.Disposed += delegate { menu.Dispose(); };

			if (button.DropDownSuperTip == null)
				button.DropDownSuperTip = Utilities.CreateSuperTip(button.Caption, "Shows a " + button.Caption + " for a specific year");

			button.ButtonStyle = BarButtonStyle.DropDown;
			button.DropDownControl = menu;
			button.ItemClick += delegate { showForm(defaultYear.Value); };
			menu.BeforePopup += delegate {
				foreach (var link in menu.ItemLinks.Cast<BarItemLink>().ToList()) //Collection will be modified
					button.Manager.Items.Remove(link.Item);

				menu.ItemLinks.Clear();

				AppFramework.LoadTable<TRow>();
				foreach (int dontUse in AppFramework.Table<TRow>().Rows
													.Select(yearGetter)
													.Where(y => y.HasValue)
													.Distinct()
													.OrderByDescending(y => y)) {
					int year = dontUse;	//Force separate variable per closure
					var item = new BarButtonItem(button.Manager, year.ToString(CultureInfo.CurrentCulture));

					item.ItemClick += delegate { showForm(year); };
					if (year == defaultYear)
						item.Appearance.Font = new Font(item.Appearance.GetFont(), FontStyle.Bold);

					menu.AddItem(item);
				}
			};
		}

		#region Tooltips
		///<summary>Creates a SuperToolTip that displays information about a person.</summary>
		public static SuperToolTip GetSuperTip(this Person person) {
			if (person == null) throw new ArgumentNullException("person");

			string body = person.MailingAddress;
			if (!string.IsNullOrEmpty(person.Phone))
				body += Environment.NewLine + Environment.NewLine + person.Phone;

			return Utilities.CreateSuperTip(person.VeryFullName, body);
		}

		///<summary>Creates a SuperToolTip that displays information about a deposit.</summary>
		public static SuperToolTip GetSuperTip(this Deposit deposit) {
			if (deposit == null) throw new ArgumentNullException("deposit");
			return Utilities.CreateSuperTip(
				body: String.Format(CultureInfo.CurrentCulture, "Deposit #{0} on {1:D}.\r\nDeposit contained {2:c} in {3} payments.",
																deposit.Number, deposit.Date, deposit.Payments.Sum(p => p.Amount), deposit.Payments.Count)
			);
		}
		#endregion

		///<summary>Confirms that a payment does not have a duplicate check number.</summary>
		///<returns>The duplicated Payment instance, if any.</returns>
		public static Payment FindDuplicate(this Payment payment) { return payment.FindDuplicate(payment.CheckNumber); }
		///<summary>Confirms that a payment does not have a duplicate check number.</summary>
		///<returns>The duplicated Payment instance, if any.</returns>
		public static Payment FindDuplicate(this Payment payment, string newCheckNumber) {
			if (payment == null) throw new ArgumentNullException("payment");
			if (payment.Person == null) return null;
			if (String.IsNullOrWhiteSpace(newCheckNumber)) return null;

			return payment.Person.Payments
					.FirstOrDefault(p => p != payment
									  && p.Account == payment.Account
									  && String.Equals(p.CheckNumber, newCheckNumber, StringComparison.CurrentCultureIgnoreCase));
		}
	}
}