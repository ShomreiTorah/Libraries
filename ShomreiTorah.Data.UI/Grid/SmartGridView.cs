using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Registrator;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;

namespace ShomreiTorah.Data.UI.Grid {
	///<summary>A grid view that automatically reads column settings from metadata.</summary>
	public class SmartGridView : GridView {
		class SgvRegistrator : GridInfoRegistrator {
			public override string ViewName { get { return "SmartGridView"; } }
			public override BaseView CreateView(GridControl grid) { return new SmartGridView((SmartGrid)grid); }
		}
		internal static readonly GridInfoRegistrator Registrator = new SgvRegistrator();
		protected override string ViewName { get { return "SmartGridView"; } }

		public SmartGridView() : this(null) { }
		public SmartGridView(SmartGrid grid)
			: base(grid) {
		}

		protected override GridColumnCollection CreateColumnCollection() { return new SmartGridColumnCollection(this); }

		protected override void PopulateColumnsCore(DevExpress.Data.DataColumnInfo[] columns) {
			base.PopulateColumnsCore(columns);
		}
	}
	class SmartGridColumnCollection : GridColumnCollection {
		public SmartGridColumnCollection(SmartGridView view) : base(view) { }
		protected override GridColumn CreateColumn() { return new SmartGridColumn(); }
	}
	///<summary>A grid column that automatically reads column settings from metadata.</summary>
	public class SmartGridColumn : GridColumn {

	}
}
