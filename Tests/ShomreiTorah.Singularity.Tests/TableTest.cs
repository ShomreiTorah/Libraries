using ShomreiTorah.Singularity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

namespace ShomreiTorah.Singularity.Tests {
	[TestClass]
	public class TableTest {
		public TestContext TestContext { get; set; }

		#region Additional test attributes
		// 
		//You can use the following additional attributes as you write your tests:
		//
		//Use ClassInitialize to run code before running the first test in the class
		//[ClassInitialize()]
		//public static void MyClassInitialize(TestContext testContext)
		//{
		//}
		//
		//Use ClassCleanup to run code after all tests in a class have run
		//[ClassCleanup()]
		//public static void MyClassCleanup()
		//{
		//}
		//
		//Use TestInitialize to run code before running each test
		//[TestInitialize()]
		//public void MyTestInitialize()
		//{
		//}
		//
		//Use TestCleanup to run code after each test has run
		//[TestCleanup()]
		//public void MyTestCleanup()
		//{
		//}
		//
		#endregion


		[TestMethod]
		public void BasicValuesTest() {
			var table = new Table("BasicValuesTest - Powers of 2");

			int rowCount = 0;
			table.RowAdded += (s, e) => {
				Assert.AreEqual(e.Row, table.Rows.Last());
				rowCount++;
				Assert.AreEqual(rowCount, table.Rows.Count);
			};
			table.RowRemoved += (s, e) => {
				Assert.IsFalse(table.Rows.Contains(e.Row));
				rowCount--;
				Assert.AreEqual(rowCount, table.Rows.Count);
			};

			table.Schema.Columns.AddValueColumn("Base", typeof(int), 2);
			table.Schema.Columns.AddValueColumn("Exponent", typeof(int), 0);
			var strValCol = table.Schema.Columns.AddValueColumn("StringValue", typeof(string), null);

			table.Rows.AddFromValues(2, 0, "1");
			for (int i = 1; i <= 6; i++) {
				table.Rows.AddFromValues(2, i, Math.Pow(2, i).ToString("n0"));
			}
			for (int i = 7; i <= 10; i++) {
				var row = new Row(table.Schema);
				row["Exponent"] = i;
				row[strValCol] = Math.Pow(2, i).ToString("n0");
				table.Rows.Add(row);
			}
			Assert.AreEqual(11, table.Rows.Count);
			Assert.AreEqual("1,024", table.Rows.Last()[strValCol]);
			table.Rows.Remove(table.Rows.Last());
			Assert.AreEqual("512", table.Rows.Last()["StringValue"]);
			Assert.AreEqual(10, table.Rows.Count);

			table.Rows.Clear();
			Assert.AreEqual(0, table.Rows.Count);
		}
		//TODO: int?
		[TestMethod]
		public void RelationTest() {
			var context = new DataContext();
			var numbersTable = new Table("Numbers");
			var powersTable = new Table("Powers");
			context.Tables.AddTable(powersTable);
			context.Tables.AddTable(numbersTable);

			numbersTable.Schema.Columns.AddValueColumn("Number", typeof(int), 0);

			var powersKeyColumn = powersTable.Schema.Columns.AddForeignKey("Number", numbersTable.Schema, "Powers");
			powersTable.Schema.Columns.AddValueColumn("Exponent", typeof(int), 0);
			powersTable.Schema.Columns.AddValueColumn("Value", typeof(int), 0);

			for (int i = 0; i < 10; i++) {
				var row = numbersTable.Rows.AddFromValues(i);

				if (i % 3 == 0) {
					var children = row.ChildRows("Powers");

					int rowCount = 0;
					children.RowAdded += (s, e) => {
						Assert.AreEqual(e.Row, children.Last());
						rowCount++;
						Assert.AreEqual(rowCount, children.Count);
					};
					children.RowRemoved += (s, e) => {
						Assert.IsFalse(children.Contains(e.Row));
						rowCount--;
						Assert.AreEqual(rowCount, children.Count);
					};
				}

				for (int j = 0; j < 11; j++) {
					powersTable.Rows.AddFromValues(row, j, (int)Math.Pow(i, j));
				}
				powersTable.Rows.Remove(powersTable.Rows.Last());
				row.ChildRows(powersKeyColumn.ChildRelation).Last().RemoveRow();

				var powerRow = new Row(powersTable.Schema);
				powersTable.Rows.Add(powerRow);
				powerRow[powersKeyColumn] = row;
				powerRow["Exponent"] = 10;
				powerRow["Value"] = (int)Math.Pow(i, 9);

				Assert.AreEqual(10, row.ChildRows(powersKeyColumn.ChildRelation).Count);
			}
			Assert.AreEqual(10, numbersTable.Rows.Count);
			Assert.AreEqual(100, powersTable.Rows.Count);

			powersTable.Rows.Clear();
			Assert.AreEqual(0, powersTable.Rows.Count);
			Assert.AreEqual(0, numbersTable.Rows[0].ChildRows(powersKeyColumn.ChildRelation).Count);


			powersTable.Rows.AddFromValues(numbersTable.Rows[0], 4, 5);
			powersTable.Schema.Columns.RemoveColumn("Number");
			Assert.AreEqual(0, numbersTable.Schema.ChildRelations.Count);
		}

