using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using DevExpress.Utils.Serializing;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;

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
		protected internal virtual string GetDisplayText(object row, object value) {
			return null;
		}
		protected internal virtual void OnShowFilterPopupListBox(FilterPopupListBoxEventArgs e) {
		}
	}

	partial class SmartGridColumn {
		///<summary>Gets the column controller used by the column, if any.</summary>
		public ColumnController Controller { get; private set; }

		object controllerDataSource;
		internal void ActivateController(bool force = false) {
			if (String.IsNullOrEmpty(FieldName)
			 || View == null
			 || View.DataSource == null
			 || (controllerDataSource == View.DataSource && !force))	//Or the datasource hasn't changed since last time
				return;

			var editorSettings = DisplaySettings.EditorRepository.GetSettings(View.DataSource, FieldName);
			if (editorSettings != null)
				SetDefaultEditor(editorSettings.CreateItem());

			Controller = DisplaySettings.GridManager.GetController(View.DataSource, FieldName);
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
