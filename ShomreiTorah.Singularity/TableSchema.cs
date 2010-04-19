using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShomreiTorah.Common;

namespace ShomreiTorah.Singularity {
	///<summary>Contains the schema of a Singularity table.</summary>
	public class TableSchema {
		///<summary>Initializes a new instance of the <see cref="TableSchema"/> class.</summary>
		public TableSchema() { Columns = new ColumnCollection(this); }

		///<summary>Gets the columns in this schema.</summary>
		public ColumnCollection Columns { get; private set; }

		#region Events
		///<summary>Occurs when the schema is changed.</summary>
		public event EventHandler SchemaChanged;
		///<summary>Raises the SchemaChanged event.</summary>
		internal protected virtual void OnSchemaChanged() { OnSchemaChanged(EventArgs.Empty); }
		///<summary>Raises the SchemaChanged event.</summary>
		///<param name="e">An EventArgs object that provides the event data.</param>
		internal protected virtual void OnSchemaChanged(EventArgs e) {
			if (SchemaChanged != null)
				SchemaChanged(this, e);
		}
		///<summary>Occurs when a column is added.</summary>
		public event EventHandler<ColumnEventArgs> ColumnAdded;
		///<summary>Raises the ColumnAdded event.</summary>
		///<param name="e">A ColumnEventArgs object that provides the event data.</param>
		internal protected virtual void OnColumnAdded(ColumnEventArgs e) {
			if (ColumnAdded != null)
				ColumnAdded(this, e);
		}
		///<summary>Occurs when a column is removed.</summary>
		public event EventHandler<ColumnEventArgs> ColumnRemoved;
		///<summary>Raises the ColumnRemoved event.</summary>
		///<param name="e">A ColumnEventArgs object that provides the event data.</param>
		internal protected virtual void OnColumnRemoved(ColumnEventArgs e) {
			if (ColumnRemoved != null)
				ColumnRemoved(this, e);
		}
		#endregion

		//Tables can never be removed
		List<WeakReference<Table>> tables = new List<WeakReference<Table>>();
		internal void AddTable(Table table) { tables.Add(new WeakReference<Table>(table)); }
		internal void EachTable(Action<Table> method) {
			foreach (var table in tables.Select(w => w.Target).Where(t => t != null))
				method(table);
		}

		///<summary>Checks whether a value is valid for a given column in a given row.</summary>
		///<param name="row">The row to validate for.</param>
		///<param name="column">The column containing the value.</param>
		///<param name="newValue">The value to validate.</param>
		///<returns>An error message, or null if the value is valid.</returns>
		public virtual string ValidateValue(Row row, Column column, object newValue) {
			if (row == null) throw new ArgumentNullException("row");
			if (column == null) throw new ArgumentNullException("column");
			if (row.Schema != this) throw new ArgumentException("Row must belong to this schema", "row");
			if (column.Schema != this) throw new ArgumentException("Column must belong to this schema", "column");

			return column.ValidateValue(newValue);
		}
	}
}
