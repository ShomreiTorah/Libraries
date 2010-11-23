using System.Windows.Forms;
using DevExpress.Utils;
using ShomreiTorah.Data.UI.Grid;
using ShomreiTorah.Singularity;

namespace ShomreiTorah.Data.UI.DisplaySettings {
	///<summary>A grid behavior that shows row detail forms when rows are double-clicked.</summary>
	class RowDetailBehavior : IGridBehavior {
		//This is only ever instantiated once, by RegisterSettings.
		public void Apply(SmartGridView view) {
			view.DoubleClick += (sender, e) => {
				var info = view.CalcHitInfo(view.GridControl.PointToClient(Control.MousePosition));

				if (info.RowHandle >= 0 && info.InRow) {
					AppFramework.Current.ShowDetails((Row)view.GetRow(info.RowHandle));

					var dx = e as DXMouseEventArgs;
					if (dx != null) dx.Handled = true;
				}
			};
		}
	}
}
