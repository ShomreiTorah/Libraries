using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data.SqlServerCe;
using ShomreiTorah.Common;
using System.Globalization;
using System.Data;

namespace ShomreiTorah.Singularity.Sql {
	///<summary>Creates SQL CE DbCommands.</summary>
	public class SqlCeSqlProvider : SqlServerSqlProvider {
		///<summary>Creates a SqlCeSqlProvider for a SQL CE database at the specified path.</summary>
		///<param name="filePath">The path to an existing SQL CE database file.</param>
		public SqlCeSqlProvider(string filePath) : base(DB.OpenFile(filePath, DatabaseFile.SqlCe)) { }

		///<summary>Gets the name of the SQL Server table referenced by a SchemaMapping.</summary>
		///<returns>The escaped table name.</returns>
		protected override string QualifyTable(SchemaMapping mapping) {
			if (mapping == null) throw new ArgumentNullException("mapping");

			return mapping.SqlName.EscapeSqlIdentifier();
		}

		///<summary>Applies an inserted row to the database.</summary>
		public override void ApplyInsert(DbConnection connection, SchemaMapping schema, Row row) {
			if (connection == null) throw new ArgumentNullException("connection");
			if (schema == null) throw new ArgumentNullException("schema");
			if (row == null) throw new ArgumentNullException("row");

			var sql = new StringBuilder();

			//INSERT INTO [SchemaName].[TableName]
			//	([FirstColumn], [SecondColumn])
			//VALUES(@Col0, @Col1);

			sql.Append("INSERT INTO ").AppendLine(QualifyTable(schema));
			sql.Append("\t(").AppendJoin(
				schema.Columns.Select(c => c.SqlName.EscapeSqlIdentifier()), ", "
			).AppendLine(")");

			sql.Append("VALUES(").AppendJoin(
				schema.Columns.Select((c, i) => "@Col" + i.ToString(CultureInfo.InvariantCulture)), ", "
			).Append(");");

			using (var command = connection.CreateCommand(sql.ToString())) {
				PopulateParameters(command, schema, row);

				if (command.ExecuteNonQuery() != 1)
					throw new DBConcurrencyException("Concurrency FAIL!");	//Exception will be handled by TableSynchronizer
			}

			row.RowVersion = Connector.Sql<object>(
				"SELECT RowVersion FROM " + QualifyTable(schema) + " WHERE " + schema.PrimaryKey.SqlName.EscapeSqlIdentifier() + " = @ID"
			).Execute(new { ID = row[schema.PrimaryKey.Column] });
		}
		///<summary>Applies an update row to the database.</summary>
		public override void ApplyUpdate(DbConnection connection, SchemaMapping schema, Row row) {
			if (connection == null) throw new ArgumentNullException("connection");
			if (schema == null) throw new ArgumentNullException("schema");
			if (row == null) throw new ArgumentNullException("row");

			var sql = new StringBuilder();

			//UPDATE [SchemaName].[TableName]
			//SET [FirstColumn] = @Col0, [SecondColumn] = @Col1
			//WHERE @IDColumn = @ID AND RowVersion = @version;

			sql.Append("UPDATE ").AppendLine(QualifyTable(schema));
			sql.Append("SET").AppendJoin(
				schema.Columns.Select((c, i) => c.SqlName.EscapeSqlIdentifier() + " = @Col" + i.ToString(CultureInfo.InvariantCulture)), ", "
			).AppendLine();
			sql.Append("WHERE ").Append(schema.PrimaryKey.SqlName.EscapeSqlIdentifier()).Append(" = @Col").Append(schema.Columns.IndexOf(schema.PrimaryKey))
								.Append(" AND RowVersion = @version;");

			using (var command = connection.CreateCommand(sql.ToString())) {
				PopulateParameters(command, schema, row);

				var versionParameter = command.CreateParameter();
				versionParameter.ParameterName = "@version";
				versionParameter.Value = row.RowVersion;
				command.Parameters.Add(versionParameter);

				if (command.ExecuteNonQuery() != 1)
					throw new DBConcurrencyException("Concurrency FAIL!");	//Exception will be handled by TableSynchronizer
			}
			row.RowVersion = Connector.Sql<object>(
				"SELECT RowVersion FROM " + QualifyTable(schema) + " WHERE " + schema.PrimaryKey.SqlName.EscapeSqlIdentifier() + " = @ID"
			).Execute(new { ID = row[schema.PrimaryKey.Column] });
		}
	}
}
