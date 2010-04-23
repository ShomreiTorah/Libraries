using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;

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
		public void FillTable() {
			using (var connection = SqlProvider.OpenConnection())
				FillTable(connection);
		}
		///<summary>Populates this instance's table from the database.</summary>
		public void FillTable(DbConnection connection) {
			//TODO: Apply changes to existing rows.

			//Maps each of this table's foreign keys to 
			//a dictionary mapping primary keys to rows.
			var keyMap = Mapping.Columns
				.Select(cm => cm.Column)
				.OfType<ForeignKeyColumn>()
				.ToDictionary(
					col => col,
					col => Table.Context.Tables[col.ForeignSchema].Rows.ToDictionary(
						foreignRow => foreignRow[foreignRow.Table.Schema.PrimaryKey]
					)
				);

			using (var command = SqlProvider.CreateSelectCommand(connection, Mapping))
			using (var reader = command.ExecuteReader()) {
				int[] columnIndices = Mapping.Columns.Select(c => reader.GetOrdinal(c.SqlName)).ToArray();
				while (reader.Read()) {
					var row = new Row(Table.Schema);

					for (int i = 0; i < Mapping.Columns.Count; i++) {
						var foreignKey = Mapping.Columns[i].Column as ForeignKeyColumn;

						var dbValue = reader[columnIndices[i]];

						if (foreignKey == null)
							row[Mapping.Columns[i].Column] = dbValue;
						else
							row[Mapping.Columns[i].Column] = keyMap[foreignKey][dbValue];
					}

					Table.Rows.Add(row);
				}
			}
		}
	}
}
