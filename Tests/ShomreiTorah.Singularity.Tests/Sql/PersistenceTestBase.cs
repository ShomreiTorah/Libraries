using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShomreiTorah.Common;
using ShomreiTorah.Common.Calendar;
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

			try {
				var schema = new TableSchema("Numbers");
				schema.PrimaryKey = schema.Columns.AddValueColumn("ID", typeof(Guid), null);
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
			} finally {
				using (var connection = SqlProvider.OpenConnection())
					connection.ExecuteNonQuery(@"DROP TABLE Numbers;");
			}
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
			schema.PrimaryKey = schema.Columns.AddValueColumn("ID", typeof(Guid), null);
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

		static readonly Random rand = new Random();
		[TestMethod]
		public void ForeignKeysTest() {
			using (var connection = SqlProvider.OpenConnection()) {
				connection.ExecuteNonQuery(@"
CREATE TABLE [Numbers](
	[ID]			UNIQUEIDENTIFIER	NOT NULL	ROWGUIDCOL	PRIMARY KEY DEFAULT(newid()),
	[Number]		INTEGER				NOT NULL,
	[Date]			DATETIME			NULL,
	[RowVersion]	RowVersion
);");
				connection.ExecuteNonQuery(@"
CREATE TABLE [Powers](
	[ID]			UNIQUEIDENTIFIER	NOT NULL	ROWGUIDCOL	PRIMARY KEY DEFAULT(newid()),
	[Number]		UNIQUEIDENTIFIER	NULL		REFERENCES Numbers(Id),
	[Exponent]		INTEGER				NOT NULL,
	[Value]			INTEGER				NOT NULL,
	[RowVersion]	RowVersion
);");
			}


			try {
				var numbersSchema = new TableSchema("Numbers");
				var powersSchema = new TableSchema("Powers");

				numbersSchema.PrimaryKey = numbersSchema.Columns.AddValueColumn("ID", typeof(Guid), null);
				numbersSchema.Columns.AddValueColumn("Number", typeof(int), 0);
				numbersSchema.Columns.AddValueColumn("Date", typeof(DateTime?), null);

				powersSchema.PrimaryKey = powersSchema.Columns.AddValueColumn("ID", typeof(Guid), null);
				powersSchema.Columns.AddForeignKey("Number", numbersSchema, "Powers").AllowNulls = true;
				powersSchema.Columns.AddValueColumn("Exponent", typeof(int), 0);
				powersSchema.Columns.AddValueColumn("Value", typeof(int), 0);

				var context = new DataContext();
				Table numbersTable, powersTable;
				context.Tables.AddTable(numbersTable = new Table(numbersSchema));
				context.Tables.AddTable(powersTable = new Table(powersSchema));

				var syncContext = new DataSyncContext(context, SqlProvider);
				syncContext.Tables.AddDefaultMappings();

				Action verify = delegate {
					var newContext = new DataContext();
					newContext.Tables.AddTable(new Table(numbersSchema));
					newContext.Tables.AddTable(new Table(powersSchema));
					var newSyncContext = new DataSyncContext(newContext, SqlProvider);
					newSyncContext.Tables.AddDefaultMappings();
					newSyncContext.ReadData();

					AssertTablesEqual(context.Tables[numbersSchema], newContext.Tables[numbersSchema]);
					AssertTablesEqual(context.Tables[powersSchema], newContext.Tables[powersSchema]);


					context.Tables[powersSchema].Rows.Clear();
					context.Tables[numbersSchema].Rows.Clear();
					syncContext.ReadData();

					AssertTablesEqual(newContext.Tables[numbersSchema], context.Tables[numbersSchema]);
					AssertTablesEqual(newContext.Tables[powersSchema], context.Tables[powersSchema]);
				};

				for (int i = 0; i < 10; i++) {
					var row = numbersTable.Rows.AddFromValues(Guid.NewGuid(), i);

					if (rand.NextDouble() < .4)
						row["Date"] = DateTime.Today.AddMinutes((int)(1000000 * (.5 - rand.NextDouble())));

					for (int j = 0; j < 10; j++) {
						powersTable.Rows.AddFromValues(Guid.NewGuid(), row, j, (int)Math.Pow(i, j));
					}
					powersTable.Rows.Remove(powersTable.Rows.Last());
				}

				powersTable.Rows.AddFromValues(Guid.NewGuid(), null, 1, -1);
				powersTable.Rows.AddFromValues(Guid.NewGuid(), null, 2, -1);
				powersTable.Rows.AddFromValues(Guid.NewGuid(), null, 3, -1);
				powersTable.Rows.AddFromValues(Guid.NewGuid(), null, 4, -1);

				syncContext.WriteData();
				verify();

				for (int i = powersTable.Rows.Count - 1; i >= 0; i--) {
					if (rand.NextDouble() < .2)
						powersTable.Rows.RemoveAt(i);
				}
				for (int i = numbersTable.Rows.Count - 1; i >= 0; i--) {
					if (rand.NextDouble() < .2) {
						while (numbersTable.Rows[i].ChildRows("Powers").Count > 0) {
							if (rand.NextDouble() < .5)
								numbersTable.Rows[i].ChildRows("Powers")[0].RemoveRow();
							else
								numbersTable.Rows[i].ChildRows("Powers")[0]["Number"] = numbersTable.Rows[rand.Next(numbersTable.Rows.Count)];
						}

						numbersTable.Rows.RemoveAt(i);
					}
				}
				syncContext.WriteData();
				verify();

				var hundred = numbersTable.Rows.AddFromValues(Guid.NewGuid(), 100);
				powersTable.Rows.AddFromValues(Guid.NewGuid(), hundred, 1, 100);
				powersTable.Rows.AddFromValues(Guid.NewGuid(), hundred, 2, 10000);
				powersTable.Rows[rand.Next(powersTable.Rows.Count)]["Number"] = hundred;
				syncContext.WriteData();
				verify();



			} finally {
				using (var connection = SqlProvider.OpenConnection()) {
					connection.ExecuteNonQuery(@"DROP TABLE Powers;");
					connection.ExecuteNonQuery(@"DROP TABLE Numbers;");
				}
			}
		}

		[TestMethod]
		public void ConflictsTest() {
			using (var connection = SqlProvider.OpenConnection())
				connection.ExecuteNonQuery(@"
CREATE TABLE [ConflictsTest](
	[ID]			UNIQUEIDENTIFIER	NOT NULL	ROWGUIDCOL	PRIMARY KEY DEFAULT(newid()),
	[Key]			INTEGER				NOT NULL	UNIQUE,
	[Value]			INTEGER				NULL,
	[RowVersion]	RowVersion
);");

			try {
				var schema = new TableSchema("ConflictsTest");
				schema.PrimaryKey = schema.Columns.AddValueColumn("ID", typeof(Guid), null);
				schema.Columns.AddValueColumn("Key", typeof(int), null).Unique = true;
				schema.Columns.AddValueColumn("Value", typeof(int?), null);

				var mapping = new SchemaMapping(schema);

				var table1 = new Table(schema);
				var syncer1 = new TableSynchronizer(table1, mapping, SqlProvider);

				var table2 = new Table(schema);
				var syncer2 = new TableSynchronizer(table2, mapping, SqlProvider);

				Func<Table, int, Row> GetRow = (t, key) => t.Rows.SingleOrDefault(r => r.Field<int>("Key") == key);

				Func<int, Row> R1 = k => GetRow(table1, k),
							   R2 = k => GetRow(table2, k);

				table1.Rows.AddFromValues(Guid.NewGuid(), 2, 67);

				table2.Rows.AddFromValues(Guid.NewGuid(), 1, null);
				table2.Rows.AddFromValues(Guid.NewGuid(), 2, 43);

				syncer1.WriteData();

				try {
					syncer2.WriteData();
					Assert.Fail("Conflict succeeded");
				} catch (RowException ex) {
					Assert.IsNotNull(ex.InnerException);
					Assert.AreEqual(R2(2), ex.Row);
				}
				table2.Rows.Clear();
				syncer2.ReadData();
				AssertTablesEqual(table1, table2);

				table1.Rows.AddFromValues(Guid.NewGuid(), 1, null);
				syncer1.WriteData();
				syncer2.ReadData();

				R1(1)["Value"] = 100;
				R2(1)["Value"] = 200;

				syncer1.WriteData();

				try {		
					syncer2.WriteData();
					Assert.Fail("Conflict succeeded - Update / Update");
				} catch (RowModifiedException ex) {
					Assert.IsNull(ex.InnerException);
					Assert.AreEqual(R2(1), ex.Row);
					Assert.AreEqual(R1(1)["Key"], ex.DatabaseValues[schema.Columns["Key"]]);
					Assert.AreEqual(R1(1)["Value"], ex.DatabaseValues[schema.Columns["Value"]]);
				}
				syncer2.ReadData();
				AssertTablesEqual(table1, table2);

				R1(1)["Value"] = 123;
				R2(1).RemoveRow();
				syncer1.WriteData();
				try {
					syncer2.WriteData();
					Assert.Fail("Conflict succeeded - Update / Delete");
				} catch (RowModifiedException ex) {
					Assert.IsNull(ex.InnerException);
					Assert.AreEqual(1, ex.Row["Key"]);
					Assert.AreEqual(R1(1)["Value"], ex.DatabaseValues[schema.Columns["Value"]]);
				}
				syncer2.ReadData();
				AssertTablesEqual(table1, table2);

				R1(1).RemoveRow();
				R2(1)["Value"] = -1;
				syncer1.WriteData();
				try {
					syncer2.WriteData();
					Assert.Fail("Conflict succeeded - Delete / Update");
				} catch (RowDeletedException ex) {
					Assert.IsNull(ex.InnerException);
					Assert.AreEqual(R2(1), ex.Row);
				}
				syncer2.ReadData();
				AssertTablesEqual(table1, table2);

				R1(2).RemoveRow();
				R2(2).RemoveRow();
				syncer1.WriteData();
				syncer2.WriteData();		//Delete / Delete - should not throw

				syncer1.ReadData();
				Assert.AreEqual(0, table1.Rows.Count);

			} finally {
				using (var connection = SqlProvider.OpenConnection())
					connection.ExecuteNonQuery(@"DROP TABLE ConflictsTest;");
			}
		}

		static void AssertTablesEqual(Table expected, Table actual) {
			Assert.AreEqual(expected.Rows.Count, actual.Rows.Count);

			var expectedRows = expected.Rows.OrderBy(r => r[expected.Schema.PrimaryKey].ToString()).ToArray();
			var actualRows = actual.Rows.OrderBy(r => r[actual.Schema.PrimaryKey].ToString()).ToArray();

			for (int r = 0; r < expected.Rows.Count; r++) {
				foreach (var c in expected.Schema.Columns) {
					var fkc = c as ForeignKeyColumn;

					if (fkc == null)
						Assert.AreEqual(expectedRows[r][c], actualRows[r][c], actual.Schema.Name + "[" + r + "]." + c.Name);
					else {
						Row e = expectedRows[r].Field<Row>(c), a = actualRows[r].Field<Row>(c);
						if (e == null || a == null)
							Assert.AreEqual(e, a, actual.Schema.Name + "[" + r + "]." + c.Name);
						else
							Assert.AreEqual(e[fkc.ForeignSchema.PrimaryKey],
											a[fkc.ForeignSchema.PrimaryKey],
											actual.Schema.Name + "[" + r + "]." + c.Name
										   );
					}
				}
			}
		}
	}
}
