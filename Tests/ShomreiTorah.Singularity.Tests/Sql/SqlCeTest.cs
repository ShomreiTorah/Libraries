using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using ShomreiTorah.Singularity.Sql;
using ShomreiTorah.Common;

namespace ShomreiTorah.Singularity.Tests.Sql {
	[TestClass]
	public class SqlCeTest : PersistenceTestBase {
		public TestContext TestContext { get; set; }

		static readonly string DBPath = Path.GetTempFileName();

		[ClassInitialize]
		public static void CreateDatabase(TestContext context) {
			File.Delete(DBPath);
			DB.CreateFile(DBPath, DatabaseFile.SqlCe);
		}
		[ClassCleanup]
		public static void DropDatabase() {
			File.Delete(DBPath);
		}

		public SqlCeTest() {
			base.SqlProvider = new SqlCeSqlProvider(DBPath);
		}
	}
}
