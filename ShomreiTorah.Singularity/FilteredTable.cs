using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using ShomreiTorah.Singularity.Dependencies;

namespace ShomreiTorah.Singularity {
	///<summary>A filtered view of an existing table.</summary>
	public class FilteredTable<TRow> : IListSource, IDisposable, ISchemaItem where TRow : Row {
		readonly ITable<TRow> typedTable;
		readonly Table untypedTable;
		readonly Func<TRow, bool> filter;
		readonly Dependency dependency;
		readonly List<TRow> writableRows;

		///<summary>Creates a FilteredTable that wraps a table with the specified filter.</summary>
		public FilteredTable(ITable<TRow> table, Expression<Func<TRow, bool>> filter) {
			if (table == null) throw new ArgumentNullException("table");
			if (filter == null) throw new ArgumentNullException("filter");

			typedTable = table;
			untypedTable = (Table)table;

			dependency = DependencyParser.GetDependencyTree(table.Schema, filter);
			this.filter = filter.Compile();
			dependency.Register(untypedTable);

			writableRows = typedTable.Rows.Where(this.filter).ToList();
			Rows = new TypedReadOnlyRowCollection<TRow>(writableRows);

			dependency.RowInvalidated += Dependency_RowInvalidated;
			//The table handlers should be added after the dependency is created.
			//This way, changes to dependent columns in our table will be handled
			//by the dependency first, and the row will be added / removed before
			//we receive ValueChanged. The one drawback is that we will then fire
			//our ValueChanged immediately after RowAdded.
			Table.RowAdded += Table_RowAdded;
			Table.RowRemoved += Table_RowRemoved;
			Table.ValueChanged += Table_ValueChanged;
		}
		///<summary>Gets the table that this instance contains rows from.</summary>
		public Table Table { get { return untypedTable; } }
		///<summary>Gets the rows that meet the filter.</summary>
		public TypedReadOnlyRowCollection<TRow> Rows { get; private set; }

		void Dependency_RowInvalidated(object sender, RowEventArgs e) {
			var row = (TRow)e.Row;

			if (row.Table == null) {
				//If the filter uses a column from this table, the dependency will fire RowInvalidated on RowRemoved.
				//When this happens, we remove the row.
				RemoveRow(row);
				return;
			}

			var currentPasses = Rows.Contains(row);
			var newPasses = filter(row);

			if (currentPasses == newPasses) return;	//Nothing to do

			if (newPasses)
				AddRow(row);
			else
				RemoveRow(row);
		}
		void Table_ValueChanged(object sender, ValueChangedEventArgs e) {
			if (Rows.Contains((TRow)e.Row)) OnValueChanged(new ValueChangedEventArgs<TRow>((TRow)e.Row, e.Column));
		}

		void Table_RowRemoved(object sender, RowListEventArgs e) {
			var row = (TRow)e.Row;
			//If the filter uses a column from this table, the dependency will fire RowInvalidated on RowRemoved.
			if (Rows.Contains(row))
				RemoveRow(row);
		}

		void Table_RowAdded(object sender, RowListEventArgs e) {
			var row = (TRow)e.Row;
			//If the filter uses a column from this table, the dependency will fire RowInvalidated on RowAdded.
			if (Rows.Contains(row)) return;

			if (filter(row))
				AddRow(row);
		}

		void AddRow(TRow row) {
			var targetTableIndex = typedTable.Rows.IndexOf(row);
			int tableIndex = 0;
			int ourIndex = 0;

			//Find the index where the row belongs
			for (ourIndex = 0; ourIndex < Rows.Count; ourIndex++) {
				//For each row in our view,
				//loop through the original
				//table and find the row's 
				//original index.

				while (Rows[ourIndex] != typedTable.Rows[tableIndex])
					tableIndex++;

				//Keep looping through our rows
				//and tracking the corresponding
				//index in the table until we pass
				//the index that we're looking for.
				//Since we're adding the row, we know
				//that it doesn't exist yet in the view.
				//Therefore, the two indices cannot be equal.
				if (tableIndex > targetTableIndex)
					break;
			}

			writableRows.Insert(ourIndex, row);
			OnRowAdded(new RowListEventArgs<TRow>(row, ourIndex));
		}
		void RemoveRow(TRow row) {
			var index = writableRows.IndexOf(row);
			writableRows.RemoveAt(index);
			OnRowRemoved(new RowListEventArgs<TRow>(row, index));
		}

