using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using ShomreiTorah.Common;

namespace ShomreiTorah.Singularity.DataBinding {
	abstract class RowCollectionBinder : PhantomCollection<Row>, IBindingList, ICancelAddNew, IRaiseItemChangedEvents, ITypedList, ISchemaItem {
		public TableSchema Schema { get; protected set; }
		protected abstract string ListName { get; }
		///<summary>Gets the table that contains the rows in the collection.</summary>
		protected abstract Table SourceTable { get; }

		protected RowCollectionBinder(TableSchema schema, IList<Row> wrappedList)
			: base(wrappedList) {
			Schema = schema;
			Schema.SchemaChanged += Schema_SchemaChanged;
		}

		#region ITypedList
		static readonly ListChangedEventArgs SchemaReset = new ListChangedEventArgs(ListChangedType.PropertyDescriptorChanged, -1);
		PropertyDescriptorCollection propertyDescriptors;
		PropertyDescriptorCollection PropertyDescriptors {
			get {
				if (propertyDescriptors == null)
					propertyDescriptors = SourceTable.CreatePropertyDescriptors();

				return propertyDescriptors;
			}
		}
		void Schema_SchemaChanged(object sender, EventArgs e) {
			propertyDescriptors = null;
			//TODO: More specifc events (requires ColumnRenamed and Relation... events)
			OnListChanged(SchemaReset);
		}

		public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors) {
			Debug.Assert(listAccessors == null || listAccessors.Length == 0, (listAccessors ?? new PropertyDescriptor[0]).Select(pd => pd.Name).Join("."));
			return listAccessors.GetProperties(() => PropertyDescriptors);
		}

		public string GetListName(PropertyDescriptor[] listAccessors) {
			Debug.Assert(listAccessors == null || listAccessors.Length == 0, (listAccessors ?? new PropertyDescriptor[0]).Select(pd => pd.Name).Join("."));
			return listAccessors.GetListName(parentListName: ListName);
		}
		#endregion

		public bool RaisesItemChangedEvents { get { return true; } }

