using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
}
