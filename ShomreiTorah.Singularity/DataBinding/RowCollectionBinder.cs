using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using ShomreiTorah.Common;

namespace ShomreiTorah.Singularity.DataBinding {
	abstract class RowCollectionBinder : PhantomCollection<Row>, IBindingList, ICancelAddNew, IRaiseItemChangedEvents, ITypedList, IRowEventClient, ISchemaItem {
		public IRowEventProvider Source { get; private set; }
		public TableSchema Schema { get { return Source.Schema; } }
		public Table SourceTable { get { return Source.SourceTable; } }

		protected virtual string ListName { get { return Schema.Name; } }

		protected RowCollectionBinder(IRowEventProvider source)
			: base(source.Rows) {
			Source = source;
			new WeakEventForwarder(Source, this).ToString();	//The ctor registers the events; I never need the object again.
		}

		#region ITypedList
		static readonly ListChangedEventArgs SchemaReset = new ListChangedEventArgs(ListChangedType.PropertyDescriptorChanged, -1);
		PropertyDescriptorCollection propertyDescriptors;
		PropertyDescriptorCollection PropertyDescriptors {
			get {
				if (propertyDescriptors == null) {
					if (SourceTable != null)
						propertyDescriptors = SourceTable.CreatePropertyDescriptors();
					else
						propertyDescriptors = this[0].Schema.CreatePropertyDescriptors();
				}

				return propertyDescriptors;
			}
		}
		public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors) {
			return listAccessors.GetProperties(() => PropertyDescriptors);
		}

		public string GetListName(PropertyDescriptor[] listAccessors) {
			Debug.Assert(listAccessors == null || listAccessors.Length == 0, (listAccessors ?? new PropertyDescriptor[0]).Select(pd => pd.Name).Join("."));
			return listAccessors.GetListName(parentListName: ListName);
		}
		#endregion

		public bool RaisesItemChangedEvents { get { return true; } }

		public void OnSchemaChanged() {
			propertyDescriptors = null;
			//TODO: More specific events (requires ColumnRenamed and Relation... events)
			OnListChanged(SchemaReset);
		}
		#region Change Events
		//If the source table is loading, don't raise any events.
		//Once it finishes loading, only raise a reset if we saw 
		//some actual changes.
		bool loadChangePending;
		public void OnValueChanged(ValueChangedEventArgs args) {
			if (SourceTable != null && SourceTable.IsLoadingData)
				loadChangePending = true;
			else
				OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, IndexOf(args.Row), new ColumnPropertyDescriptor(args.Column)));
		}
		public void OnRowAdded(RowListEventArgs args) {
			if (SourceTable != null && SourceTable.IsLoadingData)
				loadChangePending = true;
			else
				OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, args.Index));
		}
		public void OnRowRemoved(RowListEventArgs args) {
			if (SourceTable != null && SourceTable.IsLoadingData)
				loadChangePending = true;
			else
				OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, args.Index));
		}
		public void OnLoadCompleted() {
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

		//All events are forwarded by WeakEventForwarder. 
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
		public virtual bool AllowNew { get { return true; } }
		public virtual bool AllowRemove { get { return true; } }
		public bool SupportsChangeNotification { get { return true; } }
		public bool SupportsSearching { get { return false; } }
		public bool SupportsSorting { get { return false; } }
		public virtual bool IsFixedSize { get { return false; } }

		public bool IsSynchronized { get { return false; } }
		public object SyncRoot { get { return Inner; } }
		#endregion

		#region IList implementation
		int IList.Add(object value) { base.Add((Row)value); return Count - 1; }
		bool IList.Contains(object value) { return base.Contains(value as Row); }
		int IList.IndexOf(object value) { return base.IndexOf(value as Row); }
		void IList.Insert(int index, object value) { throw new NotSupportedException(); }
		void IList.Remove(object value) { base.Remove((Row)value); }
		void ICollection.CopyTo(Array array, int index) {
			if (array == null) throw new ArgumentNullException("array");

			var arr = (object[])array;
			for (int i = 0; i < Count; i++) {
				arr[index + i] = this[i];
			}
		}

		object IList.this[int index] {
			get { return this[index]; }
			set { throw new NotSupportedException(); }
		}
		#endregion
	}

	sealed class TableBinder : RowCollectionBinder {
		public TableBinder(Table table) : base(table) { }
		protected override Row CreateNewRow() { return SourceTable.CreateRow(); }
	}
	sealed class ChildRowsBinder : RowCollectionBinder {
		public ChildRowCollection ChildRows { get; private set; }

		public ChildRowsBinder(ChildRowCollection rows)
			: base(rows) {
			ChildRows = rows;
		}

		protected override string ListName { get { return ChildRows.Relation.Name; } }

		protected override Row CreateNewRow() {
			var newRow = ChildRows.ChildTable.CreateRow();
			newRow[ChildRows.Relation.ChildColumn] = ChildRows.ParentRow;
			return newRow;
		}

		//PhantomCollection wraps the original ChildRowCollection,
		//which is read-only.  I override its mutation methods and
		//modify the child table.
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
		public FilteredTableBinder(FilteredTable<TRow> ft) : base(ft) { }

		protected override Row CreateNewRow() { return SourceTable.CreateRow(); }

		//PhantomCollection wraps the original FilteredTable.Rows,
		//which is read-only.  I override its mutation methods and
		//modify the underlying table.
		public override void Add(Row item) {
			if (item == null) throw new ArgumentNullException("item");

			if (HasPhantomItem)
				CommitPhantom();

			SourceTable.Rows.Add(item);
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
