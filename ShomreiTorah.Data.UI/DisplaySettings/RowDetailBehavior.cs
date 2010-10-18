using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShomreiTorah.Data.UI.Grid;
using System.Windows.Forms;
using ShomreiTorah.Singularity;
namespace ShomreiTorah.Data.UI.DisplaySettings {
	///<summary>A grid behavior that shows row detail forms when rows are double-clicked.</summary>
	class RowDetailBehavior : IGridBehavior {
		public void Apply(SmartGridView view) {
			view.DoubleClick += delegate {
				var info = view.CalcHitInfo(view.GridControl.PointToClient(Control.MousePosition));

				if (info.RowHandle >= 0 && info.InRow)
					AppFramework.Current.ShowDetails((Row)view.GetRow(info.RowHandle));
			};
		}
	}
}
