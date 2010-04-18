using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShomreiTorah.Singularity {
	///<summary>Contains the schema of a Singularity table.</summary>
	public class TableSchema {
		///<summary>Initializes a new instance of the <see cref="TableSchema"/> class.</summary>
		public TableSchema() { Columns = new ColumnCollection(); }

		///<summary>Gets the columns in this schema.</summary>
		public ColumnCollection Columns { get; private set; }

		///<summary>Checks whether a value is valid for a given column in a given row.</summary>
		///<param name="row">The row to validate for.</param>
		///<param name="column">The column containing the value.</param>
		///<param name="newValue">The value to validate.</param>
		///<returns>An error message, or null if the value is valid.</returns>
		public virtual string ValidateValue(Row row, Column column, object newValue) {
			if (row == null) throw new ArgumentNullException("row");
			if (column == null) throw new ArgumentNullException("column");
			if (row.Schema != this) throw new ArgumentException("Row must belong to this schema", "row");
			if (!Columns.Contains(column)) throw new ArgumentException("Column must belong to this schema", "column");

			//TODO: Conversions (numeric, Nullable<T>, DBNull, etc)
			if (!column.DataType.IsInstanceOfType(newValue))
				return "The " + column.Name + " column cannot hold a " + (newValue == null ? "null" : newValue.GetType().Name) + " value.";
			return null;
		}
	}
}
