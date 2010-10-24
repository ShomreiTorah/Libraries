using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.ComponentModel;

namespace ShomreiTorah.Singularity.DataBinding {
	//Contains a single item with properties for each table in a DataContext.
	//Mirrors System.Data.DataViewManager.
	class DataContextBinder : IList, ITypedList {
		readonly DataContext context;

		public DataContextBinder(DataContext context) { this.context = context; }

		class TablePropertyDescriptor : PropertyDescriptor<DataContext>, ITypedListPropertyProvider {
			readonly Table table;
			public TablePropertyDescriptor(Table table) : base(table.Schema.Name) { this.table = table; }

			protected override object GetValue(DataContext component) { return table.GetList(); }

			protected override void SetValue(DataContext component, object value) { }
			public override bool IsReadOnly { get { return true; } }

			public override Type PropertyType { get { return typeof(IBindingList); } }

			public override bool Equals(object obj) {
				var other = obj as TablePropertyDescriptor;
				return other != null && other.table == table;
			}
			public override int GetHashCode() { return table.GetHashCode(); }

			public string ChildListName { get { return table.Schema.Name; } }
			public PropertyDescriptorCollection GetItemProperties() {
				return table.CreatePropertyDescriptors();
			}
		}

		public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors) {
			return listAccessors.GetProperties(() =>
				new PropertyDescriptorCollection(
					context.Tables.Select(t => new TablePropertyDescriptor(t)).ToArray()
				)
			);
		}

		public string GetListName(PropertyDescriptor[] listAccessors) {
			return listAccessors.GetListName(parentListName: "Singularity DataContext");
		}

		#region Dummy IList
		public int Add(object value) { throw new NotSupportedException(); }
		public void Clear() { throw new NotSupportedException(); }
		public void Insert(int index, object value) { throw new NotSupportedException(); }
		public void Remove(object value) { throw new NotSupportedException(); }
		public void RemoveAt(int index) { throw new NotSupportedException(); }
		public void CopyTo(Array array, int index) { throw new NotSupportedException(); }
		public bool Contains(object value) { return value == context; }

		public bool IsSynchronized { get { return false; } }
		public object SyncRoot { get { return context; } }

		public int Count { get { return 1; } }
		public bool IsFixedSize { get { return true; } }
		public bool IsReadOnly { get { return true; } }
		public int IndexOf(object value) { return Contains(value) ? 0 : -1; }

		public IEnumerator GetEnumerator() { yield return context; }

		public object this[int index] {
			get { return context; }
			set { throw new NotSupportedException(); }
		}
		#endregion
	}
}

