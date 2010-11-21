using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ShomreiTorah.Common;
using System.Diagnostics.CodeAnalysis;

namespace ShomreiTorah.Singularity.Dependencies {
	///<summary>A dependency on a row in a table or child relation.</summary>
	public abstract class RowDependency : Dependency {
		///<summary>Creates a new RowDependency.</summary>
		protected RowDependency(RowDependencySetup setup){
			if (setup == null) throw new ArgumentNullException("setup");

			DependentColumns = new ReadOnlyCollection<Column>(setup.DependentColumns.ToArray());
			NestedDependencies = new ReadOnlyCollection<Dependency>(setup.NestedDependencies.ToArray());

			RequiresDataContext = NestedDependencies.Any(d => d.RequiresDataContext);

			foreach (var d in NestedDependencies) 
				d.RowInvalidated += NestedDependency_RowInvalidated;
		}

		///<summary>Registers event handlers for this dependency to track changes for a table.</summary>
		public override void Register(Table table) {
			var rc = GetRowCollection(table);
			rc.RowAdded += DependentRowAdded;
			rc.RowRemoved += DependentRowRemoved;

			if (DependentColumns.Any())
				rc.ValueChanged += DependentValueChanged;

			foreach (var child in NestedDependencies)
				child.Register(table);
		}
		///<summary>Unregisters event handlers registered in Register.</summary>
		public override void Unregister(Table table) {
			var rc = GetRowCollection(table);
			rc.RowAdded -= DependentRowAdded;
			rc.RowRemoved -= DependentRowRemoved;

			if (DependentColumns.Any())
				rc.ValueChanged -= DependentValueChanged;

			foreach (var child in NestedDependencies)
				child.Unregister(table);
		}
		void DependentValueChanged(object sender, ValueChangedEventArgs e) {
			if (!DependentColumns.Contains(e.Column)) return;
			foreach (var row in GetAffectedRows(e.Row, (Table)sender))
				OnRowInvalidated(row);
		}

		void DependentRowAdded(object sender, RowListEventArgs e) {
			foreach (var row in GetAffectedRows(e.Row, (Table)sender))
				OnRowInvalidated(row);
		}
		void DependentRowRemoved(object sender, RowListEventArgs e) {
			foreach (var row in GetAffectedRows(e.Row, (Table)sender))
				OnRowInvalidated(row);
		}

		void NestedDependency_RowInvalidated(object sender, RowEventArgs e) {
			foreach (var affectedRow in GetAffectedRows(e.Row, e.Row.Table))
				OnRowInvalidated(affectedRow);
		}

		///<summary>Gets the columns in the rows represented by this dependency that affect the calculated column.</summary>
		public ReadOnlyCollection<Column> DependentColumns { get; private set; }
		///<summary>Gets the dependencies that depend on the rows represented by this dependency and affect the calculated column.</summary>
		public ReadOnlyCollection<Dependency> NestedDependencies { get; private set; }

		///<summary>Gets the row collection represented by this dependency for the specified table.</summary>
		///<returns>A RowCollection containing rows that the value of the calculated column in the given table depend on.</returns>
		protected abstract IRowEventProvider GetRowCollection(Table table);

		///<summary>Gets the row(s) affected by a change in a row.</summary>
		///<param name="modifiedRow">The dependent row that was changed.</param>
		///<param name="owner">The table containing the change.  This parameter is necessary for the RowRemoved event, which fires when the row has no table.</param>
		///<returns>The rows for which the calculated column was affected.</returns>
		protected abstract IEnumerable<Row> GetAffectedRows(Row modifiedRow, Table owner);
	}

	///<summary>Contains parameters for a RowDependency instance.</summary>
	public sealed class RowDependencySetup {
		///<summary>Creates a new RowDependencySetup.</summary>
		public RowDependencySetup(TableSchema schema) {
			if (schema == null) throw new ArgumentNullException("schema");

			Schema = schema;

			DependentColumns = new SchemaColumnCollection(Schema);
			NestedDependencies = new Collection<Dependency>();
		}

		///<summary>Gets the schema for the rows that the dependency will react to.</summary>
		public TableSchema Schema { get; private set; }

		///<summary>Gets the columns that the dependency will listen for changes in.</summary>
		public Collection<Column> DependentColumns { get; private set; }
		///<summary>Gets the child dependencies that the dependency will depend on.</summary>
		public Collection<Dependency> NestedDependencies { get; private set; }
	}
}
