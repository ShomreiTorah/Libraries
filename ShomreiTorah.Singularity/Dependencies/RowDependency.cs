using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ShomreiTorah.Common;

namespace ShomreiTorah.Singularity.Dependencies {
	///<summary>A dependency on a row in a table or child relation.</summary>
	abstract class RowDependency : Dependency, IDependencyClient {
		protected RowDependency(RowDependencySetup setup)
			: base(setup.Client) {
			DependantColumns = new ReadOnlyCollection<Column>(setup.DependantColumns);
			NestedDependencies = new ReadOnlyCollection<Dependency>(setup.NestedDependencies.Select(f => f(this)).ToArray());

			RequiresDataContext = NestedDependencies.Any(d => d.RequiresDataContext);
		}

		public override void Register(Table table) {
			var rc = GetRowCollection(table);
			rc.RowAdded += DependantRowAdded;
			rc.RowRemoved += DependantRowRemoved;

			if (DependantColumns.Any())
				rc.ValueChanged += DependantValueChanged;

			foreach (var child in NestedDependencies)
				child.Register(table);
		}
		public override void Unregister(Table table) {
			var rc = GetRowCollection(table);
			rc.RowAdded -= DependantRowAdded;
			rc.RowRemoved -= DependantRowRemoved;

			if (DependantColumns.Any())
				rc.ValueChanged -= DependantValueChanged;

			foreach (var child in NestedDependencies)
				child.Unregister(table);
		}
		void DependantValueChanged(object sender, ValueChangedEventArgs e) {
			if (!DependantColumns.Contains(e.Column)) return;
			foreach (var row in GetAffectedRows(e.Row))
				InvalidateValue(row);
		}

		void DependantRowAdded(object sender, RowListEventArgs e) {
			foreach (var row in GetAffectedRows(e.Row))
				InvalidateValue(row);
		}
		void DependantRowRemoved(object sender, RowListEventArgs e) {
			foreach (var row in GetAffectedRows(e.Row))
				InvalidateValue(row);
		}
		public void DependencyChanged(Row row) {
			foreach (var affectedRow in GetAffectedRows(row))
				InvalidateValue(affectedRow);
		}

		///<summary>Gets the columns in the rows represented by this dependency that affect the calculated column.</summary>
		public ReadOnlyCollection<Column> DependantColumns { get; private set; }
		///<summary>Gets the dependencies that depend on the rows represented by this dependency and affect the calculated column.</summary>
		public ReadOnlyCollection<Dependency> NestedDependencies { get; private set; }

		///<summary>Gets the row collection represented by this dependency for the specified table.</summary>
		///<returns>A RowCollection containing rows that the value of the calculated column in the given table depend on.</returns>
		protected abstract IRowEventProvider GetRowCollection(Table table);

		///<summary>Gets the row(s) affect by a change in a row.</summary>
		///<param name="modifiedRow">The dependant row that was changed.</param>
		///<returns>The rows for which the calculated column was affected.</returns>
		protected abstract IEnumerable<Row> GetAffectedRows(Row modifiedRow);
	}

	///<summary>Contains parameters for a RowDependency instance.</summary>
	sealed class RowDependencySetup {
		public RowDependencySetup(TableSchema schema, IDependencyClient client) {
			if (schema == null) throw new ArgumentNullException("schema");
			if (client == null) throw new ArgumentNullException("client");

			Schema = schema;
			Client = client;

			DependantColumns = new SchemaColumnCollection(Schema);
			NestedDependencies = new Collection<Func<IDependencyClient, Dependency>>();
		}

		///<summary>Gets the schema for the rows that the dependency will react to.</summary>
		public TableSchema Schema { get; private set; }
		///<summary>Gets the client that will react to changes in the dependency.</summary>
		public IDependencyClient Client { get; private set; }

		///<summary>Gets the columns that the dependency will listen for changes in.</summary>
		public Collection<Column> DependantColumns { get; private set; }
		///<summary>Gets functions that will create child dependencies that the dependency will depend on.</summary>
		public Collection<Func<IDependencyClient, Dependency>> NestedDependencies { get; private set; }
	}
}
