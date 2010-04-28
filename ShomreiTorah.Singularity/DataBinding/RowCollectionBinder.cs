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

		protected RowCollectionBinder(TableSchema schema, IList<Row> wrappedList)
			: base(wrappedList) {
			Schema = schema;
			Schema.SchemaChanged += new EventHandler(Schema_SchemaChanged);
		}

		#region ITypedList
		static readonly ListChangedEventArgs SchemaReset = new ListChangedEventArgs(ListChangedType.PropertyDescriptorChanged, -1);
		PropertyDescriptorCollection propertyDescriptors;
		PropertyDescriptorCollection PropertyDescriptors {
			get {
				if (propertyDescriptors == null)
					propertyDescriptors = CreatePropertyDescriptors(Schema);

				return propertyDescriptors;
			}
		}
		void Schema_SchemaChanged(object sender, EventArgs e) {
			propertyDescriptors = null;
			//TODO: More specifc events (requires ColumnRenamed and Relation... events)
			OnListChanged(SchemaReset);
		}
		static PropertyDescriptorCollection CreatePropertyDescriptors(TableSchema schema) {
			var descriptors = new List<PropertyDescriptor>(schema.Columns.Count + schema.ChildRelations.Count);

			descriptors.AddRange(schema.Columns.Select(c => new ColumnPropertyDescriptor(c)));
			descriptors.AddRange(schema.ChildRelations.Select(c => new ChildRelationPropertyDescriptor(c)));

			return new PropertyDescriptorCollection(descriptors.ToArray(), true);
		}

		public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors) {
			Debug.Assert(listAccessors == null || listAccessors.Length == 0, (listAccessors ?? new PropertyDescriptor[0]).Select(pd => pd.Name).Join("."));
			//TODO: Traverse listAccessors and return properties for child schema.

			if (listAccessors == null || listAccessors.Length == 0)
				return PropertyDescriptors;

			var relations = (ChildRelationPropertyDescriptor[])listAccessors;
			return CreatePropertyDescriptors(relations.Last().Relation.ChildSchema);
		}

		public string GetListName(PropertyDescriptor[] listAccessors) {
			Debug.Assert(listAccessors == null || listAccessors.Length == 0, (listAccessors ?? new PropertyDescriptor[0]).Select(pd => pd.Name).Join("."));

			if (listAccessors == null || listAccessors.Length == 0)
				return ListName;

			var relations = (ChildRelationPropertyDescriptor[])listAccessors;
			return relations.Last().Relation.ChildSchema.Name;
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
	}

	sealed class TableBinder : RowCollectionBinder {
		public TableBinder(Table table)
			: base(table.Schema, table.Rows) {
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

		protected override object AddNewCore() { return Table.CreateRow(); }
	}
	sealed class ChildRowsBinder : RowCollectionBinder {
		#region Proxy Collection
		//ChildRowsBinder wraps a ChildRowCollection, which
		//is read only.  However, the binder should not be 
		//read only, so I make a proxy IList<Row> to allow 
		//the collection to be changed.  I cannot override 
		//the mutation methods in BindingList because they 
		//contain critical logic for CancelAddNew and event
		//raising.
		//Instead, ChildRowsBinder inherits BindingList and
		//wraps a proxy type that implements IList<Row> and
		//in turn wraps the ChildRowCollection, but allows 
		//mutation.  ChildRowCollectionProxy cannot inherit
		//Collection<Row> because Collection<Row> respects 
		//the inner ChildRowCollection's ReadOnly property.
		class ChildRowCollectionProxy : IList<Row> {
			//I cannot inherit Collection<Row>; see long comment

			ChildRowCollection childRows;
			public ChildRowCollectionProxy(ChildRowCollection list) { childRows = list; }

			public void Add(Row item) { Insert(Count, item); }
			public void Insert(int index, Row item) {
				if (item == null) throw new ArgumentNullException("item");

				//The row gets added to the collection by the 
				//ForeignKeyColumn methods.  It isn't worth it
				//to honor the index parameter.

				Debug.Assert(index == Count, "A row is being inserted into the middle of a ChildRowCollectionProxy.\r\nConsider overriding ChildRowsBinder.OnListChanged to correct the index");
				//I may want to override OnListChanged in the 
				//parent BindingList to always raise Add event
				//for the last index.  (If the above assert is
				//ever failed)

				item[childRows.Relation.ChildColumn] = childRows.ParentRow;
				childRows.ChildTable.Rows.Add(item);
			}

			public Row this[int index] {
				get { return childRows[index]; }
				set {
					RemoveAt(index);
					Insert(index, value);
				}
			}

			public void Clear() {
				for (int i = Count - 1; i >= 0; i--)
					childRows[i].RemoveRow();
			}
			public void RemoveAt(int index) { Remove(this[index]); }
			public bool Remove(Row item) {
				if (item == null || !childRows.Contains(item)) return false;
				item.RemoveRow();
				return true;
			}

			public bool IsReadOnly { get { return false; } }	//Does not reflect the inner list

			public int IndexOf(Row item) { return childRows.IndexOf(item); }
			public bool Contains(Row item) { return childRows.Contains(item); }
			public void CopyTo(Row[] array, int arrayIndex) { childRows.CopyTo(array, arrayIndex); }
			public int Count { get { return childRows.Count; } }
			public IEnumerator<Row> GetEnumerator() { return childRows.GetEnumerator(); }
			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return childRows.GetEnumerator(); }
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

		protected override object AddNewCore() {
			var newRow = ChildRows.ChildTable.CreateRow();
			newRow[ChildRows.Relation.ChildColumn] = ChildRows.ParentRow;
			return newRow;
		}
	}
}