		[TestMethod]
		public void ValueCoercionTest() {
			Assert.Inconclusive("I need to design coercion");
		}
		[TestMethod]
		public void ColumnChangingTest() {
			var table1 = new Table("ColumnChangingTest");

			table1.Schema.Columns.AddValueColumn("Base", typeof(int), 2);
			table1.Schema.Columns.AddValueColumn("Exponent", typeof(int), 0);
			var strValCol = table1.Schema.Columns.AddValueColumn("StringValue", typeof(string), null);

			var table2 = new Table(table1.Schema);

			for (int i = 0; i < 10; i++) {
				var row = new Row(table1.Schema);
				row["Exponent"] = i;
				row[strValCol] = Math.Pow(2, i).ToString("n0");
				table1.Rows.Add(row);

				table2.Rows.AddFromValues(2, i, Math.Pow(2, i).ToString("n0"));
			}
			table1.Schema.Columns.RemoveColumn("StringValue");
			Assert.AreEqual(2, table2.Schema.Columns.Count);
			table1.Schema.Columns.AddValueColumn("StringValue", typeof(string), "DefVal");
			Assert.AreEqual(3, table2.Schema.Columns.Count);

			Assert.AreEqual("DefVal", table1.Rows[7].Field<string>("StringValue"));
			Assert.AreEqual("DefVal", table2.Rows[5]["StringValue"]);
			table2.Rows[5]["StringValue"] = "Other";
			Assert.AreEqual("Other", table2.Rows[5]["StringValue"]);
		}

		[TestMethod]
		public void NullableColumnTest() {
			var table = new Table("NullableColumnTest");
			var refCol = table.Schema.Columns.AddValueColumn("NoNullString", typeof(string), null);
			refCol.AllowNulls = false;

			var newRow = new Row(table.Schema);
			try {
				table.Rows.Add(newRow);
				Assert.Fail("Added row with null value");
			} catch (InvalidOperationException) { }
			var nullValCol = table.Schema.Columns.AddValueColumn("NullInt", typeof(int?), null);
			Assert.IsTrue(nullValCol.AllowNulls);
			Assert.AreEqual(typeof(int), nullValCol.DataType);
			newRow[refCol] = "abc";
			table.Rows.Add(newRow);
			Assert.AreEqual(null, newRow.Field<int?>(nullValCol));

			table.Rows.AddFromValues("def", 7);
			try {
				nullValCol.AllowNulls = false;
				Assert.Fail("Restricted column with null value");
			} catch (InvalidOperationException) { }
			newRow.RemoveRow();
			nullValCol.AllowNulls = false;
		}

		[TestMethod]
		public void XmlPersistenceTest() {
			var context = new DataContext();
			var table = new Table("ToXmlTest");
			var foreignTable = new Table("ToXmlTest - Evenness");

			context.Tables.AddTable(table);
			context.Tables.AddTable(foreignTable);

			table.Schema.Columns.AddValueColumn("Number", typeof(int), null);
			table.Schema.Columns.AddValueColumn("X Repetitions", typeof(string), null).AllowNulls = false;

			foreignTable.Schema.PrimaryKey = foreignTable.Schema.Columns.AddValueColumn("Am I Even?", typeof(bool), null);
			table.Schema.Columns.AddForeignKey("IsEven", foreignTable.Schema, "Numbers");

			var evenRow = foreignTable.Rows.AddFromValues(true);
			var oddRow = foreignTable.Rows.AddFromValues(false);

			for (int i = 0; i < 20; i++) {
				table.Rows.AddFromValues(i, new string('X', i), (i % 2 == 0) ? evenRow : oddRow);
			}

			var element = table.ToXml();

			Assert.AreEqual("ToXmlTest", element.Name);
			Assert.AreEqual(20, element.Elements().Count());

			for (int i = 0; i < 20; i++) {
				var row = element.Elements().ElementAt(i);
				Assert.AreEqual(i.ToString(), row.Element("Number").Value);
				Assert.AreEqual(new string('X', i), row.Element("X_x0020_Repetitions").Value);
				Assert.AreEqual((i % 2 == 0).ToString().ToLowerInvariant(), row.Element("IsEven").Value);
			}

			table.Rows.Clear();
			table.ReadXml(element);

			Assert.AreEqual(20, table.Rows.Count);

			ValidateRows(table.Rows, evenRow, oddRow);

			table.Schema.PrimaryKey = table.Schema.Columns[0];
			var rand = new Random();
			for (int i = 0; i < rand.Next(table.Rows.Count); i++) {
				if (rand.NextDouble() < .3)
					table.Rows.RemoveAt(rand.Next(table.Rows.Count));
			}
			table.Rows.AddFromValues(-4, "Wrong!!!", null);
			table.Rows.AddFromValues(-5, "Wrong!!!", null);
			table.ReadXml(element);
			Assert.AreEqual(20, table.Rows.Count);

			var sortedRows = table.Rows.OrderBy(r => r.Field<int>("Number")).ToArray();
			ValidateRows(sortedRows, evenRow, oddRow);
		}
		void ValidateRows(IList<Row> rows, Row evenRow, Row oddRow) {
			for (int i = 0; i < rows.Count; i++) {
				var row = rows[i];
				Assert.AreEqual(i, row.Field<int>("Number"));
				Assert.AreEqual(new string('X', i), row["X Repetitions"]);
				Assert.AreEqual((i % 2 == 0) ? evenRow : oddRow, row["IsEven"]);
			}
		}
	}
}
