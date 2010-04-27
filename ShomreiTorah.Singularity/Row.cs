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
			if (Table != null)
				Table.ProcessValueChanged(this, column);
		}

		#region Child Relations
		//Each relation's ChildRowCollection is created when first asked for.
		//After it's created, it gets maintained by ForeignKeyColumn.OnValueChanged.

		readonly Dictionary<ChildRelation, IChildRowCollection<Row>> childRelations = new Dictionary<ChildRelation, IChildRowCollection<Row>>();
		internal void OnRelationRemoved(ChildRelation relation) { childRelations.Remove(relation); }	//Won't throw if not found

		///<summary>Gets the child rows in the specified child relation.</summary>
		///<returns>A ChildRowCollection containing a live view of the child rows.</returns>
		public IChildRowCollection<Row> ChildRows(string relationName) { return ChildRows(Schema.ChildRelations[relationName]); }
		///<summary>Gets the child rows in the specified child relation.</summary>
		///<returns>A ChildRowCollection containing a live view of the child rows.</returns>
		public IChildRowCollection<Row> ChildRows(ChildRelation relation) { return ChildRows(relation, true); }
		internal IChildRowCollection<Row> ChildRows(ChildRelation relation, bool forceCreate) {
			if (relation == null) throw new ArgumentNullException("relation");

			if (Table == null)
				throw new InvalidOperationException("Child relations cannot be used on detached rows");
			if (Table.Context == null)
				throw new InvalidOperationException("Child relations can only be used for tables inside a DataContext");

			IChildRowCollection<Row> retVal;
			if (!childRelations.TryGetValue(relation, out retVal)
			 && forceCreate) {
				var childTable = Table.Context.Tables[relation.ChildSchema];
				if (childTable == null)
					throw new InvalidOperationException("Cannot find " + relation.ChildSchema.Name + " table");

				retVal = CreateChildRowCollection(relation, childTable.Rows.Where(r => r[relation.ChildColumn] == this));
				childRelations.Add(relation, retVal);
			}
			return retVal;
		}
		///<summary>Gets the typed child rows in the specified child relation.</summary>
		///<returns>A typed ChildRowCollection containing a live view of the child rows.</returns>
		protected IChildRowCollection<TChildRow> ChildRows<TChildRow>(ForeignKeyColumn column) where TChildRow : Row {
			if (column == null) throw new ArgumentNullException("column");

			return (IChildRowCollection<TChildRow>)ChildRows(column.ChildRelation);
		}

		///<summary>Creates an IChildRowCollection for the given child relation.  Overridden by typed rows.</summary>
		protected virtual IChildRowCollection<Row> CreateChildRowCollection(ChildRelation relation, IEnumerable<Row> childRows) {
			if (relation == null) throw new ArgumentNullException("relation");
			if (childRows == null) throw new ArgumentNullException("childRows");

			return new UntypedChildRowCollection<Row>(this, relation, Table.Context.Tables[relation.ChildSchema], childRows);
		}
		///<summary>Creates a typed IChildRowCollection.  Used by overridden CreateChildRowCollection implementations.</summary>
		///<remarks>This function instantiates a private child class that implements the generic  IChildRowCollection.</remarks>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Runtime type")]
		protected IChildRowCollection<Row> InstantiateChildRowCollection<TChildRow>(ChildRelation relation, IEnumerable<Row> childRows) where TChildRow : Row {
			if (relation == null) throw new ArgumentNullException("relation");

			return new TypedChildRowCollection<TChildRow>(this, relation, Table.Context.Tables[relation.ChildSchema], childRows);
		}

		class UntypedChildRowCollection<TChildRow> : ReadOnlyCollection<TChildRow>, IChildRowCollection<Row>, IMutableChildRowCollection where TChildRow : Row {
			internal UntypedChildRowCollection(Row parentRow, ChildRelation relation, Table childTable, IEnumerable<Row> childRows)
				: base(childRows.Cast<TChildRow>().ToList()) {
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

			public void AddRow(Row childRow) {
				Items.Add((TChildRow)childRow);
				OnRowAdded(new RowEventArgs(childRow));
			}
			public void RemoveRow(Row childRow) {
				Items.Remove((TChildRow)childRow);
				OnRowRemoved(new RowEventArgs(childRow));
			}

			public event EventHandler<RowEventArgs> RowAdded;
			void OnRowAdded(RowEventArgs e) {
				if (RowAdded != null)
					RowAdded(this, e);
			}
			public event EventHandler<RowEventArgs> RowRemoved;
			void OnRowRemoved(RowEventArgs e) {
				if (RowRemoved != null)
					RowRemoved(this, e);
			}

			public bool Contains(Row row) { return row != null && row.Table == ChildTable && row[Relation.ChildColumn] == ParentRow; }

			IEnumerator<Row> IEnumerable<Row>.GetEnumerator() {
				foreach (TChildRow row in this)	 //If I'm not careful, I'll get a nasty stack overflow.
					yield return row;
			}
			Row IChildRowCollection<Row>.this[int index] { get { return this[index]; } }
		}

		sealed class TypedChildRowCollection<TChildRow> : UntypedChildRowCollection<TChildRow>, IChildRowCollection<TChildRow> where TChildRow : Row {
			internal TypedChildRowCollection(Row parentRow, ChildRelation relation, Table childTable, IEnumerable<Row> childRows)
				: base(parentRow, relation, childTable, childRows) { }
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



	///<summary>Provides data for row events.</summary>
	public class RowEventArgs : EventArgs {
		///<summary>Creates a new RowEventArgs instance.</summary>
		public RowEventArgs(Row row) { Row = row; }

		///<summary>Gets the row.</summary>
		public Row Row { get; private set; }
	}
}
