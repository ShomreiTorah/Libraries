using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace ShomreiTorah.Singularity.DataBinding {
	///<summary>Exposes a static list of rows for data-binding.  Must be disposed.</summary>
	///<remarks>This binder does not allow rows to be added or 
	///deleted, but does allow columns to be changed.</remarks>
	public sealed class RowListBinder : IListSource, ISchemaItem, IDisposable, IRowEventProvider {
		///<summary>Creates a new RowListBinder that exposes the given list of rows.</summary>
		///<param name="table">The table containing the rows.  If this is null, the rows parameter must be non-empty and no change events will fire.</param>
		///<param name="rows">The rows to expose.</param>
		///<remarks>The table parameter is used to obtain schema information for an empty list.</remarks>
		public RowListBinder(Table table, IList<Row> rows) {
			if (rows == null) throw new ArgumentNullException("rows");
			if (table == null && rows.Count == 0)
				throw new ArgumentNullException("table", "If there are no rows, the table parameter is required");

			Table = table;
			Rows = rows;
			if (Table != null)
				Table.ValueChanged += Table_ValueChanged;

			binder = new Binder(this);
		}
		///<summary>Gets the table containing the rows.</summary>
		public Table Table { get; private set; }
		///<summary>Gets the list of rows exposed by the binder.</summary>
		public IList<Row> Rows { get; private set; }
		///<summary>Gets the schema for the rows exposed by the binder.</summary>
		public TableSchema Schema { get { return Table == null ? Rows[0].Schema : Table.Schema; } }

		///<summary>Raises the Reset event, causing any data-binding clients to refresh.  Call this method after adding or removing rows in the list.</summary>
		[SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
		public void RaiseListReset() {
			binder.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
		}

		void Table_ValueChanged(object sender, ValueChangedEventArgs e) {
			if (Rows.Contains(e.Row))
				OnValueChanged(e);
		}
		///<summary>Occurs when a row in the list changes.  This event is only fired if a table was passed to the constructor.</summary>
		public event EventHandler<ValueChangedEventArgs> ValueChanged;
		///<summary>Raises the ValueChanged event.</summary>
		///<param name="e">A ValueChangedEventArgs object that provides the event data.</param>
		void OnValueChanged(ValueChangedEventArgs e) {
			if (ValueChanged != null)
				ValueChanged(this, e);
		}

		readonly Binder binder;
		sealed class Binder : RowCollectionBinder {
			public Binder(RowListBinder owner) : base(owner) { }

			protected override Row CreateNewRow() { throw new NotSupportedException(); }
			public override bool IsReadOnly { get { return true; } }
			public override bool IsFixedSize { get { return true; } }
			public override bool AllowNew { get { return false; } }
			public override bool AllowRemove { get { return false; } }
		}

		bool IListSource.ContainsListCollection { get { return false; } }
		IList IListSource.GetList() { return binder; }

		///<summary>Releases all resources used by the RowListBinder.</summary>
		public void Dispose() {
			if (Table != null)
				Table.ValueChanged -= Table_ValueChanged;
		}

		Table IRowEventProvider.SourceTable { get { return Table; } }

		//We never add or remove rows
		event EventHandler<RowListEventArgs> IRowEventProvider.RowAdded { add { } remove { } }
		event EventHandler<RowListEventArgs> IRowEventProvider.RowRemoved { add { } remove { } }
	}
}
