using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using ShomreiTorah.Data.UI.Grid;

namespace ShomreiTorah.Data.UI.DisplaySettings {
	///<summary>A grid behavior that toggles detail views when master rows are double-clicked.</summary>
	public class ToggleRowsBehavior : IGridBehavior {
		private ToggleRowsBehavior() { }
		///<summary>Gets the single ToggleRowsBehavior instance.</summary>
		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
		public static readonly ToggleRowsBehavior Instance = new ToggleRowsBehavior();

		///<summary>Applies the behavior to a SmartGridView.</summary>
		public void Apply(SmartGridView view) {
			view.DoubleClick += new EventHandler(view_DoubleClick);

			foreach (var column in view.Columns) {
				var edit = column.RealColumnEdit as RepositoryItemButtonEdit;
				if (edit == null) continue;
				if (edit.TextEditStyle != TextEditStyles.Standard) {
					edit.DoubleClick += delegate { ToggleRow(view, view.FocusedRowHandle); };
				}
			}
		}

		void view_DoubleClick(object sender, EventArgs e) {
			var view = (SmartGridView)sender;
			var info = view.CalcHitInfo(view.GridControl.PointToClient(Control.MousePosition));

			if (info.RowHandle >= 0 && info.InRow) {
				ToggleRow(view, info.RowHandle);
				var dx = e as DXMouseEventArgs;
				if (dx != null) dx.Handled = true;
			}

		}
		static void ToggleRow(SmartGridView view, int handle) { view.SetMasterRowExpanded(handle, !view.GetMasterRowExpanded(handle)); }
	}
}
