using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace ShomreiTorah.Singularity {
	///<summary>A table in a Singularity database.</summary>
	public partial class Table {
		///<summary>Creates an empty table.</summary>
		public Table(string name) : this(new TableSchema(name)) { }
		///<summary>Creates a table from an existing schema.</summary>
		public Table(TableSchema schema) {
			Schema = schema;

			if (Rows == null)	//If not set by TypedTable
				Rows = new RowCollection<Row>(this);
		}

		///<summary>Gets the DataContext that contains this table.</summary>
		public DataContext Context { get; internal set; }
		///<summary>Gets the schema of this table.</summary>
		public TableSchema Schema { get; private set; }
		///<summary>Gets the schema of this table.</summary>
		public ITableRowCollection<Row> Rows { get; internal set; }

		///<summary>Returns a string representation of this instance.</summary>
		public override string ToString() { return "Table: " + Schema.Name; }

		///<summary>A collection of strongly-typed rows (See TypedTable) that can be exposed as a collection of weakly-typed rows.</summary>
		//This class inherits Collection<TRow> to make the data storage strongly 
		//typed, and implements ITableRowCollection<Row> for the base Table class
		//class to expose as a weakly-typed collection.  Thee single class cannot
		//implement ITableRowCollection twice with different parameters, so there
		//is an inherited TypedRowCollection in TypedTable for ITableRowCollection<TRow>.
		internal class RowCollection<TRow> : Collection<TRow>, ITableRowCollection<Row> where TRow : Row {
			internal RowCollection(Table table) { Table = table; }

			public Table Table { get; private set; }

			///<summary>Creates a detached row for this table.</summary>
			///<remarks>Overridden by typed tables.</remarks>
			internal virtual TRow CreateRow() { return (TRow)new Row(Table.Schema); }
			public TRow AddFromValues(params object[] values) {
				if (values == null) throw new ArgumentNullException("values");

				var retVal = CreateRow();
				int index = 0;
				foreach (var col in Table.Schema.Columns) {
					retVal[col] = values[index];
					index++;
				}
				Add(retVal);
				return retVal;
			}

			protected override void ClearItems() {
				Table.ProcessClearing();
				base.ClearItems();
				Table.OnTableCleared();
			}
			protected override void InsertItem(int index, TRow item) {
				Table.ValidateAddRow(item);
				base.InsertItem(index, item);
				Table.ProcessRowAdded(item);
			}
			protected override void RemoveItem(int index) {
				var row = this[index];
				base.RemoveItem(index);
				Table.ProcessRowRemoved(row, true);
			}
			protected override void SetItem(int index, TRow item) {
				Table.ValidateAddRow(item);
				var oldRow = this[index];
				base.SetItem(index, item);
				Table.ProcessRowRemoved(oldRow, true);
				Table.ProcessRowAdded(item);
			}

			#region ITableRowCollection<Row> passthroughs
			Row ITableRowCollection<Row>.AddFromValues(params object[] values) { return AddFromValues(values); }

			Row IList<Row>.this[int index] {
				get { return this[index]; }
				set { this[index] = (TRow)value; }
			}

			int IList<Row>.IndexOf(Row item) { return IndexOf((TRow)item); }
			void IList<Row>.Insert(int index, Row item) { Insert(index, (TRow)item); }
			void ICollection<Row>.Add(Row item) { Add((TRow)item); }
			bool ICollection<Row>.Contains(Row item) { return Contains((TRow)item); }
			void ICollection<Row>.CopyTo(Row[] array, int arrayIndex) { CopyTo((TRow[])array, arrayIndex); }
			bool ICollection<Row>.Remove(Row item) { return Remove((TRow)item); }
			IEnumerator<Row> IEnumerable<Row>.GetEnumerator() {
				foreach (TRow row in this) //If I'm not careful, I'll get a nasty stack overflow.
					yield return row;
			}
			bool ICollection<Row>.IsReadOnly { get { return false; } }
			#endregion
		}
		void ValidateAddRow(Row row) {
			if (row.Table != null)
				throw new ArgumentException("Row is already in a table", "row");
			try {
				row.Table = this;
				foreach (var column in Schema.Columns) {
					var error = Schema.ValidateValue(row, column, row[column]);
					if (!String.IsNullOrEmpty(error))
						throw new InvalidOperationException(error);
				}
			} finally { row.Table = null; }
		}

		void ProcessRowAdded(Row row) {
			row.Table = this;
			foreach (var column in Schema.Columns)
				column.OnRowAdded(row);		//Adds the row to parent relations
			OnRowAdded(new RowEventArgs(row));
		}
		void ProcessRowRemoved(Row row, bool raiseEvent) {
			row.Table = null;
			Schema.RemoveRow(row);
			foreach (var column in Schema.Columns)
				column.OnRowRemoved(row);	//Removes the row from parent relations

			if (raiseEvent)
				OnRowRemoved(new RowEventArgs(row));
		}
		void ProcessClearing() {
			foreach (var row in Rows)
				ProcessRowRemoved(row, false);
		}
		internal void ProcessValueChanged(Row row, Column column) {
			OnValueChanged(new ValueChangedEventArgs(row, column));
		}

		#region Events
		///<summary>Occurs when the table is cleared.</summary>
		public event EventHandler TableCleared;
		///<summary>Raises the TableCleared event.</summary>
		internal protected virtual void OnTableCleared() { OnTableCleared(EventArgs.Empty); }
		///<summary>Raises the TableCleared event.</summary>
		///<param name="e">An EventArgs object that provides the event data.</param>
		protected virtual void OnTableCleared(EventArgs e) {
			if (TableCleared != null)
				TableCleared(this, e);
		}

		///<summary>Occurs when a row is added to the table.</summary>
		public event EventHandler<RowEventArgs> RowAdded;
		///<summary>Raises the RowAdded event.</summary>
		///<param name="e">A RowEventArgs object that provides the event data.</param>
		protected virtual void OnRowAdded(RowEventArgs e) {
			if (RowAdded != null)
				RowAdded(this, e);
		}
		///<summary>Occurs when a row is removed from the table.</summary>
		public event EventHandler<RowEventArgs> RowRemoved;
		///<summary>Raises the RowRemoved event.</summary>
		///<param name="e">A RowEventArgs object that provides the event data.</param>
		protected virtual void OnRowRemoved(RowEventArgs e) {
			if (RowRemoved != null)
				RowRemoved(this, e);
		}
		///<summary>Occurs when a column value is changed.</summary>
		public event EventHandler<ValueChangedEventArgs> ValueChanged;
		///<summary>Raises the ValueChanged event.</summary>
		///<param name="e">A ValueChangedEventArgs object that provides the event data.</param>
		protected virtual void OnValueChanged(ValueChangedEventArgs e) {
			if (ValueChanged != null)
				ValueChanged(this, e);
		}
		#endregion
	}
	///<summary>Provides data for the ValueChanged event.</summary>
	public class ValueChangedEventArgs : RowEventArgs {
		///<summary>Creates a new ValueChangedEventArgs instance.</summary>
		public ValueChangedEventArgs(Row row, Column column) : base(row) { Column = column; }

		///<summary>Gets the column.</summary>
		public Column Column { get; private set; }
	}
	///<summary>A collection of Table objects.</summary>
	public sealed class TableCollection : ReadOnlyCollection<Table>, ICollection<Table> {
		internal TableCollection(DataContext context) : base(new List<Table>()) { Context = context; }
		///<summary>Gets the DataContext that contains this table.</summary>
		public DataContext Context { get; private set; }

		///<summary>Gets the table with the given name, or null if there is no table with that name.</summary>
		public Table this[string name] { get { return this.FirstOrDefault(t => t.Schema.Name == name); } }
		///<summary>Gets the table with the given schema, or null if there is no table with that schema.</summary>
		[SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers")]
		public Table this[TableSchema schema] { get { return this.FirstOrDefault(t => t.Schema == schema); } }

		///<summary>Adds a table to the collection.</summary>
		public void AddTable(Table table) {
			if (table == null) throw new ArgumentNullException("table");

			if (this.Any(t => t.Schema == table.Schema))	//Otherwise, we get sticky situations for child relations.  (What if there are multiple tables with the child schema?)
				throw new ArgumentException("This DataContext already has a " + table.Schema.Name + " table", "table");

			table.Context = Context;
			Items.Add(table);
		}
		void ICollection<Table>.Add(Table table) { AddTable(table); }
	}
}
