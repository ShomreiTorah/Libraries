using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShomreiTorah.Data.UI.Grid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.Utils.Menu;
using ShomreiTorah.Common;
using System.Collections.ObjectModel;

namespace ShomreiTorah.Data.UI.DisplaySettings {
	///<summary>An IGridBehavior that allows the user to show additional columns from the column context menu.</summary>
	public class AdvancedColumnsBehavior : IGridBehavior {
		///<summary>Creates an AdvancedColumnsBehavior.</summary>
		///<param name="name">The name of the column set to show in the context menu.</param>
		///<param name="fieldNames">The names of the fields to toggle.</param>
		public AdvancedColumnsBehavior(string name, IEnumerable<string> fieldNames) {
			if (fieldNames == null) throw new ArgumentNullException("fieldNames");

			FieldNames = fieldNames.ReadOnlyCopy();
			Name = name ?? FieldNames.Join(", ");
		}

		///<summary>Gets the name of the column set to show in the context menu.</summary>
		public string Name { get; private set; }
		///<summary>Gets the names of the advanced columns.</summary>
		public ReadOnlyCollection<string> FieldNames { get; private set; }

		///<summary>Applies the behavior to a SmartGridView.</summary>
		public void Apply(SmartGridView view) {
			view.ShowGridMenu += View_ShowGridMenu;

			foreach (var col in FieldNames.Select(view.Columns.ColumnByFieldName))
				col.Visible = false;
		}

		void View_ShowGridMenu(object sender, GridMenuEventArgs e) {
			if (e.MenuType == GridMenuType.Column) {
				var view = (SmartGridView)sender;

				var columns = FieldNames.Select(view.Columns.ColumnByFieldName);
				var newVisiblity = !columns.First().Visible;
				var caption = (newVisiblity ? "Show " : "Hide ") + Name;

				var item = new DXMenuItem(caption, delegate {
					foreach (var col in columns)
						col.Visible = newVisiblity;
				}, image: null);

				e.Menu.Items.Add(item);
			}
		}
	}
}
