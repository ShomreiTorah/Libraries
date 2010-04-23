using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace ShomreiTorah.Singularity.Sql {
	///<summary>Synchronizes a Singularity database with an SQL database.</summary>	
	public class DataSyncContext {
		///<summary>Creates a new DataSyncContext for the given DataContext.</summary>
		public DataSyncContext(DataContext context, ISqlProvider sqlProvider) {
			if (context == null) throw new ArgumentNullException("context");
			if (sqlProvider == null) throw new ArgumentNullException("sqlProvider");

			DataContext = context;
			SqlProvider = sqlProvider;

			Tables = new TableSynchronizerCollection(this);

			foreach (var table in context.Tables)
				Tables.AddMapping(table);
		}

		///<summary>Gets the Singularity DataContext that this instance synchronizes.</summary>
		public DataContext DataContext { get; private set; }
		///<summary>Gets an ISqlProvider implementation used to create SQL commands for the database.</summary>
		public ISqlProvider SqlProvider { get; private set; }

		///<summary>Gets the tables that are synchronized by this instance.</summary>
		public TableSynchronizerCollection Tables { get; private set; }

		///<summary>Populates the tables from the database.</summary>
		public void FillTables() {
			using (var connection = SqlProvider.OpenConnection()) {
				foreach (var table in Tables.SortDependencies()) {
					table.FillTable(connection);
				}
			}
		}
	}

	///<summary>A collection of TableSynchronizer objects.</summary>
	public class TableSynchronizerCollection : ReadOnlyCollection<TableSynchronizer> {
		///<summary>Creates a TableSynchronizerCollection that wraps a list of TableSynchronizer objects.</summary>
		internal TableSynchronizerCollection(DataSyncContext context) : base(new List<TableSynchronizer>()) { SyncContext = context; }
		///<summary>Gets the DataSyncContext that contains TableSynchronizers.</summary>
		public DataSyncContext SyncContext { get; private set; }

		///<summary>Gets the synchronizer for table with the given name, or null if the context has no table with that name.</summary>
		public TableSynchronizer this[string name] { get { return this.FirstOrDefault(t => t.Table.Schema.Name == name); } }
		///<summary>Gets the synchronizer for the given table, or null if there is no synchronizer for that table.</summary>
		[SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers")]
		public TableSynchronizer this[Table table] { get { return this.FirstOrDefault(t => t.Table == table); } }

		///<summary>Adds a synchronizer for a table.</summary>
		public void AddMapping(Table table) {
			if (table == null) throw new ArgumentNullException("table");
			if (table.Context != SyncContext.DataContext) throw new ArgumentException("Table must belong to parent DataContext", "table");

			Items.Add(new TableSynchronizer(table, new SchemaMapping(table.Schema), SyncContext.SqlProvider));
		}
		///<summary>Removes the synchronizer for the given table, preventing the table from being synchronized to the database.</summary>
		public void RemoveMapping(Table table) { Items.Remove(this[table]); }

		///<summary>Returns the synchronizers in an order in which all dependant tables are after their dependencies.</summary>
		public IEnumerable<TableSynchronizer> SortDependencies() {
			var pendingTables = this.ToList();
			var processedSchemas = new HashSet<TableSchema>();

			var returnedTables = new List<TableSynchronizer>();
			while (pendingTables.Any()) {
				returnedTables.Clear();

				foreach (var table in pendingTables.ToArray()) {
					if (!table.Table.Schema.ChildRelations.Select(cr => cr.ParentSchema).Except(processedSchemas).Any()) {
						returnedTables.Add(table);
						processedSchemas.Add(table.Table.Schema);

						yield return table;
					}
				}

				if (returnedTables.Count == 0)
					throw new InvalidOperationException("Cyclic dependency detected");
				pendingTables.RemoveAll(returnedTables.Contains);
			}
		}
	}
}
