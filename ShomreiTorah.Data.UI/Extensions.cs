using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DevExpress.Utils;
using ShomreiTorah.WinForms;

namespace ShomreiTorah.Data.UI {
	///<summary>Contains extension methods for data UIs.</summary>
	public static class Extensions {
		#region Tooltips
		///<summary>Creates a SuperToolTip that displays information about a person.</summary>
		public static SuperToolTip GetSuperTip(this Person person) {
			if (person == null) throw new ArgumentNullException("person");
			return Utilities.CreateSuperTip(person.VeryFullName, person.MailingAddress);
		}

		///<summary>Creates a SuperToolTip that displays information about a deposit.</summary>
		public static SuperToolTip GetSuperTip(this Deposit deposit) {
			if (deposit == null) throw new ArgumentNullException("deposit");
			return Utilities.CreateSuperTip(
				"Deposit",
				String.Format(CultureInfo.CurrentCulture, "Deposit #{0} on {1:F}; deposit contained {2:c}.",
														  deposit.Number, deposit.Date, deposit.Payments.Sum(p => p.Amount))
			);
		}
		#endregion
	}
}
