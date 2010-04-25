using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;

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
		}

		///<summary>Gets the table that this TableSynchronizer synchronizes.</summary>
		public Table Table { get; private set; }
		///<summary>Gets the SchemaMapping that maps the Singularity table to the SQL table.</summary>
		public SchemaMapping Mapping { get; private set; }
		///<summary>Gets an ISqlProvider implementation used to create SQL commands for the database.</summary>
		public ISqlProvider SqlProvider { get; private set; }

		///<summary>Populates this instance's table from the database.</summary>
		public void ReadData() {
			using (var connection = SqlProvider.OpenConnection())
				ReadData(connection);
		}
		///<summary>Populates this instance's table from the database.</summary>
		public void ReadData(DbConnection connection) {

			using (var command = SqlProvider.CreateSelectCommand(connection, Mapping))
			using (var reader = command.ExecuteReader()) {
				new DataReaderTablePopulator(Table, Mapping, reader).FillTable();
			}
		}
		sealed class DataReaderTablePopulator : TablePopulator<IDataRecord> {
			readonly DbDataReader reader;
			readonly SchemaMapping mapping;

			readonly int[] columnIndices;
			readonly int primaryKeyIndex;
			public DataReaderTablePopulator(Table table, SchemaMapping mapping, DbDataReader reader)
				: base(table) {
				this.reader = reader;
				this.mapping = mapping;
				columnIndices = mapping.Columns.Select(c => reader.GetOrdinal(c.SqlName)).ToArray();
				if (table.Schema.PrimaryKey != null)
					primaryKeyIndex = reader.GetOrdinal(mapping.Columns[table.Schema.PrimaryKey].SqlName);
			}

			protected override IEnumerable<Column> Columns { get { return mapping.Columns.Select(cm => cm.Column); } }

			protected override IEnumerable<IDataRecord> GetRows() { return reader.Cast<IDataRecord>(); }

			protected override IEnumerable<KeyValuePair<Column, object>> GetValues(IDataRecord values) {
				return columnIndices.Select((readerIndex, tableIndex) => new KeyValuePair<Column, object>(mapping.Columns[tableIndex].Column, values[readerIndex]));
			}

			protected override object GetPrimaryKey(IDataRecord values) { return values[primaryKeyIndex]; }
		}
	}
}
