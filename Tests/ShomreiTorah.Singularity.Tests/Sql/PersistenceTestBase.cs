using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShomreiTorah.Common;
using ShomreiTorah.Singularity.Sql;

namespace ShomreiTorah.Singularity.Tests.Sql {
	/// <summary>
	/// A base class for classes that test persistence against a database.
	/// </summary>
	[TestClass]
	public abstract class PersistenceTestBase {
		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		public ISqlProvider SqlProvider { get; protected set; }

		[TestMethod]
		public void SimpleTableTest() {
			using (var connection = SqlProvider.OpenConnection())
				connection.ExecuteNonQuery(@"
CREATE TABLE [Numbers](
	[ID]			UNIQUEIDENTIFIER	NOT NULL	ROWGUIDCOL	PRIMARY KEY DEFAULT(newid()),
	[Value]			INTEGER				NOT NULL,
	[IsEven]		BIT					NOT NULL,
	[String]		NVARCHAR(1024)		NOT NULL,
	[RowVersion]	RowVersion
);");

			var schema = new TableSchema("Numbers");
			schema.PrimaryKey = schema.Columns.AddValueColumn("ID", typeof(Guid), Guid.Empty);
			schema.Columns.AddValueColumn("Value", typeof(int), 2);
			schema.Columns.AddValueColumn("IsEven", typeof(bool), false);
			schema.Columns.AddValueColumn("String", typeof(string), null).AllowNulls = false;

			var mapping = new SchemaMapping(schema);

			var table = new Table(schema);
			var syncer = new TableSynchronizer(table, mapping, SqlProvider);

			table.Rows.AddFromValues(Guid.NewGuid(), 2, true, "2");
			syncer.ReadData();
			Assert.AreEqual(0, table.Rows.Count);

			for (int i = 0; i < 10; i++) {
				var row = table.Rows.AddFromValues(Guid.NewGuid(), i, i % 2 == 0, "t");
				row["String"] = i.ToString();
			}
			Assert.IsTrue(syncer.Changes.All(c => c.ChangeType == RowChangeType.Added));
			Assert.AreEqual(10, syncer.Changes.Count);
			Assert.IsTrue(syncer.Changes.Select(c => c.Row).SequenceEqual(table.Rows));

			syncer.WriteData();

			foreach (var row in table.Rows.Where(r => r.Field<bool>("IsEven"))) {
				row["String"] += "E";
			}
			Assert.IsTrue(syncer.Changes.All(c => c.ChangeType == RowChangeType.Changed));
			Assert.AreEqual(5, syncer.Changes.Count);

			syncer.WriteData();

			var newTable = new Table(schema);
			var newSyncer = new TableSynchronizer(newTable, mapping, SqlProvider);
			newSyncer.ReadData();

			AssertTablesEqual(table, newTable);

			table.Rows[4].RemoveRow();
			table.Rows[7].RemoveRow();
			table.Rows[2].RemoveRow();

			syncer.WriteData();

			newSyncer.ReadData();
			AssertTablesEqual(table, newTable);

			newTable = new Table(schema);
			newSyncer = new TableSynchronizer(newTable, mapping, SqlProvider);
			newSyncer.ReadData();

			AssertTablesEqual(table, newTable);
			syncer.ReadData();
			AssertTablesEqual(newTable, table);
		}

		[TestMethod]
		public void NullValuesTest() {
			using (var connection = SqlProvider.OpenConnection())
				connection.ExecuteNonQuery(@"
CREATE TABLE [NullableTest](
	[ID]			UNIQUEIDENTIFIER	NOT NULL	ROWGUIDCOL	PRIMARY KEY DEFAULT(newid()),
	[Integer]		INTEGER				NULL,
	[String]		NVARCHAR(1024)		NULL,
	[RowVersion]	RowVersion
);");


			var schema = new TableSchema("NullableTest");
			schema.PrimaryKey = schema.Columns.AddValueColumn("ID", typeof(Guid), Guid.Empty);
			schema.Columns.AddValueColumn("Integer", typeof(int?), null);
			schema.Columns.AddValueColumn("String", typeof(string), null);

			var mapping = new SchemaMapping(schema);

			var table = new Table(schema);
			var syncer = new TableSynchronizer(table, mapping, SqlProvider);

			table.Rows.AddFromValues(Guid.NewGuid());
			table.Rows.AddFromValues(Guid.NewGuid(), 4, null);
			table.Rows.AddFromValues(Guid.NewGuid(), null, "4");
			table.Rows.AddFromValues(Guid.NewGuid(), 4, "4");

			syncer.WriteData();

			var newTable = new Table(schema);
			var newSyncer = new TableSynchronizer(newTable, mapping, SqlProvider);
			newSyncer.ReadData();

			AssertTablesEqual(table, newTable);

			table.Rows[0]["Integer"] = 5;
			table.Rows[0]["String"] = "5";
			table.Rows[3]["Integer"] = null;
			table.Rows[3]["String"] = null;

			syncer.WriteData();

			newTable = new Table(schema);
			newSyncer = new TableSynchronizer(newTable, mapping, SqlProvider);
			newSyncer.ReadData();

			AssertTablesEqual(table, newTable);
			syncer.ReadData();
			AssertTablesEqual(newTable, table);
		}

		static void AssertTablesEqual(Table expected, Table actual) {
			Assert.AreEqual(expected.Rows.Count, actual.Rows.Count);

			var expectedRows = expected.Rows.OrderBy(r => r[expected.Schema.PrimaryKey].ToString()).ToArray();
			var actualRows = actual.Rows.OrderBy(r => r[actual.Schema.PrimaryKey].ToString()).ToArray();

			for (int r = 0; r < expected.Rows.Count; r++) {
				foreach (var c in expected.Schema.Columns) {
					Assert.AreEqual(expectedRows[r][c], actualRows[r][c], actual.Schema.Name + "[" + r + "]." + c.Name);
				}
			}
		}
	}
}
