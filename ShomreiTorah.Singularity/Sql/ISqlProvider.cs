using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;

namespace ShomreiTorah.Singularity.Sql {
	///<summary>Creates DbCommands for TableSynchronizer instances.</summary>
	public interface ISqlProvider {
		///<summary>Opens a connection to the database.</summary>
		DbConnection OpenConnection();

		///<summary>Creates a SELECT command for the given SchemaMapping.</summary>
		DbCommand CreateSelectCommand(DbConnection connection, SchemaMapping schema);

		///<summary>Applies an inserted row to the database.</summary>
		void ApplyInsert(TransactionContext context, SchemaMapping schema, Row row);
		///<summary>Applies an update row to the database.</summary>
		void ApplyUpdate(TransactionContext context, SchemaMapping schema, Row row);
		///<summary>Applies a deleted row to the database.</summary>
		void ApplyDelete(TransactionContext context, SchemaMapping schema, Row row);

		///<summary>Creates a table for the given schema mapping.</summary>
		///<remarks>In addition to the columns in the SchemaMapping, a RowVersion column will be created.</remarks>
		void CreateTable(DbConnection connection, SchemaMapping schema, IEnumerable<SchemaMapping> parentSchemas);
	}
}