		#region Typed Events
		///<summary>Occurs when a row is added to the table.</summary>
		public event EventHandler<RowListEventArgs<TRow>> RowAdded;
		///<summary>Raises the RowAdded event.</summary>
		///<param name="e">A RowEventArgs object that provides the event data.</param>
		void OnRowAdded(RowListEventArgs<TRow> e) {
			if (RowAdded != null)
				RowAdded(this, e);
		}
		///<summary>Occurs when a row is removed from the table.</summary>
		public event EventHandler<RowListEventArgs<TRow>> RowRemoved;
		///<summary>Raises the RowRemoved event.</summary>
		///<param name="e">A RowEventArgs object that provides the event data.</param>
		void OnRowRemoved(RowListEventArgs<TRow> e) {
			if (RowRemoved != null)
				RowRemoved(this, e);
		}
		///<summary>Occurs when a column value is changed.</summary>
		public event EventHandler<ValueChangedEventArgs<TRow>> ValueChanged;
		///<summary>Raises the ValueChanged event.</summary>
		///<param name="e">A ValueChangedEventArgs object that provides the event data.</param>
		void OnValueChanged(ValueChangedEventArgs<TRow> e) {
			if (ValueChanged != null)
				ValueChanged(this, e);
		}
		#endregion

		[SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Data-binding support")]
		bool IListSource.ContainsListCollection { get { return false; } }
		[SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Data-binding support")]
		System.Collections.IList IListSource.GetList() { return new DataBinding.FilteredTableBinder<TRow>(this); }


		///<summary>Releases all resources used by the FilteredTable.</summary>
		public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }
		///<summary>Releases the unmanaged resources used by the FilteredTable and optionally releases the managed resources.</summary>
		///<param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				Table.RowAdded -= Table_RowAdded;
				Table.RowRemoved -= Table_RowRemoved;
				Table.ValueChanged -= Table_ValueChanged;
				dependency.Unregister(untypedTable);
			}
		}

		TableSchema ISchemaItem.Schema { get { return Table.Schema; } }
	}

	///<summary>A filtered view of an untyped table.</summary>
	public class FilteredTable : FilteredTable<Row> {
		///<summary>Creates a FilteredTable that wraps a table with the specified filter.</summary>
		public FilteredTable(Table table, Expression<Func<Row, bool>> filter) : base(table, filter) { }
	}

	///<summary>A read-only collection of strongly-typed rows.</summary>
	public sealed class TypedReadOnlyRowCollection<TRow> : ReadOnlyCollection<TRow>, IList<Row> where TRow : Row {
		///<summary>Creates a new TypedReadOnlyRowCollection that wraps a typed list.</summary>
		public TypedReadOnlyRowCollection(IList<TRow> list) : base(list) { }

		int IList<Row>.IndexOf(Row item) { return IndexOf(item as TRow); }
		bool ICollection<Row>.Contains(Row item) { return Contains(item as TRow); }

		Row IList<Row>.this[int index] {
			get { return this[index]; }
			set { throw new NotSupportedException(); }
		}

		void IList<Row>.Insert(int index, Row item) { throw new NotSupportedException(); }
		void ICollection<Row>.Add(Row item) { throw new NotSupportedException(); }
		bool ICollection<Row>.Remove(Row item) { throw new NotSupportedException(); }
		void IList<Row>.RemoveAt(int index) { throw new NotSupportedException(); }
		void ICollection<Row>.Clear() { throw new NotSupportedException(); }

		IEnumerator<Row> IEnumerable<Row>.GetEnumerator() { return this.Cast<Row>().GetEnumerator(); }

		void ICollection<Row>.CopyTo(Row[] array, int arrayIndex) { CopyTo((TRow[])array, arrayIndex); }

		bool ICollection<Row>.IsReadOnly { get { return true; } }
	}
}
