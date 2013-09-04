using ShomreiTorah.Singularity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace ShomreiTorah.Singularity.Tests {


	[TestClass()]
	public class ValueColumnTest {
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

		//TODO: Test ForeignKeyColumn

		[TestMethod]
		public void AllowNullsTest() {
			var table = new Table("AllowNullsTest");

			var nullIntColumn = table.Schema.Columns.AddValueColumn("Nullable Integer", typeof(int?), null);
			var noNullIntColumn = table.Schema.Columns.AddValueColumn("Non-nullable Integer", typeof(int), null);

			var nullStringColumn = table.Schema.Columns.AddValueColumn("Nullable string", typeof(string), null);
			var noNullStringColumn = table.Schema.Columns.AddValueColumn("Non-nullable string", typeof(string), null);
			noNullStringColumn.AllowNulls = false;

			var row = table.Rows.AddFromValues(1, 2, "3", "4");

			Assert.IsNotNull(row.ValidateValue(noNullIntColumn, new DateTime()));

			Assert.IsNull(row.ValidateValue(nullIntColumn, null));
			Assert.IsNull(row.ValidateValue(nullStringColumn, null));

			Assert.IsNull(row.ValidateValue(nullIntColumn, 5));
			Assert.IsNull(row.ValidateValue(nullStringColumn, "5"));

			Assert.IsNotNull(row.ValidateValue(noNullIntColumn, null));
			Assert.IsNotNull(row.ValidateValue(noNullStringColumn, null));

			Assert.IsNull(row.ValidateValue(noNullIntColumn, 5));
			Assert.IsNull(row.ValidateValue(noNullStringColumn, "5"));

			var row2 = table.Rows.AddFromValues(1, 2, null, "4");
			try {
				nullStringColumn.AllowNulls = false;
				Assert.Fail("Made column non-null");
			} catch (InvalidOperationException) { }
		}
		[TestMethod]
		public void UniqueTest() {
			var table = new Table("AllowNullsTest");
			var column = table.Schema.Columns.AddValueColumn("Column", typeof(int?), null);

			var row = table.Rows.AddFromValues(1);
			var row2 = table.Rows.AddFromValues(1);

			try {
				column.Unique = true;
				Assert.Fail("Made column unique");
			} catch (InvalidOperationException) { }

			row2[column] = 5;
			column.Unique = true;

			Assert.IsNull(row.ValidateValue(column, null));
			Assert.IsNull(row.ValidateValue(column, 1));		//Duplicate of same row
			Assert.IsNotNull(row.ValidateValue(column, 5));

			row2[column] = null;
			Assert.IsNotNull(row.ValidateValue(column, null));
		}

		[TestMethod]
		public void ForeignKeyDeletionTest() {
			var numbersSchema = new TableSchema("Numbers");
			var powersSchema = new TableSchema("Powers");

			numbersSchema.Columns.AddValueColumn("Number", typeof(int), 0);
			numbersSchema.Columns.AddValueColumn("Date", typeof(DateTime?), null);

			powersSchema.Columns.AddForeignKey("Number", numbersSchema, "Powers");
			powersSchema.Columns.AddValueColumn("Exponent", typeof(int), 0);
			powersSchema.Columns.AddValueColumn("Value", typeof(int), 0);

			var context = new DataContext();
			Table numbersTable, powersTable;
			context.Tables.AddTable(numbersTable = new Table(numbersSchema));
			context.Tables.AddTable(powersTable = new Table(powersSchema));

			var n1 = numbersTable.Rows.AddFromValues(2);
			var p1 = powersTable.Rows.AddFromValues(n1, 2, 4);
			var p2 = powersTable.Rows.AddFromValues(n1, 4, 16);

			CollectionAssert.AreEqual(new[] { p1, p2 }, n1.ChildRows("Powers").ToList());

			n1.RemoveRow();

			p1.RemoveRow();
			Assert.AreEqual(n1, p1["Number"]);


			Assert.AreEqual(n1, p1["Number"]);
			Assert.AreEqual(n1, p2["Number"]);
		}
	}
}
