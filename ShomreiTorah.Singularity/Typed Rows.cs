using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Collections;
using System.ComponentModel;

namespace ShomreiTorah.Singularity {
	///<summary>A schema that contains strongly-typed rows.</summary>
	public class TypedSchema<TRow> : TableSchema where TRow : Row {
		///<summary>Initializes a new instance of the TypedSchema class.</summary>
		public TypedSchema(string name)
			: base(name) {
			if (Instance != null)
				throw new InvalidOperationException("TypedSchema<" + typeof(TRow) + "> already exists.");
			Instance = this;
		}

		///<summary>Gets the singleton instance of this typed schema.</summary>
		[SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Static instance of closed generic type.")]
		public static TypedSchema<TRow> Instance { get; private set; }

		internal override void AddRow(Row row) {
			if (!(row is TRow)) throw new InvalidOperationException("Typed schemas can only have typed rows");
			base.AddRow(row);
		}

		///<summary>Creates TypedSchema.</summary>
		[SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
		//[Obsolete("This pattern is no longer used.  Please use a static constructor.")]
		public static TypedSchema<TRow> Create(string name, Action<TypedSchema<TRow>> creator) {
			if (creator == null) throw new ArgumentNullException("creator");
			var retVal = new TypedSchema<TRow>(name);
			creator(retVal);
			return retVal;
		}
	}

	///<summary>A table that contains strongly-typed rows.</summary>
	public class TypedTable<TRow> : Table, ITable<TRow> where TRow : Row {
		readonly Func<TRow> rowCreator;

		///<summary>Creates a typed table from its typed schema.</summary>
		public TypedTable(TypedSchema<TRow> schema) : this(schema, Activator.CreateInstance<TRow>) { }
		///<summary>Creates a typed table from its typed schema.</summary>
		public TypedTable(TypedSchema<TRow> schema, Func<TRow> rowCreator)
			: base(schema) {
			if (rowCreator == null) throw new ArgumentNullException("rowCreator");
			this.rowCreator = rowCreator;

			Rows = new TypedRowCollection(base.Rows);
		}

		///<summary>Creates a detached row for this table.</summary>
		public override Row CreateRow() { return rowCreator(); }

		///<summary>Gets the rows in this table.</summary>
		public new ITableRowCollection<TRow> Rows { get; private set; }

		///<summary>A typed wrapper around an untyped row collection.</summary>
		sealed class TypedRowCollection : ITableRowCollection<TRow>, IListSource {
			readonly ITableRowCollection<Row> inner;
			internal TypedRowCollection(ITableRowCollection<Row> inner) { this.inner = inner; }

			public TRow AddFromValues(params object[] values) { return (TRow)inner.AddFromValues(values); }
			public Table Table { get { return inner.Table; } }
			public int IndexOf(TRow item) { return inner.IndexOf(item); }
			public void Insert(int index, TRow item) { inner.Insert(index, item); }
			public void RemoveAt(int index) { inner.RemoveAt(index); }
			public TRow this[int index] {
				get { return (TRow)inner[index]; }
				set { inner[index] = value; }
			}
			public void Add(TRow item) { inner.Add(item); }
			public void Clear() { inner.Clear(); }
			public bool Contains(TRow item) { return inner.Contains(item); }
			public void CopyTo(TRow[] array, int arrayIndex) { inner.CopyTo(array, arrayIndex); }
			public int Count { get { return inner.Count; } }
			public bool IsReadOnly { get { return inner.IsReadOnly; } }
			public bool Remove(TRow item) { return inner.Remove(item); }
			public IEnumerator<TRow> GetEnumerator() { return inner.Cast<TRow>().GetEnumerator(); }
			IEnumerator IEnumerable.GetEnumerator() { return inner.GetEnumerator(); }

			public bool ContainsListCollection { get { return false; } }
			public IList GetList() { return ((IListSource)inner).GetList(); }
		}

		//This class maintains separate backing fields for typed events, 
		//and overrides the base class' raiser methods to raise the typed
		//events as well.  (Events do not support covariance)
		#region Typed Events
		///<summary>Occurs when a row is added to the table.</summary>
		public new event EventHandler<RowListEventArgs<TRow>> RowAdded;
		///<summary>Raises the RowAdded event.</summary>
		///<param name="e">A RowEventArgs object that provides the event data.</param>
		[SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
		protected override void OnRowAdded(RowListEventArgs e) {
			base.OnRowAdded(e);
			if (RowAdded != null)
				RowAdded(this, new RowListEventArgs<TRow>((TRow)e.Row, e.Index));
		}
		///<summary>Occurs when a row is removed from the table.</summary>
		public new event EventHandler<RowListEventArgs<TRow>> RowRemoved;
		///<summary>Raises the RowRemoved event.</summary>
		///<param name="e">A RowEventArgs object that provides the event data.</param>
		[SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
		protected override void OnRowRemoved(RowListEventArgs e) {
			base.OnRowRemoved(e);
			if (RowRemoved != null)
				RowRemoved(this, new RowListEventArgs<TRow>((TRow)e.Row, e.Index));
		}
		///<summary>Occurs when a column value is changed.</summary>
		public new event EventHandler<ValueChangedEventArgs<TRow>> ValueChanged;
		///<summary>Raises the ValueChanged event.</summary>
		///<param name="e">A ValueChangedEventArgs object that provides the event data.</param>
		[SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
		protected override void OnValueChanged(ValueChangedEventArgs e) {
			base.OnValueChanged(e);
			if (ValueChanged != null)
				ValueChanged(this, new ValueChangedEventArgs<TRow>((TRow)e.Row, e.Column));
		}
		#endregion
	}

	///<summary>Provides data for strongly-typed row events.</summary>
	public class RowEventArgs<TRow> : EventArgs where TRow : Row {
		///<summary>Creates a new RowEventArgs instance.</summary>
		public RowEventArgs(TRow row) { Row = row; }

		///<summary>Gets the row.</summary>
		public TRow Row { get; private set; }
	}
	///<summary>Provides data for strongly-typed row events in a list.</summary>
	public class RowListEventArgs<TRow> : RowEventArgs<TRow> where TRow : Row {
		///<summary>Creates a new RowListEventArgs instance.</summary>
		public RowListEventArgs(TRow row, int index) : base(row) { Index = index; }

		///<summary>Gets the index of the row in the collection.</summary>
		public int Index { get; private set; }
	}
	///<summary>Provides data for the strongly-typed ValueChanged event.</summary>
	public class ValueChangedEventArgs<TRow> : ValueChangedEventArgs where TRow : Row {
		///<summary>Creates a new ValueChangedEventArgs instance.</summary>
		public ValueChangedEventArgs(TRow row, Column column) : base(row, column) { }
	}

	partial class Row {
		readonly Dictionary<ChildRelation, object> typedChildRelations = new Dictionary<ChildRelation, object>();

		///<summary>Gets the typed child rows in the specified child relation.</summary>
		///<returns>A typed ChildRowCollection containing a live view of the child rows.</returns>
		protected IChildRowCollection<TChildRow> TypedChildRows<TChildRow>(ForeignKeyColumn foreignKey) where TChildRow : Row {
			if (foreignKey == null) throw new ArgumentNullException("foreignKey");


			object retVal = null;
			if (!typedChildRelations.TryGetValue(foreignKey.ChildRelation, out retVal)) {
				retVal = new TypedChildRowCollection<TChildRow>(ChildRows(foreignKey.ChildRelation));
				typedChildRelations.Add(foreignKey.ChildRelation, retVal);
			}
			return (IChildRowCollection<TChildRow>)retVal;
		}

		sealed class TypedChildRowCollection<TChildRow> : IChildRowCollection<TChildRow>, IListSource where TChildRow : Row {
			readonly IChildRowCollection<Row> inner;
			public TypedChildRowCollection(IChildRowCollection<Row> inner) { this.inner = inner; }

			public Row ParentRow { get { return inner.ParentRow; } }
			public ChildRelation Relation { get { return inner.Relation; } }
			public Table ChildTable { get { return inner.ChildTable; } }

			public event EventHandler<RowListEventArgs> RowAdded {
				add { inner.RowAdded += value; }
				remove { inner.RowAdded -= value; }
			}
			public event EventHandler<RowListEventArgs> RowRemoved {
				add { inner.RowRemoved += value; }
				remove { inner.RowRemoved -= value; }
			}
			public event EventHandler<ValueChangedEventArgs> ValueChanged {
				add { inner.ValueChanged += value; }
				remove { inner.ValueChanged -= value; }
			}

			public TChildRow this[int index] { get { return (TChildRow)inner[index]; } }
			public int Count { get { return inner.Count; } }
			public bool Contains(TChildRow row) { return inner.Contains(row); }

			public IEnumerator<TChildRow> GetEnumerator() { return inner.Cast<TChildRow>().GetEnumerator(); }
			IEnumerator IEnumerable.GetEnumerator() { return inner.GetEnumerator(); }

			public bool ContainsListCollection { get { return false; } }
			public IList GetList() { return ((IListSource)inner).GetList(); }
		}
	}
}