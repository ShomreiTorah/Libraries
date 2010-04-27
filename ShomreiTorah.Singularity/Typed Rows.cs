using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace ShomreiTorah.Singularity {
	///<summary>A schema that contains strongly-typed rows.</summary>
	public class TypedSchema<TRow> : TableSchema where TRow : Row {
		///<summary>Initializes a new instance of the TypedSchema class.</summary>
		public TypedSchema(string name) : base(name) { }

		internal override void AddRow(Row row) {
			if (!(row is TRow)) throw new InvalidOperationException("Typed schemas can only have typed rows");
			base.AddRow(row);
		}

		///<summary>Creates TypedSchema.</summary>
		[SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes"), SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static TypedSchema<TRow> Create(string name, Action<TypedSchema<TRow>> creator) {
			if (creator == null) throw new ArgumentNullException("creator");
			var retVal = new TypedSchema<TRow>(name);
			creator(retVal);
			return retVal;
		}
	}

	///<summary>A table that contains strongly-typed rows.</summary>
	public class TypedTable<TRow> : Table where TRow : Row {
		///<summary>Creates a typed table from its typed schema.</summary>
		public TypedTable(TypedSchema<TRow> schema) : this(schema, Activator.CreateInstance<TRow>) { }
		///<summary>Creates a typed table from its typed schema.</summary>
		public TypedTable(TypedSchema<TRow> schema, Func<TRow> rowCreator)
			: base(schema) {
			if (rowCreator == null) throw new ArgumentNullException("rowCreator");

			base.Rows = new TypedRowCollection(this, rowCreator);
		}

		///<summary>Gets the schema of this table.</summary>
		public new ITableRowCollection<TRow> Rows { get { return (TypedRowCollection)base.Rows; } }

		class TypedRowCollection : RowCollection<TRow>, ITableRowCollection<TRow> {
			readonly Func<TRow> rowCreator;
			internal TypedRowCollection(TypedTable<TRow> parent, Func<TRow> rowCreator)
				: base(parent) {
				this.rowCreator = rowCreator;
			}
			public override TRow CreateRow() { return rowCreator(); }
		}

		//This class maintains separate backing fields for typed events, 
		//and overrides the base class' raiser methods to raise the typed
		//events as well.  (Events do not support covariance)
		#region Typed Events
		///<summary>Occurs when a row is added to the table.</summary>
		public new event EventHandler<RowEventArgs<TRow>> RowAdded;
		///<summary>Raises the RowAdded event.</summary>
		///<param name="e">A RowEventArgs object that provides the event data.</param>
		[SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
		protected override void OnRowAdded(RowEventArgs e) {
			base.OnRowAdded(e);
			if (RowAdded != null)
				RowAdded(this, new RowEventArgs<TRow>((TRow)e.Row));
		}
		///<summary>Occurs when a row is removed from the table.</summary>
		public new event EventHandler<RowEventArgs<TRow>> RowRemoved;
		///<summary>Raises the RowRemoved event.</summary>
		///<param name="e">A RowEventArgs object that provides the event data.</param>
		[SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
		protected override void OnRowRemoved(RowEventArgs e) {
			base.OnRowRemoved(e);
			if (RowRemoved != null)
				RowRemoved(this, new RowEventArgs<TRow>((TRow)e.Row));
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
	///<summary>Provides data for the strongly-typed ValueChanged event.</summary>
	public class ValueChangedEventArgs<TRow> : RowEventArgs where TRow : Row {
		///<summary>Creates a new ValueChangedEventArgs instance.</summary>
		public ValueChangedEventArgs(TRow row, Column column) : base(row) { Column = column; }

		///<summary>Gets the column.</summary>
		public Column Column { get; private set; }
	}

	///<summary>A collection of strongly-typed rows in a table.</summary>
	public interface ITableRowCollection<TRow> : IList<TRow> where TRow : Row {
		///<summary>Adds a row from an array of values.</summary>
		TRow AddFromValues(params object[] value);

		///<summary>Gets the table that contains these rows.</summary>
		Table Table { get; }

		///<summary>Creates a new row for this table.</summary>
		TRow CreateRow();
	}

	///<summary>Used by ForeignKeyColumn to maintain ChildRowCollections.</summary>
	internal interface IMutableChildRowCollection {
		void AddRow(Row row);
		void RemoveRow(Row row);
	}
	///<summary>A collection of strongly-typed child rows.</summary>
	public interface IChildRowCollection<TChildRow> : IEnumerable<TChildRow> where TChildRow : Row {

		///<summary>Gets the parent row for the collection's rows.</summary>
		Row ParentRow { get; }
		///<summary>Gets the child relation that this collection contains.</summary>
		ChildRelation Relation { get; }
		///<summary>Gets the child table that this collection contains rows from.</summary>
		Table ChildTable { get; }

		///<summary>Occurs when a row is added to the collection.</summary>
		event EventHandler<RowEventArgs> RowAdded;
		///<summary>Occurs when a row is removed from the collection.</summary>
		event EventHandler<RowEventArgs> RowRemoved;

		///<summary>Gets the row at the specified index.</summary>
		TChildRow this[int index] { get; }

		///<summary>Gets the number of rows in this instance.</summary>
		int Count { get; }

		///<summary>Indicates whether this collection contains a row.</summary>
		bool Contains(TChildRow row);
	}
}