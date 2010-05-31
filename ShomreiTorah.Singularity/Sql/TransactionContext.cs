using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using ShomreiTorah.Common;

namespace ShomreiTorah.Singularity.Sql {
	///<summary>The context of a transacted database update.</summary>
	///<remarks>An instance of the class is created by the WriteData 
	///methods of TableSynchronizer and DataSyncContext.  This class
	///holds the database connection and transaction, and correctly
	///handles RowVersion changes after the transaction completes.</remarks>
	public sealed class TransactionContext : IDisposable {
		///<summary>Creates a TransactionContext for a database connection.</summary>
		public TransactionContext(DbConnection connection) {
			if (connection == null) throw new ArgumentNullException("connection");
			Connection = connection;
			Transaction = connection.BeginTransaction();
		}

		///<summary>Gets the database connection.</summary>
		public DbConnection Connection { get; private set; }
		///<summary>Gets the database transaction.</summary>
		public DbTransaction Transaction { get; private set; }

		///<summary>Creates a SQL command associated with the transaction.</summary>
		public DbCommand CreateCommand(string sql) { return CreateCommand<object>(sql, null); }
		///<summary>Creates a parameterized SQL command associated with the transaction.</summary>
		public DbCommand CreateCommand<TParameters>(string sql, TParameters parameters) where TParameters : class {
			var retVal = Connection.CreateCommand(sql, parameters);
			retVal.Transaction = Transaction;
			return retVal;
		}

		List<KeyValuePair<Row, object>> newRowVersions = new List<KeyValuePair<Row, object>>();

		///<summary>Sets a row's RowVersion after the transaction is committed.</summary>
		public void SetRowVersion(Row row, object newVersion) {
			if (row == null) throw new ArgumentNullException("row");
			if (newVersion == null) throw new ArgumentNullException("newVersion");

			newRowVersions.Add(new KeyValuePair<Row, object>(row, newVersion));
		}

		internal void Commit() {
			Transaction.Commit();
			foreach (var kvp in newRowVersions) {
				kvp.Key.RowVersion = kvp.Value;
			}
		}

		///<summary>Releases all resources used by the TransactionContext.</summary>
		public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }
		///<summary>Releases the unmanaged resources used by the TransactionContext and optionally releases the managed resources.</summary>
		///<param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		void Dispose(bool disposing) {
			if (disposing) {
				if (Transaction != null) Transaction.Dispose();
			}
		}
	}
}
