using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using ShomreiTorah.Common;

namespace ShomreiTorah.Singularity {
	///<summary>A table in a Singularity database.</summary>
	public partial class Table : ITable<Row>, IRowEventProvider, IListSource, ISchemaItem {
		DataContext context;

		///<summary>Creates an empty table.</summary>
		public Table(string name) : this(new TableSchema(name)) { }
		///<summary>Creates a table from an existing schema.</summary>
		public Table(TableSchema schema) {
			Schema = schema;
			Rows = new RowCollection(this);
			foreach (var cc in Schema.Columns.OfType<CalculatedColumn>().Where(cc => !cc.Dependency.RequiresDataContext))
				cc.Dependency.Register(this);

		}
		///<summary>Creates a detached row for this table.</summary>
		///<remarks>Overridden by typed tables.</remarks>
		public virtual Row CreateRow() { return new Row(Schema); }

		///<summary>Gets the DataContext that contains this table.</summary>
		public DataContext Context {
			get { return context; }
			internal set {
				if (context != null) {
					foreach (var cc in Schema.Columns.OfType<CalculatedColumn>().Where(cc => cc.Dependency.RequiresDataContext))
						cc.Dependency.Unregister(this);
				}
				context = value;
				if (context != null) {
					foreach (var cc in Schema.Columns.OfType<CalculatedColumn>().Where(cc => cc.Dependency.RequiresDataContext))
						cc.Dependency.Register(this);
				}
			}
		}

		int loadDataCount;
		///<summary>Indicates whether the table is currently being populated.</summary>
		public bool IsLoadingData { get { return loadDataCount != 0; } }

		///<summary>Marks the table as loading data until the return value is disposed.
		///When the return value is disposed, the LoadCompleted event will be raised,
		///unless the table is still loading data from another call.</summary>
		///<param name="threadContext">An optional SynchronizationContext to
		///raise the LoadCompleted event on.</param>
		public IDisposable BeginLoadData(SynchronizationContext threadContext) {
			loadDataCount++;
			return new Disposable(delegate {
				if (--loadDataCount == 0) { //If no-one else is loading, raise LoadCompleted.
					if (threadContext == null)
						OnLoadCompleted();
					else
						threadContext.Post(delegate { OnLoadCompleted(); }, null);
				}
			});
		}

		///<summary>Gets the schema of this table.</summary>
		public TableSchema Schema { get; private set; }
		///<summary>Gets the rows in this table.</summary>
		public ITableRowCollection<Row> Rows { get; private set; }

		///<summary>Returns a string representation of this instance.</summary>
		public override string ToString() { return "Table: " + Schema.Name; }

		sealed class RowCollection : Collection<Row>, ITableRowCollection<Row>, IListSource, ISchemaItem {
			internal RowCollection(Table table) { Table = table; }

			public Table Table { get; private set; }
			public new bool Contains(Row row) { return row != null && row.Table == Table; }

			public Row AddFromValues(params object[] values) {
				if (values == null) throw new ArgumentNullException("values");

				var retVal = Table.CreateRow();
				int index = 0;
				foreach (var col in Table.Schema.Columns.Take(values.Length)) {
					retVal[col] = values[index];
					index++;
				}
				Add(retVal);
				return retVal;
			}

			protected override void ClearItems() {
				for (int i = Count - 1; i >= 0; i--) {
					RemoveAt(i);
				}
			}
			protected override void InsertItem(int index, Row item) {
				Table.ValidateAddRow(item);
				base.InsertItem(index, item);
				Table.ProcessRowAdded(item, index);
			}
			protected override void RemoveItem(int index) {
				var row = this[index];
				row.OnRemoving();
				if (!Contains(row)) return; //The row was deleted by its OnRemoving handler (this happens when a deposit clears its payments)
				base.RemoveItem(index);
				Table.ProcessRowRemoved(row, index);
			}
			protected override void SetItem(int index, Row item) {
				Table.ValidateAddRow(item);
				var oldRow = this[index];
				oldRow.OnRemoving();
				base.SetItem(index, item);
				Table.ProcessRowRemoved(oldRow, index);
				Table.ProcessRowAdded(item, index);
			}

			public bool ContainsListCollection { get { return false; } }

			public System.Collections.IList GetList() { return ((IListSource)Table).GetList(); }

			public TableSchema Schema { get { return Table.Schema; } }
		}