		#region Change Events
		//If the source table is loading, don't raise any events.
		//Once it finishes loading, only raise a reset if we saw 
		//some actual changes.
		bool loadChangePending;
		protected void OnValueChanged(ValueChangedEventArgs args) {
			if (SourceTable.IsLoadingData)
				loadChangePending = true;
			else
				OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, IndexOf(args.Row), new ColumnPropertyDescriptor(args.Column)));
		}
		protected void OnRowAdded(RowListEventArgs args) {
			if (SourceTable.IsLoadingData)
				loadChangePending = true;
			else
				OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, args.Index));
		}
		protected void OnRowRemoved(RowListEventArgs args) {
			if (SourceTable.IsLoadingData)
				loadChangePending = true;
			else
				OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, args.Index));
		}
		protected void OnLoadCompleted() {
			if (loadChangePending)
				OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
			loadChangePending = false;
		}

		///<summary>Occurs when the list is changed.</summary>
		public event ListChangedEventHandler ListChanged;
		///<summary>Raises the ListChanged event.</summary>
		///<param name="e">A  ListChangedEventArgs object that provides the event data.</param>
		internal protected virtual void OnListChanged(ListChangedEventArgs e) {
			if (ListChanged != null)
				ListChanged(this, e);
		}

		#endregion

		//All change events are forwarded from the wrapped
		//table or ChildRowCollection by derived classes. 
		//This works perfectly, except for phantom items. 
		//When a phantom item is added, we explicitly fire
		//ItemAdded.  When the phantom item cancelled, we 
		//raise ItemDeleted, and when it is committed, we 
		//fire a second RowAdded event from the table. Any
		//changes to the underlying table will affect the 
		//phantom item naturally and don't require special
		//handling.
		//As per documentation, I commit the phantom item 
		//if any other change is made through the binding 
		//list.  As per DataView's implementation, I don't
		//commit the phantom item if the table is changed 
		//elsewhere.

		#region AddNew / CancelAddNew
		public object AddNew() {
			var item = CreateNewRow();
			AddPhantom(item);
			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, Count - 1));
			return item;
		}
		protected abstract Row CreateNewRow();

		//These methods can be called with incorrect indices, which must be ignored.
		//(See the documentation)
		public void EndNew(int itemIndex) {
			if (!HasPhantomItem || itemIndex != Count - 1) return;
			CommitPhantom();
		}
		public void CancelNew(int itemIndex) {
			if (!HasPhantomItem || itemIndex != Count - 1) return;
			RemovePhantom();
		}

		//The documentation says that AddNew should raise a second
		//ItemAdded event once the item is committed.  BindingList
		//doesn't, perhaps because it implements the ICancelAddNew
		//interface.  DataView, which does not implement it, does 
		//raise a second event.  I was lazy and opted to raise the
		//through the table's RowAdded event, which is raised when
		//CommitPhantom calls Add.

		protected override void RemovePhantom() {
			base.RemovePhantom();
			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, Count));
		}
		public override void Add(Row item) {
			if (HasPhantomItem)
				CommitPhantom();
			base.Add(item);
		}
		protected override bool RemoveInner(Row item) {
			if (HasPhantomItem)
				CommitPhantom();
			return base.RemoveInner(item);
		}
		#endregion

		#region Not supported
		public void AddIndex(PropertyDescriptor property) { throw new NotSupportedException(); }
		public void ApplySort(PropertyDescriptor property, ListSortDirection direction) { throw new NotSupportedException(); }
		public int Find(PropertyDescriptor property, object key) { throw new NotSupportedException(); }
		public void RemoveIndex(PropertyDescriptor property) { }
		public void RemoveSort() { throw new NotSupportedException(); }
		public ListSortDirection SortDirection { get { return ListSortDirection.Ascending; } }
		public PropertyDescriptor SortProperty { get { return null; } }
		#endregion

		#region Boolean properties
		public bool IsSorted { get { return false; } }
		public bool AllowEdit { get { return true; } }
		public bool AllowNew { get { return true; } }
		public bool AllowRemove { get { return true; } }
		public bool SupportsChangeNotification { get { return true; } }
		public bool SupportsSearching { get { return false; } }
		public bool SupportsSorting { get { return false; } }
		public bool IsFixedSize { get { return false; } }

		public bool IsSynchronized { get { return false; } }
		public object SyncRoot { get { return Inner; } }
		#endregion

		#region IList implementation
		int IList.Add(object value) { base.Add((Row)value); return Count - 1; }
		bool IList.Contains(object value) { return base.Contains(value as Row); }
		int IList.IndexOf(object value) { return base.IndexOf(value as Row); }
		void IList.Insert(int index, object value) { throw new NotSupportedException(); }
		void IList.Remove(object value) { base.Remove((Row)value); }
		void ICollection.CopyTo(Array array, int index) { base.CopyTo(array as Row[], index); }

		object IList.this[int index] {
			get { return this[index]; }
			set { throw new NotSupportedException(); }
		}
		#endregion
	}

	sealed class TableBinder : RowCollectionBinder {
		public TableBinder(Table table)
			: base(table.Schema, table.Rows) {
			Table = table;

			Table.RowAdded += Table_RowAdded;
			Table.RowRemoved += Table_RowRemoved;
			Table.ValueChanged += Table_ValueChanged;
			Table.LoadCompleted += Table_LoadCompleted;
		}

		void Table_LoadCompleted(object sender, EventArgs e) { OnLoadCompleted(); }

		void Table_RowRemoved(object sender, RowListEventArgs e) { OnRowRemoved(e); }
		void Table_ValueChanged(object sender, ValueChangedEventArgs e) { OnValueChanged(e); }
		void Table_RowAdded(object sender, RowListEventArgs e) { OnRowAdded(e); }

		public Table Table { get; private set; }
		protected override Table SourceTable { get { return Table; } }

		protected override string ListName { get { return Schema.Name; } }

		protected override Row CreateNewRow() { return Table.CreateRow(); }
	}
	sealed class ChildRowsBinder : RowCollectionBinder {
		public ChildRowCollection ChildRows { get; private set; }

		public ChildRowsBinder(ChildRowCollection rows)
			: base(rows.ChildTable.Schema, rows) {
			ChildRows = rows;

			ChildRows.ChildTable.LoadCompleted += ChildTable_LoadCompleted;
			ChildRows.RowAdded += Rows_RowAdded;
			ChildRows.ValueChanged += Rows_ValueChanged;
			ChildRows.RowRemoved += Rows_RowRemoved;
		}

		void ChildTable_LoadCompleted(object sender, EventArgs e) { OnLoadCompleted(); }

		void Rows_RowAdded(object sender, RowListEventArgs e) { OnRowAdded(e); }
		void Rows_ValueChanged(object sender, ValueChangedEventArgs e) { OnValueChanged(e); }
		void Rows_RowRemoved(object sender, RowListEventArgs e) { OnRowRemoved(e); }

		protected override string ListName { get { return ChildRows.Relation.Name; } }
		protected override Table SourceTable { get { return ChildRows.ChildTable; } }

		protected override Row CreateNewRow() {
			var newRow = ChildRows.ChildTable.CreateRow();
			newRow[ChildRows.Relation.ChildColumn] = ChildRows.ParentRow;
			return newRow;
		}

		//PhantomCollection wraps the original ChildRowCollection,
		//which is read-only.  I override its mutation methods and
		//modify the child table

		public override void Add(Row item) {
			if (item == null) throw new ArgumentNullException("item");

			if (HasPhantomItem)
				CommitPhantom();

			item[ChildRows.Relation.ChildColumn] = ChildRows.ParentRow;
			ChildRows.ChildTable.Rows.Add(item);
		}
		protected override bool RemoveInner(Row item) {
			if (HasPhantomItem)
				CommitPhantom();
			if (item == null) return false;
			item.RemoveRow();
			return true;
		}
	}
	sealed class FilteredTableBinder<TRow> : RowCollectionBinder where TRow : Row {
		public FilteredTable<TRow> FilteredTable { get; private set; }

		public FilteredTableBinder(FilteredTable<TRow> ft)
			: base(ft.Table.Schema, ft.Rows) {
			FilteredTable = ft;

			FilteredTable.Table.LoadCompleted += Table_LoadCompleted;
			FilteredTable.RowAdded += Rows_RowAdded;
			FilteredTable.ValueChanged += Rows_ValueChanged;
			FilteredTable.RowRemoved += Rows_RowRemoved;
		}

		void Table_LoadCompleted(object sender, EventArgs e) { OnLoadCompleted(); }

		void Rows_RowAdded(object sender, RowListEventArgs<TRow> e) { OnRowAdded(new RowListEventArgs(e.Row, e.Index)); }
		void Rows_ValueChanged(object sender, ValueChangedEventArgs<TRow> e) { OnValueChanged(e); }
		void Rows_RowRemoved(object sender, RowListEventArgs<TRow> e) { OnRowRemoved(new RowListEventArgs(e.Row, e.Index)); }

		protected override string ListName { get { return FilteredTable.Table.Schema.Name; } }
		protected override Table SourceTable { get { return FilteredTable.Table; } }

		protected override Row CreateNewRow() {
			var newRow = FilteredTable.Table.CreateRow();
			return newRow;
		}

		//PhantomCollection wraps the original FilteredTable.Rows,
		//which is read-only.  I override its mutation methods and
		//modify the underlying table.

		public override void Add(Row item) {
			if (item == null) throw new ArgumentNullException("item");

			if (HasPhantomItem)
				CommitPhantom();

			FilteredTable.Table.Rows.Add(item);
		}
		protected override bool RemoveInner(Row item) {
			if (HasPhantomItem)
				CommitPhantom();
			if (item == null) return false;
			item.RemoveRow();
			return true;
		}
	}
}
