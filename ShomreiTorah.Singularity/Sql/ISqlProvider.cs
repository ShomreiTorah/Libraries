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
	}
}