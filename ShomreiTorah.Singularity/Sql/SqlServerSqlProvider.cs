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

			return connection.CreateCommand(BuildSelectCommand(schema).ToString());
		}

		///<summary>Builds a SELECT command for the given schema.</summary>
		///<returns>
		///SELECT [FirstColumn], [SecondColumn], [RowVersion]
		///FROM [SchemaName].[TableName]
		///</returns>
		protected StringBuilder BuildSelectCommand(SchemaMapping schema) {
			if (schema == null) throw new ArgumentNullException("schema");

			var sql = new StringBuilder();

			sql.Append("SELECT ");
			sql.AppendJoin(
				schema.Columns.Select(c => c.SqlName.EscapeSqlIdentifier()), ", "
			);
			sql.AppendLine(", [RowVersion]");
			sql.Append("FROM ").Append(QualifyTable(schema));

			return sql;
		}

		///<summary>Throws a RowModifiedException for a row, reading the row's current values from the database.</summary>
		protected void ThrowRowModified(TransactionContext context, SchemaMapping schema, Row row) {
			if (context == null) throw new ArgumentNullException("context");
			if (schema == null) throw new ArgumentNullException("schema");
			if (row == null) throw new ArgumentNullException("row");


			//SELECT [FirstColumn], [SecondColumn], [RowVersion]
			//FROM [SchemaName].[TableName]
			//WHERE [IDColumnName] = @ID
			var sql = BuildSelectCommand(schema);
			sql.AppendLine().Append("WHERE ").Append(schema.PrimaryKey.SqlName.EscapeSqlIdentifier()).Append(" = @ID");

			using (var command = context.CreateCommand(sql.ToString(), new { ID = row[schema.PrimaryKey.Column] }))
			using (var reader = command.ExecuteReader()) {
				if (!reader.Read()) {
					row.RowVersion = null;
					throw new RowDeletedException(row);
				}

				var dict = schema.Columns.ToDictionary(cm => cm.Column, cm => reader[cm.SqlName]);

				row.RowVersion = reader["RowVersion"];

				if (reader.Read())
					throw new InvalidOperationException("Duplicate ID");

				throw new RowModifiedException(row, dict);
			}
		}

		///<summary>Applies an inserted row to the database.</summary>
		public virtual void ApplyInsert(TransactionContext context, SchemaMapping schema, Row row) {
			if (context == null) throw new ArgumentNullException("context");
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

			using (var command = context.CreateCommand(sql.ToString())) {
				PopulateParameters(command, schema, row);

				using (var reader = command.ExecuteReader()) {
					if (!reader.Read())
						throw new DataException("INSERT command returned no rows");
					row.RowVersion = reader.GetValue(0);

					if (reader.Read())
						throw new DataException("INSERT command returned multiple rows");
				}
			}
		}
		///<summary>Applies an update row to the database.</summary>
		public virtual void ApplyUpdate(TransactionContext context, SchemaMapping schema, Row row) {
			if (context == null) throw new ArgumentNullException("context");
			if (schema == null) throw new ArgumentNullException("schema");
			if (row == null) throw new ArgumentNullException("row");

			var sql = new StringBuilder();

			//UPDATE [SchemaName].[TableName]
			//SET [FirstColumn] = @Col0, [SecondColumn] = @Col1
			//OUTPUT INSERTED.RowVersionINTO @version
			//WHERE [IDColumnName] = @ColN AND RowVersion = @version;

			sql.Append("UPDATE ").AppendLine(QualifyTable(schema));
			sql.Append("SET").AppendJoin(
				schema.Columns.Select((c, i) => c.SqlName.EscapeSqlIdentifier() + " = @Col" + i.ToString(CultureInfo.InvariantCulture)), ", "
			).AppendLine();
			sql.AppendLine("OUTPUT INSERTED.RowVersion");
			sql.Append("WHERE ").Append(schema.PrimaryKey.SqlName.EscapeSqlIdentifier()).Append(" = @Col").Append(schema.Columns.IndexOf(schema.PrimaryKey))
								.Append(" AND RowVersion = @version;");

			using (var command = context.CreateCommand(sql.ToString())) {
				PopulateParameters(command, schema, row);

				var versionParameter = command.CreateParameter();
				versionParameter.ParameterName = "@version";
				versionParameter.Value = row.RowVersion;
				command.Parameters.Add(versionParameter);

				using (var reader = command.ExecuteReader()) {
					if (!reader.Read()) {
						reader.Close();		//A single connection cannot have two DataReaders at once
						ThrowRowModified(context, schema, row);
					}
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
		public void ApplyDelete(TransactionContext context, SchemaMapping schema, Row row) {
			if (context == null) throw new ArgumentNullException("context");
			if (schema == null) throw new ArgumentNullException("schema");
			if (row == null) throw new ArgumentNullException("row");

			//Should work fine for SQLCE too
			using (var command = context.CreateCommand(
				"DELETE FROM " + QualifyTable(schema) + " WHERE " + schema.PrimaryKey.SqlName.EscapeSqlIdentifier() + " = @ID AND RowVersion = @version",
				new { ID = row[row.Schema.PrimaryKey], version = row.RowVersion }
			)) {
				if (command.ExecuteNonQuery() != 1)
					ThrowRowModified(context, schema, row);
			}
		}

		//TODO: Move to ListMaker
		#region DDL
		///<summary>Appends SQL statements to create a schema.  The SqlCommand will have a SqlSchema parameter.</summary>
		protected virtual void CreateSchema(DbConnection connection, SchemaMapping mapping) {
			if (connection == null) throw new ArgumentNullException("connection");
			if (mapping == null) throw new ArgumentNullException("mapping");

			connection.ExecuteNonQuery(@"
				IF schema_id(@SqlSchemaName) IS NULL
					EXECUTE('create schema ' + @EscapedSchemaName);",
				new { mapping.SqlSchemaName, EscapedSchemaName = mapping.SqlSchemaName.EscapeSqlIdentifier() }
			);
		}

		///<summary>Creates a table for the given schema mapping.</summary>
		///<remarks>In addition to the columns in the SchemaMapping, a RowVersion column will be created.</remarks>
		public void CreateTable(DbConnection connection, SchemaMapping schema, IEnumerable<SchemaMapping> parentSchemas) {
			if (connection == null) throw new ArgumentNullException("connection");
			if (schema == null) throw new ArgumentNullException("schema");
			CreateSchema(connection, schema);

			var sql = new StringBuilder();

			//CREATE TABLE [SchemaName].[TableName] (
			//	[FirstColumn]	TYPE	NOT NULL,
			//	[SecondColumn]	TYPE	NULL,
			//	[RowVersion]	RowVersion
			//);

			sql.Append("CREATE TABLE ").Append(QualifyTable(schema)).AppendLine("(");

			foreach (var column in schema.Columns) {
				if (column == schema.PrimaryKey)
					AppendPrimaryKey(sql, column);
				else if (column.Column is ForeignKeyColumn)
					AppendForeignKey(sql, column, parentSchemas);
				else
					AppendColumn(sql, column);

				sql.AppendLine(",");	//Even the last column gets a comma, because of the RowVersion column
			}

			sql.AppendLine("\t[RowVersion]\t\tRowVersion");
			sql.AppendLine(");");

			connection.ExecuteNonQuery(sql.ToString());
		}

		///<summary>Appends a string to create a primary key column to a string builder.</summary>
		protected virtual void AppendPrimaryKey(StringBuilder sql, ColumnMapping column) {
			if (sql == null) throw new ArgumentNullException("sql");
			if (column == null) throw new ArgumentNullException("column");

			sql.AppendFormat("\t{0,-30}\t{1,-20}\tNOT NULL\tROWGUIDCOL\tPRIMARY KEY DEFAULT(newid())",
				column.SqlName.EscapeSqlIdentifier(),
				"UNIQUEIDENTIFIER"
			);
		}
		///<summary>Appends a string to create a column to a string builder.</summary>
		protected virtual void AppendColumn(StringBuilder sql, ColumnMapping column) {
			if (sql == null) throw new ArgumentNullException("sql");
			if (column == null) throw new ArgumentNullException("column");

			var nullable = ((ValueColumn)column.Column).AllowNulls;

			sql.AppendFormat("\t{0,-30}\t{1,-20}\t{2}",
				column.SqlName.EscapeSqlIdentifier(),
				GetSqlType(column.Column.DataType),
				nullable ? "NULL" : "NOT NULL"
			);
		}
		protected virtual void AppendForeignKey(StringBuilder sql, ColumnMapping column, IEnumerable<SchemaMapping> parentSchemas) {
			if (sql == null) throw new ArgumentNullException("sql");
			if (column == null) throw new ArgumentNullException("column");

			var fkc = (ForeignKeyColumn)column.Column;
			var foreignTable = parentSchemas.First(s => s.Schema == fkc.ForeignSchema);
			//[Caller]		UNIQUEIDENTIFIER	NULL		DEFAULT(NULL)	REFERENCES Data.MasterDirectory(Id),

			sql.AppendFormat(CultureInfo.InvariantCulture, "\t{0,-30}\t{1,-20}\t{2}\tREFERENCES {3}({4})",
				column.SqlName.EscapeSqlIdentifier(),
				GetSqlType(foreignTable.PrimaryKey.Column.DataType),
				fkc.AllowNulls ? "NULL" : "NOT NULL",
				QualifyTable(foreignTable),
				foreignTable.PrimaryKey.SqlName.EscapeSqlIdentifier()
			);
		}
		static readonly Dictionary<Type, string> SqlTypes = new Dictionary<Type, string> {
		    { typeof(DateTimeOffset),	"DATETIME" },

		    { typeof(Guid),		"UNIQUEIDENTIFIER" },
		    { typeof(String),	"NVARCHAR(1024)" },
		    { typeof(DateTime),	"DATETIME" },

		    { typeof(Byte),		"TINYINT" },
		    { typeof(Int16),	"SMALLINT" },
		    { typeof(Int32),	"INTEGER" },
		    { typeof(Int64),	"BIGINT" },

		    { typeof(Decimal),	"MONEY" },
		    { typeof(Double),	"FLOAT" },
		    { typeof(Single),	"REAL" },
		    { typeof(Boolean),	"BIT" },
		};

		///<summary>Gets the name of the SQL Server type corresponding to a CLR type.</summary>
		protected virtual string GetSqlType(Type type) {
			if (type == null) throw new ArgumentNullException("type");

			type = Nullable.GetUnderlyingType(type) ?? type;
			return SqlTypes[type];
		}
		#endregion
	}
}
