using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ShomreiTorah.Singularity.Sql {
	///<summary>Synchronizes a Singularity table with a table in an SQL database.</summary>
	public class TableSynchronizer {
		///<summary>Creates a TableSynchronizer for the specified table.</summary>
		public TableSynchronizer(Table table, SchemaMapping mapping, ISqlProvider sqlProvider) {
			if (table == null) throw new ArgumentNullException("table");
			if (mapping == null) throw new ArgumentNullException("mapping");
			if (sqlProvider == null) throw new ArgumentNullException("sqlProvider");

			if (table.Schema != mapping.Schema) throw new ArgumentException("Table and mapping must have the same schema", "mapping");

			Table = table;
			Mapping = mapping;
			SqlProvider = sqlProvider;

			Table.RowAdded += Table_RowAdded;
			Table.ValueChanged += Table_ValueChanged;
			Table.RowRemoved += Table_RowRemoved;

			Changes = new ReadOnlyCollection<RowChange>(changes);
		}


		///<summary>Gets the table that this TableSynchronizer synchronizes.</summary>
		public Table Table { get; private set; }
		///<summary>Gets the SchemaMapping that maps the Singularity table to the SQL table.</summary>
		public SchemaMapping Mapping { get; private set; }
		///<summary>Gets an ISqlProvider implementation used to create SQL commands for the database.</summary>
		public ISqlProvider SqlProvider { get; private set; }

		#region Read Database
		///<summary>Populates this instance's table from the database.</summary>
		public void ReadData() {
			using (var connection = SqlProvider.OpenConnection())
				ReadData(connection);
		}
		///<summary>Populates this instance's table from the database.</summary>
		public void ReadData(DbConnection connection) {
			using (var command = SqlProvider.CreateSelectCommand(connection, Mapping)) {
				try {
					isReadingDB = true;

					using (var reader = command.ExecuteReader()) {
						new DataReaderTablePopulator(Table, Mapping, reader).FillTable();
					}
					changes.Clear();
				} finally { isReadingDB = false; }
			}
		}
		sealed class DataReaderTablePopulator : TablePopulator<IDataRecord> {
			readonly DbDataReader reader;
			readonly SchemaMapping mapping;

			readonly int[] columnIndices;
			readonly int primaryKeyIndex;
			readonly int rowVersionIndex;
			public DataReaderTablePopulator(Table table, SchemaMapping mapping, DbDataReader reader)
				: base(table) {
				this.reader = reader;
				this.mapping = mapping;
				columnIndices = mapping.Columns.Select(c => reader.GetOrdinal(c.SqlName)).ToArray();
				if (table.Schema.PrimaryKey != null)
					primaryKeyIndex = reader.GetOrdinal(mapping.Columns[table.Schema.PrimaryKey].SqlName);
				rowVersionIndex = reader.GetOrdinal("RowVersion");
			}

			protected override IEnumerable<Column> Columns { get { return mapping.Columns.Select(cm => cm.Column); } }

			protected override IEnumerable<IDataRecord> GetRows() { return reader.Cast<IDataRecord>(); }

			protected override IEnumerable<KeyValuePair<Column, object>> GetValues(IDataRecord values) {
				return columnIndices.Select((readerIndex, tableIndex) => new KeyValuePair<Column, object>(mapping.Columns[tableIndex].Column, values[readerIndex]));
			}
			protected override void OnRowPopulated(Row row, IDataRecord values) {
				base.OnRowPopulated(row, values);
				row.RowVersion = values.GetValue(rowVersionIndex);
			}

			[SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
			protected override object GetPrimaryKey(IDataRecord values) { return values[primaryKeyIndex]; }
		}
		#endregion
		#region Write Database
		///<summary>If true, all change events will be ignored.</summary>
		bool isReadingDB;

		readonly List<RowChange> changes = new List<RowChange>();
		///<summary>Gets the changed rows that have not been persisted to the database.</summary>
		public ReadOnlyCollection<RowChange> Changes { get; private set; }

		void Table_RowAdded(object sender, RowListEventArgs e) {
			if (isReadingDB) return;
			var changeIndex = GetChangeIndex(e.Row);
			if (changeIndex >= 0) {
				Debug.Assert(changes[changeIndex].ChangeType == RowChangeType.Removed, "Detached row was " + changes[changeIndex].ChangeType);
				changes.RemoveAt(changeIndex);
				changes.Add(new RowChange(e.Row, RowChangeType.Changed));	//Assume that the row was changed while it was detached
			} else
				changes.Add(new RowChange(e.Row, RowChangeType.Added));
		}

		void Table_ValueChanged(object sender, ValueChangedEventArgs e) {
			if (isReadingDB) return;
			var changeIndex = GetChangeIndex(e.Row);

			if (changeIndex >= 0)
				Debug.Assert(changes[changeIndex].ChangeType != RowChangeType.Removed);	//If it wasn't added to the DB yet, or if it's already been changed, we don't need to track value changes
			else
				changes.Add(new RowChange(e.Row, RowChangeType.Changed));
		}

		void Table_RowRemoved(object sender, RowListEventArgs e) {
			if (isReadingDB) return;
			var changeIndex = GetChangeIndex(e.Row);
			if (changeIndex >= 0) {
				var type = changes[changeIndex].ChangeType;

				Debug.Assert(type != RowChangeType.Removed, "Row was removed twice");

				//If a row was added, then removed, we shouldn't 
				//record any change.  If a row was changed, then 
				//removed, we should remove the Change change and
				//add a Remove change.

				changes.RemoveAt(changeIndex);

				if (type != RowChangeType.Added)
					changes.Add(new RowChange(e.Row, RowChangeType.Removed));

			} else
				changes.Add(new RowChange(e.Row, RowChangeType.Removed));
		}

		int GetChangeIndex(Row row) {
			return changes.FindIndex(rc => rc.Row == row);
		}

		///<summary>Saves changes in this instance's table to the database.</summary>
		public void WriteData() {
			using (var connection = SqlProvider.OpenConnection())
				WriteData(connection);
		}
		///<summary>Saves changes in this instance's table to the database.</summary>
		public void WriteData(DbConnection connection) {
			if (connection == null) throw new ArgumentNullException("connection");
			WriteChanges(connection, null);
		}

		internal void WriteChanges(DbConnection connection, RowChangeType? changeType) {
			IEnumerable<RowChange> relevantChanges;

			if (changeType == null)
				relevantChanges = changes;
			else
				relevantChanges = changes.Where(rc => rc.ChangeType == changeType);

			foreach (var change in relevantChanges) {
				try {
					switch (change.ChangeType) {
						case RowChangeType.Added:
							SqlProvider.ApplyInsert(connection, Mapping, change.Row);
							break;
						case RowChangeType.Changed:
							SqlProvider.ApplyUpdate(connection, Mapping, change.Row);
							break;
						case RowChangeType.Removed:
							SqlProvider.ApplyDelete(connection, Mapping, change.Row);
							break;
						default:
							throw new InvalidOperationException("Unknown change type: " + change.ChangeType);
					}
				} catch (DBConcurrencyException) {
					//TODO: Make my own exception class, and get the new values from the database.
					throw;
				}
			}

			//If we didn't get any exceptions, clear the changes.
			//We assume that the connection has a transaction.
			if (changeType == null)
				changes.Clear();
			else
				changes.RemoveAll(rc => rc.ChangeType == changeType);
		}
		#endregion
	}

	///<summary>Represents a changed row that will be synchronized to an SQL database.</summary>
	public sealed class RowChange {
		internal RowChange(Row row, RowChangeType type) { Row = row; ChangeType = type; }

		///<summary>Gets the row that was changed.</summary>
		public Row Row { get; private set; }

		///<summary>Gets the way that the row was changed.</summary>
		public RowChangeType ChangeType { get; private set; }

		///<summary>Returns a string representation of this change.</summary>
		public override string ToString() {
			return ChangeType + ": " + Row;
		}
	}
	///<summary>A type of change in a tracked row.</summary>
	public enum RowChangeType {
		///<summary>The row was added to a Singularity table.</summary>
		Added,
		///<summary>The row's values were changed in its Singularity table.</summary>
		Changed,
		///<summary>The row was removed from a Singularity table.</summary>
		Removed
	}
}
