using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Data.Common;

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
		}

		///<summary>Gets the Singularity DataContext that this instance synchronizes.</summary>
		public DataContext DataContext { get; private set; }
		///<summary>Gets an ISqlProvider implementation used to create SQL commands for the database.</summary>
		public ISqlProvider SqlProvider { get; private set; }

		///<summary>Gets the tables that are synchronized by this instance.</summary>
		public TableSynchronizerCollection Tables { get; private set; }

		///<summary>Populates the tables from the database.</summary>
		public void ReadData() {
			using (var connection = SqlProvider.OpenConnection()) {
				foreach (var table in Tables.SortDependencies(ts => ts.Table.Schema)) {
					table.ReadData(connection);
				}
			}
		}
		///<summary>Saves changes in the tables to the database.</summary>
		public void WriteData() {
			using (var connection = SqlProvider.OpenConnection())
				WriteData(connection);
		}
		///<summary>Saves changes in the tables to the database.</summary>
		public void WriteData(DbConnection connection) {
			using (var transactionContext = new TransactionContext(connection)) {
				foreach (var table in Tables.SortDependencies(ts => ts.Table.Schema))
					table.WriteChanges(transactionContext, RowChangeType.Added);

				foreach (var table in Tables.SortDependencies(ts => ts.Table.Schema))
					table.WriteChanges(transactionContext, RowChangeType.Changed);

				foreach (var table in Tables.SortDependencies(ts => ts.Table.Schema).Reverse())
					table.WriteChanges(transactionContext, RowChangeType.Removed);

				transactionContext.Commit();
			}
			//If we didn't get an exception, clear the changes.
			//If we did, don't clear, since we didn't commit it
			foreach (var table in Tables)
				table.ClearChanges();
		}
	}

	///<summary>A collection of TableSynchronizer objects.</summary>
	public class TableSynchronizerCollection : Collection<TableSynchronizer> {
		///<summary>Creates a TableSynchronizerCollection that wraps a list of TableSynchronizer objects.</summary>
		internal TableSynchronizerCollection(DataSyncContext context) : base(new List<TableSynchronizer>()) { SyncContext = context; }
		///<summary>Gets the DataSyncContext that contains TableSynchronizers.</summary>
		public DataSyncContext SyncContext { get; private set; }

		///<summary>Gets the synchronizer for table with the given name, or null if the context has no table with that name.</summary>
		public TableSynchronizer this[string name] { get { return this.FirstOrDefault(t => t.Table.Schema.Name == name); } }
		///<summary>Gets the synchronizer for the given table, or null if there is no synchronizer for that table.</summary>
		[SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers")]
		public TableSynchronizer this[Table table] { get { return this.FirstOrDefault(t => t.Table == table); } }

		///<summary>Adds default mappings for every unmapped table in the context.</summary>
		public void AddDefaultMappings() {
			foreach (var table in SyncContext.DataContext.Tables) {
				if (this[table] == null)
					AddTable(table);
			}
		}
		///<summary>Adds a SchemaMapping for a table in the DataContext to the collection.</summary>
		public TableSynchronizer AddMapping(SchemaMapping mapping) {
			if (mapping == null) throw new ArgumentNullException("mapping");
			var table = SyncContext.DataContext.Tables[mapping.Schema];
			if (table == null) throw new ArgumentException("Mapping must map a table in the DataContext");

			var retVal = new TableSynchronizer(table, mapping, SyncContext.SqlProvider);
			Add(retVal);
			return retVal;
		}
		///<summary>Adds SchemaMappings for tables in the DataContext to the collection.</summary>
		public void AddMappings(params SchemaMapping[] mappings) { AddMappings((IEnumerable<SchemaMapping>)mappings); }
		///<summary>Adds SchemaMappings for tables in the DataContext to the collection.</summary>
		public void AddMappings(IEnumerable<SchemaMapping> mappings) {
			if (mappings == null) throw new ArgumentNullException("mappings");

			foreach (var m in mappings)
				AddMapping(m);
		}
		///<summary>Adds a synchronizer for a table using the default SchemaMapping.</summary>
		///<remarks>If a primary SchemaMapping has been registered by calling <see cref="SchemaMapping.SetPrimaryMapping"/>, it will be used.
		///If not, a default mapping (which maps every column and preserves names) will be used.</remarks>
		public void AddTable(Table table) {
			if (table == null) throw new ArgumentNullException("table");
			if (table.Context != SyncContext.DataContext) throw new ArgumentException("Table must belong to parent DataContext", "table");

			var mapping = SchemaMapping.GetPrimaryMapping(table.Schema) ?? new SchemaMapping(table.Schema);
			Items.Add(new TableSynchronizer(table, mapping, SyncContext.SqlProvider));
		}
		///<summary>Inserts an item into the underlying collection.</summary>
		protected override void InsertItem(int index, TableSynchronizer item) {
			if (item == null) throw new ArgumentNullException("item");
			if (item.Table.Context != SyncContext.DataContext) throw new ArgumentException("Table must belong to parent DataContext", "item");

			base.InsertItem(index, item);
		}
		///<summary>Sets an item into the underlying collection.</summary>
		protected override void SetItem(int index, TableSynchronizer item) {
			if (item == null) throw new ArgumentNullException("item");
			if (item.Table.Context != SyncContext.DataContext) throw new ArgumentException("Table must belong to parent DataContext", "item");
			base.SetItem(index, item);
		}
	}
}