		IList<Row> IRowEventProvider.Rows { get { return Rows; } }      //Return base interface
		Table IRowEventProvider.SourceTable { get { return this; } }
		[SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Data-binding support")]
		bool IListSource.ContainsListCollection { get { return false; } }
		[SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Data-binding support")]
		System.Collections.IList IListSource.GetList() { return new DataBinding.TableBinder(this); }

		void ValidateAddRow(Row row) {
			if (row.Table != null)
				throw new ArgumentException("Row is already in a table", "row");
			try {
				row.Table = this;
				foreach (var column in Schema.Columns) {
					if (!column.CanValidate) continue;

					var error = row.ValidateValue(column, row[column]);
					if (!String.IsNullOrEmpty(error))
						throw new InvalidOperationException(error);
				}
			} finally { row.Table = null; }
		}

		void ProcessRowAdded(Row row, int index) {
			row.Table = this;
			foreach (var column in Schema.Columns)
				column.OnRowAdded(row);     //Adds the row to parent relations, and handles calculated columns
			OnRowAdded(new RowListEventArgs(row, index));
			row.OnAdded();      //Workaround: TableSynchronizer cannot handle ValueChanges before RowAdded; overrides will change values
		}
		void ProcessRowRemoved(Row row, int index) {
			row.Table = null;
			Schema.RemoveRow(row);
			foreach (var column in Schema.Columns)
				column.OnRowRemoved(row);   //Removes the row from parent relations, and handles calculated columns

			OnRowRemoved(new RowListEventArgs(row, index));
		}
		internal void ProcessValueChanged(Row row, Column column, object oldValue) {
			if (column is CalculatedColumn)
				OnValueChanged(new ValueChangedEventArgs(row, column)); //Calculated columns don't have an old value
			else
				OnValueChanged(new ValueChangedEventArgs(row, column, oldValue));

		}

		#region Events
		///<summary>Prevents change events from being fired reentrantly.  Otherwise, RowAdded events can trigger ValueChanged events before upstream callers see them, breaking grids.</summary>
		protected Sequentializer EventSequence { get; } = new Sequentializer();

		///<summary>Occurs when a row is added to the table.</summary>
		public event EventHandler<RowListEventArgs> RowAdded;
		///<summary>Raises the RowAdded event.</summary>
		///<param name="e">A RowEventArgs object that provides the event data.</param>
		protected virtual void OnRowAdded(RowListEventArgs e) {
			EventSequence.Execute(() => RowAdded?.Invoke(this, e));
		}
		///<summary>Occurs when a row is removed from the table.</summary>
		public event EventHandler<RowListEventArgs> RowRemoved;
		///<summary>Raises the RowRemoved event.</summary>
		///<param name="e">A RowEventArgs object that provides the event data.</param>
		protected virtual void OnRowRemoved(RowListEventArgs e) {
			EventSequence.Execute(() => RowRemoved?.Invoke(this, e));
		}
		///<summary>Occurs when a column value is changed.</summary>
		public event EventHandler<ValueChangedEventArgs> ValueChanged;
		///<summary>Raises the ValueChanged event.</summary>
		///<param name="e">A ValueChangedEventArgs object that provides the event data.</param>
		protected virtual void OnValueChanged(ValueChangedEventArgs e) {
			EventSequence.Execute(() => ValueChanged?.Invoke(this, e));
		}

		///<summary>Occurs after the table is populated from a datasource.</summary>
		public event EventHandler LoadCompleted;
		///<summary>Raises the LoadCompleted event.</summary>
		protected virtual void OnLoadCompleted() { OnLoadCompleted(EventArgs.Empty); }
		///<summary>Raises the LoadCompleted event.</summary>
		///<param name="e">An EventArgs object that provides the event data.</param>
		protected virtual void OnLoadCompleted(EventArgs e) {
			EventSequence.Execute(() => LoadCompleted?.Invoke(this, e));
		}
		#endregion
	}
	///<summary>Provides data for the ValueChanged event.</summary>
	public class ValueChangedEventArgs : EventArgs {
		readonly string oldValueError;
		readonly object oldValue;

		///<summary>Creates a new ValueChangedEventArgs instance for a change in a calculated column.</summary>
		public ValueChangedEventArgs(Row row, Column column) {
			Row = row;
			Column = column;
			oldValueError = "Previous values are not available for change events in calculated columns";
		}
		///<summary>Creates a new ValueChangedEventArgs instance for a change in a normal column.</summary>
		public ValueChangedEventArgs(Row row, Column column, object oldValue) {
			Row = row;
			Column = column;
			this.oldValue = oldValue;
		}

		///<summary>Gets the row containing the change.</summary>
		public Row Row { get; private set; }
		///<summary>Gets the column that changed.</summary>
		public Column Column { get; private set; }

		///<summary>Gets the column's previous value.  This property is not available for calculated columns.</summary>
		///<remarks>Making this property available for calculated columns would require re-calculating the column
		///after every dependency change, even if no-one needs the value yet.</remarks>
		public object OldValue {
			get {
				if (!String.IsNullOrEmpty(oldValueError))
					throw new InvalidOperationException(oldValueError);
				return oldValue;
			}
		}
	}
	///<summary>A collection of tables contained in a DataContext.</summary>
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

			//If we allow duplicate tables for the same schema, we get
			//sticky situations for child relations. (What if there are
			//multiple tables with the child schema?)  Also, it breaks
			//XML persistence.
			if (this[table.Schema.Name] != null)
				throw new ArgumentException("This DataContext already has a " + table.Schema.Name + " table", "table");

			table.Context = Context;
			Items.Add(table);
			OnTableAdded(new TableEventArgs(table));
		}
		void ICollection<Table>.Add(Table table) { AddTable(table); }

		///<summary>Occurs when a new table is added to the collection.</summary>
		public event EventHandler<TableEventArgs> TableAdded;
		///<summary>Raises the TableAdded event.</summary>
		///<param name="e">A TableEventArgs object that provides the event data.</param>
		void OnTableAdded(TableEventArgs e) => TableAdded?.Invoke(this, e);
	}

	///<summary>Provides data for Table events.</summary>
	public class TableEventArgs : EventArgs {
		///<summary>Creates a new TableEventArgs instance.</summary>
		public TableEventArgs(Table table) { Table = table; }

		///<summary>Gets the table.</summary>
		public Table Table { get; }
	}
}
