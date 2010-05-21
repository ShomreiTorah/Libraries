using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

		protected PersistenceTestBase() {
			NumbersSchema = new TableSchema("Numbers");
			NumbersSchema.PrimaryKey = NumbersSchema.Columns.AddValueColumn("ID", typeof(Guid), Guid.Empty);
			NumbersSchema.Columns.AddValueColumn("Value", typeof(int), 2);
			NumbersSchema.Columns.AddValueColumn("IsEven", typeof(bool), false);
			NumbersSchema.Columns.AddValueColumn("String", typeof(string), null).AllowNulls = false;

			NumbersMapping = new SchemaMapping(NumbersSchema);
		}

		public TableSchema NumbersSchema { get; private set; }
		public SchemaMapping NumbersMapping { get; private set; }

		public ISqlProvider SqlProvider { get; protected set; }
		[TestMethod]
		public void SimpleTableTest() {
			using (var connection = SqlProvider.OpenConnection())
			using (var command = SqlProvider.CreateTable(connection, NumbersMapping)) {
				command.ExecuteNonQuery();
			}

			var table = new Table(NumbersSchema);
			var syncer = new TableSynchronizer(table, NumbersMapping, SqlProvider);

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

			var newTable = new Table(NumbersSchema);
			var newSyncer = new TableSynchronizer(newTable, NumbersMapping, SqlProvider);
			newSyncer.ReadData();

			AssertTablesEqual(table, newTable);

			table.Rows[4].RemoveRow();
			table.Rows[7].RemoveRow();
			table.Rows[2].RemoveRow();

			syncer.WriteData();

			newSyncer.ReadData();
			AssertTablesEqual(table, newTable);

			newTable = new Table(NumbersSchema);
			newSyncer = new TableSynchronizer(newTable, NumbersMapping, SqlProvider);
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
