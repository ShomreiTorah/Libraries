using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShomreiTorah.Singularity.Dependencies {
	///<summary>A dependency on a column in the same row.</summary>
	public sealed class SameRowDependency : RowDependency {
		///<summary>Creates a new SameRowDependancy.</summary>
		public SameRowDependency(RowDependencySetup setup) : base(setup) { }

		///<summary>Gets the row collection represented by this dependency for the specified table.</summary>
		///<returns>The same table.</returns>
		protected override IRowEventProvider GetRowCollection(Table table) { return table; }
		///<summary>Gets the row(s) affected by a change in a row.</summary>
		///<returns>The same row that was modified.</returns>
		protected override IEnumerable<Row> GetAffectedRows(Row modifiedRow) { yield return modifiedRow; }
	}
	///<summary>A dependency on a column in a row's parent row.</summary>
	///<remarks>
	/// When a row in the parent table changes, all of the parent's child rows are affected.
	///</remarks>
	public sealed class ParentRowDependency : RowDependency {
		///<summary>Gets the column containing the parent row that the calculated column depends on.</summary>
		public ForeignKeyColumn ParentColumn { get; private set; }

		///<summary>Creates a new ParentRowDependency.</summary>
		public ParentRowDependency(RowDependencySetup setup, ForeignKeyColumn parentColumn)
			: base(setup) {
			ParentColumn = parentColumn;
			RequiresDataContext = true;
		}

		///<summary>Gets the row collection represented by this dependency for the specified table.</summary>
		///<returns>The table containing the parent rows.</returns>
		protected override IRowEventProvider GetRowCollection(Table table) {
			if (table == null) throw new ArgumentNullException("table");

			return table.Context.Tables[ParentColumn.ForeignSchema];
		}

		///<summary>Gets the row(s) affected by a change in a row.</summary>
		///<returns>All of the row's child rows by this relation. </returns>
		protected override IEnumerable<Row> GetAffectedRows(Row modifiedRow) {
			if (modifiedRow == null) throw new ArgumentNullException("modifiedRow");
			
			return modifiedRow.ChildRows(ParentColumn.ChildRelation);
		}
	}

	///<summary>A dependency on a column in a row's child rows.</summary>
	///<remarks>
	/// When a row in the child table changes, the child row's single parent row, if any, is affected.
	///</remarks>
	public sealed class ChildRowDependency : RowDependency {
		///<summary>Gets the relation containing the child rows that the calculated column depends on.</summary>
		public ChildRelation ChildRelation { get; private set; }

		///<summary>Creates a new ChildRowDependency.</summary>
		public ChildRowDependency(RowDependencySetup setup, ChildRelation childRelation)
			: base(setup) {
			ChildRelation = childRelation;
			RequiresDataContext = true;
		}


		///<summary>Gets the row collection represented by this dependency for the specified table.</summary>
		///<returns>The table containing the child rows.</returns>
		protected override IRowEventProvider GetRowCollection(Table table) {
			if (table == null) throw new ArgumentNullException("table");
			
			return table.Context.Tables[ChildRelation.ChildSchema];	//A change in a child row affects its parent row in the parent table
		}

		///<summary>Gets the row(s) affected by a change in a row.</summary>
		///<returns>The parent row of the child row that was changed.</returns>
		protected override IEnumerable<Row> GetAffectedRows(Row modifiedRow) {
			if (modifiedRow == null) throw new ArgumentNullException("modifiedRow");


			//When a child row changes, its parent is affected,
			//unless the child row doesn't have a parent.
			var affectedRow = modifiedRow.Field<Row>(ChildRelation.ChildColumn);
			if (affectedRow != null)
				yield return affectedRow;
		}
	}
}
