using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Diagnostics;
using System.Windows.Forms;
using DevExpress.Utils.Serializing;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Registrator;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Base.Handler;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.Handler;

namespace ShomreiTorah.Data.UI.Grid {
	///<summary>A grid view that automatically reads column settings from metadata.</summary>
	public sealed partial class SmartGridView : GridView {
		sealed class MyRegistrator : GridInfoRegistrator {
			public override string ViewName { get { return "SmartGridView"; } }
			public override BaseViewHandler CreateHandler(BaseView view) { return new MyHandler((SmartGridView)view); }
			public override BaseView CreateView(GridControl grid) {
				var view = new SmartGridView((SmartGrid)grid);
				view.SetGridControl(grid);
				return view;
			}
		}

		internal static BaseInfoRegistrator CreateRegistrator() { return new MyRegistrator(); }
		protected override string ViewName { get { return "SmartGridView"; } }
		protected override GridColumnCollection CreateColumnCollection() { return new SmartGridColumnCollection(this); }

		///<summary>Gets the columns in the view.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[XtraSerializableProperty(XtraSerializationVisibility.Collection, true, true, true, 0, XtraSerializationFlags.DefaultValue), XtraSerializablePropertyId(LayoutIdColumns)]
		public new SmartGridColumnCollection Columns { get { return (SmartGridColumnCollection)base.Columns; } }
		private new MyHandler Handler { get { return (MyHandler)base.Handler; } }

		public SmartGridView() : this(null) { }
		public SmartGridView(SmartGrid grid)
			: base(grid) {
			this.AddControllerHandlers();	//See ColumnController.cs
		}

		object lastAppliedDataSource;
		protected override void OnDataController_DataSourceChanged(object sender, EventArgs e) {
			base.OnDataController_DataSourceChanged(sender, e);
			if (DataSource != null) {
				if (lastAppliedDataSource == DataSource)
					return;
				ApplyBehaviors();
				ApplyColumnControllers();
				BestFitColumns();
				lastAppliedDataSource = DataSource;
			}
		}
		#region Behaviors
		void ApplyBehaviors() {
#if DEBUG
			if (!IsDesignMode)
				Debug.Assert(lastAppliedDataSource == null, "Two datasources applied");
#endif
			foreach (var behavior in DisplaySettings.GridManager.GetBehaviors(DataSource)) {
				behavior.Apply(this);
			}
		}
		#endregion

		///<summary>Copies the settings of a view object to the current one.</summary>
		public override void Assign(BaseView v, bool copyEvents) {
			base.Assign(v, copyEvents);

			var view = v as SmartGridView;
			if (view != null) {
				AllowEditorMouseWheel = view.AllowEditorMouseWheel;
			}
		}

		protected override void OnColumnPopulate(GridColumn column, int visibleIndex) {
			base.OnColumnPopulate(column, visibleIndex);
			if (DisplaySettings.GridManager.IsSuppressed((SmartGridColumn)column))
				Columns.Remove(column);
		}

		sealed partial class MyHandler : GridHandler {
			public MyHandler(SmartGridView view) : base(view) { }

			public new SmartGridView View { get { return (SmartGridView)base.View; } }
			///<summary>Gets the SmartGrid control that owns the view, if any.</summary>
			///<remarks>In the grid designer's feature browser, this will be null, since it uses a special designer grid control.</remarks>
			public SmartGrid SmartOwner { get { return base.View.GridControl as SmartGrid; } }
		}
	}
	///<summary>Contains the columns in a SmartGridView.</summary>
	public partial class SmartGridColumnCollection : GridColumnCollection, IEnumerable<SmartGridColumn> {
		///<summary>Creates a new SmartGridColumnCollection for a SmartGridView.</summary>
		public SmartGridColumnCollection(SmartGridView view) : base(view) { }
		///<summary>Creates a new column for the collection.</summary>
		protected override GridColumn CreateColumn() { return new SmartGridColumn(); }

		///<summary>Gets the column at the given index.</summary>
		public new SmartGridColumn this[int index] { get { return (SmartGridColumn)base[index]; } }
		///<summary>Gets the column for the given field in the original datasource.</summary>
		public new SmartGridColumn this[string fieldName] { get { return (SmartGridColumn)base[fieldName]; } }

		///<summary>Gets an enumerator that returns the items in the collection.</summary>
		public new IEnumerator<SmartGridColumn> GetEnumerator() {
			for (int i = 0; i < Count; i++)		//Avoid recursion
				yield return this[i];
		}
		///<summary>Copies the contents of the collection to an array.</summary>
		public void CopyTo(SmartGridColumn[] array, int index) {
			if (array == null) throw new ArgumentNullException("array");

			for (int i = 0; i < Count; i++)
				array[index + i] = this[i];
		}
	}
}
