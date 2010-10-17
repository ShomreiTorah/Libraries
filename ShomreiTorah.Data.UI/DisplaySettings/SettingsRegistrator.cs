using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShomreiTorah.Data.UI.Grid;
using DevExpress.Data;

namespace ShomreiTorah.Data.UI.DisplaySettings {
	///<summary>Registers grid and column behaviors.</summary>
	static class SettingsRegistrator {
		static void InitializeStandardSettings() {
			//This method will only be called once
			EditorRepository.Register(new[] { Payment.AmountColumn, Pledge.AmountColumn }, EditorRepository.CurrencyEditor);
			EditorRepository.Register(new[] { Payment.AccountColumn, Pledge.AccountColumn }, EditorRepository.AccountEditor);

			GridManager.RegisterColumn(
				Pledge.AmountColumn,
				new ColumnController(c => {
					c.DisplayFormat.Assign(c.DefaultEditor.DisplayFormat);

					c.MaxWidth = 70;
					c.SummaryItem.DisplayFormat = "{0:c} Total Pledged";
					c.SummaryItem.SummaryType = SummaryItemType.Sum;
				})
			);
			GridManager.RegisterColumn(
				Payment.AmountColumn,
				new ColumnController(c => {
					c.DisplayFormat.Assign(c.DefaultEditor.DisplayFormat);

					c.MaxWidth = 70;
					c.SummaryItem.DisplayFormat = "{0:c} Total Paid";
					c.SummaryItem.SummaryType = SummaryItemType.Sum;
				})
			);

			GridManager.RegisterColumns(
				new[] { Payment.AccountColumn, Pledge.AccountColumn },
				new ColumnController(c => { c.MaxWidth = 100; })
			);
		}

		static SettingsRegistrator() { InitializeStandardSettings(); }

		///<summary>Ensures that all grid settings have been registered.</summary>
		public static void EnsureRegistered() { }
	}
}
