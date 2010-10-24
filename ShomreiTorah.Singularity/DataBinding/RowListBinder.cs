using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace ShomreiTorah.Singularity.DataBinding {
	///<summary>Exposes a static list of rows for data-binding.</summary>
	///<remarks>This binder does not allow rows to be added or 
	///deleted, but does allow columns to be changed.</remarks>
	public sealed class RowListBinder : IListSource, ISchemaItem, IDisposable {
		///<summary>Creates a new RowListBinder that exposes the given list of rows.</summary>
		///<param name="table">The table containing the rows.</param>
		///<param name="rows">The rows to expose.</param>
		///<remarks>The table parameter is used to obtain schema information for an empty list.</remarks>
		public RowListBinder(Table table, IList<Row> rows) {
			if (rows == null) throw new ArgumentNullException("rows");
			if (table == null && rows.Count == 0)
				throw new ArgumentNullException("table", "If there are no rows, the table parameter is required");

			Table = table;
			Rows = rows;

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

		readonly Binder binder;
		sealed class Binder : RowCollectionBinder, IDisposable {
			readonly RowListBinder owner;
			public Binder(RowListBinder owner)
				: base(owner.Schema, owner.Rows) {

				this.owner = owner;
				if (owner.Table != null) {
					owner.Table.LoadCompleted += Table_LoadCompleted;
					owner.Table.ValueChanged += Table_ValueChanged;
				}
			}

			void Table_LoadCompleted(object sender, EventArgs e) { OnLoadCompleted(); }
			void Table_ValueChanged(object sender, ValueChangedEventArgs e) {
				if (owner.Rows.Contains(e.Row))
					OnValueChanged(e);
			}

			public void Dispose() {
				if (owner.Table != null) {
					owner.Table.LoadCompleted -= Table_LoadCompleted;
					owner.Table.ValueChanged -= Table_ValueChanged;
				}
			}

			protected override string ListName { get { return Schema.Name; } }
			protected override Table SourceTable { get { return owner.Table; } }

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
			binder.Dispose();
		}
	}
}
