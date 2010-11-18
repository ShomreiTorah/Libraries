using System;
using System.Globalization;
using System.Linq;
using DevExpress.Utils;
using DevExpress.XtraGrid.Views.Grid;
using ShomreiTorah.WinForms;

namespace ShomreiTorah.Data.UI {
	///<summary>Contains extension methods for data UIs.</summary>
	public static class Extensions {
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
					.FirstOrDefault(p => p != payment && String.Equals(p.CheckNumber, newCheckNumber, StringComparison.CurrentCultureIgnoreCase));
		}
	}
}
