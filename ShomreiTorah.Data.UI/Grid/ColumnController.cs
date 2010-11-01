using System;
using System.ComponentModel;
using System.Drawing;
using DevExpress.Utils;
using DevExpress.Utils.Drawing;
using DevExpress.Utils.Serializing;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;

namespace ShomreiTorah.Data.UI.Grid {
	///<summary>Controls the behavior of a column in a SmartGridView.</summary>
	///<remarks>Single instances of this class will be used to control multiple columns in multiple grids.</remarks>
	public class ColumnController {
		///<summary>Creates an empty ColumnController instance.  This constructor should be called by inherited classes.</summary>
		protected ColumnController() { }

		readonly Action<SmartGridColumn> configurator;
		///<summary>Creates a ColumnController instance that runs a configurator delegate.</summary>
		///<param name="configurator">A delegate that sets a column's properties.</param>
		public ColumnController(Action<SmartGridColumn> configurator) {
			if (configurator == null) throw new ArgumentNullException("configurator");
			this.configurator = configurator;
		}

		///<summary>Applies this controller to a column.  This method should set the column's properties.</summary>
		protected internal virtual void Apply(SmartGridColumn column) {
			configurator(column);
		}

		///<summary>Allows the controller to provide a custom display text for its column.</summary>
		protected internal virtual string GetDisplayText(object row, object value) { return null; }
		///<summary>Allows the controller to customize the items in the column's filter popup.</summary>
		protected internal virtual void OnShowFilterPopupListBox(FilterPopupListBoxEventArgs e) { }
		///<summary>Allows the controller to provide a custom tooltip for the cells in the column.</summary>
		protected internal virtual SuperToolTip GetCellToolTip(object row, object value) { return null; }
		///<summary>Allows the controller to customize the column's sorting comparisons.</summary>
		protected internal virtual void CompareValues(CustomColumnSortEventArgs e) { }
		///<summary>Allows the controller to provide data for an unbound column.</summary>
		protected internal virtual void OnCustomUnboundColumnData(CustomColumnDataEventArgs e) { }
	}

	partial class SmartGridColumn {
		///<summary>Gets the column controller used by the column, if any.</summary>
		public ColumnController Controller { get; private set; }

		object controllerDataSource;
		internal void ActivateController(bool force = false) {
			if (String.IsNullOrEmpty(FieldName)
			 || View == null
			 || (!DesignMode && View.DataSource == null) || View.GridControl == null
			 || (controllerDataSource != null && controllerDataSource == View.DataSource && !force))	//Or the datasource hasn't changed since last time
				return;

			var editorSettings = DisplaySettings.EditorRepository.GetSettings(this);
			if (editorSettings != null)
				SetDefaultEditor(editorSettings.CreateItem());

			Controller = DisplaySettings.GridManager.GetController(this);
			if (Controller != null)
				Controller.Apply(this);
			controllerDataSource = View.DataSource;
		}

		//There are no virtual methods anywhere in the 
		//code paths taken by the FieldName setter.  To
		//activate ColumnControllers, I need to replace
		//the property.  I only activate controllers if
		//the column is in a view. (since I need a data
		//source)
		///<summary>Gets or sets the name of the database field assigned to the current column.</summary>
		[Description("Gets or sets the name of the database field assigned to the current column.")]
		[Category("Data")]
		[DefaultValue("")]
		[XtraSerializableProperty]
		[TypeConverter("DevExpress.XtraGrid.TypeConverters.FieldNameTypeConverter, " + AssemblyInfo.SRAssemblyGridDesign)]
		public new string FieldName {
			get { return base.FieldName; }
			set {
				if (value == FieldName) return;
				base.FieldName = value;

				//The ActivateController method checks that we have a fieldname and datasource.
				ActivateController(force: true);
			}
		}
	}
	partial class SmartGridView {
		void AddControllerHandlers() {
			this.ShowFilterPopupListBox += SmartGridView_ShowFilterPopupListBox;
		}
		protected override void RaiseCustomColumnSort(CustomColumnSortEventArgs e) {
			base.RaiseCustomColumnSort(e);
			var column = e.Column as SmartGridColumn;
			if (column != null && column.Controller != null)
				column.Controller.CompareValues(e);
		}
		protected override void RaiseCustomUnboundColumnData(CustomColumnDataEventArgs e) {
			var column = e.Column as SmartGridColumn;
			if (column != null && column.Controller != null)
				column.Controller.OnCustomUnboundColumnData(e);
			base.RaiseCustomUnboundColumnData(e);
		}
		void SmartGridView_ShowFilterPopupListBox(object sender, FilterPopupListBoxEventArgs e) {
			var column = e.Column as SmartGridColumn;
			if (column != null && column.Controller != null)
				column.Controller.OnShowFilterPopupListBox(e);
		}
		protected override void RaiseCustomColumnDisplayText(CustomColumnDisplayTextEventArgs e) {
			base.RaiseCustomColumnDisplayText(e);
			var column = e.Column as SmartGridColumn;
			if (column != null) {
				if (column.Controller != null)
					e.DisplayText = column.Controller.GetDisplayText(null, e.Value) ?? e.DisplayText;
			}
		}
		protected override ToolTipControlInfo GetToolTipObjectInfoCore(GraphicsCache cache, Point p) {
			var baseInfo = base.GetToolTipObjectInfoCore(cache, p);
			if (baseInfo != null)
				return baseInfo;
			var ht = GetHintObjectInfo() as GridHitInfo;
			if (ht != null && ht.InRowCell) {
				var column = ht.Column as SmartGridColumn;
				if (column != null && column.Controller != null) {
					var superTip = column.Controller.GetCellToolTip(GetRow(ht.RowHandle), GetRowCellValue(ht.RowHandle, column));
					if (superTip != null)
						return new ToolTipControlInfo() { SuperTip = superTip };
				}
			}
			return null;
		}

		void ApplyColumnControllers() {
			foreach (var column in Columns) {
				column.ActivateController();
			}
		}

	}
	partial class SmartGridColumnCollection {
		protected override void OnInsertComplete(int index, object obj) {
			base.OnInsertComplete(index, obj);

			var smartColumn = obj as SmartGridColumn;
			if (smartColumn != null)
				smartColumn.ActivateController();
		}
	}
}
