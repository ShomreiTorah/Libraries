using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShomreiTorah.Common;
using ShomreiTorah.Singularity.Sql;

namespace ShomreiTorah.Singularity.Tests.Sql {
	[TestClass]
	public class SqlServerTest : PersistenceTestBase {
		public TestContext TestContext { get; set; }

		const string ConnectionString = @"Data Source=(localdb)\ProjectsV12;Initial Catalog=SingularityTests;Integrated Security=True";
		static readonly DBConnector DB = new DBConnector(SqlClientFactory.Instance, ConnectionString);

		[ClassInitialize]
		public static void CreateDatabase(TestContext context) {
			ExecWithoutDB(@"CREATE DATABASE SingularityTests;");

			DB.ExecuteNonQuery("CREATE SCHEMA Math;");
		}
		[ClassCleanup]
		public static void DropDatabase() {
			ExecWithoutDB(@"DROP DATABASE SingularityTests;");
		}
		static void ExecWithoutDB(string sql) {
			SqlConnection.ClearAllPools();
			using (var connection = new SqlConnection(@"Data Source=(localdb)\ProjectsV12;Integrated Security=True")) {
				connection.Open();
				connection.ExecuteNonQuery(sql);
			}
		}

		public SqlServerTest() {
			base.SqlProvider = new SqlServerSqlProvider(DB);
		}
	}
}
