using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShomreiTorah.Singularity.Dependencies {
	///<summary>A dependency on a column in the same row.</summary>
	sealed class SameRowDependancy : RowDependency {
		public SameRowDependancy(RowDependencySetup setup) : base(setup) { }

		protected override IRowEventProvider GetRowCollection(Table table) { return table; }
		protected override IEnumerable<Row> GetAffectedRows(Row modifiedRow) { yield return modifiedRow; }
	}
	///<summary>A dependency on a column in a row's parent row.</summary>
	///<remarks>
	/// When a row in the parent table changes, all of the parent's child rows are affected.
	///</remarks>
	sealed class ParentRowDependency : RowDependency {
		///<summary>Gets the column containing the parent row that the calculated column depends on.</summary>
		public ForeignKeyColumn ParentColumn { get; private set; }

		public ParentRowDependency(RowDependencySetup setup, ForeignKeyColumn parentColumn)
			: base(setup) {
			ParentColumn = parentColumn;
			RequiresDataContext = true;
		}

		protected override IRowEventProvider GetRowCollection(Table table) {
			return table.Context.Tables[ParentColumn.ForeignSchema];
		}

		protected override IEnumerable<Row> GetAffectedRows(Row modifiedRow) {
			return modifiedRow.ChildRows(ParentColumn.ChildRelation);
		}
	}

	///<summary>A dependency on a column in a row's child rows.</summary>
	///<remarks>
	/// When a row in the child table changes, the child row's single parent row, if any, is affected.
	///</remarks>
	sealed class ChildRowDependency : RowDependency {
		///<summary>Gets the relation containing the child rows that the calculated column depends on.</summary>
		public ChildRelation ChildRelation { get; private set; }

		public ChildRowDependency(RowDependencySetup setup, ChildRelation childRelation)
			: base(setup) {
			ChildRelation = childRelation;
			RequiresDataContext = true;
		}


		protected override IRowEventProvider GetRowCollection(Table table) {
			return table.Context.Tables[ChildRelation.ParentSchema];	//A change in a child row affects its parent row in the parent table
		}

		protected override IEnumerable<Row> GetAffectedRows(Row modifiedRow) {
			//When a child row changes, its parent is affected,
			//unless the child row doesn't have a parent.
			var affectedRow = modifiedRow.Field<Row>(ChildRelation.ChildColumn);
			if (affectedRow != null)
				yield return affectedRow;
		}
	}
}
