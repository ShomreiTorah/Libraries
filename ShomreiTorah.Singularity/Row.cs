using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Collections.ObjectModel;

namespace ShomreiTorah.Singularity {
	///<summary>An untyped row in a Singularity table.</summary>
	public class Row {
		///<summary>Initializes a new instance of the <see cref="Row"/> class for a given schema.</summary>
		public Row(TableSchema schema) {
			if (schema == null) throw new ArgumentNullException("schema");
			Schema = schema;
			values = schema.Columns.ToDictionary(c => c, c => c.DefaultValue);
		}

		readonly Dictionary<Column, object> values;

		///<summary>Gets the schema of this row.</summary>
		public TableSchema Schema { get; private set; }
		///<summary>Gets the Table containing this row, if any.</summary>
		public Table Table { get; internal set; }

		///<summary>Gets or sets the value of the specified column.</summary>
		public object this[string name] {
			get { return this[Schema.Columns[name]]; }
			set { this[Schema.Columns[name]] = value; }
		}

		///<summary>Gets or sets the value of the specified column.</summary>
		[SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers")]
		public object this[Column column] {
			get { return values[column]; }
			set {
				var error = ValidateValue(column, value);
				if (!String.IsNullOrEmpty(error))
					throw new ArgumentException(error, "value");

				values[column] = value;
				if (Table != null)
					Table.ProcessValueChanged(this, column);
			}
		}

		#region Helpers
		///<summary>Checks whether a value would be valid for a given column.</summary>
		///<param name="column">The column containing the value.</param>
		///<param name="newValue">The value to validate.</param>
		///<returns>An error message, or null if the value is valid.</returns>
		public string ValidateValue(Column column, object newValue) { return Schema.ValidateValue(this, column, newValue); }
		///<summary>Checks whether a value would be valid for a given column.</summary>
		///<param name="column">The name of column containing the value.</param>
		///<param name="newValue">The value to validate.</param>
		///<returns>An error message, or null if the value is valid.</returns>
		public string ValidateValue(string column, object newValue) { return Schema.ValidateValue(this, Schema.Columns[column], newValue); }
		///<summary>Gets the value of the specified column.</summary>
		public T Field<T>(string name) { return (T)this[name]; }
		///<summary>Gets the value of the specified column.</summary>
		public T Field<T>(Column column) { return (T)this[column]; }
		#endregion
	}
	///<summary>A collection of Row objects.</summary>
	public class RowCollection : Collection<Row> { }

	///<summary>Provides data for row events.</summary>
	public class RowEventArgs : EventArgs {
		///<summary>Creates a new RowEventArgs instance.</summary>
		public RowEventArgs(Row row) { Row = row; }

		///<summary>Gets the row.</summary>
		public Row Row { get; private set; }
	}
}
