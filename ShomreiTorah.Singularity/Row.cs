using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ShomreiTorah.Common;

namespace ShomreiTorah.Singularity {
	///<summary>An untyped row in a Singularity table.</summary>
	public partial class Row : ISchemaItem {
		///<summary>Initializes a new instance of the <see cref="Row"/> class for a given schema.</summary>
		public Row(TableSchema schema) {
			if (schema == null) throw new ArgumentNullException("schema");
			Schema = schema;
			values = schema.Columns.ToDictionary(c => c, c => c.DefaultValue);
			Schema.AddRow(this);
			if (Schema.PrimaryKey != null && schema.PrimaryKey.DataType == typeof(Guid) && schema.PrimaryKey.DefaultValue == null)
				values[Schema.PrimaryKey] = Guid.NewGuid();	//Bypass the indexer to ignore child validation (Virtual method calls from ctor)
		}

		readonly Dictionary<Column, object> values;

		internal void OnColumnAdded(Column column) { values.Add(column, column.DefaultValue); }
		internal void OnColumnRemoved(Column column) { values.Remove(column); }

		///<summary>Gets the schema of this row.</summary>
		public TableSchema Schema { get; private set; }
		///<summary>Gets the Table containing this row, if any.</summary>
		public Table Table { get; internal set; }

		///<summary>Returns a string containing the row's values.</summary>
		public override string ToString() {
			return Schema.Name + " {" + Schema.Columns.Join(", ", c => c.Name + " = " + this[c]) + "}";
		}

		///<summary>Gets or sets the value of the specified column.</summary>
		public object this[string name] {
			get {
				var col = Schema.Columns[name];
				if (col == null) throw new ArgumentException("The " + Schema.Name + " schema has no " + name + "column.", "name");
				return this[col];
			}
			set {
				var col = Schema.Columns[name];
				if (col == null) throw new ArgumentException("The " + Schema.Name + " schema has no " + name + "column.", "name");
				this[col] = value;
			}
		}
		///<summary>Gets or sets the value of the specified column.</summary>
		[SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers")]
		public object this[Column column] {
			get {
				var value = values[column];
				if (value == CalculatedColumn.UncalculatedValue) {
					value = ((CalculatedColumn)column).CalculateValue(this);
					values[column] = value;
				}
				return values[column];
			}
			set {
				if (column == null) throw new ArgumentNullException("column");
				if (column.Schema != Schema) throw new ArgumentException("Column must belong to same schema", "column");
				if (column.ReadOnly) throw new InvalidOperationException("A read-only column cannot be modified");

				if (value == DBNull.Value) value = null;
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

		///<summary>Processes an explicit change of a column value.</summary>
		///<remarks>This method is overridden by typed rows to perform custom logic.  It is not called for calculated columns.</remarks>
		internal protected virtual void OnValueChanged(Column column, object oldValue, object newValue) {
			if (column == null) throw new ArgumentNullException("column");

			column.OnValueChanged(this, oldValue, newValue);
			RaiseValueChanged(column, oldValue);
		}

		void RaiseValueChanged(Column column, object oldValue) {
			if (Table != null) {
				Table.ProcessValueChanged(this, column, oldValue);

				//Raise ValueChanged events on every foreign
				//row that contains this row as a child.  If
				//the ChildRowCollection for the parent row 
				//doesn't exist, the event obviously has no 
				//handlers, so I can skip it.  If the column
				//that changed is a ForeignKeyColumn, don't 
				//raise a ValueChanged event for its parent 
				//row (which just got a RowAdded event)
				foreach (var fkc in Schema.Columns.OfType<ForeignKeyColumn>().Where(fkc => fkc != column)) {
					var childRow = Field<Row>(fkc);
					if (childRow == null) continue;
					var parentCollection = childRow.ChildRows(fkc.ChildRelation, false);
					if (parentCollection != null)	//If the ChildRowCollection for this parent row has been created,
						parentCollection.OnValueChanged(new ValueChangedEventArgs(this, column));
				}
			}
		}

		///<summary>Called when the row is added to a table.</summary>
		///<remarks>This method allows typed rows to add logic.</remarks>
		protected internal virtual void OnAdded() { }
		///<summary>Called before the row is removed from its table.</summary>
		///<remarks>This method  allows inherited rows to add logic.</remarks>
		protected internal virtual void OnRemoving() { }

		///<summary>Clears the current value from a calculated column, causing it to be recalculated next time the value is retrieved.</summary>
		internal void InvalidateCalculatedValue(CalculatedColumn column) {
			values[column] = CalculatedColumn.UncalculatedValue;
			RaiseValueChanged(column, oldValue: DBNull.Value);	//Passing the old value would force unnecessary recalcs
		}
		///<summary>Toggles the value of a calculated column between the default value (if the row is detached) and the uncalculated value (if the row is attached).</summary>
		///<remarks>This is called by each CalculatedColumn when the row is added to or removed from the table.</remarks>
		internal void ToggleCalcColDefault(CalculatedColumn column) {
			values[column] = Table == null ? column.DefaultValue : CalculatedColumn.UncalculatedValue;
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

		///<summary>A unique identifier for the version row stored in SQL Server.</summary>
		///<remarks>This property is the only piece of the Singularity table engine that is
		///coupled to the Sql namespace.  Having this property avoids an ugly dictionary.</remarks>
		internal object RowVersion { get; set; }
	}

	///<summary>Provides data for row events.</summary>
	public class RowEventArgs : EventArgs {
		///<summary>Creates a new RowEventArgs instance.</summary>
		public RowEventArgs(Row row) { Row = row; }

		///<summary>Gets the row.</summary>
		public Row Row { get; private set; }
	}
	///<summary>Provides data for row events in a list.</summary>
	public class RowListEventArgs : RowEventArgs {
		///<summary>Creates a new RowListEventArgs instance.</summary>
		public RowListEventArgs(Row row, int index) : base(row) { Index = index; }

		///<summary>Gets the index of the row in the collection.</summary>
		public int Index { get; private set; }
	}
}
