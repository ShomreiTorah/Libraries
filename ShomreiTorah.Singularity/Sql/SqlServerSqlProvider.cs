using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using ShomreiTorah.Common;
using System.Globalization;
using System.Data;

namespace ShomreiTorah.Singularity.Sql {
	///<summary>Creates SQL Server DbCommands.</summary>
	public class SqlServerSqlProvider : ISqlProvider {
		///<summary>Creates a SqlServerSqlProvider from a DBConnector.</summary>
		public SqlServerSqlProvider(DBConnector connector) {
			if (connector == null) throw new ArgumentNullException("connector");

			Connector = connector;
		}

		///<summary>Gets the DBConnector instance used to create connections.</summary>
		public DBConnector Connector { get; private set; }

		///<summary>Opens a connection to the database.</summary>
		public DbConnection OpenConnection() { return Connector.OpenConnection(); }

		///<summary>Gets the name of the SQL Server table referenced by a SchemaMapping.</summary>
		///<returns>The table name, with the schema name if any.</returns>
		///<remarks>SQL CE doesn't support schemas, so SqlCeProvider overrides this method.</remarks>
		protected virtual string QualifyTable(SchemaMapping mapping) {
			if (mapping == null) throw new ArgumentNullException("mapping");

			string retVal = null;

			if (!String.IsNullOrEmpty(mapping.SqlSchemaName))
				retVal = mapping.SqlSchemaName.EscapeSqlIdentifier() + ".";
			retVal += mapping.SqlName.EscapeSqlIdentifier();

			return retVal;
		}


		///<summary>Creates a SELECT command for the given SchemaMapping.</summary>
		public DbCommand CreateSelectCommand(DbConnection connection, SchemaMapping schema) {
			if (connection == null) throw new ArgumentNullException("connection");
			if (schema == null) throw new ArgumentNullException("schema");

			//SELECT [FirstColumn], [SecondColumn], [RowVersion]
			//FROM [SchemaName].[TableName]
			var sql = new StringBuilder();

			sql.Append("SELECT ");
			sql.AppendJoin(
				schema.Columns.Select(c => c.SqlName.EscapeSqlIdentifier()), ", "
			);
			sql.AppendLine(", [RowVersion]");
			sql.Append("FROM ").Append(QualifyTable(schema));
			return connection.CreateCommand(sql.ToString());
		}


		///<summary>Applies an inserted row to the database.</summary>
		public void ApplyInsert(DbConnection connection, SchemaMapping schema, Row row) {
			if (connection == null) throw new ArgumentNullException("connection");
			if (schema == null) throw new ArgumentNullException("schema");
			if (row == null) throw new ArgumentNullException("row");

			var sql = new StringBuilder();

			//INSERT INTO [SchemaName].[TableName]
			//	([FirstColumn], [SecondColumn]
			//OUTPUT INSERTED.RowVersion
			//VALUES(@Col1, @Col2) INTO @NewVersion;

			sql.Append("INSERT INTO ").AppendLine(QualifyTable(schema));
			sql.Append("\t(").AppendJoin(
				schema.Columns.Select(c => c.SqlName.EscapeSqlIdentifier()), ", "
			).AppendLine(")");

			sql.AppendLine("OUTPUT INSERTED.RowVersion INTO @NewVersion");

			sql.Append("VALUES(").AppendJoin(
				schema.Columns.Select((c, i) => "@Col" + i.ToString(CultureInfo.InvariantCulture)), ", "
			).Append(");");

			using (var command = connection.CreateCommand(sql.ToString())) {
				PopulateParameters(command, schema, row);

				var versionParameter = command.CreateParameter();
				versionParameter.ParameterName = "NewVersion";
				versionParameter.Direction = ParameterDirection.Output;

				if (command.ExecuteNonQuery() != 1)
					throw new DBConcurrencyException("Concurrency FAIL!");	//Exception will be handled by TableSynchronizer
				row["RowVersion"] = versionParameter.Value;
			}
		}
		///<summary>Applies an update row to the database.</summary>
		public void ApplyUpdate(DbConnection connection, SchemaMapping schema, Row row) {
			if (connection == null) throw new ArgumentNullException("connection");
			if (schema == null) throw new ArgumentNullException("schema");
			if (row == null) throw new ArgumentNullException("row");

			var sql = new StringBuilder();

			//UPDATE [SchemaName].[TableName]
			//SET [FirstColumn] = @Col1, [SecondColumn] = @Col2
			//OUTPUT INSERTED.RowVersionINTO @version
			//WHERE @IDColumn = @ID AND RowVersion = @version;

			sql.Append("UPDATE ").AppendLine(QualifyTable(schema));
			sql.Append("SET").AppendJoin(
				schema.Columns.Select((c, i) => c.SqlName.EscapeSqlIdentifier() + " = @Col" + i.ToString(CultureInfo.InvariantCulture)), ", "
			).AppendLine();
			sql.AppendLine("OUTPUT INSERTED.RowVersion INTO @version");
			sql.Append("WHERE ").Append(schema.PrimaryKey.SqlName.EscapeSqlIdentifier()).Append(" = @ID AND RowVersion = @version;");

			using (var command = connection.CreateCommand(sql.ToString())) {
				PopulateParameters(command, schema, row);

				var versionParameter = command.CreateParameter();
				versionParameter.ParameterName = "version";
				versionParameter.Direction = ParameterDirection.InputOutput;
				versionParameter.Value = row.RowVersion;

				if (command.ExecuteNonQuery() != 1)
					throw new DBConcurrencyException("Concurrency FAIL!");	//Exception will be handled by TableSynchronizer

				row.RowVersion = versionParameter.Value;
			}
		}

		///<summary>Adds parameters for the value columns in a row to a DbCommand.</summary>
		protected static void PopulateParameters(DbCommand command, SchemaMapping schema, Row row) {
			if (command == null) throw new ArgumentNullException("command");
			if (schema == null) throw new ArgumentNullException("schema");
			if (row == null) throw new ArgumentNullException("row");

			for (int i = 0; i < schema.Columns.Count; i++) {
				var parameter = command.CreateParameter();

				parameter.ParameterName = "Col" + i.ToString(CultureInfo.InvariantCulture);
				i++;
			}
		}

		///<summary>Applies a deleted row to the database.</summary>
		public void ApplyDelete(DbConnection connection, SchemaMapping schema, Row row) {
			if (connection == null) throw new ArgumentNullException("connection");
			if (schema == null) throw new ArgumentNullException("schema");
			if (row == null) throw new ArgumentNullException("row");

			//Should work fine for SQLCE too
			using (var command = connection.CreateCommand(
				"DELETE FROM " + QualifyTable(schema) + " WHERE " + schema.PrimaryKey.SqlName.EscapeSqlIdentifier() + " = @ID AND RowVersion = @version",
				new { ID = row[row.Schema.PrimaryKey], version = row.RowVersion }
			)) {
				if (command.ExecuteNonQuery() != 1)
					throw new DBConcurrencyException("Concurrency FAIL!");
			}
		}
	}
}
