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
			Schema.AddRow(this);
			values = schema.Columns.ToDictionary(c => c, c => c.DefaultValue);
		}

		readonly Dictionary<Column, object> values;

		internal void OnColumnAdded(Column column) { values.Add(column, column.DefaultValue); }
		internal void OnColumnRemoved(Column column) { values.Remove(column); }

		///<summary>Gets the schema of this row.</summary>
		public TableSchema Schema { get; private set; }
		///<summary>Gets the Table containing this row, if any.</summary>
		public Table Table { get; internal set; }

		///<summary>Gets or sets the value of the specified column.</summary>
		public object this[string name] { get { return this[Schema.Columns[name]]; } set { this[Schema.Columns[name]] = value; } }
		///<summary>Gets or sets the value of the specified column.</summary>
		[SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers")]
		public object this[Column column] {
			get { return values[column]; }
			set {
				if (column == null) throw new ArgumentNullException("column");
				if (column.Schema != Schema) throw new ArgumentException("Column must belong to same schema", "column");

				var oldValue = this[column];
				var error = ValidateValue(column, value);
				if (!String.IsNullOrEmpty(error))
					throw new ArgumentException(error, "value");

				values[column] = value;
				column.OnValueChanged(this, oldValue, value);
				if (Table != null)
					Table.ProcessValueChanged(this, column);
			}
		}

		#region Child Relations
		//Each relation's ChildRowCollection is created when first asked for.
		//After it's created, it gets maintained by ForeignKeyColumn.OnValueChanged.

		readonly Dictionary<ChildRelation, ChildRowCollection> childRelations = new Dictionary<ChildRelation, ChildRowCollection>();
		internal void OnRelationRemoved(ChildRelation relation) { childRelations.Remove(relation); }	//Won't throw if not found

		///<summary>Gets the child rows in the specified child relation.</summary>
		///<returns>A ChildRowCollection containing a live view of the child rows.</returns>
		public ChildRowCollection ChildRows(string relationName) { return ChildRows(Schema.ChildRelations[relationName]); }
		///<summary>Gets the child rows in the specified child relation.</summary>
		///<returns>A ChildRowCollection containing a live view of the child rows.</returns>
		public ChildRowCollection ChildRows(ChildRelation relation) { return ChildRows(relation, true); }
		internal ChildRowCollection ChildRows(ChildRelation relation, bool forceCreate) {
			if (Table == null)
				throw new InvalidOperationException("Child relations cannot be used on detached rows");
			if (Table.Context == null)
				throw new InvalidOperationException("Child relations can only be used for tables inside a DataContext");

			ChildRowCollection retVal;
			if (!childRelations.TryGetValue(relation, out retVal)
			 && forceCreate) {
				var childTable = Table.Context.Tables[relation.ChildSchema];
				if (childTable == null)
					throw new InvalidOperationException("Cannot find " + relation.ChildSchema.Name + " table");

				retVal = new ChildRowCollection(this, relation, childTable, childTable.Rows.Where(r => r[relation.ChildColumn] == this));
				childRelations.Add(relation, retVal);
			}
			return retVal;
		}
		#endregion

		#region Helpers
		///<summary>Removes the row from its table.</summary>
		public virtual void RemoveRow() {
			if (Table == null) throw new InvalidOperationException("Cannot delete a detached row");
			Table.Rows.Remove(this);
		}

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
	public class RowCollection : Collection<Row> {
		///<summary>Creates a RowColelction for the specified table.</summary>
		public RowCollection(Table table) {
			if (table == null) throw new ArgumentNullException("table");

			Table = table;
		}

		///<summary>Gets the table containing the rows.</summary>
		public Table Table { get; private set; }

		///<summary>Adds a row from an array of values.</summary>
		public Row AddFromValues(params object[] values) {
			if (values == null) throw new ArgumentNullException("values");

			var retVal = new Row(Table.Schema);
			int index = 0;
			foreach (var col in Table.Schema.Columns) {
				retVal[col] = values[index];
				index++;
			}
			Add(retVal);
			return retVal;
		}
	}

	///<summary>Provides data for row events.</summary>
	public class RowEventArgs : EventArgs {
		///<summary>Creates a new RowEventArgs instance.</summary>
		public RowEventArgs(Row row) { Row = row; }

		///<summary>Gets the row.</summary>
		public Row Row { get; private set; }
	}

	///<summary>A collection of child rows belonging to a parent row.</summary>
	public class ChildRowCollection : ReadOnlyCollection<Row> {
		internal ChildRowCollection(Row parentRow, ChildRelation relation, Table childTable, IEnumerable<Row> childRows)
			: base(childRows.ToList()) {
			ParentRow = parentRow;
			ChildTable = childTable;
			Relation = relation;
		}

		///<summary>Gets the parent row for the collection's rows.</summary>
		public Row ParentRow { get; private set; }
		///<summary>Gets the child relation that this collection contains.</summary>
		public ChildRelation Relation { get; private set; }
		///<summary>Gets the child table that this collection contains rows from.</summary>
		public Table ChildTable { get; private set; }

		internal void AddRow(Row childRow) {
			Items.Add(childRow);
			OnRowAdded(new RowEventArgs(childRow));
		}
		internal void RemoveRow(Row childRow) {
			Items.Remove(childRow);
			OnRowRemoved(new RowEventArgs(childRow));
		}

		///<summary>Occurs when a row is added to the collection.</summary>
		public event EventHandler<RowEventArgs> RowAdded;
		///<summary>Raises the RowAdded event.</summary>
		///<param name="e">A RowEventArgs object that provides the event data.</param>
		protected virtual void OnRowAdded(RowEventArgs e) {
			if (RowAdded != null)
				RowAdded(this, e);
		}
		///<summary>Occurs when a row is removed from the collection.</summary>
		public event EventHandler<RowEventArgs> RowRemoved;
		///<summary>Raises the RowRemoved event.</summary>
		///<param name="e">A RowEventArgs object that provides the event data.</param>
		protected virtual void OnRowRemoved(RowEventArgs e) {
			if (RowRemoved != null)
				RowRemoved(this, e);
		}
	}
}
