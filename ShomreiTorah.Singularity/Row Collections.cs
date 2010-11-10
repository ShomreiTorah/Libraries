using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace ShomreiTorah.Singularity {
	//TypedTable<TRow> and FilteredTable<TRow> expose
	//both strongly-typed and weakly-typed lists.  If
	//I expose a public class that implements both of
	//the IEnumerables, all LINQ calls will require a
	//Cast<T>() call to disambiguate the row type.
	//Therefore, I expose a generic interface of TRow
	//which is implemented by internal classes which 
	//also implement IEnumerable<TRow>.

	///<summary>A table containing a specific row type.</summary>
	///<typeparam name="TRow">The type of the rows in the table.</typeparam>
	///<remarks>This interface is implemented by both typed and untyped tables.
	///Because events aren't covariant, it cannot (easily) contain strongly-typed row events.
	///(Unless I change Table to use EventHandler&lt;RowEventArgs&lt;Row>>, which is cumbersome)</remarks>
	public interface ITable<TRow> where TRow : Row {
		///<summary>Gets the schema of this table.</summary>
		TableSchema Schema { get; }
		///<summary>Gets the rows in this table.</summary>
		ITableRowCollection<TRow> Rows { get; }
	}

	///<summary>A collection of strongly-typed rows in a table.</summary>
	public interface ITableRowCollection<TRow> : IList<TRow> where TRow : Row {
		///<summary>Adds a row from an array of values.</summary>
		TRow AddFromValues(params object[] values);

		///<summary>Gets the table that contains these rows.</summary>
		Table Table { get; }
	}

	///<summary>A read-only collection of items.</summary>
	public interface IReadOnlyCollection<T> : IEnumerable<T> {
		///<summary>Gets the item at the specified index.</summary>
		T this[int index] { get; }

		///<summary>Gets the number of items in this instance.</summary>
		int Count { get; }

		///<summary>Indicates whether this collection contains an item.</summary>
		bool Contains(T item);

		///<summary>Determines the item of a specific row in the collection.</summary>
		int IndexOf(T item);

		///<summary>Copies the items in the collection to an array.</summary>
		void CopyTo(T[] array, int index);
	}

	///<summary>A collection of strongly-typed child rows.</summary>
	public interface IChildRowCollection<TChildRow> : IReadOnlyCollection<TChildRow> where TChildRow : Row {
		///<summary>Gets the parent row for the collection's rows.</summary>
		Row ParentRow { get; }
		///<summary>Gets the child relation that this collection contains.</summary>
		ChildRelation Relation { get; }
		///<summary>Gets the child table that this collection contains rows from.</summary>
		Table ChildTable { get; }

		///<summary>Occurs when a row is added to the collection.</summary>
		event EventHandler<RowListEventArgs> RowAdded;
		///<summary>Occurs when a row is removed from the collection.</summary>
		event EventHandler<RowListEventArgs> RowRemoved;
		///<summary>Occurs when a column value is changed in one of the rows in the collection.</summary>
		event EventHandler<ValueChangedEventArgs> ValueChanged;
	}

	///<summary>Contains untyped rows and sends change events.</summary>
	///<remarks>This is used to bind to arbitrary row collections
	///by RowCollectionBinders and RowDependencies.</remarks>
	public interface IRowEventProvider {
		///<summary>Gets the rows in the object.</summary>
		IList<Row> Rows { get; }

		///<summary>Gets the table that contains the underlying rows.</summary>
		///<remarks>This is used to handle LoadCompleted.  For RowListBinders, this may be null.</remarks>
		Table SourceTable { get; }

		///<summary>Gets the schema that the rows belong to.</summary>
		TableSchema Schema { get; }

		///<summary>Occurs when a row is added to the collection.</summary>
		event EventHandler<RowListEventArgs> RowAdded;
		///<summary>Occurs when a row is removed from the collection.</summary>
		event EventHandler<RowListEventArgs> RowRemoved;
		///<summary>Occurs when a column value is changed in one of the rows in the collection.</summary>
		event EventHandler<ValueChangedEventArgs> ValueChanged;
	}

	internal sealed class ChildRowCollection : ReadOnlyCollection<Row>, IChildRowCollection<Row>, IListSource, IRowEventProvider, ISchemaItem {
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
		public System.Collections.IList GetList() { return new DataBinding.ChildRowsBinder(this); }

		IList<Row> IRowEventProvider.Rows { get { return this; } }
		Table IRowEventProvider.SourceTable { get { return ChildTable; } }
		TableSchema ISchemaItem.Schema { get { return ChildTable.Schema; } }
		TableSchema IRowEventProvider.Schema { get { return ChildTable.Schema; } }
	}
}
