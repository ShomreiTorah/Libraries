using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using ShomreiTorah.Common;

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

		///<summary>Creates a SELECT command for the given SchemaMapping.</summary>
		public DbCommand CreateSelectCommand(DbConnection connection, SchemaMapping schema) {
			if (connection == null) throw new ArgumentNullException("connection");
			if (schema == null) throw new ArgumentNullException("schema");


			var sql = new StringBuilder();

			sql.Append("SELECT ");
			sql.AppendJoin(
				schema.Columns.Select(c => EscapeIdentifier(c.SqlName)), ", "
			);
			sql.Append("\r\nFROM ");
			if (!String.IsNullOrEmpty(schema.SqlSchemaName))
				sql.Append(EscapeIdentifier(schema.SqlSchemaName)).Append(".");
			sql.Append(EscapeIdentifier(schema.SqlName));
			return connection.CreateCommand(sql.ToString());
		}

		static string EscapeIdentifier(string identifier) {
			return "[" + identifier.Replace("]", "]]") + "]";
		}
	}
}
