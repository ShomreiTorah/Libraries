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
		///<returns>The escaped table name, with the schema name if any.</returns>
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
		public virtual void ApplyInsert(DbConnection connection, SchemaMapping schema, Row row) {
			if (connection == null) throw new ArgumentNullException("connection");
			if (schema == null) throw new ArgumentNullException("schema");
			if (row == null) throw new ArgumentNullException("row");

			var sql = new StringBuilder();

			//INSERT INTO [SchemaName].[TableName]
			//	([FirstColumn], [SecondColumn])
			//OUTPUT INSERTED.RowVersion
			//VALUES(@Col0, @Col1);

			sql.Append("INSERT INTO ").AppendLine(QualifyTable(schema));
			sql.Append("\t(").AppendJoin(
				schema.Columns.Select(c => c.SqlName.EscapeSqlIdentifier()), ", "
			).AppendLine(")");

			sql.AppendLine("OUTPUT INSERTED.RowVersion");

			sql.Append("VALUES(").AppendJoin(
				schema.Columns.Select((c, i) => "@Col" + i.ToString(CultureInfo.InvariantCulture)), ", "
			).Append(");");

			using (var command = connection.CreateCommand(sql.ToString())) {
				PopulateParameters(command, schema, row);

				using (var reader = command.ExecuteReader()) {
					if (!reader.Read())
						throw new DBConcurrencyException("Concurrency FAIL!");	//Exception will be handled by TableSynchronizer
					row.RowVersion = reader.GetValue(0);

					if (reader.Read())
						throw new DBConcurrencyException("Concurrency FAIL!");	//Exception will be handled by TableSynchronizer
				}
			}
		}
		///<summary>Applies an update row to the database.</summary>
		public virtual void ApplyUpdate(DbConnection connection, SchemaMapping schema, Row row) {
			if (connection == null) throw new ArgumentNullException("connection");
			if (schema == null) throw new ArgumentNullException("schema");
			if (row == null) throw new ArgumentNullException("row");

			var sql = new StringBuilder();

			//UPDATE [SchemaName].[TableName]
			//SET [FirstColumn] = @Col0, [SecondColumn] = @Col1
			//OUTPUT INSERTED.RowVersionINTO @version
			//WHERE @IDColumn = @ID AND RowVersion = @version;

			sql.Append("UPDATE ").AppendLine(QualifyTable(schema));
			sql.Append("SET").AppendJoin(
				schema.Columns.Select((c, i) => c.SqlName.EscapeSqlIdentifier() + " = @Col" + i.ToString(CultureInfo.InvariantCulture)), ", "
			).AppendLine();
			sql.AppendLine("OUTPUT INSERTED.RowVersion");
			sql.Append("WHERE ").Append(schema.PrimaryKey.SqlName.EscapeSqlIdentifier()).Append(" = @Col").Append(schema.Columns.IndexOf(schema.PrimaryKey))
								.Append(" AND RowVersion = @version;");

			using (var command = connection.CreateCommand(sql.ToString())) {
				PopulateParameters(command, schema, row);

				var versionParameter = command.CreateParameter();
				versionParameter.ParameterName = "@version";
				versionParameter.Value = row.RowVersion;
				command.Parameters.Add(versionParameter);

				using (var reader = command.ExecuteReader()) {
					if (!reader.Read())
						throw new DBConcurrencyException("Concurrency FAIL!");	//Exception will be handled by TableSynchronizer
					row.RowVersion = reader.GetValue(0);

					if (reader.Read())
						throw new DBConcurrencyException("Concurrency FAIL!");	//Exception will be handled by TableSynchronizer
				}
			}
		}

		///<summary>Adds parameters for the value columns in a row to a DbCommand.</summary>
		protected static void PopulateParameters(DbCommand command, SchemaMapping schema, Row row) {
			if (command == null) throw new ArgumentNullException("command");
			if (schema == null) throw new ArgumentNullException("schema");
			if (row == null) throw new ArgumentNullException("row");

			for (int i = 0; i < schema.Columns.Count; i++) {
				var parameter = command.CreateParameter();

				parameter.ParameterName = "@Col" + i.ToString(CultureInfo.InvariantCulture);
				parameter.Value = GetColumnValue(row, schema.Columns[i].Column) ?? DBNull.Value;

				command.Parameters.Add(parameter);
			}
		}
		static object GetColumnValue(Row row, Column column) {
			var foreignKey = column as ForeignKeyColumn;

			if (foreignKey == null)
				return row[column];

			if (foreignKey.ForeignSchema.PrimaryKey == null)
				throw new InvalidOperationException("A foreign key column that references a table without a primary key cannot be saved to a SQL database");

			var foreignRow = row.Field<Row>(column);
			if (foreignRow == null) return null;
			return foreignRow[foreignKey.ForeignSchema.PrimaryKey];
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

		//TODO: Move to ListMaker
		#region DDL
		/////<summary>Returns a DbCommand containing a CREATE TABLE statement for the given schema mapping.</summary>
		/////<remarks>In addition to the columns in the SchemaMapping, a RowVersion column will be created.</remarks>
		//public DbCommand CreateTable(DbConnection connection, SchemaMapping schema) {
		//    if (connection == null) throw new ArgumentNullException("connection");
		//    if (schema == null) throw new ArgumentNullException("schema");


		//    var sql = new StringBuilder();

		//    //CREATE TABLE [SchemaName].[TableName] (
		//    //	[FirstColumn]	TYPE	NOT NULL,
		//    //	[SecondColumn]	TYPE	NULL,
		//    //	[RowVersion]	RowVersion
		//    //);

		//    sql.Append("CREATE TABLE ").Append(QualifyTable(schema)).AppendLine("(");

		//    foreach (var column in schema.Columns) {
		//        if (column == schema.PrimaryKey)
		//            AppendPrimaryKey(sql, column);
		//        else if (column.Column is ForeignKeyColumn)
		//            AppendForeignKey(sql, column);
		//        else
		//            AppendColumn(sql, column);

		//        sql.Append(",");	//Even the last column gets a comma, because of the RowVersion column
		//    }

		//    sql.AppendLine();
		//    sql.AppendLine("\t[RowVersion]\t\tRowVersion");
		//    sql.AppendLine(");");

		//    return connection.CreateCommand(sql.ToString());
		//}

		/////<summary>Appends a string to create a primary key column to a string builder.</summary>
		//protected virtual void AppendPrimaryKey(StringBuilder sql, ColumnMapping column) {
		//    if (sql == null) throw new ArgumentNullException("sql");
		//    if (column == null) throw new ArgumentNullException("column");

		//    sql.AppendFormat("\t{0,-30}\t{1,-20}\tNOT NULL\tROWGUIDCOL\tPRIMARY KEY DEFAULT(newid())",
		//        column.SqlName.EscapeSqlIdentifier(),
		//        "UNIQUEIDENTIFIER"
		//    );
		//}
		/////<summary>Appends a string to create a column to a string builder.</summary>
		//protected virtual void AppendColumn(StringBuilder sql, ColumnMapping column) {
		//    if (sql == null) throw new ArgumentNullException("sql");
		//    if (column == null) throw new ArgumentNullException("column");

		//    var nullable = ((ValueColumn)column.Column).AllowNulls;

		//    sql.AppendFormat("\t{0,-30}\t{1,-20}\t{2}",
		//        column.SqlName.EscapeSqlIdentifier(),
		//        GetSqlType(column.Column.DataType),
		//        nullable ? "NULL" : "NOT NULL"
		//    );
		//}
		//protected virtual void AppendForeignKey(StringBuilder sql, ColumnMapping column) {
		//    if (sql == null) throw new ArgumentNullException("sql");
		//    if (column == null) throw new ArgumentNullException("column");

		//    var foreignKey = (ForeignKeyColumn)column.Column;

		//    throw new NotImplementedException();
		//}
		//static readonly Dictionary<Type, string> SqlTypes = new Dictionary<Type, string> {
		//    { typeof(DateTimeOffset),	"DATETIME" },

		//    { typeof(Guid),		"UNIQUEIDENTIFIER" },
		//    { typeof(String),	"NVARCHAR(1024)" },
		//    { typeof(DateTime),	"DATETIME" },

		//    { typeof(Byte),		"TINYINT" },
		//    { typeof(Int16),	"SMALLINT" },
		//    { typeof(Int32),	"INTEGER" },
		//    { typeof(Int64),	"BIGINT" },

		//    { typeof(Decimal),	"MONEY" },
		//    { typeof(Double),	"FLOAT" },
		//    { typeof(Single),	"REAL" },
		//    { typeof(Boolean),	"BIT" },

		//};
		/////<summary>Gets the name of the SQL Server type corresponding to a CLR type.</summary>
		//protected virtual string GetSqlType(Type type) {
		//    if (type == null) throw new ArgumentNullException("type");

		//    type = Nullable.GetUnderlyingType(type) ?? type;
		//    return SqlTypes[type];
		//}
		#endregion
	}
}
