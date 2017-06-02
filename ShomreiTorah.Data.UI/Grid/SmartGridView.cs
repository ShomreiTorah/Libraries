using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using DevExpress.Data;
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
			public override BaseViewHandler CreateHandler(BaseView view) {
				SmartGridView smartView = (SmartGridView)view;
				if (smartView == null)
					return base.CreateHandler(view);
				return new MyHandler(smartView);
			}
			public override BaseView CreateView(GridControl grid) {
				var smartGrid = grid as SmartGrid;
				if (smartGrid == null)
					return base.CreateView(grid);
				var view = new SmartGridView(smartGrid);
				view.SetGridControl(grid);
				return view;
			}
		}

		internal static BaseInfoRegistrator CreateRegistrator() { return new MyRegistrator(); }
		///<summary>Gets the view's name.</summary>
		protected override string ViewName { get { return "SmartGridView"; } }
		///<summary>Creates the collection that holds the grid's columns.</summary>
		protected override GridColumnCollection CreateColumnCollection() { return new SmartGridColumnCollection(this); }

		///<summary>Gets the columns in the view.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[XtraSerializableProperty(XtraSerializationVisibility.Collection, true, true, true, 0, XtraSerializationFlags.DefaultValue), XtraSerializablePropertyId(LayoutIdColumns)]
		public new SmartGridColumnCollection Columns { get { return (SmartGridColumnCollection)base.Columns; } }
		private new MyHandler Handler { get { return (MyHandler)base.Handler; } }

		///<summary>Creates a new SmartGridView.</summary>
		public SmartGridView() : this(null) { }
		///<summary>Creates a new SmartGridView in a SmartGridControl.</summary>
		public SmartGridView(SmartGrid grid)
			: base(grid) {
			this.AddControllerHandlers();   //See ColumnController.cs

			OptionsView.BestFitMaxRowCount = 50;
		}

		///<summary>Indicates whether the view will show tooltips.</summary>
		///<remarks>We want tooltips even inside of cell editors.</remarks>
		protected override bool CanShowHint {
			get { return base.CanShowHint || IsEditing; }
		}

		object lastAppliedDataSource;
		///<summary>Handles the DataSourceChanged event for the grid view's DataController.</summary>
		protected override void OnDataController_DataSourceChanged(object sender, EventArgs e) {
			base.OnDataController_DataSourceChanged(sender, e);
			if (DataSource != null || DesignMode) {
				if (GridControl == null) return;
				if (lastAppliedDataSource != null && lastAppliedDataSource == DataSource)
					return;
				if (DataController.Columns.Cast<DataColumnInfo>().All(dci => dci.Unbound))
					return; //Ignore fake datasources such as DataContexts that have no real columns.

				ApplyBehaviors();
				ApplyColumnControllers();
				BestFitColumns();
				lastAppliedDataSource = DataSource;
			}
		}
		#region Behaviors
		void ApplyBehaviors() {
#if DEBUG
			if (!IsDesignMode && AppFramework.Current != null && !AppFramework.Current.IsDesignTime)
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

		///<summary>Called when a column is automatically added from the datasource.</summary>
		protected override void OnColumnPopulate(GridColumn column, int visibleIndex) {
			base.OnColumnPopulate(column, visibleIndex);
			if (DisplaySettings.GridManager.IsSuppressed((SmartGridColumn)column))
				Columns.Remove(column);
		}

		internal void SendKeyDown(KeyEventArgs e) { Handler.SendKeyDown(e); }
		internal void SendKeyPress(KeyPressEventArgs e) { Handler.SendKeyPress(e); }
		internal void SendKeyUp(KeyEventArgs e) { Handler.SendKeyUp(e); }
		sealed partial class MyHandler : GridHandler {
			internal void SendKeyDown(KeyEventArgs e) { OnKeyDown(e); }
			internal void SendKeyPress(KeyPressEventArgs e) { OnKeyPress(e); }
			internal void SendKeyUp(KeyEventArgs e) { OnKeyUp(e); }

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
			for (int i = 0; i < Count; i++)     //Avoid recursion
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
