using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;
using ShomreiTorah.Common;
using System.Collections.ObjectModel;

namespace ShomreiTorah.Singularity.DataBinding {
	abstract class RowCollectionBinder : BindingList<Row>, IRaiseItemChangedEvents, ITypedList {
		public TableSchema Schema { get; protected set; }
		protected abstract string ListName { get; }

		protected PhantomCollection<Row> PhantomCollection { get; private set; }

		protected RowCollectionBinder(TableSchema schema, PhantomCollection<Row> wrappedList)
			: base(wrappedList) {
			PhantomCollection = wrappedList;
			Schema = schema;
			Schema.SchemaChanged += new EventHandler(Schema_SchemaChanged);
			AllowNew = true;
		}

		#region ITypedList
		static readonly ListChangedEventArgs SchemaReset = new ListChangedEventArgs(ListChangedType.PropertyDescriptorChanged, -1);
		PropertyDescriptorCollection propertyDescriptors;
		PropertyDescriptorCollection PropertyDescriptors {
			get {
				if (propertyDescriptors == null)
					propertyDescriptors = Schema.CreatePropertyDescriptors();

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

		//BindingList<T> will only return true if the rows implement
		//INotifyPropertyChanged, which they don't.  We raise change
		//events ourselves, so we must override this property to be 
		//true.  (See BindingList<T> source, line 618)
		public bool RaisesItemChangedEvents { get { return true; } }

		#region Change Events
		static readonly ListChangedEventArgs ListReset = new ListChangedEventArgs(ListChangedType.Reset, -1);
		protected void OnListReset() { OnListChanged(ListReset); }

		protected void OnValueChanged(ValueChangedEventArgs args) {
			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, IndexOf(args.Row), new ColumnPropertyDescriptor(args.Column)));
		}
		protected void OnRowAdded(RowListEventArgs args) {
			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, args.Index));
		}
		protected void OnRowRemoved(RowListEventArgs args) {
			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, args.Index));
		}
		#endregion

		bool suppressChangeEvent;
		protected override void OnListChanged(ListChangedEventArgs e) {
			if (!suppressChangeEvent)
				base.OnListChanged(e);
			suppressChangeEvent = false;
		}

		#region AddNew / CancelAddNew
		//I completely replace BindingList's AddNew / CancelAddNew
		//implementation with PhantomCollection, which doesn't add
		//the item to the list until it's commited.  (I cannot add
		//the row immediately because it will have invalid values)

		//BindingList calls EndNew / CancelNew with an internally 
		//maintained index of the item.  I perform my own tracking
		//and can't rely on BindingList's index, so I override the
		//methods that call EndNew and commit the item myself.
		protected override sealed object AddNewCore() {
			var item = CreateNewRow();
			PhantomCollection.AddPhantom(item);
			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, Count - 1));
			return item;
		}

		//These methods can be called with incorrect indices, which must be ignored.
		//(See the documentation)
		public override void EndNew(int itemIndex) {
			if (!PhantomCollection.HasPhantomItem || itemIndex != Count - 1) return;
			CommitPhantom();
		}
		public override void CancelNew(int itemIndex) {
			if (!PhantomCollection.HasPhantomItem || itemIndex != Count - 1) return;
			RemovePhantom();
		}

		protected void CommitPhantom() {
			suppressChangeEvent = true;
			PhantomCollection.CommitPhantom();	//Calls Add
			Debug.Assert(!suppressChangeEvent, "OnListChanged didn't fire???");
		}
		protected void RemovePhantom() {
			PhantomCollection.ResetPhantom();
			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, Count));
		}

		protected override void InsertItem(int index, Row item) {
			if (PhantomCollection.HasPhantomItem)
				CommitPhantom();

			base.InsertItem(index, item);
		}
		protected override void RemoveItem(int index) {
			if (PhantomCollection.HasPhantomItem && index == Count - 1)
				RemovePhantom();
			else {
				if (PhantomCollection.HasPhantomItem)
					CommitPhantom();
				base.RemoveItem(index);
			}
		}

		protected abstract Row CreateNewRow();
		#endregion
	}

	sealed class TableBinder : RowCollectionBinder {
		public TableBinder(Table table)
			: base(table.Schema, new PhantomCollection<Row>(table.Rows)) {
			Table = table;

			Table.RowAdded += Table_RowAdded;
			Table.RowRemoved += Table_RowRemoved;
			Table.ValueChanged += Table_ValueChanged;
			Table.TableCleared += Table_TableCleared;
		}

		void Table_RowRemoved(object sender, RowListEventArgs e) { OnRowRemoved(e); }
		void Table_ValueChanged(object sender, ValueChangedEventArgs e) { OnValueChanged(e); }
		void Table_RowAdded(object sender, RowListEventArgs e) { OnRowAdded(e); }
		void Table_TableCleared(object sender, EventArgs e) { OnListReset(); }

		public Table Table { get; private set; }

		protected override string ListName { get { return Schema.Name; } }

		protected override Row CreateNewRow() { return Table.CreateRow(); }
	}
	sealed class ChildRowsBinder : RowCollectionBinder {
		#region Proxy Collection
		//PhantomCollection wraps the original ChildRowCollection, 
		//which is read-only.  I inherit it and override methods to
		//support mutation.
		class ChildRowCollectionProxy : PhantomCollection<Row> {
			ChildRowCollection childRows;
			public ChildRowCollectionProxy(ChildRowCollection list) : base(list) { childRows = list; }

			protected override bool RemoveInner(Row item) {
				if (item == null) return false;
				item.RemoveRow();
				return true;
			}

			public override void Add(Row item) {
				if (item == null) throw new ArgumentNullException("item");

				item[childRows.Relation.ChildColumn] = childRows.ParentRow;
				childRows.ChildTable.Rows.Add(item);
			}

			public override bool IsReadOnly { get { return false; } }	//The wrapped ChildRowCollection is read-only; we aren't
		}
		#endregion

		public ChildRowCollection ChildRows { get; private set; }

		public ChildRowsBinder(ChildRowCollection rows)
			: base(rows.ChildTable.Schema, new ChildRowCollectionProxy(rows)) {
			ChildRows = rows;

			ChildRows.RowAdded += Rows_RowAdded;
			ChildRows.ValueChanged += Rows_ValueChanged;
			ChildRows.RowRemoved += Rows_RowRemoved;
		}

		void Rows_RowAdded(object sender, RowListEventArgs e) { OnRowAdded(e); }
		void Rows_ValueChanged(object sender, ValueChangedEventArgs e) { OnValueChanged(e); }
		void Rows_RowRemoved(object sender, RowListEventArgs e) { OnRowRemoved(e); }

		protected override string ListName { get { return ChildRows.Relation.Name; } }

		protected override Row CreateNewRow() {
			var newRow = ChildRows.ChildTable.CreateRow();
			newRow[ChildRows.Relation.ChildColumn] = ChildRows.ParentRow;
			return newRow;
		}
	}
}
