using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShomreiTorah.Data.UI.Grid;
using System.Windows.Forms;
using ShomreiTorah.WinForms;
using ShomreiTorah.Common;

namespace ShomreiTorah.Data.UI.DisplaySettings {
	///<summary>Allows rows in a grid to be deleted.</summary>
	public class DeletionBehavior : IGridBehavior {
		#region Creation Methods
		///<summary>Creates a DeletionBehavior that displays a simple confirmation message.</summary>
		///<typeparam name="T">The type of rows that the behavior applies to.</typeparam>
		///<param name="nameSingular">The name of a single element being deleted.	(e.g., "email address")</param>
		///<param name="namePlural">The name of a multiple elements being deleted.	(e.g., "email addresses")</param>
		public static DeletionBehavior WithMessages(string nameSingular, string namePlural) {
			return WithMessages<object>(
				singular: _ => "Are you sure you want to delete this " + nameSingular + "?",
				plural: ___ => "Are you sure you want to delete these " + namePlural + "?"
			);
		}
		///<summary>Creates a DeletionBehavior that displays a confirmation message.</summary>
		///<typeparam name="T">The type of rows that the behavior applies to.</typeparam>
		///<param name="singular">A delegate that generates a warning message for a single selected row.</param>
		///<param name="plural">A delegate that generates a warning message for multiple selected rows.</param>
		public static DeletionBehavior WithMessages<T>(Func<T, string> singular, Func<IEnumerable<T>, string> plural) {
			return WithMessage<T>(s => s.Has(2) ? plural(s) : singular(s.Single()));
		}

		///<summary>Creates a DeletionBehavior that displays a confirmation message.</summary>
		///<typeparam name="T">The type of rows that the behavior applies to.</typeparam>
		///<param name="messageGetter">A delegate that generates a warning message for a set of selected rows.</param>
		public static DeletionBehavior WithMessage<T>(Func<IEnumerable<T>, string> messageGetter) {
			return new DeletionBehavior(s => Dialog.Warn(messageGetter((IEnumerable<T>)s)));
		}

		///<summary>Creates a DeletionBehavior that disallows deletion.</summary>
		///<param name="message">The error message to display when the user attempts to delete a row.</param>
		public static DeletionBehavior Disallow(string message) {
			return new DeletionBehavior(_ => { Dialog.ShowError(message); return false; });
		}
		#endregion

		///<summary>Creates a new DeletionBehavior instance.</summary>
		///<param name="confirmer">A delegate that specifies whether to allow deletion of a set of selected rows.</param>
		public DeletionBehavior(Func<IEnumerable<object>, bool> confirmer) { this.confirmer = confirmer; }

		readonly Func<IEnumerable<object>, bool> confirmer;

		///<summary>Applies the behavior to a SmartGridView.</summary>
		public void Apply(SmartGridView view) {
			view.KeyDown += View_KeyDown;
		}

		void View_KeyDown(object sender, KeyEventArgs e) {
			var view = (SmartGridView)sender;
			var rows = view.GetSelectedRows().Where(r => r >= 0).Select(view.GetRow);

			if (!rows.Any()) return;
			if (confirmer(rows))
				view.DeleteSelectedRows();
		}
	}
}
