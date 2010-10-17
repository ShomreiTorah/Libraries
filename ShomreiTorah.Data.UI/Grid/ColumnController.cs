using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using DevExpress.Utils.Serializing;
using DevExpress.XtraGrid.Columns;

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
	}

	partial class SmartGridColumn {
		///<summary>Gets the column controller used by the column, if any.</summary>
		public ColumnController Controller { get; private set; }

		bool controllerActivated;
		internal void ActivateController(bool force = false) {
			if ((controllerActivated && !force)
			 || String.IsNullOrEmpty(FieldName)
			 || View == null
			 || View.DataSource == null)
				return;

			Controller = DisplaySettings.GridManager.GetController(View.DataSource, FieldName);
			if (Controller != null)
				Controller.Apply(this);
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
				controllerActivated = false;

				//The ActivateController method checks that we have a fieldname and datasource.
				ActivateController();
			}
		}

	}
	partial class SmartGridView {
		void ApplyColumnControllers(bool force) {
			foreach (var column in Columns) {
				column.ActivateController(force);
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
