using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ShomreiTorah.Singularity {
	///<summary>An untyped row in a Singularity table.</summary>
	public partial class Row {
		///<summary>Initializes a new instance of the <see cref="Row"/> class for a given schema.</summary>
		public Row(TableSchema schema) {
			if (schema == null) throw new ArgumentNullException("schema");
			Schema = schema;
			values = schema.Columns.ToDictionary(c => c, c => c.DefaultValue);
			Schema.AddRow(this);
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

				if (Equals(oldValue, value)) return;

				var error = Table == null ? ValidateValueType(column, value) : ValidateValue(column, value);
				if (!String.IsNullOrEmpty(error))
					throw new ArgumentException(error, "value");

				values[column] = value;

				OnValueChanged(column, oldValue, value);
			}
		}

		///<summary>Checks whether a value would be valid for a given column in an attached row.</summary>
		///<param name="column">The column containing the value.</param>
		///<param name="newValue">The value to validate.</param>
		///<returns>An error message, or null if the value is valid.</returns>
		///<remarks>This method is overridden by typed rows to perform custom validation logic.</remarks>
		public virtual string ValidateValue(Column column, object newValue) {
			if (column == null) throw new ArgumentNullException("column");
			return column.ValidateValue(this, newValue);
		}

		///<summary>Checks whether a value would be valid for a given column in a detached row.</summary>
		///<param name="column">The column containing the value.</param>
		///<param name="newValue">The value to validate.</param>
		///<returns>An error message, or null if the value is valid.</returns>
		///<remarks>This method is overridden by typed rows to perform custom validation logic.</remarks>
		public virtual string ValidateValueType(Column column, object newValue) {
			if (column == null) throw new ArgumentNullException("column");
			return column.ValidateValueType(newValue);
		}

		///<summary>Processes a change of a column value.</summary>
		///<remarks>This method is overridden by typed rows to perform custom logic.</remarks>
		protected virtual void OnValueChanged(Column column, object oldValue, object newValue) {
			if (column == null) throw new ArgumentNullException("column");

			column.OnValueChanged(this, oldValue, newValue);
			if (Table != null) {
				Table.ProcessValueChanged(this, column);

				//Raise ValueChanged events on every foreign
				//row that contains this row as a child.  If
				//the ChildRowCollection for the parent row 
				//doesn't exist, the event obviously has no 
				//handlers, so I can skip it.  If the column
				//that changed is a ForeignKeyColumn, don't 
				//raise a ValueChanged event for its parent 
				//row (which just got a RowAdded event)
				foreach (var parentCollection in Schema.Columns.OfType<ForeignKeyColumn>().Where(fkc => fkc != column)
													   .Select(fkc => Field<Row>(fkc).ChildRows(fkc.ChildRelation, false))) {
					if (parentCollection != null)	//If the ChildRowCollection for this parent row has been created,
						parentCollection.OnValueChanged(new ValueChangedEventArgs(this, column));
				}
			}
		}

		#region Child Relations
		//Each relation's ChildRowCollection is created when first asked for.
		//After it's created, it gets maintained by ForeignKeyColumn.OnValueChanged.

		readonly Dictionary<ChildRelation, ChildRowCollection> childRelations = new Dictionary<ChildRelation, ChildRowCollection>();
		internal void OnRelationRemoved(ChildRelation relation) { childRelations.Remove(relation); typedChildRelations.Remove(relation); }	//Won't throw if not found

		///<summary>Gets the child rows in the specified child relation.</summary>
		///<returns>A ChildRowCollection containing a live view of the child rows.</returns>
		public IChildRowCollection<Row> ChildRows(string relationName) { return ChildRows(Schema.ChildRelations[relationName]); }
		///<summary>Gets the child rows in the specified child relation.</summary>
		///<returns>A ChildRowCollection containing a live view of the child rows.</returns>
		public IChildRowCollection<Row> ChildRows(ChildRelation relation) { return ChildRows(relation, true); }

		//Returns an internal ChildRowCollection with AddRow
		//and RemoveRow methods to allow ForeignKeyColumn to
		//update the collection when it changes.
		internal ChildRowCollection ChildRows(ChildRelation relation, bool forceCreate) {
			if (relation == null) throw new ArgumentNullException("relation");

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
		///<param name="column">The name of column containing the value.</param>
		///<param name="newValue">The value to validate.</param>
		///<returns>An error message, or null if the value is valid.</returns>
		public string ValidateValue(string column, object newValue) { return ValidateValue(Schema.Columns[column], newValue); }
		///<summary>Gets the value of the specified column.</summary>
		public T Field<T>(string name) { return (T)this[name]; }
		///<summary>Gets the value of the specified column.</summary>
		public T Field<T>(Column column) { return (T)this[column]; }
		#endregion
	}

	internal sealed class ChildRowCollection : ReadOnlyCollection<Row>, IChildRowCollection<Row>, IListSource {
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
			OnRowAdded(new RowListEventArgs(childRow, Count - 1));
		}
		internal void RemoveRow(Row childRow) {
			var index = IndexOf(childRow);
			Items.RemoveAt(index);
			OnRowRemoved(new RowListEventArgs(childRow, index));
		}

		public event EventHandler<RowListEventArgs> RowAdded;
		void OnRowAdded(RowListEventArgs e) {
			if (RowAdded != null)
				RowAdded(this, e);
		}
		public event EventHandler<RowListEventArgs> RowRemoved;
		void OnRowRemoved(RowListEventArgs e) {
			if (RowRemoved != null)
				RowRemoved(this, e);
		}
		public event EventHandler<ValueChangedEventArgs> ValueChanged;
		internal void OnValueChanged(ValueChangedEventArgs e) {
			if (ValueChanged != null)
				ValueChanged(this, e);
		}

		public new bool Contains(Row row) { return row != null && row.Table == ChildTable && row[Relation.ChildColumn] == ParentRow; }

		public bool ContainsListCollection { get { return false; } }

		DataBinding.ChildRowsBinder binder;
		public System.Collections.IList GetList() {
			if (binder == null) binder = new DataBinding.ChildRowsBinder(this);
			return binder;
		}
	}

	///<summary>Provides data for row events in a list.</summary>
	public class RowListEventArgs : EventArgs {
		///<summary>Creates a new RowEventArgs instance.</summary>
		public RowListEventArgs(Row row, int index) { Row = row; Index = index; }

		///<summary>Gets the row.</summary>
		public Row Row { get; private set; }
		///<summary>Gets the index of the row in the collection.</summary>
		public int Index { get; private set; }
	}
}
