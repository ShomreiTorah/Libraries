using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace ShomreiTorah.Singularity {
	///<summary>A table in a Singularity database.</summary>
	public class Table {
		///<summary>Creates an empty table.</summary>
		public Table() {
			Schema = new TableSchema();
			Rows = new EventedRowCollection(this);
		}

		///<summary>Gets the schema of this table.</summary>
		public TableSchema Schema { get; private set; }
		///<summary>Gets the schema of this table.</summary>
		public RowCollection Rows { get; private set; }

		class EventedRowCollection : RowCollection {
			readonly Table parent;
			public EventedRowCollection(Table parent) { this.parent = parent; }

			protected override void ClearItems() {
				base.ClearItems();
				parent.ProcessCleared();
			}
			protected override void InsertItem(int index, Row item) {
				base.InsertItem(index, item);
				parent.ProcessRowAdded(item);
			}
			protected override void RemoveItem(int index) {
				var row = this[index];
				base.RemoveItem(index);
				parent.ProcessRowRemoved(row);
			}
			protected override void SetItem(int index, Row item) {
				var oldRow = this[index];
				base.SetItem(index, item);
				parent.ProcessRowRemoved(oldRow);
				parent.ProcessRowAdded(item);
			}
		}

		void ProcessRowAdded(Row row) {
			row.Table = this;
			OnRowAdded(new RowEventArgs(row));
		}
		void ProcessRowRemoved(Row row) {
			row.Table = null;
			OnRowRemoved(new RowEventArgs(row));
		}
		void ProcessCleared() {
			foreach (var row in Rows)
				row.Table = null;
			OnTableCleared();
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
	public class TableCollection : Collection<Table> { }
}
